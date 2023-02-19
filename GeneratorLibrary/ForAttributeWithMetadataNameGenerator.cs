using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading;

namespace GeneratorLibrary
{
    [Generator]
    public class ForAttributeWithMetadataNameGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var results = context.SyntaxProvider
                .ForAttributeWithMetadataName("System.SerializableAttribute", SyntaxPredicateFilter, TransformSyntax);

            context.RegisterSourceOutput(results, GenerateOutput);
        }

        private void GenerateOutput(SourceProductionContext sourceProductionContext, ClassDeclarationSyntax classSyntax)
        {

            var className = classSyntax.Identifier.ToString();
            var hint = $"{className}_SerializableGeneration.cs";

            var classSourceCode = SourceText.From(@$"
namespace GeneratorDebugConsumer
{{
    public partial class {className}
    {{
        public string GetAsSerialized()
        {{
            return System.Text.Json.JsonSerializer.Serialize(this);
        }}
    }}
}}
", Encoding.UTF8);
            sourceProductionContext.AddSource(hint, classSourceCode);
        }

        private bool SyntaxPredicateFilter(SyntaxNode syntaxNode, CancellationToken _)
        {
            return syntaxNode is ClassDeclarationSyntax;
        }

        private ClassDeclarationSyntax TransformSyntax(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken _)
        {
            return syntaxContext.TargetNode as ClassDeclarationSyntax;
        }
    }
}
