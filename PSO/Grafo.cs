using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP.PSO
{
    public class PGrafo
    {
        //VARIABLES DE CONTROL
        public int NUMERO_VERTICES { get; set; }
        public int NUMERO_ARISTAS { get; set; }

        //MATRIZ DE ADYACENCIA (CONTIENE EL PESO DE LAS CONEXIONES 
        //ENTRE VÉRTICES)
        public Dictionary<int, Dictionary<int, double>> MATRIZ_ADYACENCIA { get; set; }

        //ARREGLO QUE CONTIENE TODOS LOS VÉRTICES DEL PROBLEMA
        public Vertice[] VERTICES { get; set; }
        public int[] ORDENAMIENTO { get; set; }

        public PGrafo(int numero_vertices, int numero_aristas)
        {
            //Inicializar las variables para la resolución
            NUMERO_VERTICES = numero_vertices;
            NUMERO_ARISTAS = numero_aristas;

            MATRIZ_ADYACENCIA = new Dictionary<int, Dictionary<int, double>>();
            VERTICES = new Vertice[NUMERO_VERTICES];
            ORDENAMIENTO = new int[NUMERO_VERTICES];

            //Inicializar los objetos para la resolución del problema
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                MATRIZ_ADYACENCIA[i] = new Dictionary<int, double>();
                VERTICES[i] = new Vertice(i);
            }
        }
        public void aumentar_adyacencia(int u, int v, double peso)
        {
            //Si los índices son diferentes, incrementar el grado y peso 
            //en ambos vértices. Caso contrario, incrementarlo solo en uno para evitar duplicar
            //los datos mencionados.
            if (u != v)
            {
                VERTICES[u].incrementar_adyacencia(peso);
                VERTICES[v].incrementar_adyacencia(peso);

                MATRIZ_ADYACENCIA[u][v] = peso;
                MATRIZ_ADYACENCIA[v][u] = peso;
            }
            else
            {
                VERTICES[u].incrementar_adyacencia(peso);

                MATRIZ_ADYACENCIA[u][v] = peso;
            }
        }
    }
}
