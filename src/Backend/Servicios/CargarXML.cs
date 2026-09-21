using System;
using System.Text;
using System.Xml;
using Backend.TDA.Categoria;

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
				// Pasada previa: detectar categorías repetidas o ya existentes
				AVLCategorias vistas = new AVLCategorias();
				for (int i = 0; i < nodosCategoria.Count; i++)
				{
					XmlNode? nodoCat = nodosCategoria[i];
					if (nodoCat == null) continue;

					string nombreCat = nodoCat.InnerText.Trim();
					if (string.IsNullOrWhiteSpace(nombreCat)) continue;

					if (catalogo.BuscarCategoria(nombreCat) != null)
					{
						avisos.AppendLine($"Aviso (Categoría '{nombreCat}'): ya existe en el catálogo, se ignora.");
						continue;
					}

					try
					{
						vistas.Insertar(new NodoCategoria(nombreCat));
					}
					catch (InvalidOperationException)
					{
						avisos.AppendLine($"Aviso (Categoría '{nombreCat}'): está repetida en el archivo, se ignora la repetición.");
					}
				}

				bool huboProgreso = true;
				while (huboProgreso)
				{
					huboProgreso = false;
					for (int i = 0; i < nodosCategoria.Count; i++)
					{
						XmlNode? nodoCat = nodosCategoria[i];
						if (nodoCat == null) continue;

						string nombreCat = nodoCat.InnerText.Trim();
						if (string.IsNullOrWhiteSpace(nombreCat)) continue;

						// Si ya existe en el catalogo, ya fue procesada anteriormente
						if (catalogo.BuscarCategoria(nombreCat) != null)
							continue;

						string? padre = null;
						if (nodoCat.Attributes != null && nodoCat.Attributes["padre"] != null)
						{
							padre = nodoCat.Attributes["padre"]?.Value.Trim();
							if (string.IsNullOrWhiteSpace(padre)) padre = null;
						}

						// Si es raiz o su padre ya fue registrado, se agrega
						if (padre == null || catalogo.BuscarCategoria(padre) != null)
						{
							try
							{
								catalogo.AgregarCategoria(nombreCat, padre);
								resultado.CategoriasAgregadas++;
								huboProgreso = true;
							}
							catch (Exception ex)
							{
								avisos.AppendLine($"Aviso (Categoría '{nombreCat}'): {ex.Message}");
							}
						}
					}
				}

				// Revision final de categorias cuyos padres nunca fueron declarados
				for (int i = 0; i < nodosCategoria.Count; i++)
				{
					XmlNode? nodoCat = nodosCategoria[i];
					if (nodoCat == null) continue;

					string nombreCat = nodoCat.InnerText.Trim();
					if (string.IsNullOrWhiteSpace(nombreCat)) continue;

					if (catalogo.BuscarCategoria(nombreCat) == null)
					{
						string? padre = nodoCat.Attributes?["padre"]?.Value.Trim();
						avisos.AppendLine($"Aviso (Categoría): No se pudo agregar '{nombreCat}' porque su categoría padre '{padre}' nunca fue declarada.");
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