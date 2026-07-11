namespace OldBit.Spectron.Files.Pok;

/// <summary>
/// Represents a poke that needs to be applied to a game.
/// </summary>
/// <param name="MemoryBank">Bits 0-2 specify a memory bank for 128K games or bit 3
/// is always set for 48K games</param>
/// <param name="Address">The address (16384-65535).</param>
/// <param name="Value">Poke value (0-255). If it is null, a requester should pop up where the user can enter a value.</param>
/// <param name="OriginalValue">Original value at the address.</param>
public record Poke(int MemoryBank, Word Address, byte? Value, byte OriginalValue);

/// <summary>
/// Represents a trainer that contains a list of pokes.
/// </summary>
/// <param name="Name">The name of the trainer.</param>
/// <param name="Pokes">A list of pokes.</param>
public record Trainer(string Name, List<Poke> Pokes);

/// <summary>
/// Represents a POKE file.
/// </summary>
public sealed class PokeFile
{
    /// <summary>
    /// Gets the list of trainers in the POKE file.
    /// </summary>
    public List<Trainer> Trainers { get; } = [];

    private PokeFile()
    {
    }

    /// <summary>
    /// Loads a POKE file from the specified file and parses its contents into a PokeFile object.
    /// </summary>
    /// <param name="fileName">The path to the POKE file to be loaded.</param>
    /// <returns>A <see cref="PokeFile"/> instance containing the trainers and pokes parsed from the file.</returns>
    public static PokeFile Load(string fileName)
    {
        using FileStream stream = new(fileName, FileMode.Open, FileAccess.Read);

        return Load(stream);
    }

    /// <summary>
    /// Loads a POKE file from the provided stream and parses its contents into a PokeFile object.
    /// </summary>
    /// <param name="stream">The stream containing the POKE file data to be loaded.</param>
    /// <returns>A <see cref="PokeFile"/> instance containing the trainers and pokes parsed from the stream.</returns>
    public static PokeFile Load(Stream stream)
    {
        using StreamReader reader = new(stream);

        var pokeFile = new PokeFile();
        Trainer? trainer = null;

        while (reader.ReadLine() is { } line)
        {
            if (IsNextTrainer(line))
            {
                trainer = new Trainer(line[1..], []);
            }

            if (IsPoke(line))
            {
                var poke = ParsePoke(line);

                if (poke != null)
                {
                    trainer?.Pokes.Add(poke);
                }
            }

            if (IsLastPoke(line))
            {
                var poke = ParsePoke(line);

                if (poke != null && trainer != null)
                {
                    trainer.Pokes.Add(poke);
                    pokeFile.Trainers.Add(trainer);

                    trainer = null;
                }
            }

            if (IsLastLine(line))
            {
                break;
            }
        }

        return pokeFile;
    }

    private static Poke? ParsePoke(string line)
    {
        var parts = line.Split(' ');

        if (parts.Length != 5)
        {
            return null;
        }

        if (!int.TryParse(parts[1].Trim(), out var memoryBank))
        {
            return null;
        }

        if (!Word.TryParse(parts[2].Trim(), out var address))
        {
            return null;
        }

        if (!int.TryParse(parts[3].Trim(), out var value))
        {
            return null;
        }

        if (!byte.TryParse(parts[4].Trim(), out var originalValue))
        {
            return null;
        }

        return new Poke(memoryBank, address, value > 255 ? null : (byte)value, originalValue);
    }

    private static bool IsNextTrainer(string line) => line.StartsWith('N');

    private static bool IsLastLine(string line) => line.StartsWith('Y');

    private static bool IsPoke(string line) => line.StartsWith('M');

    private static bool IsLastPoke(string line) => line.StartsWith('Z');
}