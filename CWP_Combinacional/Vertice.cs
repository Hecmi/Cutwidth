using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP.CWP_Combinacional
{
    public class Vertice
    {
        //Datos generales del vértice
        public int vertice;
        public int indice_ordenamiento;

        //Ancho de corte entre el vértice y el anterior en el ordenamiento
        public double ancho_corte { get; set; }

        public Vertice(int vertice)
        {
            this.vertice = vertice;
            this.ancho_corte = 0;
        }

        public void etiquetar(int indice)
        {
            this.indice_ordenamiento = indice;
        }
    }
}
