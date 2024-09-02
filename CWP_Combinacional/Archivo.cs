using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWP.CWP_Combinacional
{
    class Archivo
    {
        public static bool procesar_archivo(ref Grafo grafo, string ruta_archivo, char delimitador)
        {
            //Verificar que la ruta exista
            if (!File.Exists(ruta_archivo))
            {
                Console.WriteLine($"La ruta del archivo especificado no existe.");
                return false;
            }

            //Crear el stream reader para leer el archivo
            StreamReader sreader = new StreamReader(ruta_archivo);
            int numero_vertices = 0;
            int numero_aristas = 0;

            //Variables para leer el archivo
            string linea;
            string[] elementos_linea;

            //Leer la primera línea del archivo que contiene los datos generales del
            //grafo a resolver
            if ((linea = sreader.ReadLine()) != null)
            {
                //Obtener el arreglo separado por el caracter especificado
                elementos_linea = linea.Split(delimitador);

                //Verificar que contenga el formato correcto:
                //(número de vértices repetido, número de aristas)
                if (elementos_linea.Length != 3 ||
                  !int.TryParse(elementos_linea[0], out numero_vertices) ||
                  !int.TryParse(elementos_linea[2], out numero_aristas))
                {
                    Console.WriteLine("El formato del encabezado del archivo no es válido.");
                    return false;
                }

                //Si no existen vertices en el arreglo, entonces no hay nada que resolver
                if (numero_vertices < 1 || numero_aristas < 1)
                {
                    Console.WriteLine("El número de vertices y aristas debe ser mayor que 0.");
                    return false;
                }

                grafo = new Grafo(numero_vertices, numero_aristas);
            }

            if (grafo == null) return false;

            //Recorrer las aristas según la cantidad específicada en el encabezado
            int linea_actual = 0;
            while (linea_actual < numero_aristas)
            {
                //Leer la línea
                linea = sreader.ReadLine();

                //Sí es nulo, significa que no cumple con la cantidad de aristas especificadas
                //en el encabezado
                if (linea == null)
                {
                    Console.WriteLine($"Error de formato, la línea {linea_actual} no existe, sin embargo el número de arsitas especificado indica lo contrario.");
                    return false;
                }

                //Obtener los elementos de la línea actual
                elementos_linea = linea.Split(delimitador);

                //Obtener los índices de los vértices para incrementar el grado
                int u, v = -1;
                int peso = 1;

                //Verificar que la línea de conexión tenga dos o tres elementos
                //Sí tiene dos, se asume que el peso de la conexión es equivalente a 1
                //Caso contrario, se lee le peso especificado en la línea
                if (elementos_linea.Length == 2 ||
                    elementos_linea.Length == 3)
                {
                    if (!int.TryParse(elementos_linea[0], out u) ||
                      !int.TryParse(elementos_linea[1], out v))
                    {
                        Console.WriteLine($"El formato de la línea {linea_actual} no es correcto.");
                        return false;
                    }

                    if (elementos_linea.Length == 3 &&
                      !int.TryParse(elementos_linea[2], out peso))
                    {
                        Console.WriteLine($"El formato del peso en la línea {linea_actual} no es correcto.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine($"El formato de la línea {linea_actual} no es correcto.");
                    return false;
                }

                //Restar uno a los vértices para acceder al índice creado durante la lectura
                //del encabezado
                u = u - 1;
                v = v - 1;

                //Incrementar la adyacencia en la matriz y el grado
                //de los objetos en el arreglo de vértices
                grafo.aumentar_adyacencia(u, v, peso);

                //Incrementar la línea actual
                linea_actual++;
            }

            //Cerrar el lector
            sreader.Close();
            return true;
        }

        public static void guardar_salida(string resultado, string ruta_salida)
        {
            StreamWriter s_writer = new StreamWriter(ruta_salida);
            s_writer.WriteLine($"VERTICES;ARISTAS;TIEMPO;CORTE");
            s_writer.WriteLine(resultado);
            s_writer.Close();
        }
    }
}
