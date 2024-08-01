using System;

namespace CWP
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            CWP cwp_solver = new CWP(@"C:\Users\LUIS CASANOVA\source\repos\CWP\Pruebas\1.txt");
            cwp_solver.resolver();

            /*ArbolIncidencia arbol = new ArbolIncidencia();
            arbol.agregarNodo(4, 12);
            arbol.agregarNodo(5, 4);
            arbol.agregarNodo(2, 6);
            arbol.agregarNodo(2, 8);
            arbol.agregarNodo(8, 24);

            arbol.RecorridoInorden(arbol.raiz);

            arbol.rotarArbol();
            Console.WriteLine();
            arbol.RecorridoInorden(arbol.raiz);*/
            Console.Read();
        }
    }
}
