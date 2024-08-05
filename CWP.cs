﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    class CWP
    {
        //LISTA DE ADYACENCIA ESTÁTICA
        int [] VERTICES_ADYACENTES;

        //VARIABLES GENERALES
        int NUMERO_LITERALES;
        int NUMERO_ARISTAS;
        int[,] MATRIZ_ADYACENCIA;

        int VERTICES_ORDENADOS;

        Vertice[] vertices;
        int[] ordenamiento;


        int indice_candidato;

        public CWP(string ruta_archivo)
        {
            this.parsear_problema(ruta_archivo);            
        }

        private void parsear_problema(string ruta_archivo)
        {
            //Crear el stream reader para leer el archivo
            StreamReader sreader = new StreamReader(ruta_archivo);

            //Variables para leer el archivo
            string linea;
            string[] elementos_linea;

            int numero_lineas = 0;

            //Leer el archivo
            while ((linea = sreader.ReadLine()) != null)
            {
                elementos_linea = linea.Split(' ');

                //Sí es la primera línea, es el encabezado
                if (numero_lineas == 0)
                {
                    NUMERO_LITERALES = int.Parse(elementos_linea[0]);
                    NUMERO_ARISTAS = int.Parse(elementos_linea[2]);

                    //Inicializar las variables
                    MATRIZ_ADYACENCIA = new int[NUMERO_LITERALES, NUMERO_LITERALES];
                    vertices = new Vertice[NUMERO_LITERALES];
                    ordenamiento = new int[NUMERO_LITERALES];

                    //Inicializar los objetos de tipo vértice
                    for (int i = 0; i < NUMERO_LITERALES; i++)
                    {
                        ordenamiento[i] = i;
                        vertices[i] = new Vertice(i);
                    }
                }
                else
                {
                    //Obtener los índices de los vértices para incrementar el grado                   
                    int u = int.Parse(elementos_linea[0]) - 1;
                    int v = int.Parse(elementos_linea[1]) - 1;

                    vertices[u].grado += 1;
                    vertices[v].grado += 1;

                    vertices[u].grado_org += 1;
                    vertices[v].grado_org += 1;

                    //Incrementar la adyacencia en la matriz
                    aumentarAdyacencia(u, v);
                }

                numero_lineas++;
            }

            //Cerrar el lector
            sreader.Close();

            //Calcular la adyacencia entre los vértices para almacenarla
            procesarAdyacencia();

            //Mostrar la matriz de adyacencia formada
            Console.WriteLine("MATRIZ DE ADYACENCIA");
            mostrarMatriz(MATRIZ_ADYACENCIA);
            Console.WriteLine("");

            //mostrarAdyacencia();
            //Console.WriteLine();
            //mostrarAdyacencia(9);
        }

        private void mostrarAdyacencia()
        {
            //Recorrer el vertice seleccionado con la cantidad de grados que tiene
            //la iteración empieza desde el valor guardado
            for (int x = 0; x < NUMERO_LITERALES; x++)
            {
                for (int i = 0; i < vertices[x].grado_org; i++)
                {
                    Console.WriteLine($"PARA VÉRTICE {vertices[x].vertice}, {VERTICES_ADYACENTES[i + vertices[x].indice_inicio_lista]}");
                }
            }
        }
        private void mostrarAdyacencia(int u)
        {
            //Recorrer el vertice seleccionado con la cantidad de grados que tiene
            //la iteración empieza desde el valor guardado
            for (int i = 0; i < vertices[u].grado_org; i++)
            {
                Console.WriteLine($"PARA VÉRTICE {vertices[u].vertice}, {VERTICES_ADYACENTES[i + vertices[u].indice_inicio_lista]}");
            }
        }
        private void procesarAdyacencia()
        {
            //Definir un contador para insertar en esa posición la conexón acertada entre vértices
            int contador = 0;

            //Crear el vector con el total de conexiones * 2 debido a que es un grafo no dirigido
            VERTICES_ADYACENTES = new int[NUMERO_ARISTAS + NUMERO_ARISTAS];

            for (int i = 0; i < NUMERO_LITERALES; i++)
            {
                for (int j = 0; j < NUMERO_LITERALES; j++)
                {
                    //Verificar si existe conexión entre los vértices
                    if (MATRIZ_ADYACENCIA[vertices[i].vertice, vertices[j].vertice] == 1)
                    {
                        //Si existe conexión colocarla en el índice incremental (contador)
                        VERTICES_ADYACENTES[contador] = vertices[j].vertice;
                        contador++;
                    }
                }

                //Colocar el valor al índice de inicio en la lista de adyacencia
                if (i + 1 < NUMERO_LITERALES) vertices[i + 1].indice_inicio_lista = contador;
            }
        }

        private void aumentarAdyacencia(int u, int v)
        {
            //Incrementar la adyacencia en la matriz según los 
            //vertices especificados
            MATRIZ_ADYACENCIA[u, v]++;
            MATRIZ_ADYACENCIA[v, u]++;
        }

        private void mostrarMatriz(int [,] matriz)
        {
            for (int i = 0; i <= matriz.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= matriz.GetUpperBound(1); j++)
                {
                    Console.Write(matriz[i, j] + "\t");
                }
                Console.Write("\n");
            }
        }

        private int[] getAdyacenciaVertices(int[] vertice)
        {
            indice_candidato = -1;
            List<int> adyacentes = new List<int>();

            //for (int u = 0; u < VERTICES_ORDENADOS; u++)
            //{
            //    Console.WriteLine("ENBUSQUEDA DE: " + vertice[u]);
            //}

            //Recorrer los vértices ya ordenados para verificar los vértices que tienen conexión 
            //(adyacencia) con ellos mediante la lista
            for (int u = 0; u < VERTICES_ORDENADOS; u++)
            {
                int indice = vertice[u];
                for (int i = 0; i < vertices[indice].grado_org; i++)
                {
                    int indice_inicio = vertices[indice].indice_inicio_lista;
                    int indice_adyacente = VERTICES_ADYACENTES[indice_inicio + i];
                    if (!adyacentes.Contains(indice_adyacente) && !vertices[indice_adyacente].visitado)
                    {
                        adyacentes.Add(indice_adyacente);
                    }

                    //Incrementar el valor de vertices adyacentes
                    vertices[indice_adyacente].offset += 1;
                }
            }

            //Recalcular el grado considerando el offset
            for (int j = 0; j < adyacentes.Count; j++)
            {
                vertices[adyacentes[j]].grado = vertices[adyacentes[j]].grado_org - vertices[adyacentes[j]].offset;
                vertices[adyacentes[j]].offset = 0;
            }

            return adyacentes.ToArray();
        }

        private void etiquetar(int u)
        {
            //Agregar el vertice al ordenamiento, colocar el índice en el objeto
            //e incrementar la cantidad de vértices ordenados
            ordenamiento[VERTICES_ORDENADOS] = u;
            vertices[u].etiquetar(VERTICES_ORDENADOS);

            VERTICES_ORDENADOS++;

            Console.WriteLine(vertices[u].vertice + " -> " + vertices[u].grado);
        }

        private Vertice seleccionarVertice(Vertice[] vertices)
        {
            Vertice[] verticesCandidatos = ordenarVertices(vertices, true);
            Vertice verticeSeleccionado = verticesCandidatos[0];
            return verticeSeleccionado;
        }

        public void resolver()
        {
            if (vertices.Length < 1) return;

            VERTICES_ORDENADOS = 0;

            //Iniciar seleccionando un vértice que tiene un grado bajo
            Vertice u = seleccionarVertice(vertices);
            this.etiquetar(u.vertice);

            //Recorrer la cantidad de vértices restantes menos uno (n - 1) puesto que
            //se definió anteriormente
            while (VERTICES_ORDENADOS < NUMERO_LITERALES)
            {
                //Recorrer los vértices que ya han sido establecidos en el ordenamiento                
                int[] adyacentes = getAdyacenciaVertices(ordenamiento);
                
                //Declarar una variable para guardar los vértices asociados al índice
                //de los adyacentes
                List<Vertice> lstVertices = new List<Vertice>();

                //Guardar la lista de vértices que tienen adyacencia con los previamente seleccionados
                for (int k = 0; k < adyacentes.Length; k++)
                {
                    lstVertices.Add(vertices[adyacentes[k]]);
                }

                //Transformar la lista en un arreglo
                Vertice[] vArray = lstVertices.ToArray();
                Vertice v = seleccionarVertice(vArray);
                this.etiquetar(v.vertice);
            }

            calcularAnchoCorte();
        }

        private bool cruzaParticion(int u, int v, int particion)
        {           
            //Verificar si u y v están en diferentes conjuntos respecto a la partición
            return (vertices[u].indice <= particion && vertices[v].indice > particion) 
                || (vertices[v].indice <= particion && vertices[u].indice > particion);
        }

        private int contarCortes(int particion)
        {
            ///Variable para guardar la cantidad de cortes por partición
            int corte = 0;

            for (int u = 0; u < NUMERO_LITERALES; u++)
            {
                //Índice de inicio en el vector
                int indiceInicio = vertices[u].indice_inicio_lista;

                //Cantidad de elementos a partir del índice de inicio
                int grado = vertices[u].grado_org;

                for (int i = 0; i < grado; i++)
                {
                    int v = VERTICES_ADYACENTES[indiceInicio + i];

                    //Ya que el grafo es no dirigido las aristas son dobles, la siguiente
                    //condición evita revisiones duplicadas
                    if (v > u)
                    {
                        //Sí los vértice se encuentran en lados diferentes de la partición
                        //entonces se incrementa el número de cortes
                        if (cruzaParticion(u, v, particion))
                        {
                            corte++;
                        }
                    }
                }
            }

            return corte;

            /*
             * int corte = 0;
            for (int j = 0; j < NUMERO_LITERALES; j++)
            {
                for (int k = j + 1; k < NUMERO_LITERALES; k++)
                {
                    if (MATRIZ_ADYACENCIA[j, k] > 0)
                    {
                        //Verificar si la arista cruza la partición
                        if (cruzaParticion(j, k, particion))
                        {
                            corte++;
                            //Console.WriteLine($"corte en {j + 1}-{k + 1} en partición {particion}");
                        }
                    }
                }
            }
            return corte;
            */
        }

        private void calcularAnchoCorte()
        {
            //Variables para guardar los resultados
            int corteMaximo = 0;
            int indiceCorte = 0;

            //Recorrer el ordenamiento para calcular los cortes entre pares de vértices
            for (int i = 0; i < VERTICES_ORDENADOS; i++)
            {
                //Calcular el ancho de corte entre vértices
                int corteActual = contarCortes(i);

                //Guardar el ancho de corte en el vértice u + 1 para saber que es la conexión
                //entre el y el anterior (u)
                if (i + 1 < VERTICES_ORDENADOS)
                {
                    vertices[ordenamiento[i + 1]].ancho_corte = corteActual;
                }

                //Obtener el corte máximo y guardarlo en las variables
                if (corteActual > corteMaximo)
                {
                    corteMaximo = corteActual;
                    indiceCorte = i;
                }
            }
            
            mostrarResultado();
            Console.WriteLine($"CORTE MÁXIMO = {corteMaximo} EN {(char)('A' + ordenamiento[indiceCorte])}, {(char)('A' + ordenamiento[indiceCorte + 1])}");
        }

        //FUNCIONES PARA ORDERNAR LOS VECTORES
        public Vertice[] ordenarVertices(Vertice[] array, bool asc)
        {
            Vertice[] arr = (Vertice[])array.Clone();
            MergeSort(arr, 0, arr.Length - 1, asc);
            return arr;
        }
        
        static void MergeSort(Vertice[] array, int izq, int der, bool asc)
        {
            //Verificar si la lista tiene más de un elemento
            if (izq < der)
            {
                //Calcular el índice medio del arreglo
                int medio = (izq + der) / 2;

                //Ordenar la primera mitad del arreglo (izq a centro)
                MergeSort(array, izq, medio, asc);

                //Ordenar la segunda mitad del arreglo (centro a derecha)
                MergeSort(array, medio + 1, der, asc);

                //Fusionar las dos mitades ordenadas
                Merge(array, izq, medio, der, asc);
            }
        }

        static void Merge(Vertice[] array, int izq, int medio, int der, bool asc)
        {
            //Calcular el tamaño de las dos subarreglos
            int tamanio_izquierdo = medio - izq + 1;
            int tamanio_derecha = der - medio;

            //Crear arreglos temporales para almacenar las dos mitades
            Vertice[] izqArray = new Vertice[tamanio_izquierdo];
            Vertice[] derArray = new Vertice[tamanio_derecha];

            //Copiar los datos a los arreglos temporales
            Array.Copy(array, izq, izqArray, 0, tamanio_izquierdo);
            Array.Copy(array, medio + 1, derArray, 0, tamanio_derecha);

            //Índices para iterar sobre los arreglos temporales
            int i = 0;
            int j = 0;

            //Índice para iterar sobre el arreglo original
            int k = izq;

            //Fusionar los arreglos temporales en el arreglo original
            while (i < tamanio_izquierdo && j < tamanio_derecha)
            {
                //Comparar los elementos de las dos mitades según el orden ascendente o descendente
                bool condicion = asc ? izqArray[i].grado <= derArray[j].grado : izqArray[i].grado >= derArray[j].grado;
                if (condicion)
                {                    
                    array[k] = izqArray[i];
                    i++;
                }
                else
                {                 
                    array[k] = derArray[j];
                    j++;
                }

                //Avanzar al siguiente índice en el arreglo original
                k++;
            }

            //Copiar los elementos restantes de la primera mitad (si los hay) al arreglo original
            while (i < tamanio_izquierdo)
            {
                array[k] = izqArray[i];
                i++;
                k++;
            }

            //Copiar los elementos restantes de la segunda mitad (si los hay) al arreglo original
            while (j < tamanio_derecha)
            {
                array[k] = derArray[j];
                j++;
                k++;
            }
        }


        private void mostrarResultado()
        {
            for (int x = 0; x < VERTICES_ORDENADOS; x++)
            {
                if (x + 1 < VERTICES_ORDENADOS)
                {
                    //Console.WriteLine($"{(char)('A' + ordenamiento[x])}, {(char)('A' + ordenamiento[x + 1])}, CORTE: {vertices[ordenamiento[x + 1]].ancho_corte}");
                    Console.WriteLine($"{ordenamiento[x]},  {ordenamiento[x + 1]}, CORTE: {vertices[ordenamiento[x + 1]].ancho_corte}");
                }
            }

        }
    }
}
