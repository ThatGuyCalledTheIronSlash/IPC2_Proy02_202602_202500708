using System;
using Backend.TDA.Categoria;

namespace Backend.Modelo
{
    /// <summary>
    /// Modelo que gestiona la estructura jerárquica de categorías.
    /// Mantiene las categorías raíz (nivel 0) y un índice global
    /// para búsquedas rápidas O(log n) por nombre.
    /// 
    /// Catalogo.cs delega toda la gestión de categorías a esta clase
    /// para evitar duplicación de lógica.
    /// </summary>
    public class ArbolCategorias
    {
        // Categorías principales (sin padre), organizadas alfabéticamente.
        public BSTCategorias CategoriasPrincipales { get; private set; }

        // Índice global para búsquedas rápidas de cualquier categoría por nombre.
        private BSTCategorias indiceGlobal;

        public ArbolCategorias()
        {
            CategoriasPrincipales = new BSTCategorias();
            indiceGlobal = new BSTCategorias();
        }

        /// <summary>
        /// Agrega una nueva categoría al árbol jerárquico.
        /// Si nombrePadre es null, se inserta como categoría raíz.
        /// Los duplicados se ignoran silenciosamente (carga XML incremental).
        /// </summary>
        public void AgregarCategoria(string nombre, string nombrePadre = null)
        {
            // Si ya existe, la ignoramos (XML incremental puede traer repetidas)
            if (indiceGlobal.BuscarPorNombre(nombre) != null)
            {
                return;
            }

            NodoCategoria nuevaCategoria = new NodoCategoria(nombre);

            if (!string.IsNullOrEmpty(nombrePadre))
            {
                NodoCategoria padre = indiceGlobal.BuscarPorNombre(nombrePadre);
                if (padre == null)
                {
                    throw new Exception(
                        $"No se puede agregar '{nombre}' porque la categoría padre '{nombrePadre}' no existe.");
                }

                nuevaCategoria.Padre = padre;
                padre.Hijos.Insertar(nuevaCategoria);
            }
            else
            {
                // Es una categoría raíz (Ej: "Tecnologia", "Literatura")
                CategoriasPrincipales.Insertar(nuevaCategoria);
            }

            // Registrar en el índice global
            indiceGlobal.Insertar(nuevaCategoria);
        }

        /// <summary>
        /// Busca una categoría por nombre usando el índice global. O(log n).
        /// Retorna null si no se encuentra.
        /// </summary>
        public NodoCategoria BuscarCategoria(string nombre)
        {
            return indiceGlobal.BuscarPorNombre(nombre);
        }

        /// <summary>
        /// Reinicia completamente el árbol, eliminando todas las categorías.
        /// </summary>
        public void Reiniciar()
        {
            CategoriasPrincipales = new BSTCategorias();
            indiceGlobal = new BSTCategorias();
        }
    }
}