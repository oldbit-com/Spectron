namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal partial class DiskController
{
    private void Read()
    {
        if (_currentSector == null)
        {
            _state = ControllerState.Idle;
            return;
        }

        SelectTrack();

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

    private void ReadFirstByte()
    {
        _crc = Crc.Calculate(_drive.Track!.Data[_readWritePosition - 1]); // Address/Data Mark
        _dataRegister = _drive.Track.Data[_readWritePosition];
        _crc = Crc.Calculate(_dataRegister, _crc);

        _readWritePosition += 1;
        _readWriteLength -= 1;

        _requestStatus = RequestStatus.DataRequest;
        _controllerStatus |= ControllerStatus.DataRequest;

        _next += _byteTime;
        _state = ControllerState.Wait;
        _nextState = ControllerState.Read;
    }
}