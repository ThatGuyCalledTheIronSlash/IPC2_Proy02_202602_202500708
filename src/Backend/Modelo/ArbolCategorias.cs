using System;
using Backend.TDA.Categoria;

namespace Backend.Modelo
{
  
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

//---------------------------------------------------------
        public void AgregarCategoria(string nombre, string? nombrePadre = null)
        {
            if (indiceGlobal.BuscarPorNombre(nombre) != null)
            {
                return;
            }

            NodoCategoria nuevaCategoria = new NodoCategoria(nombre);

            if (!string.IsNullOrEmpty(nombrePadre))
            {
                NodoCategoria? padre = indiceGlobal.BuscarPorNombre(nombrePadre);
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
                // Es una categoría raíz
                CategoriasPrincipales.Insertar(nuevaCategoria);
            }

            // Registrar en el índice global
            indiceGlobal.Insertar(nuevaCategoria);
        }

 //-------------------------------------
        public NodoCategoria? BuscarCategoria(string nombre)
        {
            return indiceGlobal.BuscarPorNombre(nombre);
        }

        // Reinicia completamente el árbol, eliminando todas las categorías.
        public void Reiniciar()
        {
            CategoriasPrincipales = new BSTCategorias();
            indiceGlobal = new BSTCategorias();
        }
    }
}