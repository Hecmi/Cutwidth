using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    class CWP4
    {
        // MATRIZ DE ADYACENCIA (CONTIENE EL PESO DE LAS CONEXIONES ENTRE VÉRTICES)
        public Dictionary<int, Dictionary<int, double>> MATRIZ_ADYACENCIA = new Dictionary<int, Dictionary<int, double>>();

        // VARIABLES DE CONTROL
        public int NUMERO_VERTICES;
        public int NUMERO_ARISTAS;
        public int VERTICES_ORDENADOS;

        //VARIABLES PARA SEGMENTACIÓN
        public int IDX_PIVOTE;
        public int MIN_GRADO;

        // ARREGLO QUE CONTIENE TODOS LOS VÉRTICES DEL PROBLEMA
        public Vertice[] VERTICES;

        // ORDENAMIENTO DE SALIDA (SOLO CONTIENE LOS ÍNDICES DE LOS VÉRTICES)
        public int[] ORDENAMIENTO;

        // VARIABLES DE LECTURA DEL ARCHIVO
        public char DELIMITADOR;

        // VARIABLES DE SALIDA
        public int IDX_MEJOR;
        public double MEJOR_CORTE;
        public double CORTE_MAXIMO;
        public double TIEMPO_RESOLUCION;

        // VARIABLES DEFINIDAS POR EL USUARIO
        public bool MOSTRAR_ORDENAMIENTO;
        public bool GUARDAR_SALIDA;
        public string RUTA_ENTRADA;

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

                if (VERTICES[u].grado < MIN_GRADO)
                {
                    MIN_GRADO = VERTICES[u].grado;
                    IDX_PIVOTE = u;
                }
                else if (VERTICES[v].grado < MIN_GRADO)
                {
                    MIN_GRADO = VERTICES[v].grado;
                    IDX_PIVOTE = v;
                }
            }
            else
            {
                VERTICES[u].incrementar_adyacencia(peso);

                MATRIZ_ADYACENCIA[u][v] = peso;
            }
        }
        public CWP4(bool mostrar_ordenamiento)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            DELIMITADOR = ' ';
            GUARDAR_SALIDA = false;
        }
        public CWP4(bool mostrar_ordenamiento, char delimitador)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            DELIMITADOR = delimitador;
            GUARDAR_SALIDA = false;
            MIN_GRADO = int.MaxValue;
        }
        public CWP4(bool mostrar_ordenamiento, char delimitador, bool guardar_salida)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            DELIMITADOR = delimitador;
            GUARDAR_SALIDA = guardar_salida;
        }
      
        public void segmentar(string ruta_problema)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            parsear_problema(ruta_problema);

            //Listas para particionar el pivote
            List<Vertice> p1 = new List<Vertice>();
            List<Vertice> p2 = new List<Vertice>();

            int ELEMENTOS_P1 = 0;
            int ELEMENTOS_P2 = 0;

            //Obtener el vértice que tiene menor grado y sus adyacentes
            Dictionary<int, double> adyacencia_pivote = MATRIZ_ADYACENCIA[IDX_PIVOTE];

            //Distribuir los adyacentes en diferentes particiones
            int elementos_por_p = adyacencia_pivote.Count / 2;
            int contador = 0;
            foreach (var ady in adyacencia_pivote)
            {
                if (contador < elementos_por_p)
                {
                    p1.Add(VERTICES[ady.Key]);
                }
                else
                {
                    p2.Add(VERTICES[ady.Key]);
                }
                contador++;
            }

            ELEMENTOS_P1 = p1.Count;
            ELEMENTOS_P2 = p2.Count;

            //Crear una lista para almacenar todos los vértices no incluidos en las particiones
            List<Vertice> vertices_restantes = new List<Vertice>();
            foreach (var vertice in VERTICES)
            {
                if (!p1.Contains(vertice) && !p2.Contains(vertice))
                {
                    vertices_restantes.Add(vertice);
                }
            }

            //Repartir aleatoriamente los vértices restantes en p1 y p2
            Random rnd = new Random();
            foreach (var vertice in vertices_restantes)
            {
                if (rnd.NextDouble() < 0.5)
                {
                    p1.Add(vertice);
                }
                else
                {
                    p2.Add(vertice);
                }
            }

            Subgrafo sg1 = new Subgrafo(
                MATRIZ_ADYACENCIA,
                p1.ToArray(),
                ELEMENTOS_P1,
                true
            );

            Subgrafo sg2 = new Subgrafo(
                MATRIZ_ADYACENCIA,
                p2.ToArray(),
                ELEMENTOS_P2,
                true
            );

            int[] ORDENAMIENTO1 = sg1.resolver();
            int[] ORDENAMIENTO2 = sg2.resolver();
            Array.Reverse(ORDENAMIENTO2);

            Dictionary<int, int> indices_sg1 = sg1.indices_intercambiados;
            Dictionary<int, int> indices_sg2 = sg2.indices_intercambiados;

            for (int i = 0; i < ORDENAMIENTO1.Length; i++)
            {
                int idx_modificado = ORDENAMIENTO1[i];
                int id_real = 0;
                indices_sg1.TryGetValue(idx_modificado, out id_real);
                this.etiquetar(id_real);
            }

            for (int i = 0; i < ORDENAMIENTO2.Length; i++)
            {
                int idx_modificado = ORDENAMIENTO2[i];
                int id_real = 0;
                indices_sg2.TryGetValue(idx_modificado, out id_real);
                this.etiquetar(id_real);
            }

            stopwatch.Stop();
            TIEMPO_RESOLUCION = stopwatch.Elapsed.TotalSeconds;
            CORTE_MAXIMO = VERTICES[ORDENAMIENTO[IDX_MEJOR]].ancho_corte;

            ////Console.WriteLine($"N.Vértices: {NUMERO_VERTICES}. N.Aristas: {NUMERO_ARISTAS}. Corte máximo: {CORTE_MAXIMO}. Tiempo de ejecución: {TIEMPO_RESOLUCION} segundos");
            if (MOSTRAR_ORDENAMIENTO) mostrarResultado();
            //if (GUARDAR_SALIDA) guardar_salida();
        }
        private bool etiquetar(int idx_u)
        {
            //Console.WriteLine($"2. {VERTICES_ORDENADOS} {idx_u}");

            //Agregar el vertice al ordenamiento, colocar el índice en el objeto
            //e incrementar la cantidad de vértices ordenados
            ORDENAMIENTO[VERTICES_ORDENADOS] = idx_u;
            VERTICES[idx_u].etiquetar(VERTICES_ORDENADOS);

            //Recalcular el ancho de corte debido a la inserción del nuevo vértice en 
            //el ordenamiento
            bool finalizo_vertice = recalcularCorte(idx_u);

            VERTICES_ORDENADOS++;
            return finalizo_vertice;
        }
        private bool recalcularCorte(int idx_u)
        {
            //Obtener los vértices adyacentes a u
            Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[idx_u];

            //Lista de vértices completados debido al nuevo vertice en el ordenamiento
            List<int> verticesCompletados = new List<int>();

            //Recorrer la adyacencia para recalcular el corte a partir del índice
            //de las conexiones
            foreach (var conexion in adyacencia_u)
            {
                if (conexion.Key >= VERTICES.Length) continue;

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
                        VERTICES[ORDENAMIENTO[i + 1]].ancho_corte += conexion.Value;

                        //Al incrementar el ancho de corte, determinar si es el corte que 
                        //representa el valor máximo
                        if (VERTICES[ORDENAMIENTO[i + 1]].ancho_corte > MEJOR_CORTE)
                        {
                            IDX_MEJOR = i;
                            MEJOR_CORTE = VERTICES[ORDENAMIENTO[i + 1]].ancho_corte;
                        }
                    }

                    if (VERTICES[idx_v].completado) verticesCompletados.Add(idx_inicio);
                }
            }

            if (verticesCompletados.Count > 0) return true;


            return false;
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
        public override string ToString()
        {
            return $"{NUMERO_VERTICES};{NUMERO_ARISTAS};{TIEMPO_RESOLUCION};{CORTE_MAXIMO}";
        }

    }
}
