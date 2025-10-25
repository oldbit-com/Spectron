using OldBit.Spectron.Emulation.Devices.Beta128.Controller.Commands;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal partial class DiskController
{
    private void ProcessReadWriteCommands()
    {
        if (IsNotReady)
        {
            _state = ControllerState.Idle;
            _requestStatus = RequestStatus.InterruptRequest;

            return;
        }

        if (_drive.IsSpinning)
        {
            _drive.Spin(_next + 2 * _clockHz);
        }

        _state = ControllerState.DelayBeforeCommand;;
    }

    private void ProcessForceInterruptCommand(long now, Command command)
    {
        _state = ControllerState.Idle;
        _requestStatus = RequestStatus.InterruptRequest;
        _controllerStatus &= ~ControllerStatus.Busy;

        _command = command;
        _next = now;
    }

    private void ExecuteCommandType1()
    {
        _controllerStatus = (_controllerStatus | ControllerStatus.Busy)
                            & ~(ControllerStatus.DataRequest | ControllerStatus.CrcError |
                                ControllerStatus.SeekError | ControllerStatus.WriteProtect);

        if (_drive.IsWriteProtected)
        {
            _controllerStatus |= ControllerStatus.WriteProtect;
        }

        _requestStatus = RequestStatus.None;

        _drive.Spin(_next + 2 * _clockHz);
        _nextState = ControllerState.SeekStart;

        if (_command.IsStep || _command.IsStepIn || _command.IsStepOut)
        {
            _stepIncrement = _command.IsStepOut ? -1 : 1;
            _nextState = ControllerState.Step;
        }

        _next += 32;
        _state = ControllerState.Wait;
    }

    private void ExecuteReadWriteCommand()
    {
        if ((_command.IsWriteSector || _command.IsWriteTrack) && _drive.IsWriteProtected)
        {
            _controllerStatus |= ControllerStatus.WriteProtect;
            _state = ControllerState.Idle;

            return;
        }

        if (_command.IsReadSector || _command.IsWriteSector || _command.IsReadAddress)
        {
            _maxAddressMarkWaitTime = _next + 5 * _rotationTime;
            FindMarker();

            return;
        }

        if (_command.IsWriteTrack)
        {
            _requestStatus = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;

            _next += 3 * _byteTime;
            _state = ControllerState.Write;
            _nextState = ControllerState.WriteTrack;

            return;
        }

        if (_command.IsReadTrack)
        {
            Load();
            FindTrackIndex();

            _nextState = ControllerState.Read;

            return;
        }

        _state = ControllerState.Idle;
    }

    private void DelayBeforeCommand()
    {
        if (_command.ShouldDelay)
        {
            _next += 15 * _millisecond;
        }

        _controllerStatus = (_controllerStatus | ControllerStatus.Busy) &
                            ~(ControllerStatus.DataRequest |
                              ControllerStatus.Lost |
                              ControllerStatus.NotFound |
                              ControllerStatus.RecordType |
                              ControllerStatus.WriteProtect);

        _state = ControllerState.Wait;
        _nextState = ControllerState.ReadWriteCommand;
    }
}