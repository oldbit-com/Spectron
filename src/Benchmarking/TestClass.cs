using BenchmarkDotNet.Attributes;

namespace Benchmarking;


public class TestClass
{
    private readonly int[] _array1 = [1, 2, 3, 4, 5, 6, 7, 8];
    private readonly int[] _array2 = [1, 2, 3, 4, 5, 6, 7, 8];

    [Benchmark]
    public void Method1()
    {
        _array1[0] = 0;
        _array1[1] = 1;
        _array1[2] = 2;
        _array1[3] = 3;
        _array1[4] = 4;
        _array1[5] = 5;
        _array1[6] = 6;
        _array1[7] = 7;
    }

    [Benchmark]
    public void Method2()
    {
        for(var i = 0; i < _array2.Length; i++)
        {
            _array2[i] = i;
        }
    }
}