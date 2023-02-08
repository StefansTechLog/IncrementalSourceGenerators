using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace GeneratorLibrary
{
    [Generator]
    public class CombineCompilationGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classesWithAttribute = context.SyntaxProvider
                .CreateSyntaxProvider(SyntaxPredicateFilter, TransformSyntax)
                .Where(x => x != null)
                .Combine(context.CompilationProvider)
                .Select(Select);

            context.RegisterSourceOutput(classesWithAttribute, GenerateOutput);
        }

        private (ClassDeclarationSyntax Left, Compilation Right) Select((ClassDeclarationSyntax Left, Compilation Right) combination, CancellationToken _)
        {
            return combination;
        }

        private void GenerateOutput(SourceProductionContext sourceProductionContext,
            (ClassDeclarationSyntax classSyntax, Compilation Compilation) result)
        {
            return;
        }

        private bool SyntaxPredicateFilter(SyntaxNode syntaxNode, CancellationToken _)
        {
            return syntaxNode is ClassDeclarationSyntax;
        }

        private ClassDeclarationSyntax TransformSyntax(GeneratorSyntaxContext syntaxContext, CancellationToken _)
        {
            return syntaxContext.Node as ClassDeclarationSyntax;
        }
    }
}
