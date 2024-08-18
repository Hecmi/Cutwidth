using System;
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
        //MATRIZ DE ADYACENCIA (CONTIENE EL PESO DE LAS CONEXIONES 
        //ENTRE VÉRTICES)
        int[,] MATRIZ_ADYACENCIA;

        //ARREGLO QUE CONTIENE LA ADYACENCIA DE CADA VÉRTICE
        //SE FORMA A PARTIR DE LA MATRIZ DE ADYACENCIA
        //(SIMULACIÓN DE LISTA DE ADYACENCIA)
        int [] VERTICES_ADYACENTES;

        //VARIABLES DE CONTROL
        int NUMERO_VERTICES;
        int NUMERO_ARISTAS;
        int VERTICES_ORDENADOS;

        //ARREGLO QUE CONTIENE TODOS LOS VÉRTICES DEL PROBLEMA
        Vertice[] VERTICES;

        //ORDENAMIENTO DE SALIDA (SOLO CONTIENE LOS ÍNDICES DE LOS VÉRTICES)
        int[] ORDENAMIENTO;

        //VARIABLES PARA EL CONTROL DE SALIDAS
        bool MOSTRAR_ORDENAMIENTO;
        double CORTE_MAXIMO;
        double TIEMPO_RESOLUCION;

        public CWP(bool mostrar_ordenamiento)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
        }
        public int get_NumeroVertices() { return NUMERO_VERTICES; }
        public int get_NumeroAristas() { return NUMERO_ARISTAS; }
        public double get_TiempoResolucion() { return TIEMPO_RESOLUCION; }
        public override string ToString()
        {
            return $"{NUMERO_VERTICES};{NUMERO_ARISTAS};{TIEMPO_RESOLUCION};{CORTE_MAXIMO}";
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

                //Si no existen vertices en el arreglo, entonces no hay nada que resolver
                if (NUMERO_VERTICES < 1 || NUMERO_ARISTAS < 1)
                {
                    Console.WriteLine("El número de vertices y aristas debe ser mayor que 0.");
                    return false;
                }

                //Inicializar las variables para la resolución
                MATRIZ_ADYACENCIA = new int[NUMERO_VERTICES, NUMERO_VERTICES];
                VERTICES = new Vertice[NUMERO_VERTICES];
                ORDENAMIENTO = new int[NUMERO_VERTICES];

                //Inicializar los objetos de tipo vértice
                for (int i = 0; i < NUMERO_VERTICES; i++)
                {
                    ORDENAMIENTO[i] = i;
                    VERTICES[i] = new Vertice(i);
                }
            }

            //Recorrer las aristas según la cantidad específicada en el encabezado
            int linea_actual = 0;
            while (linea_actual < NUMERO_ARISTAS)
            {
                //Leer la línea
                linea = sreader.ReadLine();

                //Sí es nulo, significa que no cumple con la cantidad de aristas especificadas
                //en el encabezado
                if (linea == null)
                {
                    Console.WriteLine($"Error de formato, la línea {linea_actual} no existe, sin embargo el número de arsitas especificado indica lo contrario.");
                    return false;
                }

                //Obtener los elementos de la línea actual
                elementos_linea = linea.Split(' ');

                //Obtener los índices de los vértices para incrementar el grado
                int u, v = -1;
                int peso = 1;

                //Verificar que la línea de conexión tenga dos o tres elementos
                //Sí tiene dos, se asume que el peso de la conexión es equivalente a 1
                //Caso contrario, se lee le peso especificado en la línea
                if (elementos_linea.Length == 2 ||
                    elementos_linea.Length == 3)
                {
                    if (!int.TryParse(elementos_linea[0], out u) ||
                      !int.TryParse(elementos_linea[1], out v))
                    {
                        Console.WriteLine($"El formato de la línea {linea_actual} no es correcto.");
                        return false;
                    }

                    if (elementos_linea.Length == 3 &&
                      !int.TryParse(elementos_linea[2], out peso))
                    {
                        Console.WriteLine($"El formato del peso en la línea {linea_actual} no es correcto.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine($"El formato de la línea {linea_actual} no es correcto.");
                    return false;
                }

                //Restar uno a los vértices para acceder al índice creado durante la lectura
                //del encabezado
                u = u - 1;
                v = v - 1;

                //Incrementar la adyacencia en la matriz y el grado
                //de los objetos en el arreglo de vértices
                aumentarAdyacencia(u, v, peso);

                //Incrementar la línea actual
                linea_actual++;
            }

            //Cerrar el lector
            sreader.Close();

            //Procesar la adyacencia para transformarla en un vector que simula
            //una lista de adyacencia
            procesarAdyacencia();

            return true;
        }
        private void mostrarAdyacencia()
        {
            //Recorrer el vertice seleccionado con la cantidad de grados que tiene
            //la iteración empieza desde el valor guardado
            for (int x = 0; x < NUMERO_VERTICES; x++)
            {
                for (int i = 0; i < VERTICES[x].grado; i++)
                {
                    Console.WriteLine($"PARA VÉRTICE {VERTICES[x].vertice}, {VERTICES_ADYACENTES[i + VERTICES[x].indice_inicio_lista]}");
                }
            }
        }
        private void mostrarAdyacencia(int u)
        {
            //Recorrer el vertice seleccionado con la cantidad de grados que tiene
            //la iteración empieza desde el valor guardado
            for (int i = 0; i < VERTICES[u].grado; i++)
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
                    if (MATRIZ_ADYACENCIA[idx_u, idx_v] > 0)
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
        private void aumentarAdyacencia(int u, int v, int peso)
        {
            //Si los índices son diferentes, incrementar el grado y peso 
            //en ambos vértices. Caso contrario, incrementarlo solo en uno para evitar duplicar
            //los datos mencionados.
            if (u != v)
            {
                VERTICES[u].incrementar_adyacencia(peso);
                VERTICES[v].incrementar_adyacencia(peso);
               
                MATRIZ_ADYACENCIA[u, v] += peso;
                MATRIZ_ADYACENCIA[v, u] += peso;
            }
            else
            {
                VERTICES[u].incrementar_adyacencia(peso);
                
                MATRIZ_ADYACENCIA[u, v] += peso;
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
        private double calcularGradoComplejidad(int idx_u)
        {
            double complejidad = 0;
            Vertice u = VERTICES[idx_u];

            //Obtener el índice de inicio y final del vértice de adyacencia
            int idx_inicio = u.indice_inicio_lista;
            int idx_final = u.grado + idx_inicio;

            for (int j = idx_inicio; j < idx_final; j++)
            {
                int idx_v = VERTICES_ADYACENTES[j];
                Vertice v = VERTICES[idx_v];

                //Sí el vértice no ha sido visitado (establecido en el ordenamiento), entonces
                //aumentar el grado de complejidad del vértice que se está evaluando
                if (!v.visitado)
                {
                    complejidad += v.peso;
                }
            }

            return complejidad;
        }
        private Vertice[] getVerticesAdyacentes(int[] vertice)
        {
            //Lista para colocar todos los índices de los vértices adyacentes
            List<int> adyacentes = new List<int>();
                     
            //Recorrer los vértices ya ordenados para verificar los vértices que tienen conexión 
            //(adyacencia) con ellos mediante el arreglo
            for (int u = 0; u < VERTICES_ORDENADOS; u++)
            {
                //Obtener el índice de los vértices enviados (u)
                int idx_u = vertice[u];

                //Obtener los índices de inicio y final del vértice u 
                int idx_inicio = VERTICES[idx_u].indice_inicio_lista;
                int idx_final = VERTICES[idx_u].grado + idx_inicio;

                //Vértices adyacentes a u
                for (int i = idx_inicio; i < idx_final; i++)
                {
                    int idx_adyacente = VERTICES_ADYACENTES[i];

                    //Agregar a la lista los vértices adyacentes si no se encuentran en la misma
                    if (!VERTICES[idx_adyacente].visitado && !adyacentes.Contains(idx_adyacente))
                    {
                        adyacentes.Add(idx_adyacente);                        
                    }

                    //Incrementar el offset para restarlo posteriormente al grado
                    //de cada vértice adyacente
                    //VERTICES[idx_adyacente].offset += 1;
                    VERTICES[idx_adyacente].offset += MATRIZ_ADYACENCIA[idx_u, idx_adyacente];
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
                //y luego restaurar el offset
                VERTICES[idx_u].peso = VERTICES[idx_u].peso_org - VERTICES[idx_u].offset;
                VERTICES[idx_u].offset = 0;
            }
            
            return vertices_adyacentes;
        }
        private void etiquetar(int u)
        {
            //Agregar el vertice al ordenamiento, colocar el índice en el objeto
            //e incrementar la cantidad de vértices ordenados
            ORDENAMIENTO[VERTICES_ORDENADOS] = u;
            VERTICES[u].etiquetar(VERTICES_ORDENADOS);

            VERTICES_ORDENADOS++;
        }
        private Vertice seleccionarVerticeComplejidad(Vertice[] vertices_candidatos, int cantidad_evaluar)
        {
            double complejidad_menor = 0;
            int idx_u_menor = 0;

            //Calcular la complejidad total de adyacencia de los vértices recibidos
            //con la cantidad especificada
            for (int i = 0; i < cantidad_evaluar; i++)
            {
                int idx_v = vertices_candidatos[i].vertice;
                VERTICES[idx_v].complejidad_adyacente = calcularGradoComplejidad(idx_v);

                //Obtener el vértice que represente una complejidad menor a todos los vértices
                //recibidos
                if (i == 0 || vertices_candidatos[i].complejidad_adyacente < complejidad_menor)
                {
                    complejidad_menor = vertices_candidatos[i].complejidad_adyacente;
                    idx_u_menor = vertices_candidatos[i].vertice;
                }
            }

            return VERTICES[idx_u_menor];
        }
        private Vertice[] getVerticesNoOrdenados()
        {
            //La nueva población es equivalente al total de vértices menos los ya ordenados
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

            //Ordenar los vértices de forma ascendente 
            Vertice[] vertices_candidatos = ordenarVertices(vertices, true, 0, vertices.Length - 1);
           
            //En caso que exista más de un vértice con el grado mínimo, entonces seleccionarlo
            //en base a la complejidad total
            int mismo_grado = 0;       

            //El vértice que tiene el menor grado es el de la posición cero
            double grado_menor = vertices_candidatos[0].peso;

            //Recorrer cada vértice del arreglo para verificar si comparten el mismo grado
            for (int i = 1; i < vertices_candidatos.Length; i++)
            {               
                if (vertices_candidatos[i].peso == grado_menor)
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
  
        private bool resolverVerticesSinAdyacencia()
        {
            //Variable para identificar si se encontró y resolvió algún vértice
            //sin adyacencia
            bool vertices_etiquetados = false;

            for(int i = 0; i < VERTICES.Length; i++)
            {
                //Sí el grado es igual a cero o tiene una conexión consigo mismo
                //entonces, agregarlo al ordenamiento
                if (VERTICES[i].grado < 2)
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

            //Repetir el proceso de selección y ordenamiento mientras existan vértices
            //que no sean conexos con otros
            bool resolucion_adyacente = resolverVerticesSinAdyacencia();

            //En caso que se hayan resuelto los vértices que presentan conexiones recursivas
            //se forma una nueva población excluyendo los mencionados etiquetados
            Vertice[] poblacion = resolucion_adyacente ? getVerticesNoOrdenados() : VERTICES;
            
            bool EXISTE_ADYACENCIA = false;
            while (!EXISTE_ADYACENCIA)
            {                
                EXISTE_ADYACENCIA = true;

                //Iniciar seleccionando un vértice que tiene un grado bajo
                Vertice u = seleccionarVertice(poblacion);
                this.etiquetar(u.vertice);

                //Mientras no se hayan ordenado o etiquetado todos los vértices
                //realizar la búsqueda basado en el cálculo de grado y complejidad
                while (VERTICES_ORDENADOS < NUMERO_VERTICES)
                {
                    //Buscar los vértices adyacentes a los ya visitados
                    Vertice[] vertices_adyacentes = getVerticesAdyacentes(ORDENAMIENTO);

                    //Seleccionar el vértice de menor grado y etiquetarlo (agregarlo al ordenamiento)
                    Vertice v = seleccionarVertice(vertices_adyacentes);

                    //Sí no se obtuvo ningún vértice (es nulo), entonces no existe adyacencia
                    //y por lo tanto el problema está formado por varios grafos
                    if (v != null) this.etiquetar(v.vertice);
                    else
                    {
                        EXISTE_ADYACENCIA = false;
                        break;
                    }
                }

                //Si no existe adyacencia, obtener la población faltante
                if (!EXISTE_ADYACENCIA) poblacion = getVerticesNoOrdenados();
            }

            //Calcular el ancho de corte de cada partición
            int idx_max_corte = calcularAnchoCorte();
            stopwatch.Stop();
            TIEMPO_RESOLUCION = stopwatch.Elapsed.TotalSeconds;
            CORTE_MAXIMO = VERTICES[ORDENAMIENTO[idx_max_corte + 1]].ancho_corte;

            //Console.WriteLine($"N.Vértices: {NUMERO_VERTICES}. N.Aristas: {NUMERO_ARISTAS}. Corte máximo: {CORTE_MAXIMO}. Tiempo de ejecución: {TIEMPO_RESOLUCION} segundos");
            if (MOSTRAR_ORDENAMIENTO) mostrarResultado();
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
                int idx_inicio = VERTICES[u].indice_inicio_lista;
                int idx_final = idx_inicio + VERTICES[u].grado;

                for (int i = idx_inicio; i < idx_final; i++)
                {
                    int v = VERTICES_ADYACENTES[i];

                    //Ya que el grafo es no dirigido las aristas son dobles, la siguiente
                    //condición evita realizar revisiones dobles
                    if (v > u && cruzaParticion(u, v, particion))
                    {
                        //Sí los vértice se encuentran en lados diferentes de la partición
                        //entonces se incrementa el número de cortes                        
                        corte += MATRIZ_ADYACENCIA[u, v];                        
                    }
                }
            }

            return corte;        
        }

        private int calcularAnchoCorte()
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
                    VERTICES[ORDENAMIENTO[i + 1]].ancho_corte = corteActual;
                }

                //Obtener el corte máximo y guardarlo en las variables
                if (corteActual > corteMaximo)
                {
                    corteMaximo = corteActual;
                    indiceCorte = i;
                }
            }
                        
            return indiceCorte;
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
                bool condicion = asc ? izqArray[i].peso <= derArray[j].peso : izqArray[i].peso >= derArray[j].peso;
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
                    Console.WriteLine($"{ORDENAMIENTO[x]},  {ORDENAMIENTO[x + 1]}, CORTE: {VERTICES[ORDENAMIENTO[x + 1]].ancho_corte}");
                }
            }

        }
    }
}

