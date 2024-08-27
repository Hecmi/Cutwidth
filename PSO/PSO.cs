using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CWP.Clases;

namespace CWP.PSO
{
    class PSO
    {
        //Variables de control del problema
        //para aplicar PSO
        List<Particula> PARTICULAS;
        int NUMERO_PARTICULAS;
        int MAX_ITERACIONES;

        double MEJOR_COSTE;
        int[] MEJOR_POS;
        CWP SOLVER;

        //Variables de configuración del PSO
        double w;
        double c1;
        double c2;

        public PSO(int numero_particulas, int max_iteraciones, CWP cw)
        {
            NUMERO_PARTICULAS = numero_particulas;
            MAX_ITERACIONES = max_iteraciones;
            SOLVER = cw;

            MEJOR_COSTE = double.MaxValue;

            inicializar_particulas();
        }

        private void inicializar_particulas()
        {
            PARTICULAS = new List<Particula>();

            Random rand = new Random();
            for (int i = 0; i < NUMERO_PARTICULAS; i++)
            {
                var posicion = Enumerable.Range(0, SOLVER.NUMERO_VERTICES).OrderBy(x => rand.Next()).ToArray();
                PARTICULAS.Add(new Particula
                {
                    pos = (int[])posicion.Clone(),
                    velocidad = 0,
                    mejor_coste = double.MaxValue,
                    mejor_pos= (int[])posicion.Clone()
                });
            }
        }

        private double evaluar_costo(int[] ordenamiento)
        {            
            return SOLVER.test_ordenamiento(ordenamiento);
        }

        public void ejecutar()
        {
            for (int iter = 0; iter < MAX_ITERACIONES; iter++)
            {
                foreach (var part in PARTICULAS)
                {
                    double costoActual = evaluar_costo(part.pos);

                    if (costoActual < part.mejor_coste)
                    {
                        part.mejor_coste = costoActual;
                        part.mejor_pos = (int[])part.pos.Clone();
                    }

                    if (costoActual < MEJOR_COSTE)
                    {
                        MEJOR_COSTE = costoActual;
                        MEJOR_POS = (int[])part.pos.Clone();
                    }
                
                    //Inicializar o calcular la nueva velocidad (en este ejemplo simplificado se usa una velocidad escalar)
                    double v = w * part.velocidad +
                                             c1 * new Random().NextDouble() * (part.mejor_coste - costoActual) +
                                             c2 * new Random().NextDouble() * (MEJOR_COSTE - costoActual);

                    //Actualizar la posición (en este ejemplo, se realiza una permutación simple)
                    part.pos = part.pos.OrderBy(x => new Random().Next()).ToArray();
                    part.velocidad = v;
                }
            }
        }
        public int[] get_mejor_posicion()
        {
            return MEJOR_POS;
        }

        public double get_mejor_costo()
        {
            return MEJOR_COSTE;
        }
    }
}
