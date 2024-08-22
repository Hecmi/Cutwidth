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
        public int vertice_particion;
        public int grado;

        public int grado_iterativo;

        public double peso_org;
        public double peso;

        //Variables para los cálculos y selección de vértices        
        public double complejidad_adyacente;
        public double offset;
        
        //Variables de control del algoritmo
        //Índice en el ordenamiento
        public int indice_ordenamiento;
        //Ancho de corte entre el vértice y el anterior en el ordenamiento
        public double ancho_corte;
        //Control de los vértices que han sido ordenados (visitados)
        public bool visitado;
        //Control de los vértices que ya han sido definidos con todas sus conexiones
        public bool completado;

        //Indice de ínicio en el vector que contiene todos los vértices adyacentes
        //(el grado es el final; en la arreglo se accede: [indice_inicia_lista, indice_inicia_lista+grado_org]
        public int indice_inicio_lista;
        public Vertice(int vertice)
        {
            this.vertice = vertice;
            this.vertice_particion = vertice;
            this.grado = 0;
            this.offset = 0;

            this.peso_org = 0;
            this.peso = 0;

            this.complejidad_adyacente = 0;

            this.visitado = false;
            this.completado = false;

            this.ancho_corte = 0;
        }

        public void incrementar_adyacencia(double peso)
        {
            this.peso_org += peso;
            this.peso += peso;

            this.grado += 1;
            this.grado_iterativo += 1;
        }

        public void etiquetar(int indice)
        {
            this.visitado = true;
            this.indice_ordenamiento = indice;
        }

        public void disminuir_grado()
        {            
            if (--this.grado_iterativo == 0)
            {
                completado = true;
            }
        }
    }
}
