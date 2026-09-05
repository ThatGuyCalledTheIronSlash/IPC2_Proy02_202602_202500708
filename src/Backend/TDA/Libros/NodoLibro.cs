using Backend.TDA.Categoria;

namespace Backend.TDA.Libros
{
    /// <summary>
    /// Representa los datos de un libro. NO contiene punteros de árbol:
    /// un mismo libro puede vivir simultáneamente en el BST global del
    /// catálogo y en el BST local de su categoría, sin conflictos.
    /// </summary>
    public class NodoLibro
    {
        public int ISBN { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public NodoCategoria Categoria { get; set; }

        public NodoLibro(int isbn, string titulo, string autor)
        {
            ISBN = isbn;
            Titulo = titulo;
            Autor = autor;
        }
    }
}