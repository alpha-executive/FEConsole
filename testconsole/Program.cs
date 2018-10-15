using System;
using System.IO;

namespace testconsole
{
    using testconsole.mathlib;
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(string.Format("{0} + {1} = {2}", 1, 1, Calculator.Sum(1, 1)));
            //Console.WriteLine("Hello World!");
        }
    }
}
