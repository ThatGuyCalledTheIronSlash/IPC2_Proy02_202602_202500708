using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using Backend.TDA.Categoria;
using Backend.TDA.Libros;

namespace Backend.Servicios
{
	public class ReporteGraphviz
	{
		public static void GenerarGraficaLibros(AVLLibros libros, string rutaSalida)
		{
			if (libros.EstaVacio())
			{
				throw new Exception("No hay libros para graficar.");
			}

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("digraph G {");
			sb.AppendLine("  node [shape=record, style=filled, fillcolor=lightblue, fontname=\"Arial\"];");
			sb.AppendLine("  rankdir=TB;");

			NodoLibro? libroAnterior = null;

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

			EjecutarDot(sb.ToString(), rutaSalida);
		}

		public static void GenerarGraficaJerarquia(NodoCategoria? raizEspecifica, AVLCategorias raicesPrincipales, string rutaSalida)
		{
			if (raizEspecifica == null && raicesPrincipales.EstaVacio())
			{
				throw new Exception("No hay categorías registradas para graficar.");
			}

			StringBuilder sb = new StringBuilder();
			sb.AppendLine("digraph G {");
			sb.AppendLine("  rankdir=TB;");

			void DibujarNodo(NodoCategoria cat)
			{
				string idCat = $"cat_{SanitizarId(cat.Nombre)}";
				string catLabel = EscaparDOT(cat.Nombre);
				sb.AppendLine($"  {idCat} [shape=folder, style=filled, fillcolor=\"#a2d9ce\", fontname=\"Arial\", label=\"{catLabel}\"];");

				cat.Hijos.RecorridoInOrder(hijo =>
				{
					string idHijo = $"cat_{SanitizarId(hijo.Nombre)}";
					sb.AppendLine($"  {idCat} -> {idHijo};");
					DibujarNodo(hijo);
				});

				cat.LibrosDirectos.RecorridoInOrder(libro =>
				{
					string idLibro = $"libro_{libro.ISBN}";
					string tit = EscaparDOT(libro.Titulo);
					string aut = EscaparDOT(libro.Autor);
					string labelLibro = $"{{ ISBN: {libro.ISBN} | {tit} | {aut} }}";
					sb.AppendLine($"  {idLibro} [shape=record, style=filled, fillcolor=\"#fff2cc\", fontname=\"Arial\", label=\"{labelLibro}\"];");
					sb.AppendLine($"  {idCat} -> {idLibro};");
				});
			}

			if (raizEspecifica != null)
			{
				DibujarNodo(raizEspecifica);
			}
			else
			{
				raicesPrincipales.RecorridoInOrder(cat => DibujarNodo(cat));
			}

			sb.AppendLine("}");

			EjecutarDot(sb.ToString(), rutaSalida);
		}

		private static void EjecutarDot(string contenidoDot, string rutaSalida)
		{
			string dotPath = rutaSalida.Replace(".png", ".dot");
			File.WriteAllText(dotPath, contenidoDot);

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

				using (Process? proc = Process.Start(info))
				{
					if (proc == null)
					{
						throw new Exception("No se pudo iniciar el proceso de Graphviz ('dot').");
					}

					string stderr = proc.StandardError.ReadToEnd();
					proc.WaitForExit();

					if (proc.ExitCode != 0)
					{
						throw new Exception($"Graphviz terminó con código de error {proc.ExitCode}: {stderr}");
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al ejecutar Graphviz (¿está instalado y en el PATH?): " + ex.Message);
			}
		}

		public static string SanitizarNombreArchivo(string nombre)
		{
			if (string.IsNullOrWhiteSpace(nombre)) return "grafica";
			StringBuilder sb = new StringBuilder();
			foreach (char c in nombre)
			{
				if (char.IsLetterOrDigit(c) || c == '_')
					sb.Append(c);
				else
					sb.Append('_');
			}
			return sb.ToString();
		}

		private static string SanitizarId(string texto)
		{
			if (string.IsNullOrEmpty(texto)) return "nodo";
			StringBuilder sb = new StringBuilder();
			foreach (char c in texto)
			{
				if (char.IsLetterOrDigit(c) || c == '_')
					sb.Append(c);
				else
					sb.Append('_');
			}
			return sb.ToString();
		}

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