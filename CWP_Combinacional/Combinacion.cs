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
        public int[] s_iteracion;

        public int[] s_actual_idx;
        public int[] s_i_idx;

        public double[] cortes;

        public int indice_cw;
        public double cw;

        public Combinacion(int tamanio)
        {
            s_actual = new int[tamanio];
            s_iteracion = new int[tamanio];

            s_actual_idx = new int[tamanio];
            s_i_idx = new int[tamanio];

            cortes = new double[tamanio];

            indice_cw = -1;
            cw = 0;
        }

        public void restaurar_s_i()
        {
            Array.Copy(s_actual, s_iteracion, s_iteracion.Length);
            Array.Copy(s_actual_idx, s_i_idx, s_i_idx.Length);
        }

        public void actualizar_s_actual()
        {
            Array.Copy(s_iteracion, s_actual, s_iteracion.Length);
            Array.Copy(s_i_idx, s_actual_idx, s_i_idx.Length);
        }

        public void intercambiar_s_i(int idx1, int idx2)
        {
            int temp = s_iteracion[idx1];
            s_iteracion[idx1] = s_iteracion[idx2];
            s_iteracion[idx2] = temp;

            s_i_idx[s_iteracion[idx1]] = idx1;
            s_i_idx[s_iteracion[idx2]] = idx2;
        }
    }
}
