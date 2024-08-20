using System;
using System.Collections.Generic;
using System.IO;

namespace CWP
{
    class Program
    {
        static void Main(string[] args)
        {

            
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\3.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\494_bus.mtx.txt";
            string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\HB\662_bus.mtx.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\HB\bcspwr03.mtx.rnd";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\1.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\bcsstk02.mtx.rnd";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\s3dkt3m2.mtx.org.txt";

            if (args.Length > 0)
            {
                ruta_archivo = args[0];
            }

            CWPC cwp_solver = new CWPC();
            cwp_solver.resolver(ruta_archivo, "");
            Console.WriteLine(cwp_solver.ToString());

            //CWP4 cwp_solver = new CWP4(false, ' ');
            //cwp_solver.segmentar(ruta_archivo);
            //Console.WriteLine(cwp_solver.ToString());

            //CWP3 cwp_solver3 = new CWP3(!false, ' ');
            //cwp_solver3.segmentar(ruta_archivo, true, 2, 1000);
            //Console.WriteLine(cwp_solver3.ToString());

            //CWP2 cwp_solver2 = new CWP2(!false, ' ');
            //cwp_solver2.parsear_problema(ruta_archivo);
            ////cwp_solver2.resolver(ruta_archivo);
            ////Console.WriteLine(cwp_solver2.ToString());
            //cwp_solver2.testOrdenamiento(cwp_solver3.ORDENAMIENTO);


            //Subgrafo sg = new Subgrafo(cwp_solver2.get_MA(), cwp_solver2.get_Vertices(), 0, true);
            //sg.resolver();

            //for (int i = 10; i < 11; i++)
            //{
            //    StreamWriter stream_writer = new StreamWriter(@$"C:\Users\LUIS CASANOVA\source\repos\CWP\output{i}.csv");
            //    string[] hb_problemas = Directory.GetFiles(@"C:\Users\LUIS CASANOVA\source\repos\CWP\HB");
            //    stream_writer.WriteLine($"VERTICES;ARISTAS;TIEMPO;CORTE");
            //    for (int j = 0; j < hb_problemas.Length; j++)
            //    {
            //        Console.WriteLine(hb_problemas[j]);
            //        CWP3 cwp_solver = new CWP3(true, ' ');
            //        cwp_solver.segmentar(hb_problemas[j], 4);

            //        stream_writer.WriteLine(cwp_solver.ToString());
            //    }

            //    stream_writer.Close();
            //}

            Console.Read();
        }
    }
}
