using System;
using System.Xml;

namespace Backend.Servicios
{
// Clase para cargar datos desde un archivo XML al catálogo de categorías y libros
    public class CargarXML
    {
        public static void LeerArchivo(string rutaArchivo, Catalogo catalogo)
        {
            XmlDocument doc = new XmlDocument();
            
            try
            {
                doc.Load(rutaArchivo);
            }
            catch (Exception ex)
            {
                throw new Exception($"No se pudo cargar el archivo XML: {ex.Message}");
            }

        //Leer las categorías
            XmlNodeList nodosCategoria = doc.SelectNodes("/config/listaCategorias/categoria");
            if (nodosCategoria != null)
            {
        // Iteramos sobre cada nodo de categoría y lo agregamos al catálogo
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
                    // Atrapamos el error individualmente para que una categoría mala no detenga la carga entera
                        Console.WriteLine($"Aviso XML (Categoría): {ex.Message}");
                    }
                }
            }

        //Leer los libros
            XmlNodeList nodosLibros = doc.SelectNodes("/config/listaLibros/libro");
            if (nodosLibros != null)
            {
                for (int i = 0; i < nodosLibros.Count; i++)
                {
                    XmlNode nodoLibro = nodosLibros[i];
                    try
                    {
                        // Extraemos los nodos hijos basándonos en la estructura del PDF
                        int isbn = int.Parse(nodoLibro["ISBN"].InnerText.Trim());
                        string titulo = nodoLibro["titulo"].InnerText.Trim();
                        string autor = nodoLibro["autor"].InnerText.Trim();
                        string categoria = nodoLibro["categoria"].InnerText.Trim();

                        catalogo.RegistrarLibro(isbn, titulo, autor, categoria);
                    }
                    catch (Exception ex)
                    {
                        // Atrapamos el error individualmente para que un libro malo no detenga la carga entera
                        Console.WriteLine($"Aviso XML (Libro): {ex.Message}");
                    }
                }
            }
        }
    }
}