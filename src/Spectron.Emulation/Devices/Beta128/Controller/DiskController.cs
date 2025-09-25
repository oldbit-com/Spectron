using OldBit.Spectron.Emulation.Devices.Beta128.Disks;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal sealed partial class DiskController
{
    private const byte ControlDriveSelect = 0b000_0011;
    private const byte ControlDriveSide = 0b0001_0000;
    private const byte ControlResetPulse = 0b0000_0100;
    private const byte ControlHeadEnable = 0b0000_1000;

    private const byte RotationSpeed = 5;

    private readonly int _millisecond;      // number of T states in 1 ms
    private readonly int _rotation;         // number of T states in 1 rotation

    private readonly int _clockHz;
    private readonly DiskDrive[] _drives = new DiskDrive[4];
    private DiskDrive _drive;

    private byte _data;
    private byte _control;

    private long _next;
    private byte _command;
    private int _shift;
    private CommandType _commandType;
    private State _state = State.Idle;
    private State _nextState = State.Idle;
    private ControllerStatus _controllerStatus = 0;
    private RequestStatus _requestStatus = 0;

    internal byte Track { get; set; }
    internal byte Sector { get; set; }

    internal RequestStatus Request => _requestStatus;

    public DiskController(float clockMhz)
    {
        _clockHz = (int)(clockMhz * 1_000_000);
        _millisecond = _clockHz / 1000;
        _rotation = _clockHz / DiskDrive.Rps;

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
            _drive.Side = (byte)((_control & ControlDriveSide) != 0 ? 0 : 1);

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

    private bool IsBusy => (_controllerStatus & ControllerStatus.Busy) != 0;
    private bool IsNotReady => (_controllerStatus & ControllerStatus.NotReady) != 0;
    private bool IsHeadLoaded => (_control & ControlHeadEnable) != 0;
    private bool IsWithinIndexHole(long now) => (now + _shift) % _rotation < 4 * _millisecond;

    private void SetFlag(ControllerStatus status) => _controllerStatus |= status;
    private void ResetFlag(ControllerStatus status) => _controllerStatus &= ~status;

    private void SetResetFlags(bool setCondition, ControllerStatus status)
    {
        if (setCondition)
        {
            SetFlag(status);
        }
        else
        {
            ResetFlag(status);
        }
    }
}