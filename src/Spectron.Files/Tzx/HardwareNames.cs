namespace OldBit.Spectron.Files.Tzx;

/// <summary>
/// Provides constants representing various hardware types and utility methods for retrieving hardware names.
/// </summary>
public static class HardwareNames
{
    /// <summary>
    /// Computer.
    /// </summary>
    public const byte Computer = 0x00;

    /// <summary>
    /// External Storage.
    /// </summary>
    public const byte ExternalStorage = 0x01;

    /// <summary>
    /// Memory Add-On.
    /// </summary>
    public const byte MemoryAddOn = 0x02;

    /// <summary>
    /// Sound Device.
    /// </summary>
    public const byte SoundDevice = 0x03;

    /// <summary>
    /// Joystick.
    /// </summary>
    public const byte Joystick = 0x04;

    /// <summary>
    /// Mouse.
    /// </summary>
    public const byte Mouse = 0x05;

    /// <summary>
    /// Other Controller.
    /// </summary>
    public const byte OtherController = 0x06;

    /// <summary>
    /// Serial Port.
    /// </summary>
    public const byte SerialPort = 0x07;

    /// <summary>
    /// Parallel Port.
    /// </summary>
    public const byte ParallelPort = 0x08;

    /// <summary>
    /// Printer.
    /// </summary>
    public const byte Printer = 0x09;

    /// <summary>
    /// Modem.
    /// </summary>
    public const byte Modem = 0x0A;

    /// <summary>
    /// Digitizer.
    /// </summary>
    public const byte Digitizer = 0x0B;

    /// <summary>
    /// Network Adapter.
    /// </summary>
    public const byte NetworkAdapter = 0x0C;

    /// <summary>
    /// Keyboard/Keypad.
    /// </summary>
    public const byte Keyboard = 0x0D;

    /// <summary>
    /// AD/DA Converter.
    /// </summary>
    public const byte AdDaConverter = 0x0E;

    /// <summary>
    /// EPROM Programmer.
    /// </summary>
    public const byte EpromProgrammer = 0x0F;

    /// <summary>
    /// Graphics.
    /// </summary>
    public const byte Graphics = 0x10;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        Computer => "Computer",
        ExternalStorage => "External Storage",
        MemoryAddOn => "ROM/RAM type add-on",
        SoundDevice => "Sound Device",
        Joystick => "Joystick",
        Mouse => "Mouse",
        OtherController => "Other Controller",
        SerialPort => "Serial Port",
        ParallelPort => "Parallel Port",
        Printer => "Printer",
        Modem => "Modem",
        Digitizer => "Digitizer",
        NetworkAdapter => "Network Adapter",
        Keyboard => "Keyboard/Keypad",
        AdDaConverter => "AD/DA Converter",
        EpromProgrammer => "EPROM Programmer",
        Graphics => "Graphics",
        _ => "Unknown Hardware Type"
    };

    /// <summary>
    /// Retrieves the full name of a hardware component, including its type and specific identifier, based on the provided hardware type and hardware ID.
    /// </summary>
    /// <param name="hardwareTypeId">The identifier representing the type of the hardware (e.g., Computer, Joystick).</param>
    /// <param name="hardwareId">The identifier representing the specific hardware within the given type.</param>
    /// <returns>A string containing the hardware type's name and the specific hardware name.</returns>
    public static string GetName(byte hardwareTypeId, byte hardwareId)
    {
        var hardwareType = GetName(hardwareTypeId);
        var hardwareName = hardwareTypeId switch
        {
            Computer => ComputerType.GetName(hardwareId),
            ExternalStorage => ExternalStorageType.GetName(hardwareId),
            MemoryAddOn => MemoryAddOnType.GetName(hardwareId),
            SoundDevice => SoundDeviceType.GetName(hardwareId),
            Joystick => JoystickType.GetName(hardwareId),
            Mouse => MouseType.GetName(hardwareId),
            OtherController => OtherControllerType.GetName(hardwareId),
            SerialPort => SerialPortType.GetName(hardwareId),
            ParallelPort => ParallelPortType.GetName(hardwareId),
            Printer => PrinterType.GetName(hardwareId),
            Modem => ModemType.GetName(hardwareId),
            Digitizer => DigitizerType.GetName(hardwareId),
            NetworkAdapter => NetworkAdapterType.GetName(hardwareId),
            Keyboard => KeyboardKeypadType.GetName(hardwareId),
            AdDaConverter => AdDaConverterType.GetName(hardwareId),
            EpromProgrammer => EpromProgrammerType.GetName(hardwareId),
            Graphics => GraphicsType.GetName(hardwareId),
            _ => "Unknown Hardware Type"
        };

        return $"{hardwareType}: {hardwareName}";
    }
}

