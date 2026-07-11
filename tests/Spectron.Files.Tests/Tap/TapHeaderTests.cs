using OldBit.Spectron.Files.Tap;

namespace OldBit.Spectron.Files.Tests.Tap;

public class TapHeaderTests
{
    [Fact]
    public void TryParse_WhenDataIsValid_ReturnsTrueAndValidHeaderData()
    {
        byte[] validData =
        [
            0x03, 0x74, 0x65, 0x73, 0x74, 0x66, 0x69, 0x6C, 0x65,
            0x20, 0x20, 0x68, 0x02, 0x00, 0x00, 0xAC, 0x80
        ];

        var result = TapHeader.TryParse(validData, out var tapeHeader);

        result.ShouldBeTrue();
        tapeHeader.ShouldNotBeNull();
        tapeHeader!.DataType.ShouldBe(HeaderDataType.Code);
        tapeHeader!.FileName.ShouldBe("testfile  ");
        tapeHeader!.DataLength.ShouldBe(616);
        tapeHeader!.Parameter1.ShouldBe(0);
        tapeHeader!.Parameter2.ShouldBe(32940);
    }

    [Fact]
    public void TryParse_WhenDataIsInvalid_ReturnsFalseAndNullHeaderData()
    {
        byte[] invalidData = [0x03, 0x74, 0x65];

        var result = TapHeader.TryParse(invalidData, out var tapeHeader);

        result.ShouldBeFalse();
        tapeHeader.ShouldBeNull();
    }
}