using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\494_bus.mtx.txt";
            //ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\4.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\HB\662_bus.mtx.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\HB\bcspwr03.mtx.rnd";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\1.txt";
            //string ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\bcsstk02.mtx.rnd";
            //ruta_archivo = @"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\s3dkt3m2.mtx.org.txt";

            if (args.Length > 0)
            {
                ruta_archivo = args[0];
            }

            //CWP.Clases.CW cw_trayectorial = new Clases.CW(ruta_archivo, ' ', false, "");
            //cw_trayectorial.resolver();
            //cw_trayectorial.test_ordenamiento(new int[] { 2,1,0,4,3,5 });
            //Console.WriteLine(cw_trayectorial.ToString());
            //cw_trayectorial.guardar_json(new int[] { 2, 1, 0, 4, 3, 5 });

            //Console.WriteLine("--------------------------------");




            //CWP_Combinacional.CWP cw = new CWP_Combinacional.CWP(ruta_archivo, ' ', true, "", 100, 1, 5000, 0.9f);
            //cw.resolver();

            //cw_trayectorial.guardar_json(cw.get_solucion());

            Process p = new Process();
            const string comando =
                "/C cd ../../../p5 &" +
                "python -m http.server 8000 & " +
                "timeout /t 10 &" +
                "start \"\" \"http://localhost:8000/empty-example/index.html\"";
            Process.Start("cmd.exe", comando);

            Console.Read();
        }
    }
}