/// <summary>
/// Represents the various types of computers.
/// </summary>
public static class ComputerType
{
    /// <summary>
    /// ZX Spectrum 16K.
    /// </summary>
    public const byte ZxSpectrum16 = 0x00;

    /// <summary>
    /// ZX Spectrum 48K.
    /// </summary>
    public const byte ZxSpectrum48 = 0x01;

    /// <summary>
    /// ZX Spectrum 48K Issue 1.
    /// </summary>
    public const byte ZxSpectrum48Issue1 = 0x02;

    /// <summary>
    /// ZX Spectrum 128K.
    /// </summary>
    public const byte ZxSpectrum128 = 0x03;

    /// <summary>
    /// ZX Spectrum 128K +2 (grey case).
    /// </summary>
    public const byte ZxSpectrum128Plus2 = 0x04;

    /// <summary>
    /// ZX Spectrum 128K +2A, +3.
    /// </summary>
    public const byte ZxSpectrum128Plus2APlus3 = 0x05;

    /// <summary>
    /// Timex Sinclair TC-2048.
    /// </summary>
    public const byte Timex2048 = 0x06;

    /// <summary>
    /// Timex Sinclair TS-2068.
    /// </summary>
    public const byte Timex2068 = 0x07;

    /// <summary>
    /// Pentagon 128.
    /// </summary>
    public const byte Pentagon128 = 0x08;

    /// <summary>
    /// Sam Coupe.
    /// </summary>
    public const byte SamCoupe = 0x09;

    /// <summary>
    /// Didaktik M.
    /// </summary>
    public const byte DidaktikM = 0x0A;

    /// <summary>
    /// Didaktik Gama.
    /// </summary>
    public const byte DidaktikGama = 0x0B;

    /// <summary>
    /// ZX-80.
    /// </summary>
    public const byte Zx80 = 0x0C;

    /// <summary>
    /// ZX-81.
    /// </summary>
    public const byte Zx81 = 0x0D;

    /// <summary>
    /// ZX Spectrum 128K Spanish version.
    /// </summary>
    public const byte ZxSpectrum128Spanish = 0x0E;

    /// <summary>
    /// ZX Spectrum Arabic version.
    /// </summary>
    public const byte ZxSpectrumArabic = 0x0F;

    /// <summary>
    /// Microdigital TK 90-X.
    /// </summary>
    public const byte MicrodigitalTk90X = 0x10;

    /// <summary>
    /// Microdigital TK 95.
    /// </summary>
    public const byte MicrodigitalTk95 = 0x11;

    /// <summary>
    /// Byte.
    /// </summary>
    public const byte Byte = 0x12;

    /// <summary>
    /// Elwro 800-3.
    /// </summary>
    public const byte Elwro800 = 0x13;

    /// <summary>
    /// ZS Scorpion 256.
    /// </summary>
    public const byte ZxScorpion256 = 0x14;

    /// <summary>
    /// Amstrad CPC 464.
    /// </summary>
    public const byte AmstradCpc464 = 0x15;

    /// <summary>
    /// Amstrad CPC 664.
    /// </summary>
    public const byte AmstradCpc664 = 0x16;

    /// <summary>
    /// Amstrad CPC 6128.
    /// </summary>
    public const byte AmstradCpc6128 = 0x17;

    /// <summary>
    /// Amstrad CPC 464+.
    /// </summary>
    public const byte AmstradCpc464Plus = 0x18;

