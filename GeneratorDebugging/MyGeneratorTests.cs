using GeneratorLibrary;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Xunit;

namespace GeneratorDebugging
{
    public class MyGeneratorTests
    {

        [Fact]
        public void DebugMarkerGenerator()
        {
            var markerGenerator = new MarkerGenerator();
            var result = GeneratorDebugger.RunDebugging(Array.Empty<SyntaxTree>(), new[] { markerGenerator });
            Debug.WriteLine(result.GeneratedTrees.Count());
        }
    }
}
