using System;
using Backend.TDA.Categoria;

namespace Backend.Modelo
{
    public class ArbolCategorias
    {
        // Raíz del árbol que contiene las categorías principales (nivel 0).
        public BSTCategorias CategoriasPrincipales { get; private set; }

       // Índice global para búsquedas rápidas de categorías por nombre.
        private BSTCategorias indiceGlobal;


       // Constructor que inicializa el árbol de categorías y el índice global. 
        public ArbolCategorias()
        {
            CategoriasPrincipales = new BSTCategorias();
            indiceGlobal = new BSTCategorias();
        }

//-------------Métodos para agregar, buscar y reiniciar categorías-----------------
        public void AgregarCategoria(string nombre, string nombrePadre = null)
        {
            //Validar unicidad (no pueden haber 2 categorías con el mismo nombre)
            if (indiceGlobal.BuscarPorNombre(nombre) != null)
            {
                throw new InvalidOperationException($"La categoría '{nombre}' ya existe en el catálogo.");
            }

            NodoCategoria padre = null;

            // Si se especificó un padre, lo buscamos rápidamente en el índice global
            if (!string.IsNullOrWhiteSpace(nombrePadre))
            {
                padre = indiceGlobal.BuscarPorNombre(nombrePadre);
                if (padre == null)
                {
                    throw new InvalidOperationException(
                        $"No se puede agregar '{nombre}' porque la categoría padre '{nombrePadre}' no existe.");
                }
            }

            //Crear el nuevo nodo para la categoría
            NodoCategoria nuevaCategoria = new NodoCategoria(nombre, padre);

            //Insertar en la estructura jerárquica (como raíz o como hijo del padre)
            if (padre == null)
            {
                CategoriasPrincipales.Insertar(nuevaCategoria);
            }
            else
            {
                padre.Hijos.Insertar(nuevaCategoria);
            }

            //Agregar la referencia al índice global para futuras búsquedas súper rápidas
            indiceGlobal.Insertar(nuevaCategoria);
        }

// Método para buscar una categoría por nombre utilizando el índice global
        public NodoCategoria ObtenerCategoria(string nombre)
        {
            return indiceGlobal.BuscarPorNombre(nombre);
        }
// Método para reiniciar el catálogo de categorías
        public void InicializarCatalogo()
        {
            CategoriasPrincipales = new BSTCategorias();
            indiceGlobal = new BSTCategorias();
        }
    }
}