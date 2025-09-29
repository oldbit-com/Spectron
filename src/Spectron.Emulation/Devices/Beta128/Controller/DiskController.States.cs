namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal partial class DiskController
{
    private void ProcessIdle()
    {
        ResetFlag(ControllerStatus.Busy);
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

    private void ProcessDelayBeforeCommand()
    {
        if (Command.ShouldDelay(_command))
        {
            _next += 15 * _millisecond;
        }

        SetFlag(ControllerStatus.Busy);
        ResetFlag(
            ControllerStatus.DataRequest |
            ControllerStatus.Lost |
            ControllerStatus.NotFound |
            ControllerStatus.WriteFault |
            ControllerStatus.WriteProtect);

        _state = State.Wait;
        _nextState = State.CommandReadWrite;
    }

    private void ProcessCommandReadWrite()
    {
        if ((Command.IsWriteSector(_command) || Command.IsWriteTrack(_command)) && _drive.IsWriteProtected)
        {
            SetFlag(ControllerStatus.WriteProtect);
            _state = State.Idle;

            return;
        }

        if (Command.IsReadSector(_command) || Command.IsWriteSector(_command) || Command.IsReadAddress(_command))
        {
            _maxAddressMarkWaitTime = _next + 5 * _rotation;
            FindMarker();

            return;
        }

        if (Command.IsWriteTrack(_command))
        {
            _requestStatus = RequestStatus.DataRequest;
            SetFlag(ControllerStatus.DataRequest);

            _next += 3 * _byteTime;
            _state = State.Write;
            _nextState = State.WriteTrack;

            return;
        }

        if (Command.IsReadTrack(_command))
        {
            _drive.Seek();
            FindIndex();
            _nextState = State.Read;

            return;
        }

        _state = State.Idle;
    }

    private void ProcessFoundNextId()
    {
        if (!_drive.IsDiskLoaded)
        {
            _maxAddressMarkWaitTime = _next + 5 * _rotation;
            FindMarker();

            return;
        }

        if (_next >= _maxAddressMarkWaitTime || _sectorFound == null)
        {
            SetFlag(ControllerStatus.NotFound);
            _state = State.Idle;

            return;
        }

        ResetFlag(ControllerStatus.CrcError);

        if (_commandType == CommandType.Type1)
        {
            if (_sectorFound.CylinderNo != TrackNo)
            {
                FindMarker();
            }
            else if (_sectorFound.CalculateIdCrc() != _sectorFound.IdCrc)
            {
                _controllerStatus |= ControllerStatus.CrcError;
                FindMarker();
            }
            else
            {
                _state = State.Idle;

                return;
            }

            if (Command.IsReadAddress(_command))
            {
                _readWritePosition = _sectorFound.IdPosition;
                _readWriteLength = 6;
                ReadFirstByte();

                return;
            }
        }

        _drive.Seek();
    }
}