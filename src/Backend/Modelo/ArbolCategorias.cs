using System;
using Backend.TDA.Categoria;

namespace Backend.Modelo
{
	public class ArbolCategorias
	{
		public AVLCategorias CategoriasPrincipales { get; private set; }
		private AVLCategorias indiceGlobal;

		public ArbolCategorias()
		{
			CategoriasPrincipales = new AVLCategorias();
			indiceGlobal = new AVLCategorias();
		}

		public void AgregarCategoria(string nombre, string? nombrePadre = null)
		{
			if (indiceGlobal.BuscarPorNombre(nombre) != null)
			{
				throw new InvalidOperationException($"La categoría '{nombre}' ya existe en el catálogo.");
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
				CategoriasPrincipales.Insertar(nuevaCategoria);
			}

			indiceGlobal.Insertar(nuevaCategoria);
		}

		public NodoCategoria? BuscarCategoria(string nombre)
		{
			return indiceGlobal.BuscarPorNombre(nombre);
		}

		public void Reiniciar()
		{
			CategoriasPrincipales = new AVLCategorias();
			indiceGlobal = new AVLCategorias();
		}
	}
}