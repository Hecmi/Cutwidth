using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    class CWP
    {
        int NUMERO_LITERALES;
        int NUMERO_ARISTAS;
        int[,] MATRIZ_ADYACENCIA;

        int VERTICES_ORDENADOS;

        Vertice[] vertices;
        int[] ordenamiento;
        public CWP(string ruta_archivo)
        {
            parsear_problema(ruta_archivo);            
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
                        vertices[i] = new Vertice(i);
                    }
                }
                else
                {
                    //Obtener los índices de los vértices para incrementar el grado
                    int x1, y1;
                    x1 = int.Parse(elementos_linea[0]) - 1;
                    y1 = int.Parse(elementos_linea[1]) - 1;

                    vertices[x1].grado += 1;
                    vertices[y1].grado += 1;

                    vertices[x1].grado_org += 1;
                    vertices[y1].grado_org += 1;

                    //Incrementar la adyacencia en la matriz
                    aumentarAdyacencia(x1, y1);
                }

                numero_lineas++;
            }

            //Cerrar el lector
            sreader.Close();
            mostrarMatriz(MATRIZ_ADYACENCIA);
        }

        private void aumentarAdyacencia(int x1, int y1)
        {
            MATRIZ_ADYACENCIA[x1, y1]++;
            MATRIZ_ADYACENCIA[y1, x1]++;
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

        private int[] getAdyacenciaVertice(int vertice)
        {
            int grado_vertice = vertices[vertice].grado;
            int[] adyacentes = new int[grado_vertice];

            int indice = 0;

            for (int j = 0; j < NUMERO_LITERALES; j++)
            {
                //Verificar que no haya conexión y  que no haya sido visitado
                if (MATRIZ_ADYACENCIA[vertice, j] == 1 && vertices[j].visitado == false)
                {
                    adyacentes[indice] = j;
                    indice++;
                }
            }

            return adyacentes;
        }

        private int[] getAdyacenciaVertices(int [] vertice)
        {        
            List<int> adyacentes = new List<int>();
          
            //Recorrer los vértices ya ordenados para verificar los vértices que tienen conexión 
            //(adyacencia) con ellos
            for (int i = 0; i < VERTICES_ORDENADOS; i++)
            {
                for (int j = 0; j < NUMERO_LITERALES; j++)
                {
                    //Verificar que no haya conexión y  que no haya sido visitado
                    if (MATRIZ_ADYACENCIA[vertice[i], j] == 1 && vertices[j].visitado == false)
                    {
                        if (!adyacentes.Contains(j))
                        {                        
                            adyacentes.Add(j);
                        }

                        //Incrementar el valor de vertices adyacentes
                        vertices[j].offset += 1;
                    }
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
            ordenamiento[VERTICES_ORDENADOS] = u;
            vertices[u].etiquetar(VERTICES_ORDENADOS);

            VERTICES_ORDENADOS++;
        }

        public void resolver()
        {
            VERTICES_ORDENADOS = 0;

            //Iniciar seleccionando un vértice que tiene un grado bajo
            Vertice[] verticesOpc = ordenarVertices((Vertice[])vertices.Clone(), true);
            this.etiquetar(verticesOpc[0].vertice);

            Console.WriteLine((char)('A' + verticesOpc[0].vertice) + " -> " + verticesOpc[0].grado);

            //Recorrer la cantidad de vértices restantes menos uno (n - 1)
            for (int i = 0; i < NUMERO_LITERALES - 1; i++)
            {                

                //Recorrer los vértices que ya han sido establecidos en el ordenamiento                
                int[] adyacentes = getAdyacenciaVertices(ordenamiento);
                List<Vertice> lstVertices = new List<Vertice>();

                //Guardar la lista de vértices que tienen adyacencia con los previamente seleccionados
                for (int k = 0; k < adyacentes.Length; k++)
                {
                    lstVertices.Add(vertices[adyacentes[k]]);
                    //Console.WriteLine($"ADYACENTES {adyacentes[k]}, GRADO = {vertices[adyacentes[k]].grado}");
                }
               
                
                //Transformar la lista en un arreglo para ordenarlo descendentemente 
                Vertice[] array = lstVertices.ToArray();
                Vertice[] opcionesDesc = ordenarVertices(array, false);

                //Obtener el vértice que presente un grado mayor y colocarlo como visitado 
                this.etiquetar(opcionesDesc[0].vertice);

                Console.WriteLine((char)('A' + opcionesDesc[0].vertice) + " -> " + opcionesDesc[0].grado);       
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
            int corte = 0;
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
                        }
                    }
                }
            }

            return corte;
        }

        private void calcularAnchoCorte()
        {
            int corteMaximo = 0;
            int indiceCorte = 0;
            for (int i = 0; i < VERTICES_ORDENADOS; i++)
            {
                int corteActual = contarCortes(i);

                if (i + 1 < VERTICES_ORDENADOS)
                    vertices[ordenamiento[i + 1]].ancho_corte = corteActual;

                if (corteActual > corteMaximo)
                {
                    corteMaximo = corteActual;
                    indiceCorte = i;
                }
            }
            
            mostrarResultado();
            Console.WriteLine($"CORTE MÁXIMO = {corteMaximo} EN {(char)('A' + ordenamiento[indiceCorte])}, {(char)('A' + ordenamiento[indiceCorte + 1])}");
        }

        public Vertice[] ordenarVertices(Vertice[] array, bool asc)
        {
            Vertice[] arr = new Vertice[array.Length];
            arr = array;
            QuickSort(arr, asc, 0, array.Length - 1);
                        
            return array;
        }

        public void mostrarVertices()
        {
            for (int x = 0; x < vertices.Length; x++)
            {
                Console.WriteLine($"Vertice = {vertices[x].vertice}, Grado = {vertices[x].grado}");
            }
        }

        public void mostrarOrds()
        {
            for (int x = 0; x < VERTICES_ORDENADOS; x++)
            {
                Console.WriteLine($"Vertice ORDENAMIENTO = {x},{VERTICES_ORDENADOS},  {ordenamiento[x]}, CORTE {vertices[ordenamiento[x]].ancho_corte}");
            }   
        }

        private void mostrarResultado()
        {
            for (int x = 0; x < VERTICES_ORDENADOS; x++)
            {
                if (x + 1 < VERTICES_ORDENADOS)
                {
                    Console.WriteLine($"{(char)('A' + ordenamiento[x])}, {(char)('A' + ordenamiento[x + 1])}, CORTE: {vertices[ordenamiento[x + 1]].ancho_corte}");
                }
            }

        }

        //FUNCIONES PARA ORDERNAR LOS VECTORES
        static void QuickSort(Vertice[] array, bool asc, int indice_inicio, int indice_final)
        {
            if (indice_inicio < indice_final)
            {
                int particion = Particionar(array, asc, indice_inicio, indice_final);
                QuickSort(array, asc, indice_inicio, particion - 1);
                QuickSort(array, asc, particion + 1, indice_final);
            }
        }

        static int Particionar(Vertice[] array, bool asc, int indice_inicio, int indice_final)
        {
            int pivot = array[indice_final].grado;
            int i = (indice_inicio - 1);

            if (asc)
            {
                for (int j = indice_inicio; j < indice_final; j++)
                {
                    if (array[j].grado < pivot)
                    {
                        i++;
                        intercambiar(array, i, j);
                    }
                }
            }
            else
            {
                for (int j = indice_inicio; j < indice_final; j++)
                {
                    if (array[j].grado > pivot)
                    {
                        i++;
                        intercambiar(array, i, j);
                    }
                }
            }

            intercambiar(array, i + 1, indice_final);
            return (i + 1);
        }

        static void intercambiar(Vertice[] array, int i, int j)
        {
            Vertice temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}