    /// <summary>
    /// Amstrad CPC 6128+.
    /// </summary>
    public const byte AmstradCpc6128Plus = 0x19;

    /// <summary>
    /// Jupiter ACE.
    /// </summary>
    public const byte JupiterAce = 0x1A;

    /// <summary>
    /// Enterprise.
    /// </summary>
    public const byte Enterprise = 0x1B;

    /// <summary>
    /// Commodore 64.
    /// </summary>
    public const byte Commodore64 = 0x1C;

    /// <summary>
    /// Commodore 128.
    /// </summary>
    public const byte Commodore128 = 0x1D;

    /// <summary>
    /// Inves Spectrum+.
    /// </summary>
    public const byte InvesSpectrumPlus = 0x1E;

    /// <summary>
    /// Profi.
    /// </summary>
    public const byte Profi = 0x1F;

    /// <summary>
    /// Grand Rom Max.
    /// </summary>
    public const byte GrandRomMax = 0x20;

    /// <summary>
    /// Kay 1024.
    /// </summary>
    public const byte Kay1024 = 0x21;

    /// <summary>
    /// Ice Felix HC 91.
    /// </summary>
    public const byte IceFelixHc91 = 0x22;

    /// <summary>
    /// Ice Felix HC 2000.
    /// </summary>
    public const byte IceFelixHc2000 = 0x23;

    /// <summary>
    /// Amaterske RADIO Mistrum.
    /// </summary>
    public const byte AmaterskeRasioMistrum = 0x24;

    /// <summary>
    /// Quorum 128.
    /// </summary>
    public const byte Quorum128 = 0x25;

    /// <summary>
    /// MicroART ATM.
    /// </summary>
    public const byte MicroArtAtm = 0x26;

    /// <summary>
    /// MicroART ATM Turbo 2.
    /// </summary>
    public const byte MicroArtAatmTurbo2 = 0x27;

    /// <summary>
    /// Chrome.
    /// </summary>
    public const byte Chrome = 0x28;

    /// <summary>
    /// ZX Badaloc.
    /// </summary>
    public const byte ZxBadaloc = 0x29;

    /// <summary>
    /// Timex Sinclair TC-1500.
    /// </summary>
    public const byte Timex1500 = 0x2A;

    /// <summary>
    /// Lambda.
    /// </summary>
    public const byte Lambda = 0x2B;

    /// <summary>
    /// TK-65.
    /// </summary>
    public const byte Tk65 = 0x2C;

    /// <summary>
    /// ZX-97.
    /// </summary>
    public const byte Zx97 = 0x2D;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        ZxSpectrum16 => "ZX Spectrum 16k",
        ZxSpectrum48 => "ZX Spectrum 48k, Plus",
        ZxSpectrum48Issue1 => "ZX Spectrum 48k ISSUE 1",
        ZxSpectrum128 => "ZX Spectrum 128k +(Sinclair)",
        ZxSpectrum128Plus2 => "ZX Spectrum 128k +2 (grey case)",
        ZxSpectrum128Plus2APlus3 => "ZX Spectrum 128k +2A, +3",
        Timex2048 => "Timex Sinclair TC-2048",
        Timex2068 => "Timex Sinclair TS-2068",
        Pentagon128 => "Pentagon 128",
        SamCoupe => "Sam Coupe",
        DidaktikM => "Didaktik M",
        DidaktikGama => "Didaktik Gama",
        Zx80 => "ZX-80",
        Zx81 => "ZX-81",
        ZxSpectrum128Spanish => "ZX Spectrum 128k, Spanish version",
        ZxSpectrumArabic => "ZX Spectrum, Arabic version",
        MicrodigitalTk90X => "Microdigital TK 90-X",
        MicrodigitalTk95 => "Microdigital TK 95",
        Byte => "Byte",
        Elwro800 => "Elwro 800-3",
        ZxScorpion256 => "ZS Scorpion 256",
        AmstradCpc464 => "Amstrad CPC 464",
        AmstradCpc664 => "Amstrad CPC 664",
        AmstradCpc6128 => "Amstrad CPC 6128",
        AmstradCpc464Plus => "Amstrad CPC 464+",
        AmstradCpc6128Plus => "Amstrad CPC 6128+",
        JupiterAce => "Jupiter ACE",
        Enterprise => "Enterprise",
        Commodore64 => "Commodore 64",
        Commodore128 => "Commodore 128",
        InvesSpectrumPlus => "Inves Spectrum+",
        Profi => "Profi",
        GrandRomMax => "GrandRomMax",
        Kay1024 => "Kay 1024",
        IceFelixHc91 => "Ice Felix HC 91",
        IceFelixHc2000 => "Ice Felix HC 2000",
        AmaterskeRasioMistrum => "Amaterske RADIO Mistrum",
        Quorum128 => "Quorum 128",
        MicroArtAtm => "MicroART ATM",
        MicroArtAatmTurbo2 => "MicroART ATM Turbo 2",
        Chrome => "Chrome",
        ZxBadaloc => "ZX Badaloc",
        Timex1500 => "TS-1500",
        Lambda => "Lambda",
        Tk65 => "TK-65",
        Zx97 => "ZX-97",
        _ => "Unknown Computer Type"
    };
}

