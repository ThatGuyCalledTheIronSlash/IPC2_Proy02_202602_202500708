using System;
using Backend.Modelo;
using Backend.TDA.Categoria;
using Backend.TDA.Libros;

namespace Backend.Servicios
{
    /// <summary>
    /// Gestor principal del sistema. Coordina la relación entre
    /// el árbol de categorías y el índice global de libros.
    /// 
    /// Delega la gestión de categorías a ArbolCategorias (Modelo)
    /// para mantener una sola fuente de verdad y evitar duplicación.
    /// </summary>
    public class Catalogo
    {
        // Índice global de libros por ISBN. Permite buscar cualquier libro
        // rápidamente sin importar en qué categoría se encuentre.
        public BSTLibros IndiceGlobalLibros { get; private set; }

        // Árbol jerárquico de categorías (modelo delegado).
        // Toda la lógica de agregar/buscar categorías vive aquí.
        private ArbolCategorias arbolCategorias;

        // Propiedad para acceso externo a las categorías raíz
        // (necesaria para el endpoint de gráfica y futuros endpoints de jerarquía).
        public ArbolCategorias Categorias => arbolCategorias;

        public Catalogo()
        {
            IndiceGlobalLibros = new BSTLibros();
            arbolCategorias = new ArbolCategorias();
        }

        // ====================================================================
        // GESTIÓN DEL CATÁLOGO
        // ====================================================================

        /// <summary>
        /// Reinicia completamente el catálogo (categorías y libros).
        /// </summary>
        public void InicializarCatalogo()
        {
            IndiceGlobalLibros = new BSTLibros();
            arbolCategorias.Reiniciar();
        }

        // ====================================================================
        // GESTIÓN DE CATEGORÍAS (delegada a ArbolCategorias)
        // ====================================================================

        public void AgregarCategoria(string nombre, string nombrePadre = null)
        {
            arbolCategorias.AgregarCategoria(nombre, nombrePadre);
        }

        public NodoCategoria BuscarCategoria(string nombre)
        {
            return arbolCategorias.BuscarCategoria(nombre);
        }

        // ====================================================================
        // GESTIÓN DE LIBROS
        // ====================================================================

        public void RegistrarLibro(int isbn, string titulo, string autor, string nombreCategoria)
        {
            NodoCategoria categoria = arbolCategorias.BuscarCategoria(nombreCategoria);
            if (categoria == null)
            {
                throw new Exception($"La categoría '{nombreCategoria}' no existe. No se puede registrar el libro.");
            }

            // Validar que el ISBN no exista ya en el catálogo global
            if (IndiceGlobalLibros.BuscarPorISBN(isbn) != null)
            {
                throw new Exception($"El libro con ISBN {isbn} ya está registrado.");
            }

            NodoLibro nuevoLibro = new NodoLibro(isbn, titulo, autor);
            nuevoLibro.Categoria = categoria;

            // Insertar en el índice global del catálogo
            IndiceGlobalLibros.Insertar(nuevoLibro);

            // Insertar en el índice local de su respectiva categoría
            categoria.LibrosDirectos.Insertar(nuevoLibro);
        }

        public NodoLibro BuscarLibro(int isbn)
        {
            return IndiceGlobalLibros.BuscarPorISBN(isbn);
        }

        public void EliminarLibro(int isbn)
        {
            NodoLibro libroAEliminar = IndiceGlobalLibros.BuscarPorISBN(isbn);
            if (libroAEliminar == null)
            {
                throw new Exception($"No se encontró ningún libro con el ISBN {isbn}.");
            }

            // 1. Eliminar del árbol local de la categoría
            libroAEliminar.Categoria.LibrosDirectos.EliminarPorISBN(isbn);

            // 2. Eliminar del árbol global del catálogo
            IndiceGlobalLibros.EliminarPorISBN(isbn);
        }

        public NodoLibro ObtenerLibroMenorISBN()
        {
            return IndiceGlobalLibros.ObtenerMinimo();
        }

        public NodoLibro ObtenerLibroMayorISBN()
        {
            return IndiceGlobalLibros.ObtenerMaximo();
        }
    }
}
