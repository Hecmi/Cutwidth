using System;

namespace CWP
{
    class Program
    {
        static void Main(string[] args)
        {
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\graph.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\494_bus.mtx.txt";
            string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\662_bus.mtx.txt";
            if (args.Length > 0)
            {
                ruta_archivo = args[0];
            }

            //CWP cwp_solver = new CWP();
            //cwp_solver.resolver(ruta_archivo);

            Console.WriteLine("");


            CWPC cwp_solver_c = new CWPC();
            cwp_solver_c.resolver(ruta_archivo, "grasp");
            
            Console.Read();
        }
    }
}
