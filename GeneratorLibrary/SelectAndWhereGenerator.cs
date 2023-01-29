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
    public class SelectAndWhereGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classesToGenerate = context.SyntaxProvider
                .CreateSyntaxProvider(SyntaxPredicateFilter, TransformSyntax)
                .Where(WhereFilter1)
                .Select(Select1)
                .Where(WhereFilter2)
                .SelectMany(Select2)
                .Collect();

            context.RegisterSourceOutput(classesToGenerate, GenerateOutput);
        }

        private ImmutableArray<string> Select2(string[] args, CancellationToken _)
        {
            if (args == null)
                return ImmutableArray<string>.Empty;

            var elements = args[0].Split(' ').ToList();
            elements.Add(args[1]);
            elements.Add(args[2]);
            return elements.ToImmutableArray();
        }

        private bool WhereFilter2(string[] arg)
        {
            var isPartial = arg[0].Contains("partial");
            return isPartial;
        }

        private string[] Select1(ClassDeclarationSyntax classSyntax, CancellationToken _)
        {
            if (classSyntax == null)
                return null;

            var strArr = new string[3];
            strArr[0] = classSyntax.Modifiers.ToString();
            strArr[1] = classSyntax.Keyword.Text;
            strArr[2] = classSyntax.Identifier.ToString();
            return strArr;
        }

        private bool WhereFilter1(ClassDeclarationSyntax classSyntax)
        {
            var modifiers = classSyntax.Modifiers.ToString();
            var isStatic = modifiers.ToLower().Contains("static");
            return !isStatic;
        }


        private bool SyntaxPredicateFilter(SyntaxNode syntaxNode, CancellationToken _)
        {
            return syntaxNode is ClassDeclarationSyntax;
        }
        private ClassDeclarationSyntax TransformSyntax(GeneratorSyntaxContext syntaxContext, CancellationToken _)
        {
            return syntaxContext.Node as ClassDeclarationSyntax;
        }
        private void GenerateOutput(SourceProductionContext sourceProductionContext, ImmutableArray<string> classElements)
        {
            var hint = $"Collect_{Guid.NewGuid()}.g.cs";
            var classSourceCode = SourceText.From(@$"
namespace GeneratorDebugConsumer
{{
    public static class ElementHelper
    {{
        public static void AllDistinctElements()
        {{
            Console.WriteLine(""{string.Join(", ", classElements.Distinct())}"");
        }}
    }}
}}
", Encoding.UTF8);
            sourceProductionContext.AddSource(hint, classSourceCode);
        }
    }
}