/// <summary>
/// Represents the various types of external storage devices.
/// </summary>
public static class ExternalStorageType
{
    /// <summary>
    /// ZX Microdrive.
    /// </summary>
    public const byte ZxMicrodrive = 0x00;

    /// <summary>
    /// Opus Discovery.
    /// </summary>
    public const byte OpusDiscovery = 0x01;

    /// <summary>
    /// MGT Disciple.
    /// </summary>
    public const byte MgtDisciple = 0x02;

    /// <summary>
    /// MGT Plus-D.
    /// </summary>
    public const byte MgtPlusD = 0x03;

    /// <summary>
    /// Rotronics Wafadrive.
    /// </summary>
    public const byte RotronicsWafadrive = 0x04;

    /// <summary>
    /// TR-DOS (BetaDisk).
    /// </summary>
    public const byte BetaDisk = 0x05;

    /// <summary>
    /// Byte Drive.
    /// </summary>
    public const byte ByteDrive = 0x06;

    /// <summary>
    /// Watsford.
    /// </summary>
    public const byte Watsford = 0x07;

    /// <summary>
    /// FIZ.
    /// </summary>
    public const byte Fiz = 0x08;

    /// <summary>
    /// Radofin.
    /// </summary>
    public const byte Radofin = 0x09;

    /// <summary>
    /// Didaktik disk drives.
    /// </summary>
    public const byte DidaktikDiskDrives = 0x0A;

    /// <summary>
    /// BS-DOS (MB-02).
    /// </summary>
    public const byte Mb02 = 0x0B;

    /// <summary>
    /// ZX Spectrum +3 disk drive.
    /// </summary>
    public const byte ZxSpectrumPlus3DiskDrive = 0x0C;

    /// <summary>
    /// JLO (Oliger) disk interface.
    /// </summary>
    public const byte JloDiskInterface = 0x0D;

    /// <summary>
    /// Timex FDD3000.
    /// </summary>
    public const byte TimexFdd3000 = 0x0E;

    /// <summary>
    /// Zebra disk drive.
    /// </summary>
    public const byte ZebraDiskDrive = 0x0F;

    /// <summary>
    /// Ramex Millenia.
    /// </summary>
    public const byte RamexMillenia = 0x10;

    /// <summary>
    /// Larken.
    /// </summary>
    public const byte Larken = 0x11;

    /// <summary>
    /// Kempston disk interface.
    /// </summary>
    public const byte KempstonDiskInterface = 0x12;

    /// <summary>
    /// Sandy.
    /// </summary>
    public const byte Sandy = 0x13;

    /// <summary>
    /// ZX Spectrum +3e hard disk.
    /// </summary>
    public const byte ZxSpectrumPlus3EHardDisk = 0x14;

    /// <summary>
    /// ZXATASP.
    /// </summary>
    public const byte ZxAtaSp = 0x15;

    /// <summary>
    /// DivIDE.
    /// </summary>
    public const byte DivIde = 0x16;

