﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    class CWP
    {
        int[,] MATRIZ_ADYACENCIA;

        //ARREGLO QUE CONTIENE LA ADYACENCIA DE CADA VÉRTICE
        //SE FORMA A PARTIR DE LA MATRIZ DE ADYACENCIA
        int [] VERTICES_ADYACENTES;

        //VARIABLES DE CONTROL
        int NUMERO_VERTICES;
        int NUMERO_ARISTAS;
        int VERTICES_ORDENADOS;

        //ARREGLO QUE CONTIENE TODOS LOS VÉRTICES DEL PROBLEMA
        Vertice[] VERTICES;

        //ORDENAMIENTO DE SALIDA (SOLO CONTIENE LOS ÍNDICES DE LOS VÉRTICES)
        int[] ordenamiento;

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

                //Si no existen vertices en el arreglo, entonces no hay nada que resolver
                if (NUMERO_VERTICES < 1 || NUMERO_ARISTAS < 1)
                {
                    Console.WriteLine("El número de vertices y aristas debe ser mayor que 0.");
                    return false;
                }

                //Inicializar las variables para la resolución
                MATRIZ_ADYACENCIA = new int[NUMERO_VERTICES, NUMERO_VERTICES];
                VERTICES = new Vertice[NUMERO_VERTICES];
                ordenamiento = new int[NUMERO_VERTICES];

                //Inicializar los objetos de tipo vértice
                for (int i = 0; i < NUMERO_VERTICES; i++)
                {
                    ordenamiento[i] = i;
                    VERTICES[i] = new Vertice(i);
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

                //Incrementar la adyacencia en la matriz y el grado
                //de los objetos en el arreglo de vértices
                aumentarAdyacencia(u, v);

                linea_actual++;
            }

            //Cerrar el lector
            sreader.Close();

            //Calcular la adyacencia entre los vértices para almacenarla
            procesarAdyacencia();

            //Mostrar la matriz de adyacencia formada
            //Console.WriteLine("MATRIZ DE ADYACENCIA");
            //mostrarMatriz(MATRIZ_ADYACENCIA);
            //mostrarAdyacencia(20);
            return true;
        }
        private void mostrarAdyacencia()
        {
            //Recorrer el vertice seleccionado con la cantidad de grados que tiene
            //la iteración empieza desde el valor guardado
            for (int x = 0; x < NUMERO_VERTICES; x++)
            {
                for (int i = 0; i < VERTICES[x].grado_org; i++)
                {
                    Console.WriteLine($"PARA VÉRTICE {VERTICES[x].vertice}, {VERTICES_ADYACENTES[i + VERTICES[x].indice_inicio_lista]}");
                }
            }
        }
        private void mostrarAdyacencia(int u)
        {
            //Recorrer el vertice seleccionado con la cantidad de grados que tiene
            //la iteración empieza desde el valor guardado
            for (int i = 0; i < VERTICES[u].grado_org; i++)
            {
                Console.WriteLine($"PARA VÉRTICE {VERTICES[u].vertice}, {VERTICES_ADYACENTES[i + VERTICES[u].indice_inicio_lista]}");
            }
        }
        private void procesarAdyacencia()
        {
            //Definir un contador para insertar en esa posición la conexión (arista)
            //acertada entre vértices
            int contador = 0;

            //Crear el vector con el total de conexiones * 2 debido a que es un grafo no dirigido
            VERTICES_ADYACENTES = new int[NUMERO_ARISTAS + NUMERO_ARISTAS];

            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                int idx_u = VERTICES[i].vertice;
                for (int j = 0; j < NUMERO_VERTICES; j++)
                {
                    int idx_v = VERTICES[j].vertice;

                    //Verificar si existe conexión entre los vértices
                    if (MATRIZ_ADYACENCIA[idx_u, idx_v] == 1)
                    {
                        //Si existe conexión colocarla en el índice incremental (contador)
                        VERTICES_ADYACENTES[contador] = VERTICES[j].vertice;
                        contador++;
                    }                   
                }

                //Colocar el valor al índice de inicio en la lista de adyacencia
                if (i + 1 < NUMERO_VERTICES) VERTICES[i + 1].indice_inicio_lista = contador;
            }
        }
        private void aumentarAdyacencia(int u, int v)
        {
            //Incrementar la adyacencia en la matriz según los
            //vertices especificados
            if (u != v)
            {
                VERTICES[u].grado += 1;
                VERTICES[v].grado += 1;

                VERTICES[u].grado_org += 1;
                VERTICES[v].grado_org += 1;

                MATRIZ_ADYACENCIA[u, v]++;
                MATRIZ_ADYACENCIA[v, u]++;
            }
            else
            {
                VERTICES[u].grado += 1;
                VERTICES[u].grado_org += 1;
                MATRIZ_ADYACENCIA[u, v]++;
            }
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
        private int calcularGradoComplejidad(int idx_u)
        {            
            int grado_complejidad = 0;
            Vertice u = VERTICES[idx_u];

            //Obtener el índice de inicio y final del vértice de adyacencia
            int idx_inicio = u.indice_inicio_lista;
            int idx_final = u.grado_org + idx_inicio;

            for (int j = idx_inicio; j < idx_final; j++)
            {
                int idx_v = VERTICES_ADYACENTES[j];
                Vertice v = VERTICES[idx_v];

                //Sí el vértice no ha sido visitado (establecido en el ordenamiento), entonces
                //aumentar el grado de complejidad
                if (!v.visitado)
                {
                    grado_complejidad += v.grado;
                }
            }

            return grado_complejidad;
        }
        private Vertice[] getVerticesAdyacentes(int[] vertice)
        {
            List<int> adyacentes = new List<int>();
                     
            //Recorrer los vértices ya ordenados para verificar los vértices que tienen conexión 
            //(adyacencia) con ellos mediante la lista
            for (int u = 0; u < VERTICES_ORDENADOS; u++)
            {
                int idx_u = vertice[u];

                //Obtener los índices de inicio y final del vértice u 
                int idx_inicio = VERTICES[idx_u].indice_inicio_lista;
                int idx_final = VERTICES[idx_u].grado_org + idx_inicio;

                //Vértices adyacentes a u
                for (int i = idx_inicio; i < idx_final; i++)
                {
                    int idx_adyacente = VERTICES_ADYACENTES[i];

                    //Agregar a la lista los vértices adyacentes si no se encuentran en la misma
                    if (!VERTICES[idx_adyacente].visitado && !adyacentes.Contains(idx_adyacente))
                    {
                        adyacentes.Add(idx_adyacente);                        
                    }

                    //Incrementar el valor de vertices adyacentes
                    VERTICES[idx_adyacente].offset += 1;
                }
            }

            //Usar la lista de índices de vértices para crear el arreglo de vértices adyacentes
            Vertice[] vertices_adyacentes = new Vertice[adyacentes.Count];

            //Recalcular el grado considerando el offset
            for (int j = 0; j < adyacentes.Count; j++)
            {
                //Establecer los índices previos de la lista ahora en el arreglo
                int idx_u = adyacentes[j];
                vertices_adyacentes[j] = this.VERTICES[idx_u];

                //Calcular el grado, restando las conexiones de los vértices ordenados
                VERTICES[idx_u].grado = VERTICES[idx_u].grado_org - VERTICES[idx_u].offset;
                VERTICES[idx_u].offset = 0;
            }
            
            return vertices_adyacentes;
        }
        private void etiquetar(int u)
        {
            //Agregar el vertice al ordenamiento, colocar el índice en el objeto
            //e incrementar la cantidad de vértices ordenados
            ordenamiento[VERTICES_ORDENADOS] = u;
            VERTICES[u].etiquetar(VERTICES_ORDENADOS);

            VERTICES_ORDENADOS++;
        }
        private Vertice seleccionarVerticeComplejidad(Vertice[] vertices_candidatos, int cantidad_evaluar)
        {
            int complejidad_menor = 0;
            int idx_u_menor = 0;

            //Calcular la complejidad total de adyacencia de los vértices recibidos
            //con la cantidad especificada
            for (int i = 0; i < cantidad_evaluar; i++)
            {
                int idx_v = vertices_candidatos[i].vertice;
                VERTICES[idx_v].grado_complejidad = calcularGradoComplejidad(vertices_candidatos[i].vertice);

                if (i == 0 || vertices_candidatos[i].grado_complejidad < complejidad_menor)
                {
                    complejidad_menor = vertices_candidatos[i].grado_complejidad;
                    idx_u_menor = vertices_candidatos[i].vertice;
                }
            }

            return VERTICES[idx_u_menor];
        }
        private Vertice[] getVerticesNoOrdenados()
        {        
            Vertice[] vertices_no_ordenados = new Vertice[NUMERO_VERTICES - VERTICES_ORDENADOS];
            int contador = 0;

            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                //Verificar que el vértice no se encuentre en el ordenamiento
                if (!VERTICES[i].visitado)
                {
                    vertices_no_ordenados[contador] = VERTICES[i];
                    contador++;
                }
            }

            return vertices_no_ordenados;
        }
        private Vertice seleccionarVertice(Vertice[] vertices)
        {
            //Retornar nulo, si no hay vértices disponibles
            if (vertices.Length == 0) return null;

            //Ordenar los vértices de forma descendente 
            Vertice[] vertices_candidatos = ordenarVertices(vertices, true, 0, vertices.Length - 1);

            //En caso que exista más de un vértice con el grado mínimo, entonces seleccionarlo
            //en base a la complejidad total
            int mismo_grado = 0;       

            //El vértice que tiene el menor grado es el de la posición cero
            int grado_menor = vertices_candidatos[0].grado;

            //Recorrer cada vértice del arreglo para verificar si comparten el mismo grado
            for (int i = 1; i < vertices_candidatos.Length; i++)
            {               
                if (vertices_candidatos[i].grado == grado_menor)
                {
                    mismo_grado++;
                }
                else
                {
                    //Sí el vértice adyacente no cumple la condición, entonces finalizar el bucle
                    //puesto que están ordenados de forma ascendente
                    break;
                }               
            }

            //Si no se encontró ningún vértice con el menor grado igual al primero
            //retornar el de la posición cero. Caso contrario realizar la evaluación de complejidad
            if (mismo_grado == 0)
            {
                return vertices_candidatos[0];
            }
            else
            {
                return seleccionarVerticeComplejidad(vertices_candidatos, mismo_grado + 1);
            }
        }
        private void intercambiar(int u, int v, int pos_x, int pos_y)
        {
            Vertice temp_u = VERTICES[u];
            Vertice temp_v = VERTICES[v];
            VERTICES[u] = VERTICES[v];
            VERTICES[v] = temp_u;

            ordenamiento[pos_x] = ordenamiento[pos_y];
            ordenamiento[pos_y] = ordenamiento[pos_x];
        }

        private bool resolverVerticesSinAdyacencia()
        {
            bool vertices_etiquetados = false;
            for(int i = 0; i < VERTICES.Length; i++)
            {
                //Sí el grado es igual a cero o tiene una conexión consigo mismo
                //entonces, agregarlo al ordenamiento
                if (VERTICES[i].grado_org < 2)
                {
                    Vertice u = VERTICES[i];
                    int idx_inicio = u.indice_inicio_lista;
                    int idx_v = VERTICES_ADYACENTES[idx_inicio];

                    if (idx_v == VERTICES[i].vertice)
                    {
                        this.etiquetar(idx_v);
                        vertices_etiquetados = true;
                    }
                }
            }

            return vertices_etiquetados;
        }

        public void resolver(string ruta_problema)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //Parsear el archivo para obtener la configuración del problema (vértices y aristas)
            bool parseado = parsear_problema(ruta_problema);
            if (!parseado) return;

            VERTICES_ORDENADOS = 0;

            //Repetir el proceso de selección y ordenamiento mientras existan vertices
            //que no sean conexos, podrían tener conexión consigo mismo pero no con los demás
            bool resuelto_vertices_bucle = resolverVerticesSinAdyacencia();


            //En caso que se hayan resuelto los vértices que presentan bucles consigo mismo
            //hay que formar una nueva población excluyendo los mencionados
            Vertice[] poblacion_inicio = VERTICES;
            if (resuelto_vertices_bucle)
            {
                poblacion_inicio = getVerticesNoOrdenados();
            }

            bool EXISTE_ADYACENCIA = false;
            int cantidad_repeticiones = 0;
            while (!EXISTE_ADYACENCIA)
            {
                EXISTE_ADYACENCIA = true;
                if (cantidad_repeticiones > 0) poblacion_inicio = getVerticesNoOrdenados();

                //Iniciar seleccionando un vértice que tiene un grado bajo
                Vertice u = seleccionarVertice(poblacion_inicio);
                this.etiquetar(u.vertice);

                //Recorrer la cantidad de vértices restantes menos uno (n - 1) puesto que
                //se definió anteriormente
                while (VERTICES_ORDENADOS < NUMERO_VERTICES)
                {
                    //Buscar los vértices adyacentes a los ya visitados
                    Vertice[] vertices_adyacentes = getVerticesAdyacentes(ordenamiento);

                    //Seleccionar el vértice de menor grado y etiquetarlo (agregarlo al ordenamiento)
                    Vertice v = seleccionarVertice(vertices_adyacentes);

                    if (v != null) this.etiquetar(v.vertice);
                    else
                    {
                        EXISTE_ADYACENCIA = false;
                        cantidad_repeticiones++;
                        break;
                    }
                }
            }

            //Calcular el ancho de corte de cada partición
            calcularAnchoCorte();


            stopwatch.Stop();
            Console.WriteLine($"Tiempo de ejecución: {stopwatch.Elapsed.TotalSeconds} segundos");

        }

        private bool cruzaParticion(int u, int v, int particion)
        {           
            //Verificar si u y v están en diferentes conjuntos respecto a la partición
            return (VERTICES[u].indice <= particion && VERTICES[v].indice > particion) 
                || (VERTICES[v].indice <= particion && VERTICES[u].indice > particion);
        }

        private int contarCortes(int particion)
        {
            ///Variable para guardar la cantidad de cortes por partición
            int corte = 0;

            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                //Índice de inicio en el vector
                int indiceInicio = VERTICES[u].indice_inicio_lista;

                //Cantidad de elementos a partir del índice de inicio
                int grado = VERTICES[u].grado_org;

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
                    VERTICES[ordenamiento[i + 1]].ancho_corte = corteActual;
                }

                //Obtener el corte máximo y guardarlo en las variables
                if (corteActual > corteMaximo)
                {
                    corteMaximo = corteActual;
                    indiceCorte = i;
                }
            }
            
            mostrarResultado();
            //Console.WriteLine($"CORTE MÁXIMO = {corteMaximo} EN {(char)('A' + ordenamiento[indiceCorte])}, {(char)('A' + ordenamiento[indiceCorte + 1])}");
            Console.WriteLine($"CORTE MÁXIMO = {corteMaximo} EN {ordenamiento[indiceCorte]}, {ordenamiento[indiceCorte + 1]}");
        }

        //MergeSort
        private Vertice[] ordenarVertices(Vertice[] array, bool asc, int inicio, int final)
        {
            Vertice[] arr = (Vertice[])array.Clone();
            MergeSort(arr, inicio, final, asc);
            //MergeSort(arr, 0, arr.Length - 1, asc);
            return arr;
        }
        
        private void MergeSort(Vertice[] array, int izq, int der, bool asc)
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

        private void Merge(Vertice[] array, int izq, int medio, int der, bool asc)
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
                    Console.WriteLine($"{ordenamiento[x]},  {ordenamiento[x + 1]}, CORTE: {VERTICES[ordenamiento[x + 1]].ancho_corte}");
                }
            }

        }
    }
}

