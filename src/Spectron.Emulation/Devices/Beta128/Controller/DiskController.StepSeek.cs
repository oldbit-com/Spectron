namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal partial class DiskController
{
    private void Step()
    {
        if (_command.IsRestore && _drive.CylinderNo == 0)
        {
            TrackRegister = 0;
            _state = ControllerState.Verify;

            return;
        }

        if (_command.IsSeek || _command.HasTrackUpdateFlagSet)
        {
            int trackRegister = TrackRegister;
            trackRegister += _stepIncrement;
            TrackRegister = (byte)trackRegister;
        }

        _drive.Step(_stepIncrement);

        var delay = _command.SteppingRate switch
        {
            0 => 6,
            1 => 12,
            2 => 20,
            3 => 30,
            _ => 0,
        };

        _next += delay * _millisecond;

        _state = ControllerState.Wait;
        _nextState = _command.IsSeek ? ControllerState.Seek : ControllerState.Verify;
    }

    private void SeekStart()
    {
        if (_command.IsRestore)
        {
            TrackRegister = 0xFF;
            _dataRegister = 0;
        }

        Seek();
    }

    private void Seek()
    {
        if (_dataRegister == TrackRegister)
        {
            _state = ControllerState.Verify;
        }
        else
        {
            _stepIncrement = _dataRegister < TrackRegister ? -1 : 1;
            _state = ControllerState.Step;
        }
    }
}