    /// <summary>
    /// ZXCF.
    /// </summary>
    public const byte ZxCf = 0x17;


    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        ZxMicrodrive => "ZX Microdrive",
        OpusDiscovery => "Opus Discovery",
        MgtDisciple => "MGT Disciple",
        MgtPlusD => "MGT Plus-D",
        RotronicsWafadrive => "Rotronics Wafadrive",
        BetaDisk => "TR-DOS (BetaDisk)",
        ByteDrive => "Byte Drive",
        Watsford => "Watsford",
        Fiz => "FIZ",
        Radofin => "Radofin",
        DidaktikDiskDrives => "Didaktik disk drives",
        Mb02 => "BS-DOS (MB-02)",
        ZxSpectrumPlus3DiskDrive => "ZX Spectrum +3 disk drive",
        JloDiskInterface => "JLO (Oliger) disk interface",
        TimexFdd3000 => "Timex FDD3000",
        ZebraDiskDrive => "Zebra disk drive",
        RamexMillenia => "Ramex Millenia",
        Larken => "Larken",
        KempstonDiskInterface => "Kempston disk interface",
        Sandy => "Sandy",
        ZxSpectrumPlus3EHardDisk => "ZX Spectrum +3e hard disk",
        ZxAtaSp => "ZXATASP",
        DivIde => "DivIDE",
        ZxCf => "ZXCF",
        _ => "Unknown External Storage"
    };
}

/// <summary>
/// Represents various constants related to memory add-on types and provides methods for retrieving their names.
/// </summary>
public static class MemoryAddOnType
{
    /// <summary>
    /// Sam Ram.
    /// </summary>
    public const byte SamRam = 0x00;

    /// <summary>
    /// Multiface One.
    /// </summary>
    public const byte MultifaceOne = 0x01;

    /// <summary>
    /// Multiface 128k.
    /// </summary>
    public const byte Multiface128 = 0x02;

    /// <summary>
    /// Multiface +3.
    /// </summary>
    public const byte MultifacePlus3 = 0x03;

    /// <summary>
    /// MultiPrint.
    /// </summary>
    public const byte MultiPrint = 0x04;

    /// <summary>
    /// MB-02 ROM/RAM expansion.
    /// </summary>
    public const byte Mb02 = 0x05;

    /// <summary>
    /// SoftROM.
    /// </summary>
    public const byte SoftRom = 0x06;

    /// <summary>
    /// 1K memory.
    /// </summary>
    public const byte Memory1K = 0x07;

    /// <summary>
    /// 16K memory.
    /// </summary>
    public const byte Memory16K = 0x08;

    /// <summary>
    /// 48K memory.
    /// </summary>
    public const byte Memory48K = 0x09;

    /// <summary>
    /// Memory in 8-16k used.
    /// </summary>
    public const byte Memory8To16K = 0x0A;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        SamRam => "Sam Ram",
        MultifaceOne => "Multiface ONE",
        Multiface128 => "Multiface 128k",
        MultifacePlus3 => "Multiface +3",
        MultiPrint => "MultiPrint",
        Mb02 => "MB-02 ROM/RAM expansion",
        SoftRom => "SoftROM",
        Memory1K => "1k",
        Memory16K => "16k",
        Memory48K => "48k",
        Memory8To16K => "Memory in 8-16k used",
        _ => "Unknown Memory Add-On"
    };
}

/// <summary>
/// Provides constants representing various sound device types and utility methods for retrieving sound device names.
/// </summary>
public static class SoundDeviceType
{
    /// <summary>
    /// Classic AY hardware (compatible with 128k ZXs).
    /// </summary>
    public const byte ClassicAy = 0x00;

    /// <summary>
    /// Fuller Box AY sound hardware.
    /// </summary>
    public const byte FullerBox = 0x01;

    /// <summary>
    /// Currah microSpeech.
    /// </summary>
    public const byte CurrahMicroSpeech = 0x02;

    /// <summary>
    /// SpecDrum.
    /// </summary>
    public const byte SpecDrum = 0x03;

