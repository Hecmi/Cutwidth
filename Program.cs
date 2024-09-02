using System;
using System.Collections.Generic;
using System.IO;
using CWP.Clases;
using CWP.CWP_Combinacional;

namespace CWP
{
    class Program
    {
        static void Main(string[] args)
        {

            
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\3.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\494_bus.mtx.txt";
            //ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\4.txt";
            string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\HB\662_bus.mtx.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\HB\bcspwr03.mtx.rnd";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\1.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\bcsstk02.mtx.rnd";
            //ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\s3dkt3m2.mtx.org.txt";

            if (args.Length > 0)
            {
                ruta_archivo = args[0];
            }

            CWP.Clases.CW cw_trayectorial = new Clases.CW(ruta_archivo, ' ', true, "");
            cw_trayectorial.resolver();
            Console.WriteLine(cw_trayectorial.ToString());

            StreamWriter sw = new StreamWriter(@"C:\Users\LUIS CASANOVA\source\repos\CWP\prueba.json");
            sw.WriteLine(cw_trayectorial.formar_json());
            sw.Close();

            Console.WriteLine("--------------------------------");
            
            CWP_Combinacional.CWP cw = new CWP_Combinacional.CWP(ruta_archivo, ' ', false, "", 100, 1, 100000);
            cw.resolver();

            Console.Read();
        }
    }
}
