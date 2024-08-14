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

        int ITERACION_ACTUAL;
        int MAXIMO_ITERACIONES;

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
                MUESTRA_EVALUADA = new int[NUMERO_VERTICES, NUMERO_VERTICES];

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
           
            return true;
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

        private void generar_solucion_aleatoria(int idx_evaluacion)
        {
            List<int> particion1 = new List<int>();
            List<int> particion2 = new List<int>();

            Random rnd = new Random();
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                if (rnd.NextDouble() < 0.5)
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

        private int calcularAnchoCorte(int[] solucion)
        {
            int corteMaximo = 0;
            int corteActual = 0;
            int indiceCorte = 0;

            for (int p = 0; p < NUMERO_VERTICES; p++)
            {
                corteActual = evaluarParticion(solucion, p);

                if (corteActual > corteMaximo)
                {
                    corteMaximo = corteActual;
                    indiceCorte = p;
                }
            }

            return corteMaximo;
        }
        private int evaluarParticion(int [] solucion, int p)
        {
            int corte = 0;
            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                for (int v = u + 1; v < NUMERO_VERTICES; v++)
                {
                    if (MATRIZ_ADYACENCIA[u, v] > 0 && cruzaParticion(solucion, u, v, p))
                    {
                        corte++;
                    }
                }
            }

            return corte;
        }

        private int evaluarParticion(int idx_muestra, int p)
        {
            int corte = 0;
            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                for (int v = u + 1; v < NUMERO_VERTICES; v++)
                {
                    if (MATRIZ_ADYACENCIA[u, v] > 0 && cruzaParticion(idx_muestra, u, v, p))
                    {
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
        private bool cruzaParticion(int[] solucion, int u, int v, int particion)
        {
            //Verificar si u y v están en diferentes particiones
            return (solucion[u] < particion && solucion[v] >= particion)
                || (solucion[v] < particion && solucion[u] >= particion);
        }

        private void intercambiar_vertices(int[] solucion)
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
            int temp = solucion[idx1];
            solucion[idx1] = solucion[idx2];
            solucion[idx2] = temp;
        }

        private int[] obtener_fila(int fila)
        {
            int[] solucion = new int[NUMERO_VERTICES];
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                solucion[i] = MUESTRA_EVALUADA[fila, i];
            }
            return solucion;
        }

        private void simulated_annealing()
        {
            Random rnd = new Random();
            double temperatura = 100.0;
            double tasa_enfriamiento = 0.75;
            double temp_minima = 0.10;

            int iteracion = 0;

            int mejorCorte = calcularAnchoCorte(ITERACION_ACTUAL - 1);
            int[] mejorSolucion = obtener_fila(ITERACION_ACTUAL - 1);

            while (temperatura > temp_minima || iteracion < MAXIMO_ITERACIONES)
            {
                iteracion++;

                // Generar una solución vecina
                int[] nuevaSolucion = (int[])mejorSolucion.Clone();
                intercambiar_vertices(nuevaSolucion);

                // Evaluar la nueva solución
                int nuevoCorte = calcularAnchoCorte(nuevaSolucion);

                // Decidir si aceptar la nueva solución
                if (nuevoCorte < mejorCorte || rnd.NextDouble() < Math.Exp((mejorCorte - nuevoCorte) / temperatura))
                {
                    mejorCorte = nuevoCorte;
                    mejorSolucion = nuevaSolucion;
                }

                // Reducir la temperatura
                temperatura *= tasa_enfriamiento;
            }

            //Guardar la mejor solución encontrada
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                MUESTRA_EVALUADA[ITERACION_ACTUAL, i] = mejorSolucion[i];
            }
        }

        public void resolver(string ruta_archivo)
        {
            if (!parsear_problema(ruta_archivo))
            {
                Console.WriteLine("ERROR");
                return;
            }

            int idx_mejor_solucion = 0;
            int corte_mejor_solucion = 0;            

            generar_solucion_aleatoria(ITERACION_ACTUAL);
            corte_mejor_solucion = calcularAnchoCorte(ITERACION_ACTUAL);
            ITERACION_ACTUAL++;

            while (ITERACION_ACTUAL < 2)
            {
                simulated_annealing();
                int ancho_corte = calcularAnchoCorte(ITERACION_ACTUAL);

                if (ancho_corte < corte_mejor_solucion)
                {
                    corte_mejor_solucion = ancho_corte;
                    idx_mejor_solucion = ITERACION_ACTUAL;
                }

                ITERACION_ACTUAL++;
            }

            mostrar_solucion(idx_mejor_solucion);
            Console.WriteLine(corte_mejor_solucion + "");
        }


        private void mostrar_solucion(int idx)
        {
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                Console.WriteLine(MUESTRA_EVALUADA[idx, i] + "");
            }
        }
    }
}