    /// <summary>
    /// AY ACB stereo (A+C=left, B+C=right); Melodik.
    /// </summary>
    public const byte AyAcbStereo = 0x04;

    /// <summary>
    /// AY ABC stereo (A+B=left, B+C=right).
    /// </summary>
    public const byte AyAbcStereo = 0x05;

    /// <summary>
    /// RAM Music Machine.
    /// </summary>
    public const byte RamMusicMachine = 0x06;

    /// <summary>
    /// Covox.
    /// </summary>
    public const byte Covox = 0x07;

    /// <summary>
    /// General Sound.
    /// </summary>
    public const byte GeneralSound = 0x08;

    /// <summary>
    /// Intec Electronics Digital Interface B8001.
    /// </summary>
    public const byte IntecB8001 = 0x09;

    /// <summary>
    /// Zon-X AY.
    /// </summary>
    public const byte ZonX = 0x0A;

    /// <summary>
    /// QuickSilva AY.
    /// </summary>
    public const byte QuickSilvaSoundBoard = 0x0B;

    /// <summary>
    /// Jupiter ACE.
    /// </summary>
    public const byte JupiterAce = 0x0C;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        ClassicAy => "Classic AY hardware (compatible with 128k ZXs)",
        FullerBox => "Fuller Box AY sound hardware",
        CurrahMicroSpeech => "Currah microSpeech",
        SpecDrum => "SpecDrum",
        AyAcbStereo => "AY ACB stereo (A+C=left, B+C=right); Melodik",
        AyAbcStereo => "AY ABC stereo (A+B=left, B+C=right)",
        RamMusicMachine => "RAM Music Machine",
        Covox => "Covox",
        GeneralSound => "General Sound",
        IntecB8001 => "Intec Electronics Digital Interface B8001",
        ZonX => "Zon-X AY",
        QuickSilvaSoundBoard => "QuickSilva AY",
        JupiterAce => "Jupiter ACE",
        _ => "Unknown Sound Device"
    };
}

/// <summary>
/// Provides constants representing various types of joysticks and a method for retrieving their names by identifier.
/// </summary>
public static class JoystickType
{
    /// <summary>
    /// Kempston.
    /// </summary>
    public const byte Kempston = 0x00;

    /// <summary>
    /// Cursor, Protek, AGF/
    /// </summary>
    public const byte Cursor = 0x01;

    /// <summary>
    /// Sinclair 2 Left (12345).
    /// </summary>
    public const byte Sinclair2Left = 0x02;

    /// <summary>
    /// Sinclair 1 Right (67890).
    /// </summary>
    public const byte Sinclair1Right = 0x03;

    /// <summary>
    /// Fuller.
    /// </summary>
    public const byte Fuller = 0x04;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        Kempston => "Kempston",
        Cursor => "Cursor, Protek, AGF",
        Sinclair2Left => "Sinclair 2 Left (12345)",
        Sinclair1Right => "Sinclair 1 Right (67890)",
        Fuller => "Fuller",
        _ => "Unknown Joystick Type"
    };
}

/// <summary>
/// Provides constants representing various mouse types and utility methods for retrieving mouse type names.
/// </summary>
public static class MouseType
{
    /// <summary>
    /// AMX mouse.
    /// </summary>
    public const byte Amx = 0x00;

    /// <summary>
    /// Kempston mouse.
    /// </summary>
    public const byte Kempston = 0x01;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        Amx => "AMX mouse",
        Kempston => "Kempston mouse",
        _ => "Unknown Mouse Type"
    };
}

/// <summary>
/// Provides constants representing various types of other controllers and methods for retrieving their names.
/// </summary>
public static class OtherControllerType
{
    /// <summary>
    /// Trickstick.
    /// </summary>
    public const byte Trickstick = 0x00;

    /// <summary>
    /// ZX Light Gun.
    /// </summary>
    public const byte ZxLightGun = 0x01;

    /// <summary>
    /// Zebra Graphics Tablet.
    /// </summary>
    public const byte ZebraGraphicsTablet = 0x02;

