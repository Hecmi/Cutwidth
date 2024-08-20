using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    public class Subgrafo
    {
        public int[] ORDENAMIENTO;
        Dictionary<int, Dictionary<int, double>> MATRIZ_ADYACENCIA;
        Vertice[] VERTICES;

        //VARIABLES DE CONTROL
        int NUMERO_VERTICES;
        public int VERTICES_ORDENADOS;

        //Variables de salida 
        bool ALEATORIO;

        public Dictionary<int, int> indices_intercambiados;

        public Subgrafo(
            Dictionary<int, Dictionary<int, double>> MATRIZ_ADYACENCIA,
            Vertice[] VERTICES,

            int cantidad_inicial,
            bool aleatorio
        )
        {
            this.NUMERO_VERTICES = VERTICES.Length;
            this.MATRIZ_ADYACENCIA = MATRIZ_ADYACENCIA;

            this.VERTICES = VERTICES;
            this.VERTICES_ORDENADOS = 0;
            ORDENAMIENTO = new int[NUMERO_VERTICES];
            
            Dictionary<int, int> nuevos_indices = new Dictionary<int, int>();
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                this.VERTICES[i].vertice_particion = i;
                nuevos_indices[this.VERTICES[i].vertice] = i;
            }

            ActualizarMatrizAdyacencia(nuevos_indices);
            indices_intercambiados = InvertirIndices(nuevos_indices);

            for (int i = 0; i < cantidad_inicial; i++)
            {
                etiquetar(VERTICES[i].vertice_particion);
            }

        }
        private Dictionary<int, int> InvertirIndices(Dictionary<int, int> original)
        {
            var invertido = new Dictionary<int, int>();
            foreach (var pair in original)
            {
                invertido[pair.Value] = pair.Key;
            }
            return invertido;
        }
        public void ActualizarMatrizAdyacencia(Dictionary<int, int> nuevos_indices)
        {
            // Crear una nueva matriz de adyacencia con las claves actualizadas
            var nuevaMatrizAdyacencia = new Dictionary<int, Dictionary<int, double>>();

            // Iterar sobre el diccionario original
            foreach (var entry in MATRIZ_ADYACENCIA)
            {
                int indice_original = entry.Key;
                int nuevo_indice = 0;
                nuevos_indices.TryGetValue(indice_original, out nuevo_indice);

                //Crear un nuevo diccionario para las adyacencias del nuevo vértice
                var nuevas_adyacencias = new Dictionary<int, double>();

                // Iterar sobre el diccionario interno
                foreach (var adyacencia in entry.Value)
                {
                    int vecino_original = adyacencia.Key;
                    int nuevo_vecino = 0;
                    nuevos_indices.TryGetValue(vecino_original, out nuevo_vecino);

                    // Agregar la adyacencia con las claves actualizadas
                    nuevas_adyacencias[nuevo_vecino] = adyacencia.Value;
                }

                //Agregar el nuevo diccionario de adyacencias a la nueva matriz
                nuevaMatrizAdyacencia[nuevo_indice] = nuevas_adyacencias;
            }

            //Reemplazar la matriz de adyacencia antigua por la nueva
            MATRIZ_ADYACENCIA = nuevaMatrizAdyacencia;            
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

                    if (idx_v >= VERTICES.Length) continue;

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
                if (v.visitado && !v.completado)
                {
                    //Disminuir el grado del vértice que presenta la conexión
                    //y al que está conectado (v...u)
                    VERTICES[idx_u].disminuirGrado();
                    VERTICES[idx_v].disminuirGrado();
                }
            }
        }
        private void etiquetar(int idx_u)
        {
            //Console.WriteLine($"2. {indices_intercambiados[idx_u]} {idx_u}");

            //if (VERTICES_ORDENADOS >= NUMERO_VERTICES) return;

            //Agregar el vertice al ordenamiento, colocar el índice en el objeto
            //e incrementar la cantidad de vértices ordenados
            ORDENAMIENTO[VERTICES_ORDENADOS] = idx_u;
            VERTICES[idx_u].etiquetar(VERTICES_ORDENADOS);

            //Recalcular el ancho de corte debido a la inserción del nuevo vértice en 
            //el ordenamiento
            //recalcularCorte(idx_u);

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
                int idx_v = vertices_candidatos[i].vertice_particion;
                VERTICES[idx_v].complejidad_adyacente = calcularGradoComplejidad(idx_v);

                //Obtener el vértice que represente una complejidad menor a todos los vértices
                //recibidos
                if (i == 0 || vertices_candidatos[i].complejidad_adyacente < complejidad_menor)
                {
                    complejidad_menor = vertices_candidatos[i].complejidad_adyacente;
                    idx_u_menor = vertices_candidatos[i].vertice_particion;
                }
            }

            return VERTICES[idx_u_menor];
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
        private Vertice seleccionarVertice_aleatorio(Vertice[] vertices)
        {
            //Retornar nulo, si no hay vértices disponibles
            if (vertices.Length == 0) return null;

            return vertices[new Random().Next(vertices.Length)];
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
                    this.etiquetar(VERTICES[i].vertice_particion);
                    vertices_etiquetados = true;
                }
                //Caso contrario, donde el grado es 1 y su conexión es recursiva;
                //es decir, consigo mismo, simplemente ordenarlo
                else if (VERTICES[i].grado == 1)
                {
                    Vertice u = VERTICES[i];

                    Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[i];
                    int idx_v = adyacencia_u.First().Key;

                    if (idx_v == VERTICES[i].vertice_particion)
                    {
                        this.etiquetar(idx_v);
                        vertices_etiquetados = true;
                    }
                }

            }

            return vertices_etiquetados;
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
        public int [] resolver()
        {
            bool resolucion_adyacente = resolverVerticesSinAdyacencia();

            //if (ALEATORIO)
            //{
            //    while (VERTICES_ORDENADOS < NUMERO_VERTICES)
            //    {
            //        Vertice[] vertices_adyacentes = getVerticesAdyacentes(ORDENAMIENTO);
            //        Vertice u = seleccionarVertice_aleatorio(vertices_adyacentes);
            //        etiquetar(u.vertice_particion);
            //    }
            //}
            //else
            {

                //En caso que se hayan resuelto los vértices que presentan conexiones recursivas
                //se forma una nueva población excluyendo los mencionados etiquetados
                Vertice[] poblacion = resolucion_adyacente ? getVerticesNoOrdenados() : VERTICES;

                bool EXISTE_ADYACENCIA = false;
                while (!EXISTE_ADYACENCIA)
                {
                    EXISTE_ADYACENCIA = true;

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
                        if (v != null)
                        {
                            this.etiquetar(v.vertice_particion);
                        }
                        else
                        {
                            EXISTE_ADYACENCIA = false;
                            poblacion = getVerticesNoOrdenados();

                            //Iniciar seleccionando un vértice que tiene un grado bajo
                            Vertice u = seleccionarVertice(poblacion);
                            this.etiquetar(u.vertice_particion);

                            break;
                        }
                    }
                }
            }


            //IDX_MEJOR = calcularAnchoCorte();
            //Console.WriteLine(VERTICES[IDX_MEJOR].ancho_corte);

            ////Console.WriteLine($"N.Vértices: {NUMERO_VERTICES}. N.Aristas: {NUMERO_ARISTAS}. Corte máximo: {CORTE_MAXIMO}. Tiempo de ejecución: {TIEMPO_RESOLUCION} segundos");
            
            return ORDENAMIENTO;
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
    }
}
