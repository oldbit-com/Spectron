using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal partial class DiskController
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

    private void ProcessDelayBeforeCommand()
    {
        if (Command.ShouldDelay(_command))
        {
            _next += 15 * _millisecond;
        }

        _controllerStatus |= ControllerStatus.Busy;
        _controllerStatus &= ~(ControllerStatus.DataRequest |
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
            _controllerStatus |= ControllerStatus.WriteProtect;
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
            _controllerStatus |= ControllerStatus.DataRequest;

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

        if (_next >= _maxAddressMarkWaitTime || _currentSector == null)
        {
            _controllerStatus |= ControllerStatus.NotFound;
            _state = State.Idle;

            return;
        }

        _controllerStatus &= ~ControllerStatus.CrcError;
        _drive.Seek();

        if (_commandType == CommandType.Type1)
        {
            if (_currentSector.CylinderNo != TrackNo)
            {
                FindMarker();
            }
            else if (!_currentSector.VerifyIdCrc())
            {
                _controllerStatus |= ControllerStatus.CrcError;
                FindMarker();
            }
            else
            {
                _state = State.Idle;
            }

            return;
        }

        if (Command.IsReadAddress(_command))
        {
            _readWritePosition = _currentSector.IdPosition;
            _readWriteLength = 6;
            ReadFirstByte();

            return;
        }

        if (_currentSector.CylinderNo != TrackNo || _currentSector.SectorNo != SectorNo ||
            (Command.IsSideCompareFlagSet(_command) && (Command.GetSideSelectFlag(_command) ^ _currentSector.SideNo) != 0))
        {
            FindMarker();

            return;
        }

        if (!_currentSector.VerifyIdCrc())
        {
            _controllerStatus |= ControllerStatus.CrcError;
            FindMarker();

            return;
        }

        // if(cmd & 0x20) //write sector(s)
        // {
        // 	rqs = R_DRQ;
        // 	status |= ST_DRQ;
        // 	next += fdd->TSByte() * 9;
        // 	state = S_WAIT;
        // 	state_next = S_WRSEC;
        // 	break;
        // }
    }
}