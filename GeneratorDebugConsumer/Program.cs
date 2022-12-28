namespace GeneratorDebugConsumer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }

    public partial class Foobar
    {
        [OnDispose(2)]
        public void Free2()
        {
            Console.WriteLine("Free2");
        }

        [OnDispose(1)]
        public void Free1()
        {
            Console.WriteLine("Free1");
        }
        [OnDispose]
        public void Free3()
        {
            Console.WriteLine("Free3");
        }

        [OnDisposeAttribute(CallOrder = 0)]
        public void Free0()
        {
            Console.WriteLine("Free0");
        }

        public void NotFree()
        {
            Console.WriteLine("This will not be called");
        }
    }
}