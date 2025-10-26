using OldBit.Spectron.Emulation.Devices.Beta128.Controller.Commands;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

/// <summary>
/// Based on UnrealSpeccyP implementation: https://github.com/djdron/UnrealSpeccyP/blob/master/devices/fdd/wd1793.cpp
/// </summary>
internal sealed partial class DiskController
{
    private readonly IDiskDriveProvider _diskDriveProvider;

    private const byte ControlDriveSelect = 0b000_0011;
    private const byte ControlDriveSide = 0b0001_0000;
    private const byte ControlResetPulse = 0b0000_0100;
    private const byte ControlHeadEnable = 0b0000_1000;

    private readonly int _millisecond;      // number of T states in 1 ms
    private readonly int _rotationTime;     // number of T states in 1 disk rotation
    private readonly int _byteTime;         // number of T states per 1 byte
    private readonly int _trackTime;        // number of T states per 1 track

    private DiskDrive _drive;

    private byte _sideNo;
    private byte _dataRegister;
    private byte _controlRegister;

    private long _next;
    private long _maxAddressMarkWaitTime;
    private Sector? _currentSector;
    private int _stepIncrement = 1;

    private Command _command;
    private Word _crc;

    private int _readWritePosition;
    private int _readWriteLength;

    private ControllerState _state = ControllerState.Idle;
    private ControllerState _nextState = ControllerState.Idle;
    private ControllerStatus _controllerStatus = ControllerStatus.None;
    private RequestStatus _requestStatus = RequestStatus.None;

    internal byte TrackRegister { get; set; }
    internal byte SectorRegister { get; set; }

    internal byte ControlRegister
    {
        get => GetControlRegister();
        set => SetControlRegister(value);
    }

    internal byte DataRegister
    {
        get => GetDataRegister();
        set => SetDataRegister(value);
    }

    internal byte CommandRegister
    {
        get => _command.CommandRegister;
        set => _command = new Command(value);
    }

    internal byte StatusRegister => GetStatusRegister();
    internal bool IsIdle => _state is ControllerState.Idle;

    public DiskController(float clockMhz, IDiskDriveProvider diskDriveProvider)
    {
        _diskDriveProvider = diskDriveProvider;

        var clockHz = (int)(clockMhz * 1_000_000);

        _millisecond = clockHz / 1000;
        _rotationTime = clockHz / DiskDrive.Rps;
        _byteTime = clockHz / (Track.MaxLength * DiskDrive.Rps);
        _trackTime = Track.MaxLength * _byteTime;

        _drive = _diskDriveProvider.Drives[DriveId.DriveA];
    }

    private byte GetDataRegister()
    {
        ResetDataRequest();

        return _dataRegister;
    }

    private void SetDataRegister(byte value)
    {
        _dataRegister = value;

        ResetDataRequest();
    }

    private byte GetControlRegister()
    {
        var result = _requestStatus;
        _requestStatus &= ~(RequestStatus.InterruptRequest);

        return (byte)(result | ~(RequestStatus.InterruptRequest | RequestStatus.DataRequest));
    }

    private void SetControlRegister(byte value)
    {
        _controlRegister = value;

        var driveId = (DriveId)((_controlRegister & ControlDriveSelect) + 1);

        _drive = _diskDriveProvider.Drives[driveId];
        _sideNo = (byte)((_controlRegister & ControlDriveSide) != 0 ? 0 : 1);

        if ((_controlRegister & ControlResetPulse) != 0)
        {
            return;
        }

        _controllerStatus = ControllerStatus.NotReady;
        _requestStatus = RequestStatus.InterruptRequest;
        _state = ControllerState.Idle;

        _drive.MotorOff();
    }

    private byte GetStatusRegister()
    {
        _requestStatus &= ~RequestStatus.InterruptRequest;

        return (byte)_controllerStatus;
    }

    internal void ProcessState(long now)
    {
        if (now > _drive.SpinTime && IsHeadLoaded && _drive.IsMotorOn)
        {
            _drive.MotorOff();
        }

        _controllerStatus &= ~ControllerStatus.NotReady;

        if (!_drive.IsDiskInserted)
        {
            _controllerStatus |= ControllerStatus.NotReady;
        }

        if (_command.Type is CommandType.Type1 or CommandType.Type4)
        {
            _controllerStatus &= ~(ControllerStatus.TrackZero | ControllerStatus.Index);

            if (_drive.IsMotorOn && IsHeadLoaded)
            {
                _controllerStatus |= ControllerStatus.HeadLoaded;
            }

            if (_drive.IsTrackZero)
            {
                _controllerStatus |= ControllerStatus.TrackZero;
            }

            if (_drive is { IsDiskInserted: true, IsMotorOn: true } && IsWithinIndexHole(now))
            {
                _controllerStatus |= ControllerStatus.Index;
            }
        }

        while (true)
        {
            switch (_state)
            {
                case ControllerState.Idle:
                    Idle();
                    return;

                case ControllerState.Wait:
                    if (!ShouldWait(now))
                    {
                        return;
                    }
                    break;

                case ControllerState.DelayBeforeCommand:
                    DelayBeforeCommand();
                    break;

                case ControllerState.Type1Command:
                    ExecuteCommandType1();
                    break;

                case ControllerState.ReadWriteCommand:
                    ExecuteReadWriteCommand();
                    break;

                case ControllerState.ReadSector:
                    ReadSector();
                    break;

                case ControllerState.Read:
                    Read();
                    break;

                case ControllerState.Write:
                    Write();
                    break;

                case ControllerState.WriteSector:
                    WriteSector();
                    break;

                case ControllerState.WriteTrack:
                    WriteTrack();
                    break;

                case ControllerState.WriteTrackData:
                    WriteTrackData();
                    break;

                case ControllerState.Step:
                    Step();
                    break;

                case ControllerState.SeekStart:
                    SeekStart();
                    break;

                case ControllerState.Seek:
                    Seek();
                    break;

                case ControllerState.Verify:
                    Verify();
                    break;

                case ControllerState.Reset:
                    Reset();
                    break;

                case ControllerState.FoundNextId:
                    FoundNextId();
                    break;
            }
        }
    }

    internal void ProcessCommand(long now, byte commandRegister)
    {
        var command = new Command(commandRegister);

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

        _state = ControllerState.Type1Command;
    }

    internal void ResetController()
    {
        _state = ControllerState.Idle;
        _nextState = ControllerState.Idle;
        _controllerStatus = ControllerStatus.None;
        _requestStatus = RequestStatus.None;

        _next = 0;
        _maxAddressMarkWaitTime = 0;
        _currentSector = null;
        _stepIncrement = 1;

        _sideNo = 0;
        _dataRegister = 0;
        _controlRegister = 0;

        TrackRegister = 0;
        SectorRegister = 0;

        _drive.MotorOff();
    }

    private void ResetDataRequest()
    {
        _controllerStatus &= ~ControllerStatus.DataRequest;
        _requestStatus &= ~RequestStatus.DataRequest;
    }

    private void SelectTrack() => _drive.SelectTrack(_sideNo);

    private bool IsBusy => (_controllerStatus & ControllerStatus.Busy) != 0;
    private bool IsNotReady => (_controllerStatus & ControllerStatus.NotReady) != 0;
    private bool IsHeadLoaded => (_controlRegister & ControlHeadEnable) != 0;
    private bool IsWithinIndexHole(long now) => now % _rotationTime < 4 * _millisecond;
}