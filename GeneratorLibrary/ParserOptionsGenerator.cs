using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace GeneratorLibrary
{
    [Generator]
    public class ParserOptionsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.ParseOptionsProvider, GenerateOutput);
        }

        private void GenerateOutput(SourceProductionContext sourceProductionContext, ParseOptions options)
        {
            var hint = $"Options_Generator.g.cs";
            var classSourceCode = SourceText.From(@$"
namespace GeneratorDebugConsumer
{{
    public static class MyOptionsHelper
    {{
        public static void AllErrors()
        {{
            Console.WriteLine(@""{string.Join(", ", options.Errors.Select(x => x.GetMessage()))}"");
        }}

        public static void AllFeatures()
        {{
            Console.WriteLine(@""{string.Join(", ", options.Features.Select(x => x.Key + ": " + x.Value))}"");
        }}

        public static void AllPreprocessorSymbols()
        {{
            Console.WriteLine(@""{string.Join(", ", options.PreprocessorSymbolNames)}"");
        }}

        public static void Language()
        {{
            Console.WriteLine(""{options.Language} version {((CSharpParseOptions)options).LanguageVersion}"");
        }}
    }}
}}
", Encoding.UTF8);

            sourceProductionContext.AddSource(hint, classSourceCode);
        }
    }
}
