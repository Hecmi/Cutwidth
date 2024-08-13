using System;

namespace CWP
{
    class Program
    {
        static void Main(string[] args)
        {
            string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\2.txt";
            if (args.Length > 0)
            {
                ruta_archivo = args[0];
            }

            CWP cwp_solver = new CWP();
            cwp_solver.resolver(ruta_archivo);
            Console.Read();
        }
    }
}
