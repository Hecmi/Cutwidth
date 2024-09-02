using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP.CWP_Combinacional
{
    class CWP
    {
        Grafo GRAFO;

        //REFERENCIAS PARA ACCEDER RAPIDAMENTE A LAS VARIABLES DEL GRAFO
        Dictionary<int, Dictionary<int, double>> MATRIZ_ADYACENCIA => GRAFO.MATRIZ_ADYACENCIA;
        public int NUMERO_VERTICES => GRAFO.NUMERO_VERTICES;
        int NUMERO_ARISTAS => GRAFO.NUMERO_ARISTAS;
        Vertice[] VERTICES => GRAFO.VERTICES;

        //VARIABLES DE SALIDA
        Combinacion MEJOR_COMBINACION;
        double MEJOR_CORTE;
        double TIEMPO_RESOLUCION;

        //VARIABLES DEFINIDAS POR EL USUARIO
        int NUMERO_MUESTRA;
        double TEMPERATURA_INICIAL;
        double TEMPERATURA;
        double TASA_ENFRIAMIENTO;

        string RUTA_ARCHIVO;
        string RUTA_SALIDA;
        bool GUARDAR_SALIDA;
        bool MOSTRAR_ORDENAMIENTO;

        public CWP(string ruta_archivo, char delimitador, bool mostrar_ordenamiento, string ruta_salida, int num_muestra, double t0, double t1)
        {
            //Inicializar las variables definidas por el usuario
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            RUTA_SALIDA = ruta_salida;
            RUTA_ARCHIVO = ruta_archivo;

            GUARDAR_SALIDA = !string.IsNullOrEmpty(RUTA_SALIDA);

            this.NUMERO_MUESTRA = num_muestra;
            this.TEMPERATURA_INICIAL = t0;
            this.TEMPERATURA = t1;

            //Lee el archivo, envíando por referencia el grafo no instanciado
            Archivo.procesar_archivo(ref GRAFO, ruta_archivo, delimitador);
        }

        private int[] generar_solucion_aleatoria(float a)
        {
            List<int> p1 = new List<int>();
            List<int> p2 = new List<int>();

            int[] s = new int[NUMERO_VERTICES];

            Random rnd = new Random();
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                if (rnd.NextDouble() < a)
                    p1.Add(i);
                else
                    p2.Add(i);
            }

            for (int i = 0; i < p1.Count; i++)
            {
                s[i] = p1[i];
            }
            for (int i = 0; i < p2.Count; i++)
            {
                s[i + p1.Count] = p2[i];
            }


            for (int i = 0; i < NUMERO_VERTICES / 2; i++)
            {
                //Elegir dos índices aleatorios para intercambiar
                int idx1 = rnd.Next(NUMERO_VERTICES);
                int idx2;
                do
                {
                    idx2 = rnd.Next(NUMERO_VERTICES);
                } while (idx1 == idx2);

                //Intercambiar los vértices
                int temp = s[idx1];
                s[idx1] = s[idx2];
                s[idx2] = temp;
            }

            return s;
        }

        public double calcularAnchoCorte(Combinacion c)
        {
            double corte_maximo = 0;
            double corte_actual = 0;
            int indice_max_corte = 0;

            for (int p = 1; p < NUMERO_VERTICES; p++)
            {
                corte_actual = evaluarParticion(c.s_actual, p);

                //Incrementar el total del corte del ordenamiento de
                //la combinación evaluada
                c.total_corte += corte_actual;

                if (corte_actual > corte_maximo)
                {
                    corte_maximo = corte_actual;
                    indice_max_corte = p;
                }
            }

            c.cw = corte_maximo;
            c.indice_cw = indice_max_corte;
            
            return corte_maximo;
        }

        private double evaluarParticion(int[] ordenamiento, int particion)
        {
            double corte = 0;
            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                //Adyacencia del vértice u
                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[ordenamiento[u]];

                foreach (var conexion in adyacencia_u)
                {
                    int v = Array.IndexOf(ordenamiento, conexion.Key);

                    //Ya que el grafo es no dirigido las aristas son dobles, la siguiente
                    //condición evita realizar revisiones dobles
                    if (v > u && cruzaParticion(ordenamiento, u, v, particion))
                    {
                        //Sí los vértice se encuentran en lados diferentes de la partición
                        //entonces se incrementa el ancho de corte según el peso de la arista
                        corte += conexion.Value;
                    }
                }
            }

            return corte;
        }

        private bool cruzaParticion(int[] ordenamiento, int u, int v, int particion)
        {
            //Verificar si u y v están en diferentes particiones
            return (u < particion && v >= particion)
                || (v < particion && u >= particion);
        }

        public double recalcular_corte(Combinacion c)
        {
            int idx_corte_max = -1;
            double[] cortes = new double[NUMERO_VERTICES];
            double corte_maximo = 0;

            for (int i = 0;  i < NUMERO_VERTICES; i++){
                int idx_u = c.s_actual[i];

                //Obtener los vértices adyacentes a u
                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[idx_u];

                //Recorrer la adyacencia para recalcular el corte a partir del índice
                //de las conexiones
                foreach (var conexion in adyacencia_u)
                {
                    //Obtener el vértice v
                    int idx_v = Array.IndexOf(c.s_actual, conexion.Key);
                    
                    for (int idx = i; idx < idx_v; idx++)
                    {
                        cortes[idx + 1] += conexion.Value;

                        //Al incrementar el ancho de corte, determinar si es el corte que 
                        //representa el valor máximo
                        if (cortes[idx + 1] > corte_maximo)
                        {
                            corte_maximo = cortes[idx + 1];
                            idx_corte_max = i;
                        }
                    }
                }
            }
            //Console.WriteLine($"EL CORTE MAX  = {corte_maximo}, INDICE = {idx_corte_max}");

            c.cw = corte_maximo;
            c.indice_cw = idx_corte_max;

            return corte_maximo;
        }

        private void intercambiar_vertices(Combinacion c)
        {
            Random rnd = new Random();

            //Elegir dos índices aleatorios para intercambiar
            int idx1 = rnd.Next(NUMERO_VERTICES);
            int idx2;
            do
            {
                idx2 = rnd.Next(NUMERO_VERTICES);
            } while (idx1 == idx2);

            //Intercambiar los índices
            c.s_i = c.s_actual;

            int temp = c.s_i[idx1];
            c.s_i[idx1] = c.s_i[idx2];
            c.s_i[idx2] = temp;
        }

        private int[] generar_solucion_aleatoria_()
        {
            List<int> indices = Enumerable.Range(0, NUMERO_VERTICES).ToList();
            Random rnd = new Random();

            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                int temp = indices[i];
                indices[i] = indices[j];
                indices[j] = temp;
            }

            return indices.ToArray();
        }
        
        public void resolver()
        {
            //Inicializar las variables
            Combinacion[] muestra = new Combinacion[NUMERO_MUESTRA];
            MEJOR_COMBINACION = new Combinacion(NUMERO_VERTICES);

            //Variable global que representa el mejor valor de ajuste
            MEJOR_CORTE = Double.MaxValue;

            //Variables correspondientes a los cortes actuales por cada iteración
            double mejor_corte_actual = Double.MaxValue;
            double corte_actual = 0;

            //Variables utilizadas para menejar el proceso estocástico del algoritmo
            Random rnd = new Random();

            //Variables para el control iterativo
            double TEMPERATURA_ = TEMPERATURA;
            TASA_ENFRIAMIENTO = 0.99;

            //Iterar hasta el número de muestras especificados
            for (int i = 0; i < NUMERO_MUESTRA; i++)
            {
                //Inicializar le objeto para su evaluación
                muestra[i] = new Combinacion(NUMERO_VERTICES);

                //Generar la solución aleatoria para las muestras definidas
                //y aplicar la función objetivo
                muestra[i].s_actual = generar_solucion_aleatoria_();
                corte_actual = recalcular_corte(muestra[i]);
                mejor_corte_actual = corte_actual;

                //Verificar si el corte actual es mejor que el global
                if (corte_actual < MEJOR_CORTE)
                {
                    Console.WriteLine($"{String.Join(',', muestra[i].s_actual)} - {corte_actual}");
                    MEJOR_CORTE = corte_actual;
                }

                TEMPERATURA = TEMPERATURA_;
                while (TEMPERATURA > TEMPERATURA_INICIAL)
                {
                    //Intercambiar el orden de los vértices de la muestra actual
                    //(esto implica intercambiar el arreglo del objeto)
                    intercambiar_vertices(muestra[i]);
                    corte_actual = recalcular_corte(muestra[i]);

                    //Verificar si el corte actual es mejor que el global
                    if (corte_actual < mejor_corte_actual)
                    {
                        mejor_corte_actual = corte_actual;
                        muestra[i].s_actual = muestra[i].s_i;

                        if (corte_actual < MEJOR_CORTE)
                        {
                            MEJOR_CORTE = corte_actual;

                            MEJOR_COMBINACION = muestra[i];
                            MEJOR_COMBINACION.s_actual = muestra[i].s_actual;
                            MEJOR_COMBINACION.cw = MEJOR_CORTE;
                            Console.WriteLine(MEJOR_CORTE);
                        }
                    }

                    double p = Math.Exp((mejor_corte_actual - corte_actual) / TEMPERATURA);
                    double r = rnd.NextDouble();

                    //Verificar si se acepta la solución actual independientemente
                    //si es mejor o peor a la mejor global
                    if (p > r)
                    {
                        muestra[i].s_actual= muestra[i].s_i;
                        mejor_corte_actual = corte_actual;
                    }      

                    //Decrementar la temperatura
                    TEMPERATURA = TEMPERATURA * TASA_ENFRIAMIENTO;
                }
            }

            calcularAnchoCorte(MEJOR_COMBINACION);
            Console.WriteLine($"MUESTRA = {String.Join(',', MEJOR_COMBINACION.s_actual)}, CORTE = {MEJOR_CORTE}");
        }

        public int [] get_ordenamiento()
        {
            return MEJOR_COMBINACION.s_actual;
        }
    }
}
