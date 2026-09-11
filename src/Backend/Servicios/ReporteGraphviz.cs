using System;
using System.IO;
using System.Text;
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
                throw new Exception("La categoría no tiene libros para graficar.");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("digraph G {");
            sb.AppendLine("  node [shape=record, style=filled, fillcolor=lightblue, fontname=\"Arial\"];");
            sb.AppendLine("  rankdir=TB;"); // TB = Top to Bottom

            NodoLibro libroAnterior = null;

            // Recorrido In-Order garantiza orden ascendente por ISBN
            libros.RecorridoInOrder(libroActual =>
            {
                string tituloEscapado = EscaparDOT(libroActual.Titulo);
                string autorEscapado = EscaparDOT(libroActual.Autor);

                string label = $"{{ ISBN: {libroActual.ISBN} | {tituloEscapado} | {autorEscapado} }}";
                sb.AppendLine($"  node{libroActual.ISBN} [label=\"{label}\"];");

                if (libroAnterior != null)
                {
                    sb.AppendLine($"  node{libroAnterior.ISBN} -> node{libroActual.ISBN};");
                }

                libroAnterior = libroActual;
            });

            sb.AppendLine("}");

            string dotPath = rutaSalida.Replace(".png", ".dot");
            File.WriteAllText(dotPath, sb.ToString());

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
                throw new Exception("Error al ejecutar Graphviz (¿está instalado y en el PATH?): " + ex.Message);
            }
        }

        /// <summary>
        /// Escapa caracteres especiales que romperían la sintaxis DOT
        /// dentro de labels tipo record ({, }, |, <, >, ").
        /// </summary>
        private static string EscaparDOT(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            StringBuilder resultado = new StringBuilder();
            for (int i = 0; i < texto.Length; i++)
            {
                char c = texto[i];
                switch (c)
                {
                    case '"':  resultado.Append("\\\""); break;
                    case '{':  resultado.Append("\\{");  break;
                    case '}':  resultado.Append("\\}");  break;
                    case '|':  resultado.Append("\\|");  break;
                    case '<':  resultado.Append("\\<");  break;
                    case '>':  resultado.Append("\\>");  break;
                    case '\\': resultado.Append("\\\\"); break;
                    default:   resultado.Append(c);      break;
                }
            }
            return resultado.ToString();
        }
    }
}