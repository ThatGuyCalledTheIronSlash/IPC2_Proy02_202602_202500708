using System;
using System.Text;
using System.Xml;

namespace Backend.Servicios
{
    public class CargarXML
    {
        public static string LeerArchivo(string rutaArchivo, Catalogo catalogo)
        {
            XmlDocument doc = new XmlDocument();
            StringBuilder avisos = new StringBuilder();

            try
            {
                doc.Load(rutaArchivo);
            }
            catch (Exception ex)
            {
                throw new Exception($"No se pudo cargar el archivo XML: {ex.Message}");
            }
            // Leer las categorías
            XmlNodeList? nodosCategoria = doc.SelectNodes("/config/listaCategorias/categoria");
            if (nodosCategoria != null)
            {
                for (int i = 0; i < nodosCategoria.Count; i++)
                {
                    XmlNode? nodoCat = nodosCategoria[i];
                    if (nodoCat == null) continue;

                    string nombreCategoria = nodoCat.InnerText.Trim();
                    string? padre = null;

                    if (nodoCat.Attributes != null && nodoCat.Attributes["padre"] != null)
                    {
                        padre = nodoCat.Attributes["padre"]?.Value.Trim();

                        if (string.IsNullOrWhiteSpace(padre))
                        {
                            padre = null;
                        }
                    }

                    try
                    {
                        catalogo.AgregarCategoria(nombreCategoria, padre);
                    }
                    catch (Exception ex)
                    {
                        avisos.AppendLine($"Aviso (Categoría): {ex.Message}");
                    }
                }
            }
          // Leer los libros
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

                        if (isbnText == null || titulo == null || autor == null || categoria == null)
                        {
                            avisos.AppendLine("Aviso (Libro): Elemento XML incompleto, se omite.");
                            continue;
                        }

                        int isbn = int.Parse(isbnText);
                        catalogo.RegistrarLibro(isbn, titulo, autor, categoria);
                    }
                    catch (Exception ex)
                    {
                        avisos.AppendLine($"Aviso (Libro): {ex.Message}");
                    }
                }
            }

            return avisos.ToString();
        }
    }
}