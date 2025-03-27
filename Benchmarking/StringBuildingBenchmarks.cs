using BenchmarkDotNet.Attributes;
using System.Text;

namespace Benchmarking
{
    [MemoryDiagnoser]
    public class StringBuildingBenchmarks
    {
        private const string str1 = "Hello";
        private int value = 0;

        [Benchmark]
        public string StringInterpolation()
        {
            value++;
            return $"{str1} {value}";
        }

        [Benchmark]
        public string StringFormat()
        {
            return string.Format("{0} {1}", str1, value);
        }

        [Benchmark]
        public string StringConcatenation()
        {
            return str1 + " " + value;
        }

        [Benchmark]
        public string StringBuilder()
        {
            var sb = new StringBuilder();
            sb.Append(str1);
            sb.Append(' ');
            sb.Append(value);
            return sb.ToString();
        }
    }
}
