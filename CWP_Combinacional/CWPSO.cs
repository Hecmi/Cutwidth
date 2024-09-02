using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP.CWP_Combinacional
{
    public class Particula
    {
        public int[] posicion;
        public double pBest_ajuste;
        public int[] pBest;
        public double[] velocidad;

        public Particula(int dimensiones)
        {
            posicion = new int[dimensiones];
            pBest = new int[dimensiones];
            pBest_ajuste = Double.MaxValue;
            velocidad = new double[dimensiones];
        }
    }

    class CWPSO
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

        public CWPSO(string ruta_archivo, char delimitador, bool mostrar_ordenamiento, string ruta_salida, int num_muestra, double t0, double t1)
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
        public double recalcular_corte(int [] ordenamiento)
        {
            int idx_corte_max = -1;
            double[] cortes = new double[NUMERO_VERTICES];
            double corte_maximo = 0;

            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                int idx_u = ordenamiento[i];

                //Obtener los vértices adyacentes a u
                Dictionary<int, double> adyacencia_u = MATRIZ_ADYACENCIA[idx_u];

                //Recorrer la adyacencia para recalcular el corte a partir del índice
                //de las conexiones
                foreach (var conexion in adyacencia_u)
                {
                    //Obtener el vértice v
                    int idx_v = Array.IndexOf(ordenamiento, conexion.Key);

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

            return corte_maximo;
        }

        private int [] generar_solucion_aleatoria_()
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

        private Particula[] inicializar_particulas(int num_particulas)
        {
            Particula[] particulas = new Particula[num_particulas];

            for (int i = 0; i < num_particulas; i++)
            {
                //Crear un objeto de tipo partícula y establecer un ordenamiento
                //aleatorio (permutación)
                Particula p = new Particula(NUMERO_VERTICES);
                p.posicion = generar_solucion_aleatoria_();

                //Calcular el valor de ajuste que es equivalente al máximo ancho de corte
                p.pBest_ajuste = recalcular_corte(p.posicion.ToArray());

                //Debido a que es la inicializacón establecer que el ordenamiento mejor local
                //es el mismo ordenamiento aleatorio
                p.pBest = p.posicion;

                if (p.pBest_ajuste < gBest.pBest_ajuste)
                {
                    gBest = p;
                }

                particulas[i] = p;
            }

            return particulas;
        }
        private void actualizar_particulas(Particula[] particulas)
        {
            Random rnd = new Random();

            for (int i = 0; i < particulas.Length; i++)
            {
                Particula p = particulas[i];

                double w = 1;
                double c1 = 1.5;
                double c2 = 1.5;

                for (int j = 0; j < NUMERO_VERTICES; j++)
                {
                    //Calcular la nueva velocidad
                    int velocidad = (int)(w * p.velocidad[j] +
                          c1 * rnd.Next() * (p.pBest[j] - p.posicion[j]) +
                          c2 * rnd.Next() * (gBest.posicion[j] - p.posicion[j]));
                                        
                    int nueva_posicion = p.posicion[j] + velocidad;
                    if (nueva_posicion > 0 && nueva_posicion < NUMERO_VERTICES - 1)
                    {
                        p.velocidad[j] = velocidad;

                        //Intercambiar las posiciones
                        int temp = p.posicion[j];
                        p.posicion[Array.IndexOf(p.posicion, nueva_posicion)] = p.posicion[j];
                        p.posicion[j] = temp;
                    }
                }

                //Actualizar velocidad y posiciones basados en la velocidad
                //for (int j = 0; j < NUMERO_VERTICES; j++)
                //{
                //    if (rnd.NextDouble() < p.velocidad[j])
                //    {
                //        //Intercambiar dos posiciones en la permutación
                //        int idx = rnd.Next(NUMERO_VERTICES);
                //        int temp = p.posicion[i];
                //        p.posicion[i] = p.posicion[j];
                //        p.posicion[j] = temp;
                //    }
                //}

                //Calcular el valor de ajuste de la nueva posición de la partícula
                double valorActual = recalcular_corte(p.posicion);
                if (valorActual < p.pBest_ajuste)
                {
                    p.pBest_ajuste = valorActual;
                    p.pBest = p.posicion;
                }
            }
        }
        Particula gBest;
        public void resolver(int numParticulas, int maxIteraciones)
        {
            gBest = new Particula(NUMERO_VERTICES);
            Particula [] particulas = inicializar_particulas(numParticulas);            
            double[] velocidad_maxima = new double[NUMERO_VERTICES];

            //Inicializar velocidad máxima para cada dimensión
            for (int i = 0; i < NUMERO_VERTICES; i++)
            {
                velocidad_maxima[i] = 1.0;
            }

            //Iterar cada una de las partículas
            for (int iter = 0; iter < maxIteraciones; iter++)
            {
                actualizar_particulas(particulas);

                for (int i = 0; i < particulas.Length; i++)
                {
                    Particula p = particulas[i];
                    if (p.pBest_ajuste < gBest.pBest_ajuste)
                    {
                        gBest = p;
                        Console.WriteLine($"{String.Join(',',p.posicion)}: {p.pBest_ajuste}");
                    }
                }
            }

            MEJOR_COMBINACION = new Combinacion(NUMERO_VERTICES)
            {
                s_actual = gBest.pBest.ToArray()
            };
            MEJOR_CORTE = gBest.pBest_ajuste;

            Console.WriteLine($"Mejor Corte = {MEJOR_CORTE}");
        }


        public int[] get_ordenamiento()
        {
            return MEJOR_COMBINACION.s_actual;
        }
    }
}
