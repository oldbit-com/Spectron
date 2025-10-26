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

    private void FindTrackIndex()
    {
        var position = _next % _trackTime;

        _next += _trackTime - position;
        _readWritePosition = 0;
        _readWriteLength = Track.MaxLength;
    }

    private void FindMarker()
    {
        SelectTrack();

        var wait = 10 * _rotationTime;
        _currentSector = null;

        if (_drive is { IsMotorOn: true, IsDiskInserted: true, Track: not null })
        {
            var position = (int)(_next % _trackTime / _byteTime);

            wait = long.MaxValue;

            for (var sectorNo = 1; sectorNo <= 16; sectorNo++)
            {
                var sector = _drive.Track[sectorNo];
                var idPosition = sector.IdPosition;

                var distance = idPosition > position ?
                    idPosition - position :
                    Track.MaxLength + idPosition - position;

                if (distance < wait)
                {
                    wait = distance;
                    _currentSector = sector;
                }
            }

            wait = _currentSector != null ? wait * _byteTime : 10 * _rotationTime;
        }

        _next += wait;

        if (_drive.IsDiskInserted && _next > _maxAddressMarkWaitTime)
        {
            _next = _maxAddressMarkWaitTime;
            _currentSector = null;
        }

        _state = ControllerState.Wait;
        _nextState = ControllerState.FoundNextId;
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
        SelectTrack();

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

        SelectTrack();
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