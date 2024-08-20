using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    class CWP3
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

        public int NUMERO_SOLUCIONES;
        public int[,] SOLUCIONES;
        public double[] SOLUCIONES_MEJORCORTE;
        public int SOLUCION_ACTUAL;

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
        public CWP3(bool mostrar_ordenamiento)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            DELIMITADOR = ' ';
            GUARDAR_SALIDA = false;
        }
        public CWP3(bool mostrar_ordenamiento, char delimitador)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            DELIMITADOR = delimitador;
            GUARDAR_SALIDA = false;
            MIN_GRADO = int.MaxValue;
        }
        public CWP3(bool mostrar_ordenamiento, char delimitador, bool guardar_salida)
        {
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            DELIMITADOR = delimitador;
            GUARDAR_SALIDA = guardar_salida;
        }

        public void segmentar(string ruta_problema, bool aleatorio, int numeroDeParticiones, int num_soluciones)
        {
            NUMERO_SOLUCIONES = num_soluciones;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            parsear_problema(ruta_problema);
            SOLUCIONES = new int[NUMERO_SOLUCIONES, NUMERO_VERTICES];
            SOLUCIONES_MEJORCORTE = new double[NUMERO_VERTICES];
            //if (NUMERO_VERTICES < 1000) numeroDeParticiones = 1;

            //Listas para particionar el pivote
            //Obtener el vértice que tiene menor grado y sus adyacentes
            if (aleatorio)
            {
                IDX_PIVOTE = new Random().Next(VERTICES.Length);
            }
            Dictionary<int, double> adyacencia_pivote = MATRIZ_ADYACENCIA[IDX_PIVOTE];

            //Distribuir los adyacentes en diferentes particiones
            int elementosPorParticion = adyacencia_pivote.Count/numeroDeParticiones;
            int contador = 0;

            while (elementosPorParticion <= 0)
            {
                numeroDeParticiones--;
                elementosPorParticion = adyacencia_pivote.Count / numeroDeParticiones;
            }

            //Instanciar las particiones en la lista general
            List<List<Vertice>> particiones = new List<List<Vertice>>(numeroDeParticiones);
            List<int> particion_segmentada = new List<int>(); 

            for (int i = 0; i < numeroDeParticiones; i++)
            {
                particiones.Add(new List<Vertice>());
            }

            //Recorrer los vértices adyacentes al pivote
            foreach (var ady in adyacencia_pivote)
            {
                //Dividr el contador entre los elementos por partición para identificar
                //a que partición debe ir
                int particionIndex = contador / elementosPorParticion;
                if (particionIndex < numeroDeParticiones)
                {
                    particiones[particionIndex].Add(VERTICES[ady.Key]);
                }
                else
                {
                    particiones[numeroDeParticiones - 1].Add(VERTICES[ady.Key]);
                }

                contador++;
            }

            for (int i = 0; i < numeroDeParticiones; i++)
            {
                particion_segmentada.Add(particiones[i].Count);
            }
            //Crear una lista para almacenar todos los vértices no incluidos en las particiones
            List<Vertice> verticesRestantes = new List<Vertice>();
            foreach (var vertice in VERTICES)
            {
                if (particiones.All(p => !p.Contains(vertice)))
                {
                    verticesRestantes.Add(vertice);
                }
            }

            //Repartir aleatoriamente los vértices restantes en las particiones
            Random rnd = new Random();
            foreach (var vertice in verticesRestantes)
            {
                int particionIndex = rnd.Next(numeroDeParticiones);
                particiones[particionIndex].Add(vertice);
            }

            //Resolver cada partición
            int[][] ordenamientos = new int[numeroDeParticiones][];
            Dictionary<int, int>[] indicesParticiones = new Dictionary<int, int>[numeroDeParticiones];

            for (int i = 0; i < numeroDeParticiones; i++)
            {
                Subgrafo sg = new Subgrafo(
                    MATRIZ_ADYACENCIA,
                    particiones[i].ToArray(),
                    particion_segmentada[i],
                    aleatorio
                );

                ordenamientos[i] = sg.resolver();
                indicesParticiones[i] = sg.indices_intercambiados;
            }

            //Procesar y etiquetar cada partición
            for (int i = 0; i < numeroDeParticiones; i++)
            {
                // Invertir el orden del resultado si es necesario
                if (i % 2 == 1)
                {
                    Array.Reverse(ordenamientos[i]);
                }

                for (int j = 0; j < ordenamientos[i].Length; j++)
                {
                    int idx_mod = ordenamientos[i][j];
                    int idx_real= 0;
                    if (indicesParticiones[i].TryGetValue(idx_mod, out idx_real))
                    {
                        this.etiquetar(idx_real);
                    }
                }
            }


            //Reordenar el grafo en busca de una mejor solución.
            for(int i = 0; i < NUMERO_VERTICES; i++)
            {
                SOLUCIONES[SOLUCION_ACTUAL, i] = ORDENAMIENTO[i];
            }

            int IDX_MEJOR = calcularAnchoCorte();
            SOLUCIONES_MEJORCORTE[0] = VERTICES[IDX_MEJOR].ancho_corte;
            SOLUCION_ACTUAL++;

            simulated_annealing();

            stopwatch.Stop();
            TIEMPO_RESOLUCION = stopwatch.Elapsed.TotalSeconds;
            //CORTE_MAXIMO = VERTICES[IDX_MEJOR].ancho_corte;

            // Mostrar resultado si es necesario
            if (MOSTRAR_ORDENAMIENTO) mostrarResultado();
            // if (GUARDAR_SALIDA) guardar_salida();
        }
        private int simulated_annealing()
        {
            Random rnd = new Random();
            double temperatura = 10000.0;
            double tasa_enfriamiento = 0.95;
            double temp_minima = 0.10;

            double mejorCorte = MEJOR_CORTE;
            double corteTemp = 0;
            int IDX_TEMP = 0;
            int IDX_MEJOR = 0;

            while (
                temperatura > temp_minima 
                && SOLUCION_ACTUAL < NUMERO_SOLUCIONES
            )
            {

                //Colocar inicialmente para la nueva solución los vértices iguales
                //a la solución temporal
                for (int i = 0; i < NUMERO_VERTICES; i++)
                {
                    SOLUCIONES[SOLUCION_ACTUAL, i] = SOLUCIONES[IDX_TEMP, i];
                }

                //Generar una solución vecina
                intercambiar_vertices(SOLUCION_ACTUAL);

                //Evaluar la nueva solución
                corteTemp = calcularAnchoCorte(SOLUCION_ACTUAL);

                //Guardar el índice de la mejor combinación
                if (corteTemp < mejorCorte)
                {
                    mejorCorte = corteTemp;
                    IDX_TEMP = SOLUCION_ACTUAL;
                    IDX_MEJOR = SOLUCION_ACTUAL;
                    Console.WriteLine(mejorCorte+"");
                }

              
                //Decidir si aceptar la nueva solución para explorar combinaciones vecinas
                if (rnd.NextDouble() < Math.Exp((mejorCorte - corteTemp) / temperatura))
                {
                    //mejorCorte = corteTemp;
                    IDX_TEMP = SOLUCION_ACTUAL;
                }

                //Reducir la temperatura
                temperatura *= tasa_enfriamiento;
                SOLUCION_ACTUAL++;
            }

            return IDX_MEJOR;
        }
        private int calcularAnchoCorte(int idx_muestra)
        {
            int corteMaximo = 0;
            int corteActual = 0;
            int indiceCorte = 0;

            for (int p = 0; p < NUMERO_VERTICES; p++)
            {
                corteActual = evaluarParticion(idx_muestra, p);

                if (corteActual > corteMaximo)
                {
                    corteMaximo = corteActual;
                    indiceCorte = p;
                }
            }

            return corteMaximo;
        }
        private int evaluarParticion(int idx_muestra, int particion)
        {
            int corte = 0;
            for (int u = 0; u < NUMERO_VERTICES; u++)
            {             
                //Obtener los vértices adyacentes a u
                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[u];

                foreach (var conexion in adyacencia_u)
                {
                    int v = conexion.Key;

                    //Ya que el grafo es no dirigido las aristas son dobles, la siguiente
                    //condición evita realizar revisiones dobles
                    if (v > u && cruzaParticion(idx_muestra, u, v, particion))
                    {
                        //Sí los vértice se encuentran en lados diferentes de la partición
                        //entonces se incrementa el número de cortes                        
                        corte++;
                    }
                }
            }
            return corte;
        }

        private bool cruzaParticion(int idx_muestra, int u, int v, int particion)
        {
            //Verificar si u y v están en diferentes particiones
            return (SOLUCIONES[idx_muestra, u] < particion && SOLUCIONES[idx_muestra, v] >= particion)
                || (SOLUCIONES[idx_muestra, v] < particion && SOLUCIONES[idx_muestra, u] >= particion);
        }
        private void intercambiar_vertices(int idx)
        {
            Random rnd = new Random();

            //Elegir dos índices aleatorios para intercambiar
            int idx1 = rnd.Next(NUMERO_VERTICES);
            int idx2;
            do
            {
                idx2 = rnd.Next(NUMERO_VERTICES);
            } while (idx1 == idx2);

            //Intercambiar los vértices
            int temp = SOLUCIONES[idx, idx1];
            SOLUCIONES[idx, idx1] = SOLUCIONES[idx, idx2];
            SOLUCIONES[idx, idx2] = temp;
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
                    indiceCorte = ORDENAMIENTO[i + 1];
                }
            }

            return indiceCorte;
        }

        private void etiquetar(int idx_u)
        {
            //Console.WriteLine($"2. {VERTICES_ORDENADOS} {idx_u}");

            //Agregar el vertice al ordenamiento, colocar el índice en el objeto
            //e incrementar la cantidad de vértices ordenados
            ORDENAMIENTO[VERTICES_ORDENADOS] = idx_u;
            VERTICES[idx_u].etiquetar(VERTICES_ORDENADOS);

            //Recalcular el ancho de corte debido a la inserción del nuevo vértice en 
            //el ordenamiento
            recalcularCorte(idx_u);

            VERTICES_ORDENADOS++;
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
                            IDX_MEJOR = ORDENAMIENTO[i + 1];
                            MEJOR_CORTE = VERTICES[IDX_MEJOR].ancho_corte;
                        }
                    }
                }
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
        public override string ToString()
        {
            return $"{NUMERO_VERTICES};{NUMERO_ARISTAS};{TIEMPO_RESOLUCION};{CORTE_MAXIMO}";
        }

    }
}
