using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    public class Vertice
    {
        public int vertice;
        public int grado;
        public int grado_org;
        
        public int offset;
        public bool visitado;

        public int indice;
        public int ancho_corte;


        public int indice_inicio_lista;
        public Vertice(int vertice)
        {
            this.vertice = vertice;
            this.grado = 0;
            this.offset = 0;
            this.visitado = false;
            this.ancho_corte = 0;
        }

        public void etiquetar(int indice)
        {
            this.visitado = true;
            this.indice = indice;
        }
    }
}
