using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CWP.Clases
{
    public class CW
    {
        Grafo GRAFO;

        //REFERENCIAS PARA ACCEDER RAPIDAMENTE A LAS VARIABLES DEL GRAFO
        Dictionary<int, Dictionary<int, double>> MATRIZ_ADYACENCIA => GRAFO.MATRIZ_ADYACENCIA;
        int NUMERO_VERTICES => GRAFO.NUMERO_VERTICES;
        int NUMERO_ARISTAS  => GRAFO.NUMERO_ARISTAS;
        Vertice [] VERTICES => GRAFO.VERTICES;
        int[] ORDENAMIENTO => GRAFO.ORDENAMIENTO;

        //VARIABLES DE CONTROL PARA LA RESOLUCIÓN
        //ORDENAMIENTO DE SALIDA (SOLO CONTIENE LOS ÍNDICES DE LOS VÉRTICES)
        int VERTICES_ORDENADOS;

        //VARIABLES DE SALIDA
        int IDX_MEJOR;
        double MEJOR_CORTE;
        double TIEMPO_RESOLUCION;

        //VARIABLES DEFINIDAS POR EL USUARIO
        string RUTA_ARCHIVO;
        string RUTA_SALIDA;
        bool GUARDAR_SALIDA;
        bool MOSTRAR_ORDENAMIENTO;

        public CW(string ruta_archivo, char delimitador, bool mostrar_ordenamiento, string ruta_salida)
        {
            //Inicializar las variables definidas por el usuario
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            RUTA_SALIDA = ruta_salida;
            RUTA_ARCHIVO = ruta_archivo;

            GUARDAR_SALIDA = !string.IsNullOrEmpty(RUTA_SALIDA);

            //Lee el archivo, envíando por referencia el grafo no instanciado
            Archivos.procesar_archivo(ref GRAFO, ruta_archivo, delimitador);
        }
        
        private double calcular_complejidad(int idx_u)
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
        private Vertice[] get_vertices_adyacentes(int[] vertice)
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
                    //adyacentes, si es así, no se evalúa
                    if (v.completado) continue;

                    //Sí el vértice se encuentra en el ordenamiento, no está completo y no se 
                    //encuenetra en la lista de vértices adyacentes, agregarlo
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
        private void incrementar_corte(int idx_inicio, int idx_final, double incremento)
        {
            for (int i = idx_inicio; i < idx_final; i++)
            {
                VERTICES[ORDENAMIENTO[i + 1]].ancho_corte += incremento;

                //Al incrementar el ancho de corte, determinar si es el corte que 
                //representa el valor máximo
                if (VERTICES[ORDENAMIENTO[i + 1]].ancho_corte > MEJOR_CORTE)
                {
                    IDX_MEJOR = ORDENAMIENTO[i + 1];
                    MEJOR_CORTE = VERTICES[IDX_MEJOR].ancho_corte;
                }
            }
        }
        private void recalcular_corte(int idx_u)
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
                    int idx_inicio = v.indice_ordenamiento;

                    //Disminuir el grado del vértice que presenta la conexión
                    //y al que está conectado (v...u)
                    VERTICES[idx_u].disminuir_grado();
                    VERTICES[idx_v].disminuir_grado();

                    incrementar_corte(idx_inicio, VERTICES_ORDENADOS, conexion.Value);                  
                }
            }
        }
        private void etiquetar(int idx_u)
        {
            //Agregar el vertice al ordenamiento, colocar el índice en el objeto
            //e incrementar la cantidad de vértices ordenados
            ORDENAMIENTO[VERTICES_ORDENADOS] = idx_u;
            VERTICES[idx_u].etiquetar(VERTICES_ORDENADOS);

            //Recalcular el ancho de corte debido a la inserción del nuevo vértice en 
            //el ordenamiento
            recalcular_corte(idx_u);

            VERTICES_ORDENADOS++;
        }

        private Vertice seleccionar_vertice_complejidad(Vertice[] vertices_candidatos, int cantidad_evaluar)
        {
            double complejidad_menor = double.MaxValue;
            int idx_u_menor = 0;

            //Calcular la complejidad total de adyacencia de los vértices recibidos
            //con la cantidad especificada
            for (int i = 0; i < cantidad_evaluar; i++)
            {
                int idx_v = vertices_candidatos[i].vertice;
                VERTICES[idx_v].complejidad_adyacente = calcular_complejidad(idx_v);

                //Obtener el vértice que represente una complejidad menor a todos los vértices
                //recibidos
                if (VERTICES[idx_v].complejidad_adyacente < complejidad_menor)
                {
                    complejidad_menor = VERTICES[idx_v].complejidad_adyacente;
                    idx_u_menor = VERTICES[idx_v].vertice;
                }
            }

            return VERTICES[idx_u_menor];
        }
        private Vertice[] get_vertices_no_ordenados()
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
        private Vertice seleccionar_vertice(Vertice[] vertices)
        {
            //Retornar nulo, si no hay vértices disponibles
            if (vertices.Length == 0) return null;

            //Sí existe exactamente un único vértice retornarlo
            if (vertices.Length == 1) return vertices[0];

            //Ordenar los vértices de forma ascendente 
            Vertice[] vertices_candidatos = ordenar_vertices(vertices, true, 0, vertices.Length - 1);

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
                return seleccionar_vertice_complejidad(vertices_candidatos, mismo_grado + 1);
            }
        }
        private bool resolver_vertices_sin_adyacencia()
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
        public void resolver()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
                   
            VERTICES_ORDENADOS = 0;

            //Repetir el proceso de selección y ordenamiento mientras existan vértices
            //que no sean conexos con otros
            bool resolucion_adyacente = resolver_vertices_sin_adyacencia();

            //En caso que se hayan resuelto los vértices que presentan conexiones recursivas
            //se forma una nueva población excluyendo los mencionados etiquetados
            Vertice[] poblacion = resolucion_adyacente ? get_vertices_no_ordenados() : VERTICES;

            bool EXISTE_ADYACENCIA = false;
            while (!EXISTE_ADYACENCIA)
            {
                EXISTE_ADYACENCIA = true;

                //Iniciar seleccionando un vértice que tiene un grado bajo
                Vertice u = seleccionar_vertice(poblacion);
                this.etiquetar(u.vertice);

                //Mientras no se hayan ordenado o etiquetado todos los vértices
                //realizar la búsqueda basado en el cálculo de grado y complejidad
                while (VERTICES_ORDENADOS < NUMERO_VERTICES)
                {
                    //Buscar los vértices adyacentes a los ya visitados
                    Vertice[] vertices_adyacentes = get_vertices_adyacentes(ORDENAMIENTO);

                    //Seleccionar el vértice de menor grado y etiquetarlo (agregarlo al ordenamiento)
                    Vertice v = seleccionar_vertice(vertices_adyacentes);

                    //Sí no se obtuvo ningún vértice (es nulo), entonces no existe adyacencia
                    //y por lo tanto el problema está formado por varios grafos
                    if (v != null)
                    {
                         this.etiquetar(v.vertice); 
                    }
                    else
                    {
                        EXISTE_ADYACENCIA = false;
                        poblacion = get_vertices_no_ordenados();
                        break;
                    }
                }
            }

            //Detener el tiempo de resolución
            stopwatch.Stop();
            TIEMPO_RESOLUCION = stopwatch.Elapsed.TotalSeconds;

            if (MOSTRAR_ORDENAMIENTO) mostrar_resultado();
            if (GUARDAR_SALIDA) Archivos.guardar_salida(ToString(), $@"{RUTA_SALIDA}\output.csv");
        }

        //MergeSort
        private Vertice[] ordenar_vertices(Vertice[] array, bool asc, int inicio, int final)
        {
            Vertice[] arr = (Vertice[])array.Clone();
            merge_sort(arr, inicio, final, asc);
            //MergeSort(arr, 0, arr.Length - 1, asc);
            return arr;
        }

        private void merge_sort(Vertice[] array, int izq, int der, bool asc)
        {
            //Verificar si la lista tiene más de un elemento
            if (izq < der)
            {
                //Calcular el índice medio del arreglo
                int medio = (izq + der) / 2;

                //Ordenar la primera mitad del arreglo (izq a centro)
                merge_sort(array, izq, medio, asc);

                //Ordenar la segunda mitad del arreglo (centro a derecha)
                merge_sort(array, medio + 1, der, asc);

                //Fusionar las dos mitades ordenadas
                merge(array, izq, medio, der, asc);
            }
        }

        private void merge(Vertice[] array, int izq, int medio, int der, bool asc)
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
        private void mostrar_resultado()
        {
            for (int x = 0; x < VERTICES_ORDENADOS; x++)
            {
                if (x + 1 < VERTICES_ORDENADOS)
                {
                    Console.WriteLine($"IDX_MEJOR = {IDX_MEJOR}, {ORDENAMIENTO[x]},  {ORDENAMIENTO[x + 1]}, CORTE: {VERTICES[ORDENAMIENTO[x + 1]].ancho_corte}");
                }
            }
            //Console.WriteLine($"CORTE MÁXIMO = {MEJOR_CORTE} EN [{VERTICES[IDX_MEJOR - 1].vertice} - {VERTICES[IDX_MEJOR].vertice}]");
        }
        public int [] get_ordenamiento()
        {
            return ORDENAMIENTO;
        }

        public void test_ordenamiento(int[] ordenamiento)
        {
            VERTICES_ORDENADOS = 0;
            IDX_MEJOR = 0;
            MEJOR_CORTE  = 0;

            for (int i = 0; i < VERTICES.Length; i++)
            {
                VERTICES[i].visitado = false;
                VERTICES[i].completado = false;
                VERTICES[i].grado_iterativo = VERTICES[i].grado;
                VERTICES[i].ancho_corte = 0;
                VERTICES[i].peso = VERTICES[i].peso_org;
            }


            for (int i = 0; i < ordenamiento.Length; i++)
            {
                etiquetar(ordenamiento[VERTICES_ORDENADOS]);
            }

            Console.WriteLine($"ORD: {String.Join(',', ordenamiento)}, {VERTICES[IDX_MEJOR].ancho_corte}");
        }
        public override string ToString()
        {
            return $"{NUMERO_VERTICES};{NUMERO_ARISTAS};{TIEMPO_RESOLUCION};{MEJOR_CORTE}";
        }


        public string formar_json()
        {
            //Crear un objeto que combine todas las partes
            var data = new {
                MA = MATRIZ_ADYACENCIA,
                ORDENAMIENTO = ORDENAMIENTO,
                VERTICES = VERTICES
            };
                      
            JsonSerializerOptions options = new JsonSerializerOptions {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(data, options);
        }

        public string formar_json(int [] ORDENAMIENTO)
        {
            //Crear un objeto que combine todas las partes
            var data = new
            {
                MA = MATRIZ_ADYACENCIA,
                ORDENAMIENTO = ORDENAMIENTO,
                VERTICES = VERTICES
            };

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(data, options);
        }
    }
}
