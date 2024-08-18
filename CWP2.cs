using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    class CWP2
    {
        //MATRIZ DE ADYACENCIA (CONTIENE EL PESO DE LAS CONEXIONES 
        //ENTRE VÉRTICES)
        Dictionary<int, Dictionary<int, double>> MATRIZ_ADYACENCIA;
        
        //VARIABLES DE CONTROL
        int NUMERO_VERTICES;
        int NUMERO_ARISTAS;
        int VERTICES_ORDENADOS;

        //ARREGLO QUE CONTIENE TODOS LOS VÉRTICES DEL PROBLEMA
        Vertice[] VERTICES;

        //ORDENAMIENTO DE SALIDA (SOLO CONTIENE LOS ÍNDICES DE LOS VÉRTICES)
        int[] ORDENAMIENTO;

        //VARIABLES DE LECTURA DEL ARCHIVO
        char DELIMITADOR;

        //VARIABLES DE SALIDA
        int IDX_MEJOR;
        double MEJOR_CORTE;
        double CORTE_MAXIMO;
        double TIEMPO_RESOLUCION;

        //VARIABLES DEFINIDAS POR EL USUARIO
        bool MOSTRAR_ORDENAMIENTO;
        bool GUARDAR_SALIDA;
        string RUTA_ENTRADA;

        public CWP2(bool mostrar_ordenamiento)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            DELIMITADOR = ' ';
            GUARDAR_SALIDA = false;
        }
        public CWP2(bool mostrar_ordenamiento, char delimitador)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            DELIMITADOR = delimitador;
            GUARDAR_SALIDA = false;
        }
        public CWP2(bool mostrar_ordenamiento, char delimitador, bool guardar_salida)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            DELIMITADOR = delimitador;
            GUARDAR_SALIDA = guardar_salida;
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
            //Verificar que la ruta exista
            if (!File.Exists(ruta_archivo))
            {
                Console.WriteLine($"La ruta del archivo especificado no existe.");
                return false;
            }

            RUTA_ENTRADA = ruta_archivo;

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
                elementos_linea = linea.Split(DELIMITADOR);
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
                MATRIZ_ADYACENCIA = new Dictionary<int, Dictionary<int, double>>();
                VERTICES = new Vertice[NUMERO_VERTICES];
                ORDENAMIENTO = new int[NUMERO_VERTICES];

                //Inicializar los objetos de tipo vértice
                for (int i = 0; i < NUMERO_VERTICES; i++)
                {
                    MATRIZ_ADYACENCIA[i] = new Dictionary<int, double>();                    

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
                elementos_linea = linea.Split(DELIMITADOR);

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

                    //if (elementos_linea.Length == 3 &&
                    //  !int.TryParse(elementos_linea[2], out peso))
                    //{
                    //    Console.WriteLine($"El formato del peso en la línea {linea_actual} no es correcto.");
                    //    return false;
                    //}
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

            return true;
        }
        private void mostrarAdyacencia()
        {
            //Recorrer el vertice seleccionado con la cantidad de grados que tiene
            //la iteración empieza desde el valor guardado
            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[u];
                foreach (var conexion in adyacencia_u)
                {
                    int v = conexion.Key;
                    double peso = conexion.Value;
                    Console.WriteLine($"Vértice {u} conectado con vértice {v} con peso {peso}");
                }
            }
        }
        private void mostrarAdyacencia(int u)
        {
            //Recorrer el vertice seleccionado con la cantidad de grados que tiene
            //la iteración empieza desde el valor guardado
            Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[u];
            foreach (var conexion in adyacencia_u)
            {
                int v = conexion.Key;
                double peso = conexion.Value;
                Console.WriteLine($"Vértice {u} conectado con vértice {v} con peso {peso}");
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

                MATRIZ_ADYACENCIA[u][v] = peso;
                MATRIZ_ADYACENCIA[v][u] = peso;
            }
            else
            {
                VERTICES[u].incrementar_adyacencia(peso);

                MATRIZ_ADYACENCIA[u][v] = peso;
            }
        }
        private void mostrarMatriz(int[,] matriz)
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

            Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[idx_u];
            foreach (var conexion in adyacencia_u)
            {
                int idx_v = conexion.Key;

                Vertice v = VERTICES[idx_v];
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

                //Verificar si el vértice u ya ha sido ordenado en conjunto a todos sus
                //adyacentes
                if (VERTICES[idx_u].completado) continue; 

                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[idx_u];
                foreach (var conexion in adyacencia_u)
                {
                    int idx_v = conexion.Key;
                    double peso = conexion.Value;
                    Vertice v = VERTICES[idx_v];

                    //Verificar si el vértice v ya ha sido ordenado en conjunto a todos sus
                    //adyacentes
                    if (v.completado) continue;

                    if (!v.visitado && !adyacentes.Contains(idx_v))
                    {
                        adyacentes.Add(idx_v);
                    }

                    //Incrementar el offset para restarlo posteriormente al grado
                    //de cada vértice adyacente
                    VERTICES[idx_v].offset += peso;
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

        private void recalcularCorte(int idx_u)
        {
            //Obtener los vértices adyacentes a u
            Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[idx_u];

            //Recorrer la adyacencia para recalcular el corte a partir del índice
            //de las conexiones
            foreach (var conexion in adyacencia_u)
            {
                //Obtener el vértice v
                int idx_v = conexion.Key;
                Vertice v = VERTICES[idx_v];

                //Verificar si ya ha sido etiquetado en conjunto a su adyacencia, para no 
                //seguir con el proceso
                if (v.completado) continue;

                //Verificar que esté en el ordenamiento (etiquetado)
                if (v.visitado)
                { 
                    //El índice de inicio, donde se empezará a recalcular es el 
                    //del vértice adyacente hasta u (v...u)
                    int idx_inicio = v.indice;

                    //Disminuir el grado del vértice que presenta la conexión
                    //y al que está conectado (v...u)
                    VERTICES[idx_u].disminuirGrado();
                    VERTICES[idx_v].disminuirGrado();

                    for (int i = idx_inicio; i < VERTICES_ORDENADOS; i++)
                    {
                        VERTICES[ORDENAMIENTO[i]].ancho_corte += conexion.Value;

                        //Al incrementar el ancho de corte, determinar si es el corte que 
                        //representa el valor máximo
                        if (VERTICES[ORDENAMIENTO[i]].ancho_corte > MEJOR_CORTE)
                        {
                            IDX_MEJOR = i;
                            MEJOR_CORTE = VERTICES[ORDENAMIENTO[i]].ancho_corte;
                        }
                    }
                }
            }
        }
        private void etiquetar(int idx_u)
        {
            //Console.WriteLine($"{VERTICES_ORDENADOS} {idx_u}");

            //Agregar el vertice al ordenamiento, colocar el índice en el objeto
            //e incrementar la cantidad de vértices ordenados
            ORDENAMIENTO[VERTICES_ORDENADOS] = idx_u;
            VERTICES[idx_u].etiquetar(VERTICES_ORDENADOS);

            //Recalcular el ancho de corte debido a la inserción del nuevo vértice en 
            //el ordenamiento
            recalcularCorte(idx_u);

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

            for (int i = 0; i < VERTICES.Length; i++)
            {
                //Sí el grado es igual a cero significa que no fue especificada
                //ninguna arista para el vértice
                if (VERTICES[i].grado == 0)
                {
                    this.etiquetar(VERTICES[i].vertice);
                    vertices_etiquetados = true;
                }
                //Caso contrario, donde el grado es 1 y su conexión es recursiva;
                //es decir, consigo mismo, simplemente ordenarlo
                else if (VERTICES[i].grado == 1)
                {
                    Vertice u = VERTICES[i];

                    Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[i];
                    int idx_v = adyacencia_u.First().Key;

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
                        poblacion = getVerticesNoOrdenados();
                        break;
                    }
                }
            }

            //Calcular el ancho de corte de cada partición
            //int idx_max_corte = calcularAnchoCorte();
            stopwatch.Stop();
            TIEMPO_RESOLUCION = stopwatch.Elapsed.TotalSeconds;
            CORTE_MAXIMO = VERTICES[ORDENAMIENTO[IDX_MEJOR]].ancho_corte;

            ////Console.WriteLine($"N.Vértices: {NUMERO_VERTICES}. N.Aristas: {NUMERO_ARISTAS}. Corte máximo: {CORTE_MAXIMO}. Tiempo de ejecución: {TIEMPO_RESOLUCION} segundos");
            if (MOSTRAR_ORDENAMIENTO) mostrarResultado();
            if (GUARDAR_SALIDA) guardar_salida();
        }
        private void guardar_salida()
        {
            string ruta_salida = RUTA_ENTRADA + ".output";
            StreamWriter s_writer = new StreamWriter(ruta_salida);
            s_writer.WriteLine($"VERTICES;ARISTAS;TIEMPO;CORTE");
            s_writer.WriteLine(this.ToString());
            s_writer.Close();
        }
        private bool cruzaParticion(int u, int v, int particion)
        {
            //Verificar si u y v están en diferentes conjuntos respecto a la partición
            return (VERTICES[u].indice <= particion && VERTICES[v].indice > particion)
                || (VERTICES[v].indice <= particion && VERTICES[u].indice > particion);
        }

        private double contarCortes(int particion)
        {
            ///Variable para guardar la cantidad de cortes por partición
            double corte = 0;

            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[u];
                foreach (var conexion in adyacencia_u)
                {
                    int v = conexion.Key;

                    //Ya que el grafo es no dirigido las aristas son dobles, la siguiente
                    //condición evita realizar revisiones dobles
                    if (v > u && cruzaParticion(u, v, particion))
                    {
                        //Sí los vértice se encuentran en lados diferentes de la partición
                        //entonces se incrementa el número de cortes                        
                        corte += MATRIZ_ADYACENCIA[u][v];
                    }
                }
            }

            return corte;
        }
        private int calcularAnchoCorte()
        {
            //Variables para guardar los resultados
            double corteMaximo = 0;
            int indiceCorte = 0;

            //Recorrer el ordenamiento para calcular los cortes entre pares de vértices
            for (int i = 0; i < VERTICES_ORDENADOS; i++)
            {
                //Calcular el ancho de corte entre vértices
                double corteActual = contarCortes(i);

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

