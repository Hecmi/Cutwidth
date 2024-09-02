using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP.CWP_Combinacional
{
    public class Combinacion
    {
        public int[] s_actual;
        public int[] s_i;

        public int[] s_actual_ord;

        public double total_corte;

        public int indice_cw;
        public double cw;

        public Combinacion(int tamanio)
        {
            s_actual = new int[tamanio];
            s_i = new int[tamanio];
            s_actual_ord = new int[tamanio];

            indice_cw = -1;
            total_corte = 0;
            cw = 0;
        }
    }
}
