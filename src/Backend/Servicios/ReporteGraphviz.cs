using System;
using System.IO;
using System.Diagnostics;
using Backend.TDA.Libros;

namespace Backend.Servicios
{
    public class ReporteGraphviz
    {
        /// <summary>
        /// Genera una imagen PNG usando Graphviz que muestra los libros
        /// en estricto orden ascendente por ISBN.
        /// </summary>
        public static void GenerarGraficaLibros(BSTLibros libros, string rutaSalida)
        {
            if (libros.EstaVacio())
            {
                Console.WriteLine("La categoría no tiene libros para graficar.");
                return;
            }

            string dotContent = "digraph G {\n";
            dotContent += "  node [shape=record, style=filled, fillcolor=lightblue, fontname=\"Arial\"];\n";
            dotContent += "  rankdir=TB;\n"; // TB = Top to Bottom

            NodoLibro libroAnterior = null;

            // Tu método In-Order nos garantiza el recorrido Ascendente requerido por el PDF!
            libros.RecorridoInOrder(libroActual =>
            {
                string label = $"{{ ISBN: {libroActual.ISBN} | {libroActual.Titulo} | {libroActual.Autor} }}";
                dotContent += $"  node{libroActual.ISBN} [label=\"{label}\"];\n";

                if (libroAnterior != null)
                {
                    dotContent += $"  node{libroAnterior.ISBN} -> node{libroActual.ISBN};\n";
                }

                libroAnterior = libroActual;
            });

            dotContent += "}\n";

            string dotPath = rutaSalida.Replace(".png", ".dot");
            File.WriteAllText(dotPath, dotContent);

            try
            {
                ProcessStartInfo info = new ProcessStartInfo("dot")
                {
                    Arguments = $"-Tpng \"{dotPath}\" -o \"{rutaSalida}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = Process.Start(info))
                {
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al ejecutar Graphviz: " + ex.Message);
            }
        }
    }
}