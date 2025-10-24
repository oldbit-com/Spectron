using OldBit.Spectron.Emulation.Devices.Beta128.Controller.Commands;
using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal partial class DiskController
{
    private void Idle()
    {
        _controllerStatus &= ~ControllerStatus.Busy;
        _requestStatus = RequestStatus.InterruptRequest;
    }

    private bool ShouldWait(long now)
    {
        if (_next > now)
        {
            return false;
        }

        _state = _nextState;

        return true;
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
            FindIndex();

            _nextState = ControllerState.Read;

            return;
        }

        _state = ControllerState.Idle;
    }

    private void FoundNextId()
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
            _state = ControllerState.Idle;
            return;
        }

        _controllerStatus &= ~ControllerStatus.CrcError;
        Load();

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
                _state = ControllerState.Idle;
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
            (_command.HasSideCompareFlagSet && (_command.SideSelectFlag ^ _currentSector.SideNo) != 0))
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

            _state = ControllerState.Wait;
            _nextState = ControllerState.WriteSector;
        }
        else if (_command.IsReadSector)
        {
            if (_currentSector == null)
            {
                FindMarker();
                return;
            }

            _next += _byteTime * (_currentSector.DataPosition - _currentSector.IdPosition);

            _state = ControllerState.Wait;
            _nextState = ControllerState.ReadSector;
        }
    }

    private void ReadSector()
    {
        if (_currentSector == null)
        {
            return;
        }

        _controllerStatus &= ~ControllerStatus.RecordType;

        if (_currentSector.DataAddressMark == AddressMark.Deleted)
        {
            _controllerStatus |= ControllerStatus.RecordType;
        }

        _readWritePosition = _currentSector.DataPosition;
        _readWriteLength = _currentSector.Length;

        ReadFirstByte();
    }

    private void Read()
    {
        if (_currentSector == null)
        {
            _state = ControllerState.Idle;
            return;
        }

        Load();

        if (_readWriteLength > 0)
        {
            if ((_requestStatus & RequestStatus.DataRequest) != 0)
            {
                _controllerStatus |= ControllerStatus.Lost;
            }

            _dataRegister = _drive.Track!.Data[_readWritePosition];
            _crc = Crc.Calculate(_dataRegister, _crc);

            _readWritePosition += 1;
            _readWriteLength -= 1;

            _requestStatus = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;

            _next += _byteTime;
            _state = ControllerState.Wait;
            _nextState = ControllerState.Read;
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
                    _state = ControllerState.ReadWriteCommand;

                    return;
                }
            }

            if (_command.IsReadAddress && !_currentSector.VerifyIdCrc())
            {
                _controllerStatus |= ControllerStatus.CrcError;
            }

            _state = ControllerState.Idle;
        }
    }

    private void WriteSector()
    {
        Load();

        if (_currentSector == null || (_requestStatus & RequestStatus.DataRequest) != 0)
        {
            _controllerStatus |= ControllerStatus.Lost;
            _state = ControllerState.Idle;

            return;
        }

        _currentSector.DataAddressMark = _command.HasDeletedAddressMarkFlagSet ?
            AddressMark.Deleted :
            AddressMark.Normal;

        _crc = Crc.Calculate(_currentSector.DataAddressMark);

        _readWritePosition = _currentSector.DataPosition;
        _readWriteLength = _currentSector.Length;

        _state = ControllerState.Write;
    }

    private void Write()
    {
        if (_drive.Track == null)
        {
            _controllerStatus |= ControllerStatus.Lost;
            return;
        }

        if ((_requestStatus & RequestStatus.DataRequest) != 0)
        {
            _controllerStatus |= ControllerStatus.Lost;
            _dataRegister = 0;
        }

        _drive.Track.Write(_readWritePosition, _dataRegister);
        _crc = Crc.Calculate(_dataRegister, _crc);

        _readWritePosition += 1;
        _readWriteLength -= 1;

        if (_readWritePosition == _drive.Track.Length)
        {
            _readWritePosition = 0;
        }

        if (_readWriteLength > 0)
        {
            _next += _byteTime;

            _requestStatus = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;

            _state = ControllerState.Wait;
            _nextState = ControllerState.Write;
        }
        else
        {
            _drive.Track.Write(_readWritePosition++, (byte)(_crc >> 8));
            _drive.Track.Write(_readWritePosition++, (byte)_crc);

            if (_command.HasMultipleFlagSet)
            {
                SectorRegister += 1;
                _state = ControllerState.ReadWriteCommand;

                return;
            }

            _state = ControllerState.Idle;
        }
    }

    private void WriteTrack()
    {
        if (_currentSector == null || (_requestStatus & RequestStatus.DataRequest) != 0)
        {
            _controllerStatus |= ControllerStatus.Lost;
            _state = ControllerState.Idle;

            return;
        }

        FindIndex();

        _maxAddressMarkWaitTime = _next + 5 * _rotationTime;
        // start_crc = -1;
        // GetIndex();
        // end_waiting_am = next + 5 * Z80FQ/FDD_RPS;
        // break;

        _nextState = ControllerState.WriteTrackData;
    }

    private void WriteTrackData()
    {
        if (_drive.Track == null || _currentSector == null)
        {
            _controllerStatus |= ControllerStatus.Lost;
            return;
        }

        if ((_requestStatus & RequestStatus.DataRequest) != 0)
        {
            _controllerStatus |= ControllerStatus.Lost;
            _dataRegister = 0;
        }

        Load();

        var isMarker = false;
        var data = _dataRegister;

        switch (data)
        {
            case Track.StartIdDataMarker:
                data = 0xA1;
                isMarker = true;
                break;

            case Track.StartIndexMarker:
                data = 0xC2;
                isMarker = true;
                break;

            case Track.WriteCrcMarker:
                data = (byte)_crc;
                _drive.Track.Write(_readWritePosition, (byte)(_crc >> 8));

                _readWritePosition += 1;
                _readWriteLength -= 1;
                break;
        }

        _drive.Track.Write(_readWritePosition, data, isMarker);

        _readWritePosition += 1;
        _readWriteLength -= 1;

        if (_readWriteLength > 0)
        {
            _next += _byteTime;

            _requestStatus = RequestStatus.DataRequest;
            _controllerStatus |= ControllerStatus.DataRequest;

            _state = ControllerState.Wait;
            _nextState = ControllerState.WriteTrackData;

            return;
        }

        //_drive.Track.Update();

        _state = ControllerState.Idle;
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

    private void Verify()
    {
        if (!_command.HasVerifyFlagSet)
        {
            _controllerStatus |= ControllerStatus.Busy;
            _state = ControllerState.Wait;
            _nextState = ControllerState.Idle;
            _next += 128;

            return;
        }

        _maxAddressMarkWaitTime = _next + 6 * _rotationTime;

        Load();
        FindMarker();
    }

    private void Reset()
    {
        if (_drive.IsTrackZero)
        {
            _state = ControllerState.Idle;
        }
        else
        {
            _drive.Step(-1);
        }

        _next += 6 * _millisecond;
    }
}