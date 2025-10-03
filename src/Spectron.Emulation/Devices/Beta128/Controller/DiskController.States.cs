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
        if (_command.ShouldDelay)
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
        if ((_command.IsWriteSector || _command.IsWriteTrack) && _drive.IsWriteProtected)
        {
            _controllerStatus |= ControllerStatus.WriteProtect;
            _state = State.Idle;

            return;
        }

        if (_command.IsReadSector || _command.IsWriteSector || _command.IsReadAddress)
        {
            _maxAddressMarkWaitTime = _next + 5 * _rotation;
            FindMarker();

            return;
        }

        if (_command.IsWriteTrack)
        {
            _requestStatus = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;

            _next += 3 * _byteTime;
            _state = State.Write;
            _nextState = State.WriteTrack;

            return;
        }

        if (_command.IsReadTrack)
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

        if (_command.Type == CommandType.Type1)
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

        if (_command.IsReadAddress)
        {
            _readWritePosition = _currentSector.IdPosition;
            _readWriteLength = 6;

            ReadFirstByte();
            return;
        }

        if (_currentSector.CylinderNo != TrackNo || _currentSector.SectorNo != SectorNo ||
            (_command.IsSideCompareFlagSet && (_command.GetSideSelectFlag ^ _currentSector.SideNo) != 0))
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

        if (_command.IsWriteSector)
        {
            _requestStatus = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;
            _next += _byteTime * 9;

            _state = State.Wait;
            _nextState =  State.WriteSector;
            return;
        }

        if (_command.IsReadSector)
        {
            // if(!found_sec->data)
            // {
            // 	FindMarker();
            // 	break;
            // }

            _next += _byteTime*(_currentSector.DataPosition - _currentSector.IdPosition);

            _state = State.Wait;
            _nextState =  State.ReadSector;
        }
    }

    private void ProcessReadSector()
    {
        if (_currentSector == null)
        {
            return;
        }

        _controllerStatus &= ~ControllerStatus.RecordType;

        if (_currentSector.DataAddressMark == DataAddressMark.Deleted)
        {
            _controllerStatus |= ControllerStatus.RecordType;
        }

        _readWritePosition = _currentSector.DataPosition;
        _readWriteLength = _currentSector.Length;

        ReadFirstByte();
    }

    private void ProcessRead()
    {
        if (_currentSector == null)
        {
            _state = State.Idle;
            return;
        }

        _drive.Seek();

        if (_readWriteLength > 0)
        {
            if ((_requestStatus & RequestStatus.DataRequest) != 0)
            {
                _controllerStatus |= ControllerStatus.Lost;
            }

            _data = _drive.Track!.Data[_readWritePosition];
            _crc = Crc.Calculate(_data, _crc);

            _readWritePosition += 1;
            _readWriteLength -= 1;

            _requestStatus = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;

            _next += _byteTime;
            _state = State.Wait;
            _nextState  = State.Read;
        }
        else
        {
            if (_command.IsReadSector)
            {
                if (_crc != _currentSector.DataCrc)
                {
                    _controllerStatus |= ControllerStatus.CrcError;
                }

                if (_command.IsMultiple)
                {
                    SectorNo += 1;
                    _state = State.CommandReadWrite;

                    return;
                }
            }

            if (_command.IsReadAddress)
            {
                if (!_currentSector.VerifyIdCrc())
                {
                    _controllerStatus |= ControllerStatus.CrcError;
                }
            }

            _state = State.Idle;
        }
    }

    private void ProcessCommandType1()
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
        _nextState = State.SeekStart;

        if (_command.IsStep || _command.IsStepIn || _command.IsStepOut)
        {
            _stepIncrement = _command.IsStepOut ? -1 : 1;
            _nextState = State.Step;
        }

        //if(!wd93_nodelay)
        // {
        // 	next += 32;
        // }
        // state = S_WAIT;

        _state = State.Wait;
    }
}