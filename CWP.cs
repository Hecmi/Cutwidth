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
        //LISTA DE ADYACENCIA ESTÁTICA
        int [] VERTICES_ADYACENTES;

        //VARIABLES GENERALES
        int NUMERO_VERTICES;
        int NUMERO_ARISTAS;
        int[,] MATRIZ_ADYACENCIA;

        int VERTICES_ORDENADOS;

        Vertice[] vertices;
        int[] ordenamiento;

        public CWP()
        {

        }

        private bool parsear_problema(string ruta_archivo)
        {
            //Crear el stream reader para leer el archivo
            StreamReader sreader = new StreamReader(ruta_archivo);

            //Variables para leer el archivo
            string linea;
            string[] elementos_linea;

            //Leer la primera línea del archivo que contiene los datos generales del
            //grafo a resolver
            if ((linea = sreader.ReadLine()) != null)
            {
                //Obtener el arreglo separado por un caracter vacío
                elementos_linea = linea.Split(' ');
                if (elementos_linea.Length != 3 ||
                  !int.TryParse(elementos_linea[0], out NUMERO_VERTICES) ||
                  !int.TryParse(elementos_linea[2], out NUMERO_ARISTAS))
                {
                    Console.WriteLine("El formato del encabezado del archivo no es válido.");
                    return false;
                }

                ////Configurar las variables para solucionar el problema
                //NUMERO_VERTICES = int.Parse(elementos_linea[0]);
                //NUMERO_ARISTAS = int.Parse(elementos_linea[2]);

                //Inicializar las variables
                MATRIZ_ADYACENCIA = new int[NUMERO_VERTICES, NUMERO_VERTICES];
                vertices = new Vertice[NUMERO_VERTICES];
                ordenamiento = new int[NUMERO_VERTICES];

                //Inicializar los objetos de tipo vértice
                for (int i = 0; i < NUMERO_VERTICES; i++)
                {
                    ordenamiento[i] = i;
                    vertices[i] = new Vertice(i);
                }
            }

            //Recorrer las aristas según la cantidad específicada en el encabezado
            int linea_actual = 0;
            while (linea_actual < NUMERO_ARISTAS)
            {
                linea = sreader.ReadLine();

                if (linea == null)
                {
                    Console.WriteLine($"Error de formato, la línea {linea_actual} no existe, sin embargo el número de arsitas especificado indica lo contrario.");
                    return false;
                }

                //Obtener los elementos de la línea actual
                elementos_linea = linea.Split(' ');

                //Obtener los índices de los vértices para incrementar el grado
                int u, v = -1;
                if (elementos_linea.Length != 2 ||
                  !int.TryParse(elementos_linea[0], out u) ||
                  !int.TryParse(elementos_linea[1], out v))
                {
                    Console.WriteLine($"El formato de la línea {linea_actual} no es correcto.");
                    return false;
                }

                u = u - 1;
                v = v - 1;

                vertices[u].grado += 1;
                vertices[v].grado += 1;

                vertices[u].grado_org += 1;
                vertices[v].grado_org += 1;

                //Incrementar la adyacencia en la matriz
                aumentarAdyacencia(u, v);


                linea_actual++;
            }

            //Cerrar el lector
            sreader.Close();

            //Si no existen vertices en el arreglo, entonces no hay nada que resolver
            if (NUMERO_VERTICES < 1 || NUMERO_ARISTAS < 1)
            {
                Console.WriteLine("El número de vertices y aristas debe ser mayor que 0.");
                return false;
            }

            //Calcular la adyacencia entre los vértices para almacenarla
            procesarAdyacencia();

            //Mostrar la matriz de adyacencia formada
            Console.WriteLine("MATRIZ DE ADYACENCIA");
            mostrarMatriz(MATRIZ_ADYACENCIA);

            return true;
        }

        private void mostrarAdyacencia()
        {
            //Recorrer el vertice seleccionado con la cantidad de grados que tiene
            //la iteración empieza desde el valor guardado
            for (int x = 0; x < NUMERO_VERTICES; x++)
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

            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                for (int j = 0; j < NUMERO_VERTICES; j++)
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
                if (i + 1 < NUMERO_VERTICES) vertices[i + 1].indice_inicio_lista = contador;
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
                    if (!vertices[indice_adyacente].visitado && !adyacentes.Contains(indice_adyacente))
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

        private Vertice[] getVAdyacenciaVertices(int[] vertice)
        {
            List<int> adyacentes = new List<int>();
                     
            //Recorrer los vértices ya ordenados para verificar los vértices que tienen conexión 
            //(adyacencia) con ellos mediante la lista
            for (int u = 0; u < VERTICES_ORDENADOS; u++)
            {
                int indice = vertice[u];
                for (int i = 0; i < vertices[indice].grado_org; i++)
                {
                    int indice_inicio = vertices[indice].indice_inicio_lista;
                    int indice_adyacente = VERTICES_ADYACENTES[indice_inicio + i];
                    if (!vertices[indice_adyacente].visitado && !adyacentes.Contains(indice_adyacente))
                    {
                        adyacentes.Add(indice_adyacente);
                    }

                    //Incrementar el valor de vertices adyacentes
                    vertices[indice_adyacente].offset += 1;
                }
            }

            //Usar la lista de índices de vértices para crear el arreglo de vértices adyacentes
            Vertice[] vertices_adyacentes = new Vertice[adyacentes.Count];

            //Recalcular el grado considerando el offset
            for (int j = 0; j < adyacentes.Count; j++)
            {
                int indice_u_adyacente = adyacentes[j];
                vertices_adyacentes[j] = this.vertices[indice_u_adyacente];

                vertices[indice_u_adyacente].grado = vertices[indice_u_adyacente].grado_org - vertices[indice_u_adyacente].offset;
                vertices[indice_u_adyacente].offset = 0;
            }

            return vertices_adyacentes;
        }

        private void etiquetar(int u)
        {
            //Agregar el vertice al ordenamiento, colocar el índice en el objeto
            //e incrementar la cantidad de vértices ordenados
            ordenamiento[VERTICES_ORDENADOS] = u;
            vertices[u].etiquetar(VERTICES_ORDENADOS);

            VERTICES_ORDENADOS++;

            //Console.WriteLine(vertices[u].vertice + " -> " + vertices[u].grado);
        }

        private Vertice seleccionarVertice(Vertice[] vertices)
        {
            Vertice[] verticesCandidatos = ordenarVertices(vertices, true);
            return verticesCandidatos.Length > 0 ? verticesCandidatos[0] : null;
        }

        private void resVerticesNOCon()
        {
            //Repetir el proceso de selección y ordenamiento mientras existan vertices
            //que no sean conexos
            Vertice[] verticesOrdenados = ordenarVertices(vertices, true);
            while (verticesOrdenados[VERTICES_ORDENADOS].grado_org == 0)
            {
                this.etiquetar(verticesOrdenados[VERTICES_ORDENADOS].vertice);
            }
        }

        public void resolver(string ruta_problema)
        {
            //Parsear el archivo para obtener la configuración del problema (vértices y aristas)
            bool parseado = parsear_problema(ruta_problema);
            if (!parseado) return;

            VERTICES_ORDENADOS = 0;

            //Repetir el proceso de selección y ordenamiento mientras existan vertices
            //que no sean conexos
            //resVerticesNOCon();

            //Iniciar seleccionando un vértice que tiene un grado bajo
            Vertice u = seleccionarVertice(vertices);
            this.etiquetar(u.vertice);

            //Recorrer la cantidad de vértices restantes menos uno (n - 1) puesto que
            //se definió anteriormente
            while (VERTICES_ORDENADOS < NUMERO_VERTICES)
            {
                //Buscar los vértices adyacentes a los ya visitados
                Vertice[] vertices_adyacentes = getVAdyacenciaVertices(ordenamiento);
                
                //Seleccionar el vértice de menor grado y etiquetarlo (agregarlo al ordenamiento)
                Vertice v = seleccionarVertice(vertices_adyacentes);
                this.etiquetar(v.vertice);
            }

            //Calcular el ancho de corte de cada posible partición
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

            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                //Índice de inicio en el vector
                int indiceInicio = vertices[u].indice_inicio_lista;

                //Cantidad de elementos a partir del índice de inicio
                int grado = vertices[u].grado_org;

                for (int i = 0; i < grado; i++)
                {
                    int v = VERTICES_ADYACENTES[indiceInicio + i];

                    //Ya que el grafo es no dirigido las aristas son dobles, la siguiente
                    //condición evita realizar revisiones dobles
                    if (v > u && cruzaParticion(u, v, particion))
                    {
                        //Sí los vértice se encuentran en lados diferentes de la partición
                        //entonces se incrementa el número de cortes                        
                        corte++;                        
                    }
                }
            }

            return corte;        
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

