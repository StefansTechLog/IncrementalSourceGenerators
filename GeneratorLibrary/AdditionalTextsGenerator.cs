using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace GeneratorLibrary
{
    [Generator]
    public class AdditionalTextsGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classesWithAttribute = context.SyntaxProvider
                .CreateSyntaxProvider(SyntaxPredicateFilter, TransformSyntax)
                .Combine(context.AdditionalTextsProvider.Collect())
                .Select(Select)
                .Where(x => x.Text != null)
                .Collect();

            context.RegisterSourceOutput(classesWithAttribute, GenerateOutput);
        }

        private void GenerateOutput(SourceProductionContext sourceProductionContext,
            ImmutableArray<(ClassDeclarationSyntax ClassSyntax, AdditionalText Text)> classInfo)
        {
            var generatedClasses = new List<string>();

            foreach (var item in classInfo)
            {
                var className = item.ClassSyntax.Identifier.ToString();

                if (generatedClasses.Contains(className))
                    continue;

                var hint = $"AdditionalTexts_{Guid.NewGuid()}.g.cs";
                var classSourceCode = SourceText.From(@$"
namespace GeneratorDebugConsumer
{{
    public partial class {className}
    {{
        public void PrintText()
        {{
            Console.WriteLine(""{item.Text.GetText()}"");
        }}
    }}
}}
", Encoding.UTF8);
                sourceProductionContext.AddSource(hint, classSourceCode);

                generatedClasses.Add(className);
            }
        }

        private (ClassDeclarationSyntax ClassSyntax, AdditionalText Text) Select(
            (ClassDeclarationSyntax ClassSyntax, ImmutableArray<AdditionalText> AddtionalTexts) combination,
            CancellationToken _)
        {
            if (combination.ClassSyntax == null)
                return new(combination.ClassSyntax, null);

            var desiredFileName = combination.ClassSyntax.Identifier.ToString() + ".txt";

            foreach (var text in combination.AddtionalTexts)
            {
                if (text.Path.EndsWith(desiredFileName))
                {
                    return new(combination.ClassSyntax, text);
                }
            }
            return new(combination.ClassSyntax, null);
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
