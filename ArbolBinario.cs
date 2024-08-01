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
        int max_altura_izquierda;
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
                            else max_altura_izquierda = nuevoNodo.altura;

                            q.derecha = nuevoNodo;
                            insertado = true;
                        }
                    }
                    else
                    {
                        p = p.izquierda;

                        if (p == null)
                        {
                            nuevoNodo.altura = q.altura + 1;

                            if (derecha) max_altura_derecha = nuevoNodo.altura;
                            else max_altura_izquierda = nuevoNodo.altura;

                            q.izquierda = nuevoNodo;
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
                RecorridoInorden(raiz.izquierda);
                Console.WriteLine($"Grado: {raiz.grado}, Altura: {raiz.altura}");
                RecorridoInorden(raiz.derecha);
            }
        }

        public void rotarArbol()
        {
            int balance = max_altura_derecha - max_altura_izquierda;
            Console.WriteLine("BALANCE = " + balance);
            Console.WriteLine($"max_altura_derecha: {max_altura_derecha}, max_altura_izquierda: {max_altura_izquierda}");

            if (balance == 1 || balance == -1)
            {
                return;
            }

            if (balance < 0)
            {
                balance = -balance;
                for (int i = 0; i < balance; i++)
                {
                    raiz = RotacionIzquierda(raiz);
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
            nodo.derecha = nueva_raiz.izquierda;
            nueva_raiz.izquierda = nodo;

            return nueva_raiz;
        }

        public Nodo RotacionIzquierda(Nodo nodo)
        {
            if (nodo == null || nodo.izquierda == null)
            {
                return nodo;
            }

            Nodo nueva_raiz = nodo.izquierda;
            //nodo.izquierda = null;
            //nueva_raiz.derecha = nodo;
            //nodo.izquierda = nueva_raiz.derecha;
            //nueva_raiz.derecha = nodo;

            return nueva_raiz;
        }
    }

    public class Nodo
    {
        public Nodo derecha;
        public Nodo izquierda;
        public int vertice;
        public int grado;
        public int altura;

        public Nodo(int vertice, int grado)
        {
            this.vertice = vertice;
            this.grado = grado;
            this.derecha = null;
            this.izquierda = null;
            this.altura = 1;
        }
    }
}
