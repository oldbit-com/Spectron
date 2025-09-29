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
    private readonly int _rotation;         // number of T states in 1 disk rotation
    private readonly int _byteTime;         // number of T states per 1 byte

    private readonly int _clockHz;
    private readonly DiskDrive[] _drives = new DiskDrive[4];
    private DiskDrive _drive;

    private byte _data;
    private byte _control;

    private long _next;
    private long _maxAddressMarkWaitTime;
    private Sector? _sectorFound;

    private byte _command;
    private int _shift;
    private int _readWritePosition;
    private int _readWriteLength;

    private CommandType _commandType;
    private State _state = State.Idle;
    private State _nextState = State.Idle;
    private ControllerStatus _controllerStatus = 0;
    private RequestStatus _requestStatus = 0;

    internal byte TrackNo
    {
        get => _drive.SectorNo;
        set => _drive.SectorNo = value;
    }

    internal byte SectorNo
    {
        get => _drive.SectorNo;
        set => _drive.SectorNo = value;
    }
    // internal byte SectorNo { get; set; }

    internal RequestStatus Request => _requestStatus;

    public DiskController(float clockMhz)
    {
        _clockHz = (int)(clockMhz * 1_000_000);
        _millisecond = _clockHz / 1000;
        _rotation = _clockHz / DiskDrive.Rps;

        // ts_byte
        _byteTime = _clockHz / (Track.DataLength * DiskDrive.Rps);

        _drives[0] = new DiskDrive();   // A:
        _drives[1] = new DiskDrive();   // B:
        _drives[2] = new DiskDrive();   // C:
        _drives[3] = new DiskDrive();   // D:

        _drive = _drives[0];
    }

    internal byte Data
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

    internal byte Control
    {
        set
        {
            _control = value;

            _drive = _drives[_control & ControlDriveSelect];
            _drive.SideNo = (byte)((_control & ControlDriveSide) != 0 ? 0 : 1);

            if ((_control & ControlResetPulse) != 0)
            {
                return;
            }

            _controllerStatus = ControllerStatus.NotReady;
            _requestStatus = RequestStatus.InterruptRequest;
            _state = State.Idle;

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
            ResetFlag(ControllerStatus.NotReady);
        }
        else
        {
            SetFlag(ControllerStatus.NotReady);
        }

        if (_commandType != CommandType.Type4)
        {
            ResetFlag(ControllerStatus.TrackZero | ControllerStatus.Index);

            if (_drive.IsSpinning && IsHeadLoaded)
            {
                SetFlag(ControllerStatus.HeadLoaded);
            }

            if (_drive.IsTrackZero)
            {
                SetFlag(ControllerStatus.TrackZero);
            }

            if (_drive is { IsDiskLoaded: true, IsSpinning: true } && IsWithinIndexHole(now))
            {
                SetFlag(ControllerStatus.Index);
            }
        }

        while (true)
        {
            switch (_state)
            {
                case State.Idle:
                    ProcessIdle();
                    return;

                case State.Wait:
                    ProcessWait(now);
                    break;

                case State.DelayBeforeCommand:
                    ProcessDelayBeforeCommand();
                    break;

                case State.CommandReadWrite:
                    ProcessCommandReadWrite();
                    break;

                case State.FOUND_NEXT_ID:
                    ProcessFoundNextId();
                    break;

                default:
                    // Temporary exit
                    //_controllerStatus |= ControllerStatus.NotReady;
                    return;
            }
        }
    }

    internal void ProcessCommand(long now, byte command)
    {
        var commandType = Command.GetType(command);

        if (commandType == CommandType.Type4)
        {
            ProcessForceInterruptCommand(now, command);
            return;
        }

        if (IsBusy)
        {
            return;
        }

        _command = command;
        _commandType = commandType;
        _next = now;

        _controllerStatus |= ControllerStatus.Busy;
        _requestStatus = RequestStatus.None;

        if (commandType is CommandType.Type2 or CommandType.Type3)
        {
            ProcessReadWriteCommands();
            return;
        }

        _state = State.CommandType1;
    }

    private void ProcessForceInterruptCommand(long now, byte command)
    {
        _state = State.Idle;
        _requestStatus = RequestStatus.InterruptRequest;
        _controllerStatus &= ~ControllerStatus.Busy;

        _command = command;
        _next = now;
    }

    private void ProcessReadWriteCommands()
    {
        if (IsNotReady)
        {
            _state = State.Idle;
            _requestStatus = RequestStatus.InterruptRequest;

            return;
        }

        if (_drive.IsSpinning)
        {
            _drive.Spin(_next + 2 * _clockHz);
        }

        _state = State.DelayBeforeCommand;;
    }

    private void FindMarker()
    {
        _drive.Seek();

        var wait = 10 * _rotation;
        _sectorFound = null;

        if (_drive is { IsSpinning: true, IsDiskLoaded: true, Track: not null })
        {
            var trackDuration = _drive.Track.Data.Length * _byteTime; // TODO: This is constant value
            var position = (int)((_next + _shift) % trackDuration / _byteTime);

            wait = int.MaxValue;

            foreach (var sector in _drive.Track.Sectors)
            {
                var idPosition = sector.IdPosition;

                var distance = idPosition > position ?
                    idPosition - position :
                    _drive.Track.Data.Length + idPosition - position;

                if (distance < wait)
                {
                    wait = distance;
                    _sectorFound = sector;
                }
            }

            wait = _sectorFound != null ? wait * _byteTime : 10 * _rotation;
        }

        _next += wait;

        if (_drive.IsDiskLoaded && _next > _maxAddressMarkWaitTime)
        {
            _next = _maxAddressMarkWaitTime;
            _sectorFound = null;
        }

        _state = State.Wait;
        _nextState = State.FOUND_NEXT_ID;
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
        //
    }

    private bool IsBusy => (_controllerStatus & ControllerStatus.Busy) != 0;
    private bool IsNotReady => (_controllerStatus & ControllerStatus.NotReady) != 0;
    private bool IsHeadLoaded => (_control & ControlHeadEnable) != 0;
    private bool IsWithinIndexHole(long now) => (now + _shift) % _rotation < 4 * _millisecond;

    private void SetFlag(ControllerStatus status) => _controllerStatus |= status;
    private void ResetFlag(ControllerStatus status) => _controllerStatus &= ~status;
}