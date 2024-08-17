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

        public int peso_org;
        public int peso;

        //Variables para los cálculos y selección de vértices        
        public int complejidad_adyacente;
        public int offset;
        
        //Variables de control del algoritmo
        //Índice en el ordenamiento
        public int indice;
        //Ancho de corte entre el vértice y el anterior en el ordenamiento
        public int ancho_corte;
        //Control de los vértices que pueden ser utilizados
        public bool visitado;

        //Indice de ínicio en el vector que contiene todos los vértices adyacentes
        //(el grado es el final; en la arreglo se accede: [indice_inicia_lista, indice_inicia_lista+grado_org]
        public int indice_inicio_lista;
        public Vertice(int vertice)
        {
            this.vertice = vertice;
            this.grado = 0;
            this.offset = 0;

            this.peso_org = 0;
            this.peso = 0;

            this.complejidad_adyacente = 0;

            this.visitado = false;
            this.ancho_corte = 0;
        }

        public void incrementar_adyacencia(int peso)
        {
            this.peso_org += peso;
            this.peso += peso;

            this.grado += 1;
        }

        public void etiquetar(int indice)
        {
            this.visitado = true;
            this.indice = indice;
        }
    }
}
