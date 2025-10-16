using OldBit.Spectron.Emulation.Devices.Beta128.Controller.Commands;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal partial class DiskController
{
    private void ProcessIdle()
    {
        _controllerStatus &= ~ControllerStatus.Busy;
        Request = RequestStatus.InterruptRequest;
    }

    private bool ProcessWait(long now)
    {
        if (_next > now)
        {
            return false;
        }

        _controllerState = _nextControllerState;

        return true;
    }

    private void ProcessDelayBeforeCommand()
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

        _controllerState = ControllerState.Wait;
        _nextControllerState = ControllerState.CommandReadWrite;
    }

    private void ProcessCommandReadWrite()
    {
        if ((_command.IsWriteSector || _command.IsWriteTrack) && _drive.IsWriteProtected)
        {
            _controllerStatus |= ControllerStatus.WriteProtect;
            _controllerState = ControllerState.Idle;

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
            Request = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;

            _next += 3 * _byteTime;
            _controllerState = ControllerState.Write;
            _nextControllerState = ControllerState.WriteTrack;

            return;
        }

        if (_command.IsReadTrack)
        {
            Seek();
            FindIndex();
            _nextControllerState = ControllerState.Read;

            return;
        }

        _controllerState = ControllerState.Idle;
    }

    private void ProcessFoundNextId()
    {
        if (!_drive.IsDiskInserted)
        {
            _maxAddressMarkWaitTime = _next + 5 * _rotationTime;

            FindMarker();
            return;
        }

        if (_next >= _maxAddressMarkWaitTime || _currentSector == null)
        {
            _controllerStatus |= ControllerStatus.NotFound;
            _controllerState = ControllerState.Idle;
            return;
        }

        _controllerStatus &= ~ControllerStatus.CrcError;
        Seek();

        if (_command.Type == CommandType.Type1)
        {
            if (_currentSector.CylinderNo != TrackRegister)
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
                _controllerState = ControllerState.Idle;
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

        if (_currentSector.CylinderNo != TrackRegister || _currentSector.SectorNo != SectorRegister ||
            (_command.HasSideCompareFlagSet && (_command.SideSelectFlagSet ^ _currentSector.SideNo) != 0))
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
            Request = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;
            _next += _byteTime * 9;

            _controllerState = ControllerState.Wait;
            _nextControllerState = ControllerState.WriteSector;
            return;
        }

        if (_command.IsReadSector)
        {
            if (_currentSector == null)
            {
                FindMarker();
                return;
            }
            // if(!found_sec->data)
            // {
            // 	FindMarker();
            // 	break;
            // }

            _next += _byteTime * (_currentSector.DataPosition - _currentSector.IdPosition);

            _controllerState = ControllerState.Wait;
            _nextControllerState = ControllerState.ReadSector;
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
            _controllerState = ControllerState.Idle;
            return;
        }

        Seek();

        if (_readWriteLength > 0)
        {
            if ((Request & RequestStatus.DataRequest) != 0)
            {
                _controllerStatus |= ControllerStatus.Lost;
            }

            _dataRegister = _drive.Track!.Data[_readWritePosition];
            _crc = Crc.Calculate(_dataRegister, _crc);

            _readWritePosition += 1;
            _readWriteLength -= 1;

            Request = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;

            _next += _byteTime;
            _controllerState = ControllerState.Wait;
            _nextControllerState = ControllerState.Read;
        }
        else
        {
            if (_command.IsReadSector)
            {
                if (_crc != _currentSector.DataCrc)
                {
                    _controllerStatus |= ControllerStatus.CrcError;
                }

                if (_command.HasMultipleFlagSet)
                {
                    SectorRegister += 1;
                    _controllerState = ControllerState.CommandReadWrite;

                    return;
                }
            }

            if (_command.IsReadAddress && !_currentSector.VerifyIdCrc())
            {
                _controllerStatus |= ControllerStatus.CrcError;
            }

            _controllerState = ControllerState.Idle;
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

        Request = RequestStatus.None;

        _drive.Spin(_next + 2 * _clockHz);
        _nextControllerState = ControllerState.SeekStart;

        if (_command.IsStep || _command.IsStepIn || _command.IsStepOut)
        {
            _stepIncrement = _command.IsStepOut ? -1 : 1;
            _nextControllerState = ControllerState.Step;
        }

        _next += 32;
        _controllerState = ControllerState.Wait;
    }

    private void ProcessStep()
    {
        if (_command.IsRestore && _drive.CylinderNo == 0)
        {
            TrackRegister = 0;
            _controllerState = ControllerState.Verify;

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

        _controllerState = ControllerState.Wait;
        _nextControllerState = _command.IsSeek ? ControllerState.Seek : ControllerState.Verify;
    }

    private void ProcessSeekStart()
    {
        if (_command.IsRestore)
        {
            TrackRegister = 0xFF;
            _dataRegister = 0;
        }

        ProcessSeek();
    }

    private void ProcessSeek()
    {
        if (_dataRegister == TrackRegister)
        {
            _controllerState = ControllerState.Verify;
        }
        else
        {
            _stepIncrement = _dataRegister < TrackRegister ? -1 : 1;
            _controllerState = ControllerState.Step;
        }
    }

    private void ProcessVerify()
    {
        if (!_command.HasVerifyFlagSet)
        {
            _controllerStatus |= ControllerStatus.Busy;
            _controllerState = ControllerState.Wait;
            _nextControllerState = ControllerState.Idle;
            _next += 128;

            return;
        }

        _maxAddressMarkWaitTime = _next + 6 * _rotationTime;

        Seek();
        FindMarker();
    }

    private void ProcessReset()
    {
        if (_drive.IsTrackZero)
        {
            _controllerState = ControllerState.Idle;
        }
        else
        {
            _drive.Step(-1);
        }

        _next += 6 * _millisecond;
    }
}