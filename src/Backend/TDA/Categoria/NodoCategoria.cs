using Backend.TDA.Libros;

namespace Backend.TDA.Categoria
{
    public class NodoCategoria
    {
        public string Nombre { get; set; }
        public NodoCategoria? Padre { get; set; }
        public BSTCategorias Hijos { get; set; }

        // Libros clasificados directamente en esta categoría
        public BSTLibros LibrosDirectos { get; set; }

        public NodoCategoria(string nombre, NodoCategoria? padre = null)
        {
            Nombre = nombre;
            Padre = padre;
            Hijos = new BSTCategorias();
            LibrosDirectos = new BSTLibros();
        }
    }
}