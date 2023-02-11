using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace GeneratorDebugging
{
    internal static class GeneratorDebugger
    {
        internal static GeneratorDriverRunResult RunDebugging(
            IEnumerable<SyntaxTree> sourceCode,
            IIncrementalGenerator[] generators,
            IEnumerable<AdditionalText>? additionalTexts = null
            )
        {
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            var inputCompilation = CSharpCompilation.Create("compilationAssemblyName", sourceCode, null, compilationOptions);

            var driver = (GeneratorDriver)CSharpGeneratorDriver.Create(generators);

            if (additionalTexts != null)
                driver = driver.AddAdditionalTexts(ImmutableArray.CreateRange(additionalTexts));

            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out _, out _);
            return driver.GetRunResult();
        }
    }
}
