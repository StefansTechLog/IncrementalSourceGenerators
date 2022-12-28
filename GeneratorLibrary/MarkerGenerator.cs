using Microsoft.CodeAnalysis;

namespace GeneratorLibrary
{
    [Generator]
    public class MarkerGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var hint = "MyMarkerFilename.g.cs";
            var markerSource = @"
using System;

namespace GeneratorDebugConsumer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnDisposeAttribute: Attribute
    {
        public int CallOrder { get; set; } = int.MaxValue;

        public OnDisposeAttribute(int callOrder = int.MaxValue)
        {            
            this.CallOrder = callOrder;
        }
    }
}
";

            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource(hint, markerSource);
            });
        }
    }
}
