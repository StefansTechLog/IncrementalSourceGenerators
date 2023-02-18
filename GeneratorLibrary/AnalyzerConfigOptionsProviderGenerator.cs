using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Threading;

namespace GeneratorLibrary
{
    [Generator]
    public class AnalyzerConfigOptionsProviderGenerator : IIncrementalGenerator
    {
        readonly string MessageRule = "dotnet_diagnostic.ABC001.should_not_start_with_j";
        readonly string WarningRule = "dotnet_diagnostic.ABC001.should_not_start_with_k";
        readonly string ErrorRule = "dotnet_diagnostic.ABC001.must_not_start_with_l";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var results = context.SyntaxProvider
                .CreateSyntaxProvider(SyntaxPredicateFilter, TransformSyntax)
                .Where(x => x != null)
                .Combine(context.AnalyzerConfigOptionsProvider);

            context.RegisterSourceOutput(results, GenerateOutput);
        }

        private void GenerateOutput(SourceProductionContext sourceProductionContext,
            (ClassDeclarationSyntax classSyntax, AnalyzerConfigOptionsProvider configProvider) result)
        {
            var classSyntax = result.classSyntax;
            var cfgProvider = result.configProvider.GetOptions(classSyntax.SyntaxTree);
            var className = result.classSyntax.Identifier.ToString();
            var location = classSyntax.GetLocation();

            if (className.StartsWith("j") || className.StartsWith("J"))
            {
                if (cfgProvider.TryGetValue(MessageRule, out var value))
                {
                    //throw new Exception(value);
                    AddDiagnostics(sourceProductionContext, DiagnosticSeverity.Info, location, value);
                }
            }

            else if (className.StartsWith("k") || className.StartsWith("K"))
            {
                if (cfgProvider.TryGetValue(WarningRule, out var value))
                {
                    //throw new Exception(value);
                    AddDiagnostics(sourceProductionContext, DiagnosticSeverity.Warning, location, value);
                }
            }

            else if (className.StartsWith("l") || className.StartsWith("L"))
            {
                if (cfgProvider.TryGetValue(ErrorRule, out var value))
                {
                    //throw new Exception(value);
                    AddDiagnostics(sourceProductionContext, DiagnosticSeverity.Error, location, value);
                }
            }
        }
        private void AddDiagnostics(SourceProductionContext sourceProductionContext, DiagnosticSeverity diagnosticSeverity, Location location, string message)
        {
            var diagnostocDescription = new DiagnosticDescriptor(
                id: "ABC001",
                title: "My demo diagnostics",
                messageFormat: message,
                category: "demostration",
                defaultSeverity: diagnosticSeverity,
                isEnabledByDefault: true);

            var diagnostics = Diagnostic.Create(diagnostocDescription, location);
            sourceProductionContext.ReportDiagnostic(diagnostics);
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
