using GeneratorLibrary;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using Xunit;

namespace GeneratorDebugging
{
    public class MyGeneratorTests
    {

        [Fact]
        public void DebugMarkerGenerator()
        {
            var markerGenerator = new MarkerGenerator();
            var result = GeneratorDebugger.RunDebugging(Array.Empty<SyntaxTree>(), new[] { markerGenerator });
            Debug.WriteLine(result.GeneratedTrees.Count());
        }

        [Fact]
        public void DebugOnDisposeGenerator()
        {
            var markerGenerator = new MarkerGenerator();
            var disposableGenerator = new OnDisposeGenerator();
            var ProgramCode = CSharpSyntaxTree.ParseText(@"
namespace GeneratorDebugConsumer
{
    public partial class Foobar
    {
        [OnDispose( 1 )]
        public void Free1()
        {
            Console.WriteLine(""Free1"");
        }

        [OnDispose]
        public void Free3()
        {
            Console.WriteLine(""Free3"");
        }

        [OnDisposeAttribute(CallOrder = 2)]
        public void Free2()
        {
            Console.WriteLine(""Free2"");
        }

    }
}

");
            //var result = GeneratorDebugger.RunDebugging(new[] { ProgramCode }, new IIncrementalGenerator[] { disposableGenerator });
            var result = GeneratorDebugger.RunDebugging(new[] { ProgramCode }, new IIncrementalGenerator[] { markerGenerator, disposableGenerator });
            Debug.WriteLine(result.GeneratedTrees.Count());
        }

        [Fact]
        public void CollectVsNotCollectGenerator()
        {
            var generator = new CollectVsNotCollectGenerator();
            var ProgramCode = CSharpSyntaxTree.ParseText(@"
namespace GeneratorDebugConsumer
{
    public partial class Foobar
    {

    }

    public partial class Bob
    {

    }

    public partial class Alice
    {

    }
}

");

            var result = GeneratorDebugger.RunDebugging(new[] { ProgramCode }, new IIncrementalGenerator[] { generator });
            Debug.WriteLine(result.GeneratedTrees.Count());
        }

        [Fact]
        public void SelectWhereGenerator()
        {
            var generator = new SelectAndWhereGenerator();
            var ProgramCode = CSharpSyntaxTree.ParseText(@"
namespace GeneratorDebugConsumer
{
    public partial class Foobar
    {

    }

    public static class Bob
    {

    }

    public class Alice
    {

    }

    public abstract class Eric
    {

    }

    public partial class Betty
    {

    }
}

");

            var result = GeneratorDebugger.RunDebugging(new[] { ProgramCode }, new IIncrementalGenerator[] { generator });
            Debug.WriteLine(result.GeneratedTrees.Count());
        }

        [Fact]
        public void WithComparerGenerator()
        {
            var generator = new WithComparerGenerator();
            var ProgramCode = CSharpSyntaxTree.ParseText(@"
namespace GeneratorDebugConsumer
{
    public partial class Foobar : IInterfaceA
    {

    }

    public partial class Barbar : IInterfaceA, IInterfaceB, IInterfaceC
    {

    }

    public interface IInterfaceA
    {

    }

    public interface IInterfaceB
    {

    }

    public interface IInterfaceC
    {

    }
}

");

            var result = GeneratorDebugger.RunDebugging(new[] { ProgramCode }, new IIncrementalGenerator[] { generator });
            Debug.WriteLine(result.GeneratedTrees.Count());
        }

        [Fact]
        public void AdditionalTextGenerator()
        {
            var textfile = "Foobar.txt";
            var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
            var additionalText = new MockAdditionalText(text, textfile);

            var generator = new AdditionalTextsGenerator();
            var ProgramCode = CSharpSyntaxTree.ParseText(@"
namespace GeneratorDebugConsumer
{
    public partial class Foobar
    {

    }

    public partial class Betty
    {

    }
}

");

            var result = GeneratorDebugger.RunDebugging(new[] { ProgramCode }, new[] { generator }, new[] { additionalText });
            Debug.WriteLine(result.GeneratedTrees.Count());
        }

        [Fact]
        public void ParserOptionsGenerator()
        {
            var parserOptions = new CSharpParseOptions(LanguageVersion.CSharp10,
                DocumentationMode.Parse,
                SourceCodeKind.Script,
                new[] { "DEBUG", "dotnet", "subscribe to Stefan's TechLog" });

            var generator = new ParserOptionsGenerator();
            var ProgramCode = CSharpSyntaxTree.ParseText(@"
namespace GeneratorDebugConsumer
{
    public class Foobar
    {
    }
}
");

            var result = GeneratorDebugger.RunDebugging(new[] { ProgramCode }, new[] { generator }, null, parserOptions);
            Debug.WriteLine(result.GeneratedTrees.Count());
        }

        [Fact]
        public void MetadataReferenceGenerator()
        {
            var generator = new MetadataGenerator();
            var ProgramCode = CSharpSyntaxTree.ParseText(@"
namespace GeneratorDebugConsumer
{
    public class Foobar
    {
    }
}
");
            
            var references = AppDomain.CurrentDomain.GetAssemblies()
                                      .Where(assembly => !assembly.IsDynamic)
                                      .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                                      .Cast<MetadataReference>();

            var result = GeneratorDebugger.RunDebugging(new[] { ProgramCode }, new[] { generator }, null, null, references);
            Debug.WriteLine(result.GeneratedTrees.Count());
        }

        [Fact]
        public void AnalyzerConfigOptionsGenerator()
        {
            var generator = new AnalyzerConfigOptionsProviderGenerator();
            var ProgramCode = CSharpSyntaxTree.ParseText(@"
namespace GeneratorDebugConsumer
{
    public class Juicebox
    {
    }

    public class Kilohertz
    {
    }

    public class Lemontree
    {
    }
}
");
            var options = new Dictionary<string, string>
            {
                {"dotnet_diagnostic.ABC001.should_not_start_with_j", "starting with j is unusual"},
                {"dotnet_diagnostic.ABC001.should_not_start_with_k", "you really should not start with k"},
                {"dotnet_diagnostic.ABC001.must_not_start_with_l", "starting with l is absolutely forbidden!"}
            };
            var analyzerCfgOptionsProvider = new MockAnalyzerConfigOptionsProvider(options);

            var result = GeneratorDebugger.RunDebugging(new[] { ProgramCode }, new[] { generator }, null, null, null, analyzerCfgOptionsProvider);
            Debug.WriteLine(result.GeneratedTrees.Count());
        }
    }
}
