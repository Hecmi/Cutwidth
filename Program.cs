using System;
using System.IO;

namespace CWP
{
    class Program
    {
        static void Main(string[] args)
        {
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\graph.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\494_bus.mtx.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\662_bus.mtx.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\1.txt";
            string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\HB\bcsstk02.mtx.rnd";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\s3dkt3m2.mtx.org.txt";

            if (args.Length > 0)
            {
                ruta_archivo = args[0];
            }

            StreamWriter stream_writer = new StreamWriter(@"C:\Users\LUIS CASANOVA\source\repos\CWP\output1.csv");
            string[] hb_problemas = Directory.GetFiles(@"C:\Users\LUIS CASANOVA\source\repos\CWP\HB");
            stream_writer.WriteLine($"VERTICES;ARISTAS;TIEMPO;CORTE");
            for (int i = 0; i < hb_problemas.Length; i++)
            {
                Console.WriteLine(hb_problemas[i]);
                CWP2 cwp_solver = new CWP2(false);
                cwp_solver.resolver(hb_problemas[i]);

                stream_writer.WriteLine(cwp_solver.ToString());
            }

            stream_writer.Close();

            //CWP2 cwp_solver = new CWP2(false, ';');
            //CWP2 cwp_solver = new CWP2(false, ' ', true);
            //cwp_solver.resolver(ruta_archivo);
            //Console.WriteLine(cwp_solver.ToString());

            //CWPC cwp_solver_c = new CWPC();
            //cwp_solver_c.resolver(ruta_archivo, "SA");

            //CWPD cwpd_solver = new CWPD(1000);
            //cwpd_solver.resolver(ruta_archivo);

            Console.Read();
        }
    }
}
