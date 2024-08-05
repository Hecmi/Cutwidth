using System;

namespace CWP
{
    class Program
    {
        static void Main(string[] args)
        {
            CWP cwp_solver = new CWP(@"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\4.txt");
            cwp_solver.resolver();
            Console.Read();
        }
    }
}
