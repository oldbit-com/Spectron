using System.Reflection;
using System.Text.Json;

namespace OldBit.Spectron.Disassembly.Tests.Support;

public static class TestCaseLoader
{
    public static IEnumerable<object[]> GetTestData(string fileName)
    {
        var testGroups = Load(fileName);

        foreach (var testCase in testGroups.SelectMany(testGroup => testGroup.Value))
        {
            yield return [testCase];
        }
    }

    private static Dictionary<string, List<InstructionTestCase>> Load(string fileName)
    {
        var json = File.ReadAllText(Path.Combine(BinFolder, "TestFiles", fileName));

        return JsonSerializer.Deserialize<Dictionary<string, List<InstructionTestCase>>>(json)!;
    }

    private static string BinFolder => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
}