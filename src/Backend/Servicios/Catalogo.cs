using System;
using Backend.TDA.Categoria;
using Backend.TDA.Libros;

namespace Backend.Servicios
{
    /// <summary>
    /// Actúa como el motor principal del sistema (Gestor).
    /// Mantiene referencias a los índices globales para búsquedas rápidas (O(log n))
    /// y gestiona la lógica de negocio para conectar libros con categorías.
    /// </summary>
    public class Catalogo
    {
        // 1. Índice global de libros. Permite buscar cualquier ISBN rápidamente
        //    sin importar en qué categoría o subcategoría se encuentre.
        public BSTLibros IndiceGlobalLibros { get; private set; }

        // 2. Índice global de categorías. Sirve para saber si una categoría
        //    ya existe al leer el XML o registrar un libro, sin tener que
        //    recorrer todo el árbol jerárquico.
        public BSTCategorias IndiceGlobalCategorias { get; private set; }

        // 3. Árbol de categorías raíz. Guarda únicamente las categorías
        //    que NO tienen padre. A partir de aquí se "cuelga" todo el organigrama.
        public BSTCategorias CategoriasPrincipales { get; private set; }

        public Catalogo()
        {
            IndiceGlobalLibros = new BSTLibros();
            IndiceGlobalCategorias = new BSTCategorias();
            CategoriasPrincipales = new BSTCategorias();
        }

        // ====================================================================
        // GESTIÓN DE CATEGORÍAS
        // ====================================================================
        
        public void AgregarCategoria(string nombre, string nombrePadre = null)
        {
            // Validar si la categoría ya existe (el XML es incremental y puede venir repetida)
            if (IndiceGlobalCategorias.BuscarPorNombre(nombre) != null)
            {
                return; // Ya existe, simplemente la ignoramos
            }

            NodoCategoria nuevaCategoria = new NodoCategoria(nombre);

            if (!string.IsNullOrEmpty(nombrePadre))
            {
                NodoCategoria padre = IndiceGlobalCategorias.BuscarPorNombre(nombrePadre);
                if (padre != null)
                {
                    // Se enlazan mutuamente
                    nuevaCategoria.Padre = padre;
                    padre.Hijos.Insertar(nuevaCategoria);
                }
                else
                {
                    throw new Exception($"Error: La categoría padre '{nombrePadre}' no existe.");
                }
            }
            else
            {
                // Es una categoría raíz principal (Ej: "Tecnologia", "Literatura")
                CategoriasPrincipales.Insertar(nuevaCategoria);
            }

            // Registrar en el índice global para futuras búsquedas O(log n)
            IndiceGlobalCategorias.Insertar(nuevaCategoria);
        }

        // ====================================================================
        // GESTIÓN DE LIBROS
        // ====================================================================

        public void RegistrarLibro(int isbn, string titulo, string autor, string nombreCategoria)
        {
            NodoCategoria categoria = IndiceGlobalCategorias.BuscarPorNombre(nombreCategoria);
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
            nuevoLibro.Categoria = categoria; // El libro conoce a qué categoría pertenece

            // Insertar en el índice global del catálogo
            IndiceGlobalLibros.Insertar(nuevoLibro);

            // Insertar en el índice local de su respectiva categoría
            categoria.LibrosDirectos.Insertar(nuevoLibro);
        }

        public NodoLibro BuscarLibro(int isbn)
        {
            // Búsqueda ultrarrápida gracias al BST global
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
