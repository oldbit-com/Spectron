namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

partial class DiskController
{
    private void ProcessIdle()
    {
        _controllerStatus &= ~ControllerStatus.Busy;
        _requestStatus = RequestStatus.InterruptRequest;
    }

    private void ProcessWait(long now)
    {
        if (_next > now)
        {
            return;
        }

        _state = _nextState;
    }
}