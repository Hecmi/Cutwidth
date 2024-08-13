using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    public class Vertice
    {
        //Datos generales del vértice
        public int vertice;
        public int grado;
        public int grado_org;
        public int grado_complejidad;
        
        //Variables de control del algoritmo
        //Índice en el ordenamiento
        public int indice;
        //Ancho de corte entre el vértice y el anterior en el ordenamiento
        public int ancho_corte;
        //Vértices ordenados que contienen una conexión con el objeto actual
        public int offset;
        //Control de los vértices que pueden ser utilizados
        public bool visitado;


        //Indice de ínicio en el vector que contiene todos los vértices adyacentes
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
