namespace GeneratorDebugConsumer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            using (var foobar = new Foobar())
            {
                Console.WriteLine("in scope");
                foobar.NotCollected();
                foobar.GetNumberOfInterfaces();
            }
            Console.WriteLine("out of scope");

            var bob = new Bob();
            bob.NotCollected();
            bob.GetNumberOfInterfaces();

            var alice = new Alice();
            alice.NotCollected();
            alice.GetNumberOfInterfaces();

            ClassHelper.GetAllClasses();
            ElementHelper.AllDistinctElements();
        }
    }

    public interface Interface1 { }
    public interface Interface2 { }
    public interface Interface3 { }

    public partial class Bob : Interface1
    {

    }

    public partial class Alice : Interface1, Interface2
    {

    }

    public partial class Foobar : Interface1, Interface2, Interface3
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