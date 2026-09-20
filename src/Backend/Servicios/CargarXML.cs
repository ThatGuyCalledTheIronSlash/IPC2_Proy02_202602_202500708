using System;
using System.Text;
using System.Xml;

namespace Backend.Servicios
{
	public class ResultadoCargaXML
	{
		public int CategoriasAgregadas { get; set; }
		public int LibrosAgregados { get; set; }
		public string Avisos { get; set; } = "";
	}

	public class CargarXML
	{
		private class EntradaCat
		{
			public string Nombre;
			public string? Padre;
			public bool Agregada;

			public EntradaCat(string nombre, string? padre)
			{
				Nombre = nombre;
				Padre = padre;
				Agregada = false;
			}
		}

		public static ResultadoCargaXML LeerArchivo(string rutaArchivo, Catalogo catalogo)
		{
			XmlDocument doc = new XmlDocument();
			StringBuilder avisos = new StringBuilder();
			ResultadoCargaXML resultado = new ResultadoCargaXML();

			try
			{
				doc.Load(rutaArchivo);
			}
			catch (Exception ex)
			{
				throw new Exception($"No se pudo cargar el archivo XML: {ex.Message}");
			}

			XmlNodeList? nodosCategoria = doc.SelectNodes("/config/listaCategorias/categoria");
			if (nodosCategoria != null && nodosCategoria.Count > 0)
			{
				int totalCats = nodosCategoria.Count;
				EntradaCat[] entradas = new EntradaCat[totalCats];

				for (int i = 0; i < totalCats; i++)
				{
					XmlNode? nodoCat = nodosCategoria[i];
					string nombreCat = nodoCat?.InnerText.Trim() ?? "";
					string? padre = null;

					if (nodoCat?.Attributes != null && nodoCat.Attributes["padre"] != null)
					{
						padre = nodoCat.Attributes["padre"]?.Value.Trim();
						if (string.IsNullOrWhiteSpace(padre)) padre = null;
					}

					entradas[i] = new EntradaCat(nombreCat, padre);
				}

				bool huboProgreso = true;
				while (huboProgreso)
				{
					huboProgreso = false;
					for (int i = 0; i < totalCats; i++)
					{
						if (entradas[i].Agregada || string.IsNullOrWhiteSpace(entradas[i].Nombre))
							continue;

						if (entradas[i].Padre == null || catalogo.BuscarCategoria(entradas[i].Padre!) != null)
						{
							try
							{
								catalogo.AgregarCategoria(entradas[i].Nombre, entradas[i].Padre);
								resultado.CategoriasAgregadas++;
							}
							catch (Exception ex)
							{
								avisos.AppendLine($"Aviso (Categoría '{entradas[i].Nombre}'): {ex.Message}");
							}

							entradas[i].Agregada = true;
							huboProgreso = true;
						}
					}
				}

				for (int i = 0; i < totalCats; i++)
				{
					if (!entradas[i].Agregada && !string.IsNullOrWhiteSpace(entradas[i].Nombre))
					{
						avisos.AppendLine($"Aviso (Categoría): No se pudo agregar '{entradas[i].Nombre}' porque su categoría padre '{entradas[i].Padre}' nunca fue declarada.");
					}
				}
			}

			XmlNodeList? nodosLibros = doc.SelectNodes("/config/listaLibros/libro");
			if (nodosLibros != null)
			{
				for (int i = 0; i < nodosLibros.Count; i++)
				{
					XmlNode? nodoLibro = nodosLibros[i];
					if (nodoLibro == null) continue;

					try
					{
						string? isbnText  = nodoLibro["ISBN"]?.InnerText.Trim();
						string? titulo    = nodoLibro["titulo"]?.InnerText.Trim();
						string? autor     = nodoLibro["autor"]?.InnerText.Trim();
						string? categoria = nodoLibro["categoria"]?.InnerText.Trim();

						if (string.IsNullOrWhiteSpace(isbnText) || string.IsNullOrWhiteSpace(titulo) ||
							string.IsNullOrWhiteSpace(autor) || string.IsNullOrWhiteSpace(categoria))
						{
							avisos.AppendLine("Aviso (Libro): Elemento XML con campos incompletos, se omite.");
							continue;
						}

						if (!long.TryParse(isbnText, out long isbn))
						{
							avisos.AppendLine($"Aviso (Libro): El ISBN '{isbnText}' no es un número entero válido.");
							continue;
						}

						catalogo.RegistrarLibro(isbn, titulo, autor, categoria);
						resultado.LibrosAgregados++;
					}
					catch (Exception ex)
					{
						avisos.AppendLine($"Aviso (Libro): {ex.Message}");
					}
				}
			}

			resultado.Avisos = avisos.ToString();
			return resultado;
		}
	}
}