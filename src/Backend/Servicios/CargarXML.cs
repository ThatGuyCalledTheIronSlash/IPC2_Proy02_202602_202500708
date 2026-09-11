using System;
using System.Text;
using System.Xml;

namespace Backend.Servicios
{
    /// <summary>
    /// Clase para cargar datos desde un archivo XML al catálogo de categorías y libros.
    /// Devuelve un string con los avisos acumulados durante la carga para que el
    /// endpoint pueda informarlos al usuario web.
    /// </summary>
    public class CargarXML
    {
        /// <summary>
        /// Lee un archivo XML y carga las categorías y libros al catálogo.
        /// Retorna un string con los avisos/errores individuales que ocurrieron
        /// durante la carga (categorías o libros que no se pudieron procesar).
        /// Un string vacío indica que todo se procesó sin problemas.
        /// </summary>
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

            // ---------------------------------------------------------
            // Leer las categorías
            // ---------------------------------------------------------
            XmlNodeList nodosCategoria = doc.SelectNodes("/config/listaCategorias/categoria");
            if (nodosCategoria != null)
            {
                for (int i = 0; i < nodosCategoria.Count; i++)
                {
                    XmlNode nodoCat = nodosCategoria[i];

                    string nombreCategoria = nodoCat.InnerText.Trim();
                    string padre = null;

                    // Extraer el atributo 'padre' si existe
                    if (nodoCat.Attributes != null && nodoCat.Attributes["padre"] != null)
                    {
                        padre = nodoCat.Attributes["padre"].Value.Trim();

                        // Si el atributo viene vacío (""), se toma como nulo
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
                        // Acumulamos el aviso para devolverlo al usuario
                        avisos.AppendLine($"Aviso (Categoría): {ex.Message}");
                    }
                }
            }

            // ---------------------------------------------------------
            // Leer los libros
            // ---------------------------------------------------------
            XmlNodeList nodosLibros = doc.SelectNodes("/config/listaLibros/libro");
            if (nodosLibros != null)
            {
                for (int i = 0; i < nodosLibros.Count; i++)
                {
                    XmlNode nodoLibro = nodosLibros[i];
                    try
                    {
                        int isbn = int.Parse(nodoLibro["ISBN"].InnerText.Trim());
                        string titulo = nodoLibro["titulo"].InnerText.Trim();
                        string autor = nodoLibro["autor"].InnerText.Trim();
                        string categoria = nodoLibro["categoria"].InnerText.Trim();

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