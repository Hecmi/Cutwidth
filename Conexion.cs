using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP
{
    class Conexion
    {
        public int u;
        public int v;
        public int peso;

        public Conexion(int u, int v, int peso)
        {
            this.u = u;
            this.v = v;
            this.peso = peso;
        }
    }
}
