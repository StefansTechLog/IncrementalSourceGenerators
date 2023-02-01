using Microsoft.CodeAnalysis;
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
    public class WithComparerGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classesToGenerate = context.SyntaxProvider
                .CreateSyntaxProvider(SyntaxPredicateFilter, TransformSyntax)
                .Where(x => x != null)
                .WithComparer(new CustomComparer())
                .Collect();

            context.RegisterImplementationSourceOutput(classesToGenerate, GenerateOutput);
        }

        private bool SyntaxPredicateFilter(SyntaxNode syntaxNode, CancellationToken _)
        {
            if (syntaxNode is not ClassDeclarationSyntax classSyntax)
                return false;

            var modifiers = classSyntax.Modifiers.ToString();
            if (modifiers.Contains("static"))
                return false;

            if (!modifiers.Contains("partial"))
                return false;

            return classSyntax.BaseList != null;

        }

        private GeneratorInfo TransformSyntax(GeneratorSyntaxContext syntaxContext, CancellationToken _)
        {
            var classSyntax = syntaxContext.Node as ClassDeclarationSyntax;

            var declaredSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classSyntax);
            if (declaredSymbol is not INamedTypeSymbol classSymbol)
                return null;

            if (classSymbol.AllInterfaces.Count() <= 0)
                return null;

            return new GeneratorInfo
            {
                ClassName = classSyntax.Identifier.ToString(),
                Interfaces = classSymbol.AllInterfaces.Select(x => x.Name).ToList()
            };
        }


        private void GenerateOutput(SourceProductionContext sourceProductionContext, ImmutableArray<GeneratorInfo> classInfo)
        {
            var generatedClasses = new List<string>();

            foreach (var item in classInfo)
            {
                if (generatedClasses.Contains(item.ClassName))
                    continue;

                var hint = $"WithComparer_{Guid.NewGuid()}.g.cs";
                var classSourceCode = SourceText.From(@$"
namespace GeneratorDebugConsumer
{{
    public partial class {item.ClassName}
    {{
        public void GetNumberOfInterfaces()
        {{
            Console.WriteLine(""{item.Interfaces.Count()}"");
        }}
    }}
}}
", Encoding.UTF8);
                sourceProductionContext.AddSource(hint, classSourceCode);

                generatedClasses.Add(item.ClassName);
            }
        }
    }

    internal class CustomComparer : IEqualityComparer<GeneratorInfo>
    {
        public bool Equals(GeneratorInfo x, GeneratorInfo y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(GeneratorInfo obj)
        {
            return obj.ClassName.GetHashCode();
        }
    }

    internal class GeneratorInfo
    {
        public string ClassName { get; set; }
        public List<string> Interfaces { get; set; }
    }
}
