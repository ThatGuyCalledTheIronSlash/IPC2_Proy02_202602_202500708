using System;
using System.Net;
using System.Text;
using Backend.Modelo;
using Backend.TDA.Categoria;
using Backend.TDA.Libros;

namespace Backend.Servicios
{
	public class Catalogo
	{
		public BSTLibros IndiceGlobalLibros { get; private set; }
		private ArbolCategorias arbolCategorias;
		public ArbolCategorias Categorias => arbolCategorias;

		public Catalogo()
		{
			IndiceGlobalLibros = new BSTLibros();
			arbolCategorias = new ArbolCategorias();
		}

		public void InicializarCatalogo()
		{
			IndiceGlobalLibros = new BSTLibros();
			arbolCategorias.Reiniciar();
		}

		public void AgregarCategoria(string nombre, string? nombrePadre = null)
		{
			arbolCategorias.AgregarCategoria(nombre, nombrePadre);
		}

		public NodoCategoria? BuscarCategoria(string nombre)
		{
			return arbolCategorias.BuscarCategoria(nombre);
		}

		public void RegistrarLibro(long isbn, string titulo, string autor, string nombreCategoria)
		{
			NodoCategoria? categoria = arbolCategorias.BuscarCategoria(nombreCategoria);
			if (categoria == null)
			{
				throw new Exception($"La categoría '{nombreCategoria}' no existe. No se puede registrar el libro.");
			}

			if (IndiceGlobalLibros.BuscarPorISBN(isbn) != null)
			{
				throw new Exception($"El libro con ISBN {isbn} ya está registrado.");
			}

			NodoLibro nuevoLibro = new NodoLibro(isbn, titulo, autor);
			nuevoLibro.Categoria = categoria;

			IndiceGlobalLibros.Insertar(nuevoLibro);
			categoria.LibrosDirectos.Insertar(nuevoLibro);
		}

		public NodoLibro? BuscarLibro(long isbn)
		{
			return IndiceGlobalLibros.BuscarPorISBN(isbn);
		}

		public void EliminarLibro(long isbn)
		{
			NodoLibro? libroAEliminar = IndiceGlobalLibros.BuscarPorISBN(isbn);
			if (libroAEliminar == null)
			{
				throw new Exception($"No se encontró ningún libro con el ISBN {isbn}.");
			}

			libroAEliminar.Categoria?.LibrosDirectos.EliminarPorISBN(isbn);
			IndiceGlobalLibros.EliminarPorISBN(isbn);
		}

		public NodoLibro? ObtenerLibroMenorISBN()
		{
			return IndiceGlobalLibros.ObtenerMinimo();
		}

		public NodoLibro? ObtenerLibroMayorISBN()
		{
			return IndiceGlobalLibros.ObtenerMaximo();
		}

		public BSTLibros ObtenerLibrosDeCategoriaYSubcategorias(string nombreCategoria)
		{
			NodoCategoria? cat = BuscarCategoria(nombreCategoria);
			if (cat == null)
			{
				throw new Exception($"La categoría '{nombreCategoria}' no existe en el catálogo.");
			}

			BSTLibros acumulador = new BSTLibros();

			void Recolectar(NodoCategoria actual)
			{
				actual.LibrosDirectos.RecorridoInOrder(libro => acumulador.Insertar(libro));
				actual.Hijos.RecorridoInOrder(hijo => Recolectar(hijo));
			}

			Recolectar(cat);
			return acumulador;
		}

		public int ContarTotalLibros()
		{
			int total = 0;
			IndiceGlobalLibros.RecorridoInOrder(_ => total++);
			return total;
		}

		public int ContarTotalCategorias()
		{
			int total = 0;
			void ContarRecursivo(NodoCategoria cat)
			{
				total++;
				cat.Hijos.RecorridoInOrder(hijo => ContarRecursivo(hijo));
			}

			Categorias.CategoriasPrincipales.RecorridoInOrder(raiz => ContarRecursivo(raiz));
			return total;
		}

		public string GenerarJsonTodosLibros()
		{
			if (IndiceGlobalLibros.EstaVacio())
			{
				return "[]";
			}

			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			bool primero = true;
			IndiceGlobalLibros.RecorridoInOrder(libro =>
			{
				if (!primero) sb.Append(",");
				primero = false;
				string titulo = (libro.Titulo ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
				string autor = (libro.Autor ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
				string cat = (libro.Categoria?.Nombre ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
				sb.Append($"{{\"isbn\":{libro.ISBN},\"titulo\":\"{titulo}\",\"autor\":\"{autor}\",\"categoria\":\"{cat}\"}}");
			});
			sb.Append("]");
			return sb.ToString();
		}

		public string GenerarArbolHtml(string? nombreSubcat = null)
		{
			NodoCategoria? subcatRaiz = null;
			if (!string.IsNullOrWhiteSpace(nombreSubcat))
			{
				subcatRaiz = BuscarCategoria(nombreSubcat.Trim());
				if (subcatRaiz == null)
				{
					throw new Exception($"La subcategoría '{nombreSubcat}' no existe en el catálogo.");
				}
			}

			if (subcatRaiz == null && Categorias.CategoriasPrincipales.EstaVacio())
			{
				return "<p style='color:#7f8c8d;'>No hay categorías registradas en el catálogo.</p>";
			}

			StringBuilder sb = new StringBuilder();
			sb.Append("<ul style='list-style-type: none; padding-left: 15px;'>");

			void ConstruirNodo(NodoCategoria cat)
			{
				string nombreSeguro = WebUtility.HtmlEncode(cat.Nombre);
				sb.Append($"<li style='margin-bottom: 6px; font-size: 15px;'>📁 <strong>{nombreSeguro}</strong>");

				bool tieneLibros = !cat.LibrosDirectos.EstaVacio();
				bool tieneHijos = !cat.Hijos.EstaVacio();

				if (tieneLibros || tieneHijos)
				{
					sb.Append("<ul style='list-style-type: none; padding-left: 20px; border-left: 2px solid #bdc3c7; margin-top: 4px; margin-bottom: 4px;'>");

					cat.LibrosDirectos.RecorridoInOrder(libro =>
					{
						string tit = WebUtility.HtmlEncode(libro.Titulo);
						string aut = WebUtility.HtmlEncode(libro.Autor);
						sb.Append($"<li style='color: #2c3e50; font-size: 13px; margin: 3px 0;'>📖 <span style='color:#e67e22;'>[{libro.ISBN}]</span> <em>{tit}</em> - <small>{aut}</small></li>");
					});

					cat.Hijos.RecorridoInOrder(hijo => ConstruirNodo(hijo));

					sb.Append("</ul>");
				}
				sb.Append("</li>");
			}

			if (subcatRaiz != null)
			{
				ConstruirNodo(subcatRaiz);
			}
			else
			{
				Categorias.CategoriasPrincipales.RecorridoInOrder(cat => ConstruirNodo(cat));
			}

			sb.Append("</ul>");
			return sb.ToString();
		}
	}
}
