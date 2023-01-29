using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace GeneratorLibrary
{
    [Generator]
    public class CollectVsNotCollectGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classesCollected = context.SyntaxProvider
            .CreateSyntaxProvider(SyntaxPredicateFilter, TransformSyntax)
            .Where(x => x != null)
            .Collect();

            context.RegisterSourceOutput(classesCollected, GenerateOutput_Collected);


            var classesNotCollected = context.SyntaxProvider
                .CreateSyntaxProvider(SyntaxPredicateFilter, TransformSyntax)
                .Where(x => x != null);

            context.RegisterSourceOutput(classesNotCollected, GenerateOutput_NotCollected);
        }

        private void GenerateOutput_Collected(SourceProductionContext sourceProductionContext, ImmutableArray<string> classNames)
        {
            var hint = $"Collect_{Guid.NewGuid()}.g.cs";
            var classSourceCode = SourceText.From(@$"
namespace GeneratorDebugConsumer
{{
    public static class ClassHelper
    {{
        public static void GetAllClasses()
        {{
            Console.WriteLine(""{string.Join(", ", classNames)}"");
        }}
    }}
}}
", Encoding.UTF8);
            sourceProductionContext.AddSource(hint, classSourceCode);
        }

        private void GenerateOutput_NotCollected(SourceProductionContext sourceProductionContext, string className)
        {

            var hint = $"NoCollect_{Guid.NewGuid()}.g.cs";
            var classSourceCode = SourceText.From(@$"
namespace GeneratorDebugConsumer
{{
    public partial class {className}
    {{
        public void NotCollected()
        {{
            Console.WriteLine(""{className} not collected!!!"");
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
        private string TransformSyntax(GeneratorSyntaxContext syntaxContext, CancellationToken _)
        {
            var classSyntax = syntaxContext.Node as ClassDeclarationSyntax;
            var className = classSyntax.Identifier.ToString();

            if (className == "Program")
                return null;

            if (className.Contains("Attribute"))
                return null;

            return classSyntax.Identifier.ToString();
        }
    }
}
