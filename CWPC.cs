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
        int[,] MUESTRA_EVALUADA;
        int[] CORTES_MUESTRA;

        //CONTROL DE ITERACIONES
        int ITERACION_ACTUAL;
        int MAXIMO_ITERACIONES;

        //SOLUCIONES
        int IDX_MEJOR;
        int IDX_PEOR;

        public CWPC()
        {
            ITERACION_ACTUAL = 0;
            MAXIMO_ITERACIONES = 100;
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

                MUESTRA_EVALUADA = new int[MAXIMO_ITERACIONES, NUMERO_VERTICES];
                CORTES_MUESTRA = new int[MAXIMO_ITERACIONES];

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
        private void generar_solucion_aleatoria(int idx_evaluacion, float alfa)
        {
            List<int> particion1 = new List<int>();
            List<int> particion2 = new List<int>();

            Random rnd = new Random();
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                if (rnd.NextDouble() < alfa)
                    particion1.Add(i);
                else
                    particion2.Add(i);
            }


            for (int i = 0; i < particion1.Count; i++)
            {
                MUESTRA_EVALUADA[idx_evaluacion, i] = particion1[i];
            }
            for (int i = 0; i < particion2.Count; i++)
            {
                MUESTRA_EVALUADA[idx_evaluacion, i + particion1.Count] = particion2[i];
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
            
            //int corte = 0;
            //for (int u = 0; u < NUMERO_VERTICES; u++)
            //{
            //    for (int v = u + 1; v < NUMERO_VERTICES; v++)
            //    {
            //        if (MATRIZ_ADYACENCIA[u, v] > 0 && cruzaParticion(idx_muestra, u, v, p))
            //        {
            //            corte++;
            //        }
            //    }
            //}

            //return corte;
        }

        private bool cruzaParticion(int idx_muestra, int u, int v, int particion)
        {
            //Verificar si u y v están en diferentes particiones
            return (MUESTRA_EVALUADA[idx_muestra, u] < particion && MUESTRA_EVALUADA[idx_muestra, v] >= particion)
                || (MUESTRA_EVALUADA[idx_muestra, v] < particion && MUESTRA_EVALUADA[idx_muestra, u] >= particion);
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

       
        private void GRASP()
        {
            float alpha = 0.5f;
            float factor_crecimiento = 0.8f;
            float factor_decaimiento = 0.6f;

            int mejor_corte = CORTES_MUESTRA[ITERACION_ACTUAL - 1];

            while(ITERACION_ACTUAL < MAXIMO_ITERACIONES)
            {
                generar_solucion_aleatoria(ITERACION_ACTUAL, alpha);
                intercambiar_vertices(ITERACION_ACTUAL);

                //Evaluar la solución
                CORTES_MUESTRA[ITERACION_ACTUAL] = calcularAnchoCorte(ITERACION_ACTUAL);

                // Actualizar la mejor solución encontrada
                if (CORTES_MUESTRA[ITERACION_ACTUAL] < CORTES_MUESTRA[IDX_MEJOR])
                {
                    alpha *= factor_crecimiento;
                    mejor_corte = CORTES_MUESTRA[ITERACION_ACTUAL];
                    IDX_MEJOR = ITERACION_ACTUAL;
                }

                if (CORTES_MUESTRA[ITERACION_ACTUAL] > CORTES_MUESTRA[IDX_MEJOR])
                {
                    alpha *= factor_decaimiento;
                    IDX_PEOR = ITERACION_ACTUAL;
                }

                ITERACION_ACTUAL++;                
            }
        }

        private void simulated_annealing()
        {
            Random rnd = new Random();
            double temperatura = 10000.0;
            double tasa_enfriamiento = 0.75;
            double temp_minima = 0.10;

            int mejorCorte = CORTES_MUESTRA[ITERACION_ACTUAL- 1];
            int IDX_TEMP = 0;

            while (
                temperatura > temp_minima &&
                ITERACION_ACTUAL < MAXIMO_ITERACIONES)
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
        }

        public void resolver(string ruta_archivo, string metodo)
        {
            if (metodo == "grasp")
            {
                resolver_por_grasp(ruta_archivo);
            }
            else
            {
                resolver_por_sa(ruta_archivo);
            }
        }

        private void resolver_por_grasp(string ruta_archivo)
        {
            if (!parsear_problema(ruta_archivo))
            {
                Console.WriteLine("ERROR");
                return;
            }

            generar_solucion_aleatoria(ITERACION_ACTUAL, 0.5f);
            CORTES_MUESTRA[ITERACION_ACTUAL] = calcularAnchoCorte(ITERACION_ACTUAL);
            IDX_MEJOR = 0;
            ITERACION_ACTUAL++;

            GRASP();

            mostrar_solucion(IDX_MEJOR);
            //mostrar_solucion(IDX_PEOR);
        }

        private void resolver_por_sa(string ruta_archivo)
        {
            if (!parsear_problema(ruta_archivo))
            {
                Console.WriteLine("ERROR");
                return;
            }

            generar_solucion_aleatoria(ITERACION_ACTUAL, 0.5f);
            CORTES_MUESTRA[ITERACION_ACTUAL] = calcularAnchoCorte(ITERACION_ACTUAL);
            IDX_MEJOR = 0;
            ITERACION_ACTUAL++;

            simulated_annealing();

            mostrar_solucion(IDX_MEJOR);
            //mostrar_solucion(IDX_PEOR);
        }


        private void mostrar_solucion(int idx)
        {
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                Console.WriteLine(MUESTRA_EVALUADA[idx, i] + "");
            }
            Console.WriteLine($"CORTE OBTENIDO = {CORTES_MUESTRA[idx]}");
        }
    }
}
