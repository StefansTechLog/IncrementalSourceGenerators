using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace GeneratorLibrary
{
    [Generator]
    public class OnDisposeGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {

            var classesWithAttribute = context.SyntaxProvider
                .CreateSyntaxProvider(SyntaxPredicateFilter, TransformSyntax)
                .Where(x => x != null)
                .Collect();

            context.RegisterSourceOutput(classesWithAttribute, GenerateOutput);
        }
        private bool SyntaxPredicateFilter(SyntaxNode syntaxNode, CancellationToken _)
        {
            if (syntaxNode is not ClassDeclarationSyntax classSyntax)
                return false;

            if (classSyntax.Parent is not NamespaceDeclarationSyntax parentNameSpace)
                return false;

            foreach (var item in classSyntax.Members)
            {
                if (item is MethodDeclarationSyntax)
                    return true;
                continue;
            }

            return false;
        }


        const string FullyQuallyfiedMarkerName = "GeneratorDebugConsumer.OnDisposeAttribute";
        private OnDisposeClassInfo TransformSyntax(GeneratorSyntaxContext syntaxContext, CancellationToken _)
        {
            var classSyntax = syntaxContext.Node as ClassDeclarationSyntax;

            var methods = new List<OnDisposeMethodInfo>();
            foreach (var member in classSyntax.Members)
            {
                if (member is not MethodDeclarationSyntax methodSyntax)
                    continue;

                if (!member.AttributeLists.Any())
                    continue;

                var methodSemanticModel = syntaxContext.SemanticModel.Compilation.GetSemanticModel(member.SyntaxTree);
                if (methodSemanticModel.GetDeclaredSymbol(member) is not IMethodSymbol methodSymbol)
                    continue;

                var memberAttributes = methodSymbol.GetAttributes();
                foreach (var attribute in memberAttributes)
                {
                    if (!FullyQuallyfiedMarkerName.Contains(attribute.AttributeClass.ToString()))
                        continue;

                    var methodInfo = new OnDisposeMethodInfo();
                    methodInfo.MethodName = methodSymbol.Name;

                    if (attribute.NamedArguments.IsEmpty && attribute.ConstructorArguments.IsEmpty)
                    {
                        methodInfo.SortOrder = int.MaxValue;
                    }
                    else if (!attribute.NamedArguments.IsEmpty)
                    {
                        var value = attribute.NamedArguments[0].Value.Value;
                        methodInfo.SortOrder = (int)value;
                    }
                    else if (!attribute.ConstructorArguments.IsEmpty)
                    {
                        var value = attribute.ConstructorArguments[0].Value;
                        methodInfo.SortOrder = (int)value;
                    }

                    methods.Add(methodInfo);
                    break;
                }
            }

            if (!methods.Any())
                return null;

            var namespaceSyntax = syntaxContext.Node.Parent as NamespaceDeclarationSyntax;
            return new OnDisposeClassInfo
            {
                Methods = methods,
                ClassName = classSyntax.Identifier.ToString(),
                NameSpace = namespaceSyntax.Name.ToString(),
            };
        }

        private void GenerateOutput(SourceProductionContext sourceProductionContext, ImmutableArray<OnDisposeClassInfo> result)
        {
            foreach (var item in result)
            {
                var orderedMethodCalls = item.Methods.OrderBy(x => x.SortOrder).Select(x => $"{x.MethodName}();");
                var hint = $"{item.ClassName}_OnDisposeResult.g.cs";
                var classSourceCode = SourceText.From(@$"
namespace {item.NameSpace}
{{
    public partial class {item.ClassName} : IDisposable
    {{
        public void Dispose()
        {{
            {string.Join(Environment.NewLine + "            ", orderedMethodCalls)}
        }}
    }}
}}
", Encoding.UTF8);
                sourceProductionContext.AddSource(hint, classSourceCode);

            }
        }
    }

    public class OnDisposeClassInfo
    {
        public string ClassName { get; set; }
        public string NameSpace { get; set; }
        public List<OnDisposeMethodInfo> Methods { get; set; }
    }
    public class OnDisposeMethodInfo
    {
        public string MethodName { get; set; }
        public int SortOrder { get; set; }
    }

}
