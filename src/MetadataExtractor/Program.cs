using System;

namespace MetadataExtractor
{
    public class Program
    {
        static void Main(string[] args)
        {
            var result = Test(7, 5);
            Console.WriteLine($"Hello World! { result }");
        }

        public static int Test(int a, int b) {
            return a * b;
        }
    }
}