    /// <summary>
    /// Defender Light Gun.
    /// </summary>
    public const byte DefenderLightGun = 0x03;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        Trickstick => "Trickstick",
        ZxLightGun => "ZX Light Gun",
        ZebraGraphicsTablet => "Zebra Graphics Tablet",
        DefenderLightGun => "Defender Light Gun",
        _ => "Unknown Other Controller"
    };
}


/// <summary>
/// Provides constants representing various serial port types and methods for retrieving their names.
/// </summary>
public static class SerialPortType
{
    /// <summary>
    /// ZX Interface 1.
    /// </summary>
    public const byte ZxInterface1 = 0x00;

    /// <summary>
    /// ZX Spectrum 128k.
    /// </summary>
    public const byte ZxSpectrum128 = 0x01;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        ZxInterface1 => "ZX Interface 1",
        ZxSpectrum128 => "ZX Spectrum 128k",
        _ => "Unknown Serial Port Type"
    };
}

/// <summary>
/// Provides constants representing various parallel port types and methods for retrieving their names.
/// </summary>
public static class ParallelPortType
{
    /// <summary>
    /// Kempston S.
    /// </summary>
    public const byte KempstonS = 0x00;

    /// <summary>
    /// Kempston E.
    /// </summary>
    public const byte KempstonE = 0x01;

    /// <summary>
    /// ZX Spectrum +3.
    /// </summary>
    public const byte ZxSpectrumPlus3 = 0x02;

    /// <summary>
    /// Tasman.
    /// </summary>
    public const byte Tasman = 0x03;

    /// <summary>
    /// DK'Tronics.
    /// </summary>
    public const byte DkTronics = 0x04;

    /// <summary>
    /// Hilderbay.
    /// </summary>
    public const byte Hilderbay = 0x05;

    /// <summary>
    /// INES Printerface.
    /// </summary>
    public const byte InesPrinterface = 0x06;

    /// <summary>
    /// ZX LPrint Interface 3.
    /// </summary>
    public const byte ZxLPrintInterface3 = 0x07;

    /// <summary>
    /// MultiPrint.
    /// </summary>
    public const byte MultiPrint = 0x08;

    /// <summary>
    /// Opus Discovery.
    /// </summary>
    public const byte OpusDiscovery = 0x09;

    /// <summary>
    /// Standard 8255 chip with ports 31,63,95.
    /// </summary>
    public const byte Standard8255Chip = 0x0A;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        KempstonS => "Kempston S",
        KempstonE => "Kempston E",
        ZxSpectrumPlus3 => "ZX Spectrum +3",
        Tasman => "Tasman",
        DkTronics => "DK'Tronics",
        Hilderbay => "Hilderbay",
        InesPrinterface => "INES Printerface",
        ZxLPrintInterface3 => "ZX LPrint Interface 3",
        MultiPrint => "MultiPrint",
        OpusDiscovery => "Opus Discovery",
        Standard8255Chip => "Standard 8255 chip with ports 31,63,95",
        _ => "Unknown Parallel Port Type"
    };
}

/// <summary>
/// Provides constants representing various printer types and methods for retrieving their names.
/// </summary>
public static class PrinterType
{
    /// <summary>
    /// ZX Printer, Alphacom 32 and compatibles.
    /// </summary>
    public const byte ZxPrinterCompatible = 0x00;

    /// <summary>
    /// Generic printer.
    /// </summary>
    public const byte GenericPrinter = 0x01;

    /// <summary>
    /// EPSON compatible.
    /// </summary>
    public const byte EpsonCompatible = 0x02;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        ZxPrinterCompatible => "ZX Printer, Alphacom 32 & compatibles",
        GenericPrinter => "Generic printer",
        EpsonCompatible => "EPSON compatible",
        _ => "Unknown Printer Type"
    };
}

/// <summary>
/// Provides constants representing various modem types and methods for retrieving their names.
/// </summary>
public static class ModemType
{
    /// <summary>
    /// Prism VTX 5000.
    /// </summary>
    public const byte PrismVtx5000 = 0x00;

