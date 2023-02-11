using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GeneratorLibrary
{
    [Generator]
    public class MetadataGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.MetadataReferencesProvider.Collect(), GenerateOutput);
        }

        private void GenerateOutput(SourceProductionContext sourceProductionContext, ImmutableArray<MetadataReference> MetadataReferences)
        {
            var refs = MetadataReferences.Select(x => @$"            Console.WriteLine(""{x.Display.Replace("\\", "\\\\")}"");");

            var hint = $"MetadataReference_Generator.g.cs";
            var classSourceCode = SourceText.From(@$"
namespace GeneratorDebugConsumer
{{
    public static class MetadataReferenceHelper
    {{
        public static void AllReferences()
        {{
{string.Join(Environment.NewLine, refs)}
        }}
    }}
}}
", Encoding.UTF8);

            sourceProductionContext.AddSource(hint, classSourceCode);
        }
    }
}
