using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    public class ArbolIncidencia
    {
        public Nodo raiz;
        int max_altura_izq;
        int max_altura_derecha;

        public void agregarNodo(int vertice, int grado)
        {
            Nodo nuevoNodo = new Nodo(vertice, grado);

            bool insertado = false;            

            if (raiz == null)
            {
                raiz = nuevoNodo;
            }
            else
            {
                Nodo p = raiz; //Movimiento
                Nodo q = null; //Anterior a p

                bool derecha = (nuevoNodo.grado >= p.grado);

                while (!insertado)
                {
                    q = p;

                    if (nuevoNodo.grado >= p.grado)
                    {
                        p = p.derecha;

                        if (p == null)
                        {
                            nuevoNodo.altura = q.altura + 1;

                            if (derecha) max_altura_derecha = nuevoNodo.altura;
                            else max_altura_izq = nuevoNodo.altura;

                            q.derecha = nuevoNodo;
                            insertado = true;
                        }
                    }
                    else
                    {
                        p = p.izq;

                        if (p == null)
                        {
                            nuevoNodo.altura = q.altura + 1;

                            if (derecha) max_altura_derecha = nuevoNodo.altura;
                            else max_altura_izq = nuevoNodo.altura;

                            q.izq = nuevoNodo;
                            insertado = true;
                        }
                    }
                }
            }
        }

        public void RecorridoInorden(Nodo raiz)
        {
            if (raiz != null)
            {
                RecorridoInorden(raiz.izq);
                Console.WriteLine($"Grado: {raiz.grado}, Altura: {raiz.altura}");
                RecorridoInorden(raiz.derecha);
            }
        }

        public void rotarArbol()
        {
            int balance = max_altura_derecha - max_altura_izq;
            Console.WriteLine("BALANCE = " + balance);
            Console.WriteLine($"max_altura_derecha: {max_altura_derecha}, max_altura_izq: {max_altura_izq}");

            if (balance == 1 || balance == -1)
            {
                return;
            }

            if (balance < 0)
            {
                balance = -balance;
                for (int i = 0; i < balance; i++)
                {
                    raiz = Rotacionizq(raiz);
                }
            }
            else
            {
                for (int i = 0; i < balance; i++)
                {
                    raiz = RotacionDerecha(raiz);
                }
            }            
        }

        public Nodo RotacionDerecha(Nodo nodo)
        {
            if (nodo == null || nodo.derecha == null)
            {
                return nodo;
            }

            Nodo nueva_raiz = nodo.derecha;
            nodo.derecha = nueva_raiz.izq;
            nueva_raiz.izq = nodo;

            return nueva_raiz;
        }

        public Nodo Rotacionizq(Nodo nodo)
        {
            if (nodo == null || nodo.izq == null)
            {
                return nodo;
            }

            Nodo nueva_raiz = nodo.izq;
            //nodo.izq = null;
            //nueva_raiz.derecha = nodo;
            //nodo.izq = nueva_raiz.derecha;
            //nueva_raiz.derecha = nodo;

            return nueva_raiz;
        }
    }

    public class Nodo
    {
        public Nodo derecha;
        public Nodo izq;
        public int vertice;
        public int grado;
        public int altura;

        public Nodo(int vertice, int grado)
        {
            this.vertice = vertice;
            this.grado = grado;
            this.derecha = null;
            this.izq = null;
            this.altura = 1;
        }
    }
}