    /// <summary>
    /// T/S 2050 or Westridge 2050.
    /// </summary>
    public const byte Ts2050Westridge2050 = 0x01;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        PrismVtx5000 => "Prism VTX 5000",
        Ts2050Westridge2050 => "T/S 2050 or Westridge 2050",
        _ => "Unknown Modem Type"
    };
}

/// <summary>
/// Provides constants representing various digitizer types and methods for retrieving their names.
/// </summary>
public static class DigitizerType
{
    /// <summary>
    /// RD Digital Tracer.
    /// </summary>
    public const byte RdDigitalTracer = 0x00;

    /// <summary>
    /// DK'Tronics Light Pen.
    /// </summary>
    public const byte DkTronicsLightPen = 0x01;

    /// <summary>
    /// British MicroGraph Pad.
    /// </summary>
    public const byte BritishMicroGraphPad = 0x02;

    /// <summary>
    /// Romantic Robot Videoface.
    /// </summary>
    public const byte RomanticRobotVideoface = 0x03;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        RdDigitalTracer => "RD Digital Tracer",
        DkTronicsLightPen => "DK'Tronics Light Pen",
        BritishMicroGraphPad => "British MicroGraph Pad",
        RomanticRobotVideoface => "Romantic Robot Videoface",
        _ => "Unknown Digitizer Type"
    };
}

/// <summary>
/// Provides constants representing various network adapter types and methods for retrieving their names.
/// </summary>
public static class NetworkAdapterType
{
    /// <summary>
    /// ZX Interface 1.
    /// </summary>
    public const byte ZxInterface1 = 0x00;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        ZxInterface1 => "ZX Interface 1",
        _ => "Unknown Network Adapter Type"
    };
}

/// <summary>
/// Provides constants representing various keyboard keypad types and methods for retrieving their names.
/// </summary>
public static class KeyboardKeypadType
{
    /// <summary>
    /// Keypad for ZX Spectrum 128k.
    /// </summary>
    public const byte ZxSpectrum128Keypad = 0x00;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        ZxSpectrum128Keypad => "Keypad for ZX Spectrum 128k",
        _ => "Unknown Keyboard Keypad Type"
    };
}

/// <summary>
/// Provides constants representing various AD/DA Converter types and methods for retrieving the associated names.
/// </summary>
public static class AdDaConverterType
{
    /// <summary>
    /// Harley Systems ADC 8.2
    /// </summary>
    public const byte HarleySystemsAdc = 0x00;

    /// <summary>
    /// Blackboard Electronics.
    /// </summary>
    public const byte BlackboardElectronics = 0x01;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        HarleySystemsAdc => "Harley Systems ADC 8.2",
        BlackboardElectronics => "Blackboard Electronics",
        _ => "Unknown ADDA Converter Type"
    };
}

/// <summary>
/// Provides constants representing various EPROM programmer types and a method for retrieving their names.
/// </summary>
public static class EpromProgrammerType
{
    /// <summary>
    /// Orme Electronics.
    /// </summary>
    public const byte OrmeElectronics = 0x00;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        OrmeElectronics => "Orme Electronics",
        _ => "Unknown EPROM Programmer Type"
    };
}

/// <summary>
/// Provides constants representing various graphics types and methods for retrieving their names.
/// </summary>
public static class GraphicsType
{
    /// <summary>
    /// WRX Hi-Res.
    /// </summary>
    public const byte WrxHiRes = 0x00;

    /// <summary>
    /// G007.
    /// </summary>
    public const byte G007 = 0x01;

    /// <summary>
    /// Memotech.
    /// </summary>
    public const byte Memotech = 0x02;

    /// <summary>
    /// Lambda Colour.
    /// </summary>
    public const byte LambdaColour = 0x03;

    /// <summary>
    /// Retrieves the name of the hardware type based on the provided identifier.
    /// </summary>
    /// <param name="id">The identifier representing a specific hardware type.</param>
    /// <returns>The name of the hardware type corresponding to the identifier.</returns>
    public static string GetName(byte id) => id switch
    {
        WrxHiRes => "WRX Hi-Res",
        G007 => "G007",
        Memotech => "Memotech",
        LambdaColour => "Lambda Colour",
        _ => "Unknown Graphics Type"
    };
}

