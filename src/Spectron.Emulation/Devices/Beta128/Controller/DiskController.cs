using OldBit.Spectron.Emulation.Devices.Beta128.Controller.Commands;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal sealed partial class DiskController
{
    private const byte ControlDriveSelect = 0b000_0011;
    private const byte ControlDriveSide = 0b0001_0000;
    private const byte ControlResetPulse = 0b0000_0100;
    private const byte ControlHeadEnable = 0b0000_1000;

    private readonly int _millisecond;      // number of T states in 1 ms
    private readonly int _rotationTime;     // number of T states in 1 disk rotation
    private readonly int _byteTime;         // number of T states per 1 byte
    private readonly int _clockHz;

    private readonly DiskDrive[] _drives = new DiskDrive[4];
    private DiskDrive _drive;

    private byte _data;
    private byte _control;

    private long _next;
    private long _maxAddressMarkWaitTime;
    private Sector? _currentSector;
    private int _stepIncrement = 1;

    private Command _command;

    private int _shift;

    private Word _crc;
    private int _readWritePosition;
    private int _readWriteLength;

    private ControllerState _controllerState = ControllerState.Idle;
    private ControllerState _nextControllerState = ControllerState.Idle;
    private ControllerStatus _controllerStatus = 0;
    private RequestStatus _requestStatus = 0;

    internal byte TrackRegister { get; set; }
    internal byte SectorRegister { get; set; }
    internal byte SideNo { get; set; }

    internal RequestStatus Request => _requestStatus;

    public DiskController(float clockMhz)
    {
        _clockHz = (int)(clockMhz * 1_000_000);
        _millisecond = _clockHz / 1000;
        _rotationTime = _clockHz / DiskDrive.Rps;

        // ts_byte
        _byteTime = _clockHz / (Track.DataLength * DiskDrive.Rps);

        _drives[0] = new DiskDrive();   // A:
        _drives[1] = new DiskDrive();   // B:
        _drives[2] = new DiskDrive();   // C:
        _drives[3] = new DiskDrive();   // D:

        _drive = _drives[0];
    }

    internal byte DataRegister
    {
        get
        {
            _controllerStatus &= ~ControllerStatus.DataRequest;
            _requestStatus &= ~RequestStatus.DataRequest;

            return _data;
        }
        set
        {
            _data = value;

            _controllerStatus &= ~ControllerStatus.DataRequest;
            _requestStatus &= ~RequestStatus.DataRequest;
        }
    }

    internal byte ControlRegister
    {
        set
        {
            _control = value;

            _drive = _drives[_control & ControlDriveSelect];
            SideNo = (byte)((_control & ControlDriveSide) != 0 ? 0 : 1);

            if ((_control & ControlResetPulse) != 0)
            {
                return;
            }

            _controllerStatus = ControllerStatus.NotReady;
            _requestStatus = RequestStatus.InterruptRequest;
            _controllerState = ControllerState.Idle;

            _drive.Stop();
        }
    }

    internal byte Status
    {
        get
        {
            _requestStatus &= ~RequestStatus.InterruptRequest;
            return (byte)_controllerStatus;
        }
    }

    internal void ProcessState(long now)
    {
        if (now > _drive.SpinTime && IsHeadLoaded)
        {
            _drive.Stop();
        }

        if (_drive.IsDiskLoaded)
        {
            _controllerStatus &= ~ControllerStatus.NotReady;
        }
        else
        {
            _controllerStatus |= ControllerStatus.NotReady;
        }

        if (_command.Type != CommandType.Type4)
        {
            _controllerStatus &= ~(ControllerStatus.TrackZero | ControllerStatus.Index);

            if (_drive.IsSpinning && IsHeadLoaded)
            {
                _controllerStatus |= ControllerStatus.HeadLoaded;
            }

            if (_drive.IsTrackZero)
            {
                _controllerStatus |= ControllerStatus.TrackZero;
            }

            if (_drive is { IsDiskLoaded: true, IsSpinning: true } && IsWithinIndexHole(now))
            {
                _controllerStatus |= ControllerStatus.Index;
            }
        }

        while (true)
        {
            switch (_controllerState)
            {
                case ControllerState.Idle:
                    ProcessIdleState();
                    return;

                case ControllerState.Wait:
                    if (!ProcessWaitState(now))
                    {
                        return;
                    }
                    break;

                case ControllerState.DelayBeforeCommand:
                    ProcessDelayBeforeCommandState();
                    break;

                case ControllerState.CommandType1:
                    ProcessCommandType1State();
                    break;

                case ControllerState.CommandReadWrite:
                    ProcessCommandReadWriteState();
                    break;

                case ControllerState.ReadSector:
                    ProcessReadSectorState();
                    break;

                case ControllerState.Read:
                    ProcessReadState();
                    break;

                case ControllerState.Write:
                    break;

                case ControllerState.WriteSector:
                    // TODO: Implement WriteSector
                    break;

                case ControllerState.WriteTrack:
                    // TODO: Implement WriteTrack
                    break;

                case ControllerState.WriteTrackData:
                    // TODO: Implement WriteTrackData
                    break;

                case ControllerState.Step:
                    ProcessStepState();
                    break;

                case ControllerState.SeekStart:
                    ProcessSeekStartState();
                    break;

                case ControllerState.Seek:
                    ProcessSeekState();
                    break;

                case ControllerState.Verify:
                    ProcessVerifyState();
                    break;

                case ControllerState.Reset:
                    ProcessResetState();
                    break;

                case ControllerState.FOUND_NEXT_ID:
                    ProcessFoundNextIdState();
                    break;
            }
        }
    }

    internal void ProcessCommand(long now, byte commandCode)
    {
        var command = new Command(commandCode);

        if (command.IsForceInterrupt)
        {
            ProcessForceInterruptCommand(now, command);
            return;
        }

        if (IsBusy)
        {
            return;
        }

        _command = command;
        _next = now;

        _controllerStatus |= ControllerStatus.Busy;
        _requestStatus = RequestStatus.None;

        if (_command.Type is CommandType.Type2 or CommandType.Type3)
        {
            ProcessReadWriteCommands();
            return;
        }

        _controllerState = ControllerState.CommandType1;
    }

    private void ProcessForceInterruptCommand(long now, Command command)
    {
        _controllerState = ControllerState.Idle;
        _requestStatus = RequestStatus.InterruptRequest;
        _controllerStatus &= ~ControllerStatus.Busy;

        _command = command;
        _next = now;
    }

    private void ProcessReadWriteCommands()
    {
        if (IsNotReady)
        {
            _controllerState = ControllerState.Idle;
            _requestStatus = RequestStatus.InterruptRequest;

            return;
        }

        if (_drive.IsSpinning)
        {
            _drive.Spin(_next + 2 * _clockHz);
        }

        _controllerState = ControllerState.DelayBeforeCommand;;
    }

    private void FindMarker()
    {
        Seek();

        var wait = 10 * _rotationTime;
        _currentSector = null;

        if (_drive is { IsSpinning: true, IsDiskLoaded: true, Track: not null })
        {
            var trackDuration = _drive.Track.Data.Length * _byteTime; // TODO: This is constant value
            var position = (int)((_next + _shift) % trackDuration / _byteTime);

            wait = int.MaxValue;

            for(var sectorNo = 1; sectorNo <= 16; sectorNo++)
            {
                var sector = _drive.Track[sectorNo];
                var idPosition = sector.IdPosition;

                var distance = idPosition > position ?
                    idPosition - position :
                    _drive.Track.Data.Length + idPosition - position;

                if (distance < wait)
                {
                    wait = distance;
                    _currentSector = sector;
                }
            }

            wait = _currentSector != null ? wait * _byteTime : 10 * _rotationTime;
        }

        _next += wait;

        if (_drive.IsDiskLoaded && _next > _maxAddressMarkWaitTime)
        {
            _next = _maxAddressMarkWaitTime;
            _currentSector = null;
        }

        _controllerState = ControllerState.Wait;
        _nextControllerState = ControllerState.FOUND_NEXT_ID;
    }

    private void FindIndex()
    {
        var trackDuration = _drive.Track!.Data.Length * _byteTime ;
        var position = (int)((_next + _shift) % trackDuration / _byteTime);

        _next += trackDuration - position;
        _readWritePosition = 0;
        _readWriteLength = _drive.Track.Data.Length;
    }

    private void ReadFirstByte()
    {
        _crc = Crc.Calculate(_drive.Track!.Data[_readWritePosition - 1]); // Address/Data Mark
        _data = _drive.Track.Data[_readWritePosition];
        _crc = Crc.Calculate(_data, _crc);

        _readWritePosition += 1;
        _readWriteLength -= 1;

        _requestStatus = RequestStatus.DataRequest;
        _controllerStatus |= ControllerStatus.DataRequest;

        _controllerState = ControllerState.Wait;
        _nextControllerState = ControllerState.Read;
    }

    private void Seek() => _drive.Seek(TrackRegister, SideNo);

    private bool IsBusy => (_controllerStatus & ControllerStatus.Busy) != 0;
    private bool IsNotReady => (_controllerStatus & ControllerStatus.NotReady) != 0;
    private bool IsHeadLoaded => (_control & ControlHeadEnable) != 0;
    private bool IsWithinIndexHole(long now) => (now + _shift) % _rotationTime < 4 * _millisecond;
}