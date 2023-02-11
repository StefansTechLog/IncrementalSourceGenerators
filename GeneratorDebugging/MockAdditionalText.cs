using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace GeneratorDebugging
{
    public class MockAdditionalText : AdditionalText
    {
        public MockAdditionalText(string text, string path)
        {
            Text = text;
            Path = path;
        }

        private string Text { get; }
        public override string Path { get; }

        public override SourceText? GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(Text);
        }
    }
}
