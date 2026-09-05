using Backend.TDA.Libros;

namespace Backend.TDA.Categoria
{
    /// <summary>
    /// Representa un nodo dentro del árbol N-ario de categorías.
    /// Cada categoría conoce a su padre (si tiene), mantiene sus
    /// subcategorías ordenadas alfabéticamente en un BST propio,
    /// y mantiene los libros clasificados directamente en ella
    /// (no en sus subcategorías) en su propio BST de libros.
    /// </summary>
    public class NodoCategoria
    {
        public string Nombre { get; set; }
        public NodoCategoria Padre { get; set; }

        // Subcategorías de esta categoría, ordenadas alfabéticamente
        // gracias al recorrido in-order del BST.
        public BSTCategorias Hijos { get; set; }

        // Libros clasificados directamente en esta categoría
        // (reutilizamos el mismo TDA que ya construimos y probamos).
        public BSTLibros LibrosDirectos { get; set; }

        public NodoCategoria(string nombre, NodoCategoria padre = null)
        {
            Nombre = nombre;
            Padre = padre;
            Hijos = new BSTCategorias();
            LibrosDirectos = new BSTLibros();
        }
    }
}