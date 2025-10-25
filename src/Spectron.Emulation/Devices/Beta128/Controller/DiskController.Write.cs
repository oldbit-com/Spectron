using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal partial class DiskController
{
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

    private void WriteSector()
    {
        Load();

        if (_currentSector == null || (_requestStatus & RequestStatus.DataRequest) != 0)
        {
            _controllerStatus |= ControllerStatus.Lost;
            _state = ControllerState.Idle;

            return;
        }

        _currentSector.DataAddressMark = _command.HasDeletedAddressMarkFlagSet ? AddressMark.Deleted : AddressMark.Normal;

        _crc = Crc.Calculate(_currentSector.DataAddressMark);

        _readWritePosition = _currentSector.DataPosition;
        _readWriteLength = _currentSector.Length;

        _state = ControllerState.Write;
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
}
