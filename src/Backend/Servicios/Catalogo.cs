using System;
using Backend.Modelo;
using Backend.TDA.Categoria;
using Backend.TDA.Libros;

namespace Backend.Servicios
{
    public class Catalogo
    {
        // Índice global de libros por ISBN. Permite buscar cualquier libro rápidamente sin importar en qué categoría se encuentre.
        public BSTLibros IndiceGlobalLibros { get; private set; }

        private ArbolCategorias arbolCategorias;

        public ArbolCategorias Categorias => arbolCategorias;

        public Catalogo()
        {
            IndiceGlobalLibros = new BSTLibros();
            arbolCategorias = new ArbolCategorias();
        }


        // Gestion de Catalogo


        // Reinicia completamente el catálogo (categorías y libros).
        public void InicializarCatalogo()
        {
            IndiceGlobalLibros = new BSTLibros();
            arbolCategorias.Reiniciar();
        }

        // Gestion de Categorias

        public void AgregarCategoria(string nombre, string? nombrePadre = null)
        {
            arbolCategorias.AgregarCategoria(nombre, nombrePadre);
        }

        public NodoCategoria? BuscarCategoria(string nombre)
        {
            return arbolCategorias.BuscarCategoria(nombre);
        }

        // Gestion de Libros

        public void RegistrarLibro(int isbn, string titulo, string autor, string nombreCategoria)
        {
            NodoCategoria? categoria = arbolCategorias.BuscarCategoria(nombreCategoria);
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

        public NodoLibro? BuscarLibro(int isbn)
        {
            return IndiceGlobalLibros.BuscarPorISBN(isbn);
        }

        public void EliminarLibro(int isbn)
        {
            NodoLibro? libroAEliminar = IndiceGlobalLibros.BuscarPorISBN(isbn);
            if (libroAEliminar == null)
            {
                throw new Exception($"No se encontró ningún libro con el ISBN {isbn}.");
            }

            //Eliminar del árbol local de la categoría
            libroAEliminar.Categoria?.LibrosDirectos.EliminarPorISBN(isbn);

            //Eliminar del árbol global del catálogo
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
    }
}
