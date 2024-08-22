using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    class CWPC
    {
        int[,] MATRIZ_ADYACENCIA;

        //ARREGLO QUE CONTIENE LA ADYACENCIA DE CADA VÉRTICE
        //SE FORMA A PARTIR DE LA MATRIZ DE ADYACENCIA
        int[] VERTICES_ADYACENTES;

        //VARIABLES DE CONTROL
        int NUMERO_VERTICES;
        int NUMERO_ARISTAS;

        //ARREGLO QUE CONTIENE TODOS LOS VÉRTICES DEL PROBLEMA
        Vertice[] VERTICES;

        //ORDENAMIENTO DE SALIDA (SOLO CONTIENE LOS ÍNDICES DE LOS VÉRTICES)
        int[,] SOLUCIONES;
        int[] CORTES_SOLUCION;
        int[,] MUESTRA_EVALUADA;

        //CONTROL DE ITERACIONES
        int NUMERO_SOLUCIONES;
        int SOLUCION_ACTUAL;
        int NUMERO_COMBINACIONES;
        int ITERACION_ACTUAL;

        //SOLUCIONES
        int IDX_MEJOR;
        int IDX_PEOR;

        public CWPC()
        {
            ITERACION_ACTUAL = 0;
            NUMERO_SOLUCIONES = 10;
            NUMERO_COMBINACIONES = 10;
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

                //Si no existen vertices en el arreglo, entonces no hay nada que resolver
                if (NUMERO_VERTICES < 1 || NUMERO_ARISTAS < 1)
                {
                    Console.WriteLine("El número de vertices y aristas debe ser mayor que 0.");
                    return false;
                }

                //Inicializar las variables para la resolución
                MATRIZ_ADYACENCIA = new int[NUMERO_VERTICES, NUMERO_VERTICES];
                VERTICES = new Vertice[NUMERO_VERTICES];

                SOLUCIONES = new int[NUMERO_COMBINACIONES, NUMERO_VERTICES];
                CORTES_SOLUCION = new int[NUMERO_COMBINACIONES];

                //Inicializar los objetos de tipo vértice
                for (int i = 0; i < NUMERO_VERTICES; i++)
                {
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
                aumentarAdyacencia(u, v);

                linea_actual++;
            }

            //Cerrar el lector
            sreader.Close();

            //Calcular la adyacencia entre los vértices para almacenarla
            procesarAdyacencia();

            return true;
        }
        private void aumentarAdyacencia(int u, int v)
        {
            //Incrementar la adyacencia en la matriz según los
            //vertices especificados
            if (u != v)
            {
                VERTICES[u].peso += 1;
                VERTICES[v].peso += 1;

                VERTICES[u].grado += 1;
                VERTICES[v].grado += 1;

                MATRIZ_ADYACENCIA[u, v]++;
                MATRIZ_ADYACENCIA[v, u]++;
            }
            else
            {
                VERTICES[u].peso += 1;
                VERTICES[u].grado += 1;
                MATRIZ_ADYACENCIA[u, v]++;
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
        private void generar_solucion_aleatoria(int idx_evaluacion, float alpha)
        {
            List<int> particion1 = new List<int>();
            List<int> particion2 = new List<int>();

            Random rnd = new Random();
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                if (rnd.NextDouble() < alpha)
                    particion1.Add(i);
                else
                    particion2.Add(i);
            }


            for (int i = 0; i < particion1.Count; i++)
            {
                SOLUCIONES[idx_evaluacion, i] = particion1[i];
            }
            for (int i = 0; i < particion2.Count; i++)
            {
                SOLUCIONES[idx_evaluacion, i + particion1.Count] = particion2[i];
            }           
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
        private int calcularAnchoCorte(int[,] opciones, int indice)
        {
            int corteMaximo = 0;
            int corteActual = 0;
            int indiceCorte = 0;

            for (int p = 0; p < NUMERO_VERTICES; p++)
            {
                corteActual = evaluarParticion(opciones, indice, p);

                if (corteActual > corteMaximo)
                {
                    corteMaximo = corteActual;
                    indiceCorte = p;
                }
            }

            return corteMaximo;
        }
        private int evaluarParticion(int [,] opciones, int indice, int particion)
        {
            int corte = 0;
            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                //Índice de inicio en el vector
                int indiceInicio = VERTICES[u].indice_inicio_lista;

                //Cantidad de elementos a partir del índice de inicio
                int grado = VERTICES[u].grado;

                for (int i = 0; i < grado; i++)
                {
                    int v = VERTICES_ADYACENTES[indiceInicio + i];

                    //Ya que el grafo es no dirigido las aristas son dobles, la siguiente
                    //condición evita realizar revisiones dobles
                    if (v > u && cruzaParticion(opciones, indice, u, v, particion))
                    {
                        //Sí los vértice se encuentran en lados diferentes de la partición
                        //entonces se incrementa el número de cortes                        
                        corte++;
                    }
                }
            }
            return corte;
        }
        private int evaluarParticion(int idx_muestra, int particion)
        {
            int corte = 0;
            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                //Índice de inicio en el vector
                int indiceInicio = VERTICES[u].indice_inicio_lista;

                //Cantidad de elementos a partir del índice de inicio
                int grado = VERTICES[u].grado;

                for (int i = 0; i < grado; i++)
                {
                    int v = VERTICES_ADYACENTES[indiceInicio + i];

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
            return (MUESTRA_EVALUADA[idx_muestra, u] < particion && MUESTRA_EVALUADA[idx_muestra, v] >= particion)
                || (MUESTRA_EVALUADA[idx_muestra, v] < particion && MUESTRA_EVALUADA[idx_muestra, u] >= particion);
        }
        private bool cruzaParticion(int [,] opciones, int indice, int u, int v, int particion)
        {
            //Verificar si u y v están en diferentes particiones
            return (opciones[indice, u] < particion && opciones[indice, v] >= particion)
                || (opciones[indice, v] < particion && opciones[indice, u] >= particion);
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
            int temp = MUESTRA_EVALUADA[idx, idx1];
            MUESTRA_EVALUADA[idx, idx1] = MUESTRA_EVALUADA[idx, idx2];
            MUESTRA_EVALUADA[idx, idx2] = temp;
        }

        private void simulated_annealing(int iteracion)
        {
            MUESTRA_EVALUADA = new int[NUMERO_COMBINACIONES, NUMERO_VERTICES];
            int[] CORTES_MUESTRA = new int[NUMERO_COMBINACIONES];

            Random rnd = new Random();
            double temperatura = 10000.0;
            double tasa_enfriamiento = 0.75;
            double temp_minima = 0.10;


            ITERACION_ACTUAL = 0;           

            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                MUESTRA_EVALUADA[ITERACION_ACTUAL, i] = SOLUCIONES[iteracion, i];
            }

            CORTES_MUESTRA[ITERACION_ACTUAL] = calcularAnchoCorte(MUESTRA_EVALUADA, iteracion);

            int mejorCorte = CORTES_MUESTRA[ITERACION_ACTUAL];
            
            int MEJOR_CORTE = CORTES_MUESTRA[ITERACION_ACTUAL];
            int IDX_TEMP = 0;
            
            ITERACION_ACTUAL++;

            while (
                temperatura > temp_minima &&
                ITERACION_ACTUAL < NUMERO_COMBINACIONES)
            {

                //Colocar inicialmente para la nueva solución los vértices iguales
                //a la solución temporal
                for (int i = 0; i < NUMERO_VERTICES; i++)
                {
                    MUESTRA_EVALUADA[ITERACION_ACTUAL, i] = MUESTRA_EVALUADA[IDX_TEMP, i];
                }

                //Generar una solución vecina
                intercambiar_vertices(ITERACION_ACTUAL);

                //Evaluar la nueva solución
                CORTES_MUESTRA[ITERACION_ACTUAL] = calcularAnchoCorte(ITERACION_ACTUAL);

                //Guardar el índice de la mejor combinación
                if (CORTES_MUESTRA[ITERACION_ACTUAL] < CORTES_MUESTRA[IDX_MEJOR])
                {
                    mejorCorte = CORTES_MUESTRA[ITERACION_ACTUAL];
                    IDX_TEMP = ITERACION_ACTUAL;
                    IDX_MEJOR = ITERACION_ACTUAL;
                }

                if (mejorCorte < MEJOR_CORTE)
                {
                    MEJOR_CORTE = mejorCorte;
                    IDX_MEJOR = ITERACION_ACTUAL;
                    Console.WriteLine($"iteracion -> {iteracion} -> {mejorCorte}");
                }

                //Guardar el índice de la peor combinación
                if (CORTES_MUESTRA[ITERACION_ACTUAL] > CORTES_MUESTRA[IDX_PEOR])
                {
                    IDX_PEOR = ITERACION_ACTUAL;
                }

                //Decidir si aceptar la nueva solución para explorar combinaciones vecinas
                if (rnd.NextDouble() < Math.Exp((mejorCorte - CORTES_MUESTRA[ITERACION_ACTUAL]) / temperatura))
                {
                    mejorCorte = CORTES_MUESTRA[ITERACION_ACTUAL];
                    IDX_TEMP = ITERACION_ACTUAL;
                }

                //Reducir la temperatura
                temperatura *= tasa_enfriamiento;
                ITERACION_ACTUAL++;
            }

            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                SOLUCIONES[iteracion, i] = MUESTRA_EVALUADA[IDX_MEJOR, i];
            }
            CORTES_SOLUCION[iteracion] = MEJOR_CORTE;
        }


        public void resolver(string ruta_archivo, string metodo)
        {
            //Leer el archivo
            if (!parsear_problema(ruta_archivo))
            {
                Console.WriteLine("ERROR");
                return;
            }

            //Generar las soluciones
            SOLUCIONES = new int[NUMERO_SOLUCIONES, NUMERO_VERTICES];

            for (int i = 0; i < NUMERO_SOLUCIONES; i++)
            {
                generar_solucion_aleatoria(i, 0.5f);
            }

            resolver_por_sa();            
        }

        private void resolver_por_sa()
        {
            for (int i = 0; i < NUMERO_SOLUCIONES; i++)
            {
                simulated_annealing(i);
                Console.WriteLine(calcularAnchoCorte(SOLUCIONES, i));
            }
            
            int menor_corte = int.MaxValue;
            int indice = 0;
            for (int i = 0; i < NUMERO_SOLUCIONES; i++)
            {
                if (CORTES_SOLUCION[i] < menor_corte)
                {
                    menor_corte = CORTES_SOLUCION[i];
                    indice = i;
                }
            }

            mostrar_solucion(indice);
            //mostrar_solucion(IDX_PEOR);
        }


        private void mostrar_solucion(int idx)
        {
            //for (int i = 0; i < NUMERO_VERTICES; i++)
            //{
            //    Console.WriteLine(SOLUCIONES[idx, i] + "");
            //}
            Console.WriteLine($"CORTE OBTENIDO = {CORTES_SOLUCION[idx]}");
        }
    }
}
