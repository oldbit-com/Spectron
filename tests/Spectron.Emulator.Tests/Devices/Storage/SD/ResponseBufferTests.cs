using OldBit.Spectron.Emulation.Devices.Storage.SD;

namespace OldBit.Spectron.Emulator.Tests.Devices.Storage.SD;

public class ResponseBufferTests
{
    private readonly ResponseBuffer _responseBuffer = new();

    [Fact]
    public void PutStatus_ShouldSetBuffer()
    {
        _responseBuffer.Put(Status.IllegalCommand);

        var result = _responseBuffer.Read();

        result.ShouldBe((byte)Status.IllegalCommand);
        ShouldNotHaveMoreData();
    }

    [Fact]
    public void PutStatus_AndByteToken_ShouldSetBuffer()
    {
        _responseBuffer.Put(Status.Idle, 0xAA);

        var result = _responseBuffer.Read();
        result.ShouldBe((byte)Status.Idle);

        result = _responseBuffer.Read();
        result.ShouldBe(0xAA);

        ShouldNotHaveMoreData();
    }

    [Fact]
    public void PutStatus_AndUnsignedInt_ShouldSetBuffer()
    {
        _responseBuffer.Put(Status.Idle, 0x11223344);

        var result = _responseBuffer.Read();
        result.ShouldBe((byte)Status.Idle);

        result = _responseBuffer.Read();
        result.ShouldBe(0x11);

        result = _responseBuffer.Read();
        result.ShouldBe(0x22);

        result = _responseBuffer.Read();
        result.ShouldBe(0x33);

        result = _responseBuffer.Read();
        result.ShouldBe(0x44);

        ShouldNotHaveMoreData();
    }

    [Fact]
    public void PutStatus_AndVoltage_ShouldSetBuffer()
    {
        _responseBuffer.Put(Status.Idle, 0x45, 0xAA);

        var result = _responseBuffer.Read();
        result.ShouldBe((byte)Status.Idle);

        result = _responseBuffer.Read();
        result.ShouldBe(0);

        result = _responseBuffer.Read();
        result.ShouldBe(0);

        result = _responseBuffer.Read();
        result.ShouldBe(0x05);

        result = _responseBuffer.Read();
        result.ShouldBe(0xAA);

        ShouldNotHaveMoreData();
    }

    [Fact]
    public void PutStatus_AndDataBuffer_ShouldSetBuffer()
    {
        byte[] buffer = [0x01, 0x02, 0x03, 0x04, 0x05];
        _responseBuffer.Put(Status.Idle, 0xCC, buffer, 0xAA, 0xBB);

        var result = _responseBuffer.Read();
        result.ShouldBe((byte)Status.Idle);

        result = _responseBuffer.Read();
        result.ShouldBe(0xCC);

        result = _responseBuffer.Read();
        result.ShouldBe(0x01);

        result = _responseBuffer.Read();
        result.ShouldBe(0x02);

        result = _responseBuffer.Read();
        result.ShouldBe(0x03);

        result = _responseBuffer.Read();
        result.ShouldBe(0x04);

        result = _responseBuffer.Read();
        result.ShouldBe(0x05);

        result = _responseBuffer.Read();
        result.ShouldBe(0xAA);

        result = _responseBuffer.Read();
        result.ShouldBe(0xBB);

        ShouldNotHaveMoreData();
    }

    private void ShouldNotHaveMoreData()
    {
        var result = _responseBuffer.Read();

        result.ShouldBe(0xFF);
    }
}