using System.Reflection;
using OldBit.Spectron.Files.Pok;

namespace OldBit.Spectron.Files.Tests.Pok;

public class PokeFileTests
{
    [Fact]
    public void PokFile_ShouldLoad()
    {
        var pokeFile = PokeFile.Load(GetTestFilePath("ManicMiner.pok"));

        pokeFile.Trainers.Count.ShouldBe(66);

        pokeFile.Trainers[0].Name.ShouldBe("Infinite Lives (Software Project)");
        pokeFile.Trainers[0].Pokes.Count.ShouldBe(1);
        pokeFile.Trainers[0].Pokes[0].MemoryBank.ShouldBe(8);
        pokeFile.Trainers[0].Pokes[0].Address.ShouldBe(35142);
        pokeFile.Trainers[0].Pokes[0].Value.ShouldBe((byte)0);
        pokeFile.Trainers[0].Pokes[0].OriginalValue.ShouldBe(0);

        pokeFile.Trainers[1].Name.ShouldBe("Air supply (Bug Byte)");
        pokeFile.Trainers[1].Pokes.Count.ShouldBe(6);

        pokeFile.Trainers[4].Name.ShouldBe("Infinite Lives on/off");
        pokeFile.Trainers[4].Pokes.Count.ShouldBe(1);
        pokeFile.Trainers[4].Pokes[0].MemoryBank.ShouldBe(8);
        pokeFile.Trainers[4].Pokes[0].Address.ShouldBe(35136);
        pokeFile.Trainers[4].Pokes[0].Value.ShouldBe((byte)0);
        pokeFile.Trainers[4].Pokes[0].OriginalValue.ShouldBe(53);

        pokeFile.Trainers[65].Name.ShouldBe("Restart game in last visited cavern(Software Projects)");
        pokeFile.Trainers[65].Pokes.Count.ShouldBe(1);
        pokeFile.Trainers[65].Pokes[0].MemoryBank.ShouldBe(8);
        pokeFile.Trainers[65].Pokes[0].Address.ShouldBe(34260);
        pokeFile.Trainers[65].Pokes[0].Value.ShouldBe((byte)88);
        pokeFile.Trainers[65].Pokes[0].OriginalValue.ShouldBe(0);
    }

    private static string GetTestFilePath(string fileName)
    {
        var location = typeof(PokeFileTests).GetTypeInfo().Assembly.Location;
        var dir = Path.GetDirectoryName(location) ?? throw new InvalidOperationException();

        return  Path.Combine(dir, "TestFiles", fileName);
    }
}