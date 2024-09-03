using CWP.Clases;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        //VARIABLES DE SALIDA
        Combinacion MEJOR_COMBINACION;
        double MEJOR_CORTE;
        double TIEMPO_RESOLUCION;

        //VARIABLES DEFINIDAS POR EL USUARIO
        int NUMERO_MUESTRA;
        double TEMP_MIN;
        double TEMP;
        double TASA_ENFRIAMIENTO;

        string RUTA_ARCHIVO;
        string RUTA_SALIDA;
        bool GUARDAR_SALIDA;
        bool MOSTRAR_ORDENAMIENTO;

        public CWP(string ruta_archivo, char delimitador, bool mostrar_ordenamiento, string ruta_salida, int num_muestra, double temp_min, double temp, float tasa_enfriamiento)
        {
            //Inicializar las variables definidas por el usuario
            MOSTRAR_ORDENAMIENTO = mostrar_ordenamiento;
            RUTA_SALIDA = ruta_salida;
            RUTA_ARCHIVO = ruta_archivo;

            GUARDAR_SALIDA = !string.IsNullOrEmpty(RUTA_SALIDA);

            this.NUMERO_MUESTRA = num_muestra;
            this.TEMP_MIN = temp_min;
            this.TEMP = temp;
            this.TASA_ENFRIAMIENTO = tasa_enfriamiento;

            //Lee el archivo, envíando por referencia el grafo no instanciado
            Archivo.procesar_archivo(ref GRAFO, ruta_archivo, delimitador);
        }

        public double calcular_corte(Combinacion c)
        {
            int idx_corte_max = -1;
            double[] cortes = new double[NUMERO_VERTICES];
            double corte_maximo = 0;

            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                int idx_u = c.s_iteracion[i];

                //Obtener los vértices adyacentes a u
                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[idx_u];

                //Recorrer la adyacencia para recalcular el corte a partir del índice
                //de las conexiones
                foreach (var conexion in adyacencia_u)
                {
                    //Obtener el vértice v
                    //int idx_v = Array.IndexOf(solucion, conexion.Key);
                    //int idx_v = Array.IndexOf(c.s_iteracion, conexion.Key);
                    int idx_v = c.s_i_idx[conexion.Key];

                    for (int idx = i; idx < idx_v; idx++)
                    {
                        cortes[idx + 1] += conexion.Value;

                        //Al incrementar el ancho de corte, determinar si es el corte que 
                        //representa el valor máximo
                        if (cortes[idx + 1] > corte_maximo)
                        {
                            corte_maximo = cortes[idx + 1];
                            idx_corte_max = idx + 1;
                        }
                    }
                }
            }

            c.cw = corte_maximo;
            c.indice_cw = idx_corte_max;
            c.cortes = cortes;
          
            return corte_maximo;
        }
        public double calcular_corte(Combinacion c, int [] solucion)
        {
            int idx_corte_max = -1;
            double[] cortes = new double[NUMERO_VERTICES];
            double corte_maximo = 0;

            for (int i = 0;  i < NUMERO_VERTICES; i++){
                int idx_u = solucion[i];

                //Obtener los vértices adyacentes a u
                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[idx_u];

                //Recorrer la adyacencia para recalcular el corte a partir del índice
                //de las conexiones
                foreach (var conexion in adyacencia_u)
                {
                    //Obtener el vértice v
                    //int idx_v = Array.IndexOf(solucion, conexion.Key);
                    int idx_v = c.s_i_idx[conexion.Key];

                    for (int idx = i; idx < idx_v; idx++)
                    {
                        cortes[idx + 1] += conexion.Value;

                        //Al incrementar el ancho de corte, determinar si es el corte que 
                        //representa el valor máximo
                        if (cortes[idx + 1] > corte_maximo)
                        {
                            corte_maximo = cortes[idx + 1];
                            idx_corte_max = idx + 1;
                        }
                    }
                }
            }

            c.cw = corte_maximo;
            c.indice_cw = idx_corte_max;
            c.cortes = cortes;

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

            c.restaurar_s_i();

            //Intercambiar los índices
            c.intercambiar_s_i(idx1, idx2);
        }

        private void generar_solucion_aleatoria(Combinacion c)
        {
            c.s_iteracion = Enumerable.Range(0, NUMERO_VERTICES).ToArray();
            Random rnd = new Random();

            //Realizar el intercambio para generar una permutación aleatoria
            for (int i = NUMERO_VERTICES - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                c.intercambiar_s_i(i, j);
            }
        }

        public void ejecutar()
        {
            //Inicializar el contador
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //Inicializar las variables para la resolución del problema
            Combinacion[] muestra = new Combinacion[NUMERO_MUESTRA];
            MEJOR_COMBINACION = new Combinacion(NUMERO_VERTICES);

            //Variable global que representa el mejor valor de ajuste (cutwidth)
            MEJOR_CORTE = Double.MaxValue;

            //Variables correspondientes a los cortes actuales por cada iteración
            //y combinación
            double mejor_corte_actual = Double.MaxValue;
            double corte_actual = 0;

            //Variables utilizadas para menejar el proceso estocástico del algoritmo
            Random rnd = new Random();

            //Variables para el control iterativo
            double TEMPERATURA_ = TEMP;

            //Iterar hasta el número de muestras especificados
            for (int i = 0; i < NUMERO_MUESTRA; i++)
            {
                //Inicializar le objeto para su evaluación
                muestra[i] = new Combinacion(NUMERO_VERTICES);

                //Generar la solución aleatoria para las muestras definidas
                //y realizar el calculo del ancho de corte
                generar_solucion_aleatoria(muestra[i]);
                muestra[i].actualizar_s_actual();

                corte_actual = calcular_corte(muestra[i], muestra[i].s_iteracion);
                mejor_corte_actual = corte_actual;

                //Verificar si el corte actual es mejor que el global
                if (corte_actual < MEJOR_CORTE)
                {
                    MEJOR_CORTE = corte_actual;
                }

                TEMPERATURA_ = TEMP;
                while (TEMPERATURA_ > TEMP_MIN)
                {
                    //Intercambiar el orden de los vértices de la muestra actual
                    //(esto implica intercambiar el arreglo del objeto)
                    intercambiar_vertices(muestra[i]);
                    corte_actual = calcular_corte(muestra[i], muestra[i].s_iteracion);

                    //Verificar si el corte actual es mejor que el global
                    if (corte_actual < mejor_corte_actual)
                    {
                        mejor_corte_actual = corte_actual;
                        muestra[i].actualizar_s_actual();

                        if (corte_actual < MEJOR_CORTE)
                        {
                            MEJOR_CORTE = corte_actual;
                            MEJOR_COMBINACION = muestra[i];
                        }
                    }
                    else
                    {
                        double p = Math.Exp((mejor_corte_actual - corte_actual) / TEMP);
                        double r = rnd.NextDouble();

                        //Verificar si se acepta la solución actual independientemente
                        //si es mejor o peor a la mejor global
                        if (p > r)
                        {
                            //muestra[i].s_actual = muestra[i].s_i;
                            muestra[i].actualizar_s_actual();
                            mejor_corte_actual = corte_actual;
                        }      
                    }

                    //Disminuir la temperatura
                    TEMPERATURA_ = TEMPERATURA_ * TASA_ENFRIAMIENTO;
                }
            }

            //Detener el tiempo de resolución
            stopwatch.Stop();
            TIEMPO_RESOLUCION = stopwatch.Elapsed.TotalSeconds;

            if (MOSTRAR_ORDENAMIENTO)
                Console.WriteLine($"MUESTRA = {String.Join(',', MEJOR_COMBINACION.s_actual)}, CORTE = {MEJOR_CORTE}");
            mostrar_resultado();
        }
        public void actualizar_solucion(Combinacion c1, Combinacion c2)
        {
            c1.s_actual = (int[])c2.s_actual.Clone();
            c1.cortes = (double[])c2.cortes.Clone();
            c1.indice_cw = c2.indice_cw;
            c1.cw = c2.cw;
        }

        public void resolver()
        {
            //Inicializar el contador
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //Inicializar las variables para la resolución del problema
            Combinacion[] muestra = new Combinacion[NUMERO_MUESTRA];

            //Inicializar la mejor combinación global que será obtenida
            //con respecto a todas las muestras definidas
            MEJOR_COMBINACION = new Combinacion(NUMERO_VERTICES);
            MEJOR_CORTE = double.MaxValue;

            //Variables correspondientes a los cortes actuales por cada iteración
            //y combinación
            Combinacion mejor_combinacion_iterativa = new Combinacion(NUMERO_VERTICES);
            double mejor_corte_iterativo = Double.MaxValue;
            double mejor_corte_actual = Double.MaxValue;
            double corte_actual = 0;

            //Variables utilizadas para menejar el proceso estocástico del algoritmo
            Random rnd = new Random();

            //Iterar hasta el número de muestras especificados
            for (int i = 0; i < NUMERO_MUESTRA; i++)
            {
                //Inicializar, estableciendo la combinación para cada muestra
                //para su evaluación (cálculo del ancho de corte)
                muestra[i] = new Combinacion(NUMERO_VERTICES);
                mejor_corte_iterativo = Double.MaxValue;

                //Generar la solución aleatoria para las muestras definidas
                //y realizar el calculo del ancho de corte
                generar_solucion_aleatoria(muestra[i]);

                //Debido a que la solución aleatoria obtenida es asignada a la solución
                //iterativa inicial, se actualiza la solución local
                muestra[i].actualizar_s_actual();

                //Calcular le ancho de corte
                corte_actual = calcular_corte(muestra[i]);

                //Así mismo, debido a que es la iteración 0, el mejor corte actual
                //de la muestra i, será igual a la calculada
                mejor_corte_actual = corte_actual;
                mejor_corte_iterativo = corte_actual;

                actualizar_solucion(mejor_combinacion_iterativa, muestra[i]); 
               
                double TEMP_ = TEMP;
                //Realizar el decremento de la temperatura general
                while (TEMP_ > TEMP_MIN)
                {
                    //Se realiza el intercambio de vértices para la solución actual
                    //(lo que implica recalcular le corte nuevamente)
                    intercambiar_vertices(muestra[i]);
                    corte_actual = calcular_corte(muestra[i]);
                  
                    //Verificar si el corte actual es mejor que el global
                    if (corte_actual < mejor_corte_actual)
                    {
                        mejor_corte_actual = corte_actual;
                        muestra[i].actualizar_s_actual();

                        if (corte_actual < mejor_corte_iterativo)
                        {                            
                            actualizar_solucion(mejor_combinacion_iterativa, muestra[i]);
                            mejor_corte_iterativo = corte_actual;
                        }
                    }
                    else
                    {
                        //Calcular la probabilidad y obtener un valor aleatorio para 
                        //evaluar si debe o no aceptar la solución (a pesar de que sea peor)
                        double p = Math.Exp((mejor_corte_actual - corte_actual) / TEMP_);
                        double r = rnd.NextDouble();

                        //Verificar si se acepta la solución actual independientemente
                        //si es mejor o peor a la mejor global
                        if (p > r)
                        {
                            //Actualizar la solución actual a la recién obtenida
                            muestra[i].actualizar_s_actual();
                            mejor_corte_actual = corte_actual;
                        }
                    }

                    //Disminuir la temperatura
                    TEMP_ = TEMP_ * TASA_ENFRIAMIENTO;
                }

                //Verificar si el ajuste (corte) obtenido al finalizar la iteración
                //es mejor que el global hasta el momento
                if (mejor_corte_iterativo < MEJOR_CORTE)
                {
                    MEJOR_CORTE = mejor_corte_iterativo;
                    Console.WriteLine(MEJOR_CORTE);
                    actualizar_solucion(MEJOR_COMBINACION, mejor_combinacion_iterativa);
                }
            }

            //Detener el tiempo de resolución
            stopwatch.Stop();
            TIEMPO_RESOLUCION = stopwatch.Elapsed.TotalSeconds;

            if (MOSTRAR_ORDENAMIENTO) mostrar_resultado();
            if (GUARDAR_SALIDA) Archivos.guardar_salida(ToString(), $@"{RUTA_SALIDA}\output.csv");
        }

        private void mostrar_resultado()
        {
            Console.WriteLine($"CORTE EN: {MEJOR_COMBINACION.s_actual[MEJOR_COMBINACION.indice_cw - 1]},  {MEJOR_COMBINACION.s_actual[MEJOR_COMBINACION.indice_cw]}, CORTE: {MEJOR_COMBINACION.cw} TIEMPO: {TIEMPO_RESOLUCION}");
        }

        public int [] get_solucion()
        {
            return MEJOR_COMBINACION.s_actual;
        }
    }
}
