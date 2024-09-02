using CWP.Clases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP.CWP_Combinacional
{
    class CW
    {
        Grafo GRAFO;

        //REFERENCIAS PARA ACCEDER RAPIDAMENTE A LAS VARIABLES DEL GRAFO
        Dictionary<int, Dictionary<int, double>> MATRIZ_ADYACENCIA => GRAFO.MATRIZ_ADYACENCIA;
        int NUMERO_VERTICES => GRAFO.NUMERO_VERTICES;
        int NUMERO_ARISTAS => GRAFO.NUMERO_ARISTAS;
        Vertice[] VERTICES => GRAFO.VERTICES;

        //VARIABLES DE SALIDA
        int IDX_MEJOR;
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

        public CW(string ruta_archivo, char delimitador, bool mostrar_ordenamiento, string ruta_salida, int num_muestra, double t0, double t1)
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

        private int [] generar_solucion_aleatoria(float a)
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

        private double calcularAnchoCorte(Combinacion c)
        {
            double corte_maximo = 0;
            double corte_actual = 0;
            int indice_max_corte = 0;

            for (int p = 0; p < NUMERO_VERTICES; p++)
            {
                corte_actual = evaluarParticion(c.s_i, p);

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
        private double evaluarParticion(int [] ordenamiento, int particion)
        {
            double corte = 0;
            for (int u = 0; u < NUMERO_VERTICES; u++)
            {
                //Adyacencia del vértice u
                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[u];

                foreach (var conexion in adyacencia_u)
                {
                    int v = conexion.Key;

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

        private bool cruzaParticion(int [] ordenamiento, int u, int v, int particion)
        {
            //Verificar si u y v están en diferentes particiones
            return (ordenamiento[u] < particion && ordenamiento[v] >= particion)
                || (ordenamiento[v] < particion && ordenamiento[u] >= particion);
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

            //Intercambiar los vértices
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
            Combinacion mejor_combinacion = new Combinacion(NUMERO_VERTICES);

            //Variables utilizadas para menejar el proceso estocástico del algoritmo
            MEJOR_CORTE = Double.MaxValue;
            double mejor_corte_actual = Double.MaxValue;
            double corte_actual = 0;

            Random rnd = new Random();
            double TEMPERATURA_ = TEMPERATURA;
            TASA_ENFRIAMIENTO = 0.8;


            //Iterar hasta el número de muestras especificados
            for (int i = 0; i < NUMERO_MUESTRA; i++)
            {
                //Generar la solución aleatoria para las muestras definidas (ord0)
                muestra[i] = new Combinacion(NUMERO_VERTICES);
                muestra[i].s_actual = generar_solucion_aleatoria_();
                Array.Copy(muestra[i].s_actual, muestra[i].s_i, NUMERO_VERTICES);

                //Definir el mejor corte de la iteración o evaluación a la muestra actual
                corte_actual = calcularAnchoCorte(muestra[i]);

                //Verificar si el corte actual es mejor que el global
                if (corte_actual < MEJOR_CORTE)
                {
                    mejor_corte_actual = corte_actual;
                    MEJOR_CORTE = corte_actual;
                }

                //Mientras la temperatura (t1) sea mayor que la min (t0), desarrollar el
                //recido simulado
                TEMPERATURA = TEMPERATURA_;
                while (TEMPERATURA > TEMPERATURA_INICIAL)
                {
                    //Intercambiar el orden de los vértices de la muestra actual
                    //(esto implica intercambiar el arreglo del objeto ord1)
                    intercambiar_vertices(muestra[i]);

                    corte_actual = calcularAnchoCorte(muestra[i]);

                    //Console.WriteLine($"MUESTRA {i} = {String.Join(',', muestra[i].ord1)}, CORTE = {corte_actual}");                    
                    //Verificar si el corte actual es mejor que el global
                    if (corte_actual < MEJOR_CORTE)
                    {
                        muestra[i].s_actual = muestra[i].s_i;
                        mejor_combinacion = muestra[i];
                        mejor_corte_actual = corte_actual;
                        MEJOR_CORTE = corte_actual;
                    }

                    //Verificar si se acepta la solución actual independientemente si es mejor o peor
                    //a la mejor global
                    if (rnd.NextDouble() < Math.Exp((mejor_corte_actual - corte_actual) / TEMPERATURA))
                    {
                        mejor_corte_actual = corte_actual;
                    }
                    else
                    {
                        muestra[i].s_i = muestra[i].s_actual;
                    }


                    //Decrementar la temperatura
                    TEMPERATURA = TEMPERATURA * TASA_ENFRIAMIENTO;
                }                
            }

            Console.WriteLine($"MUESTRA = {String.Join(',', mejor_combinacion.s_actual)}, CORTE = {MEJOR_CORTE}");
        }

        public void resolver_2()
        {
            //Inicializar las variables
            Combinacion[] muestra = new Combinacion[NUMERO_MUESTRA];
            Combinacion mejor_combinacion = new Combinacion(NUMERO_VERTICES);

            //Variable global que representa el mejor valor de ajuste
            MEJOR_CORTE = Double.MaxValue;

            //Variables correspondientes a los cortes actuales por cada iteración
            double mejor_corte_actual = Double.MaxValue;
            double corte_actual = 0;

            //Variables utilizadas para menejar el proceso estocástico del algoritmo
            Random rnd = new Random();

            //Variables para el control iterativo
            double TEMPERATURA_ = TEMPERATURA;
            TASA_ENFRIAMIENTO = 0.8;


            //Iterar hasta el número de muestras especificados
            for (int i = 0; i < NUMERO_MUESTRA; i++)
            {
                //Inicializar le objeto para su evaluación
                muestra[i] = new Combinacion(NUMERO_VERTICES);

                //Generar la solución aleatoria para las muestras definidas
                muestra[i].s_actual = generar_solucion_aleatoria_();
                muestra[i].s_i = muestra[i].s_actual;

                //Definir el mejor corte de la iteración o evaluación a la muestra actual
                corte_actual = calcularAnchoCorte(muestra[i]);

                //Verificar si el corte actual es mejor que el global
                if (corte_actual < MEJOR_CORTE)
                {
                    mejor_corte_actual = corte_actual;
                    MEJOR_CORTE = corte_actual;
                }

                //Mientras la temperatura (t1) sea mayor que la min (t0), desarrollar el
                //recido simulado
                TEMPERATURA = TEMPERATURA_;
                while (TEMPERATURA > TEMPERATURA_INICIAL)
                {
                    //Intercambiar el orden de los vértices de la muestra actual
                    //(esto implica intercambiar el arreglo del objeto)
                    intercambiar_vertices(muestra[i]);

                    corte_actual = calcularAnchoCorte(muestra[i]);

                    //Console.WriteLine($"MUESTRA {i} = {String.Join(',', muestra[i].ord1)}, CORTE = {corte_actual}");                    
                    //Verificar si el corte actual es mejor que el global
                    if (corte_actual < MEJOR_CORTE)
                    {
                        muestra[i].s_actual = muestra[i].s_i;
                        mejor_combinacion = muestra[i];
                        mejor_corte_actual = corte_actual;
                        MEJOR_CORTE = corte_actual;
                    }

                    //Verificar si se acepta la solución actual independientemente si es mejor o peor
                    //a la mejor global
                    if (rnd.NextDouble() < Math.Exp((mejor_corte_actual - corte_actual) / TEMPERATURA))
                    {
                        mejor_corte_actual = corte_actual;
                    }
                    else
                    {
                        muestra[i].s_i = muestra[i].s_actual;
                    }


                    //Decrementar la temperatura
                    TEMPERATURA = TEMPERATURA * TASA_ENFRIAMIENTO;
                }
            }

            Console.WriteLine($"MUESTRA = {String.Join(',', mejor_combinacion.s_actual)}, CORTE = {MEJOR_CORTE}");
        }
    }
}
