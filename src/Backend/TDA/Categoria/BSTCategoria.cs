using System;
using System.Globalization;

namespace Backend.TDA.Categoria
{
	public delegate void AccionCategoria(NodoCategoria categoria);

	public class BSTCategorias
	{
		private class Nodo
		{
			public NodoCategoria Categoria;
			public Nodo? Izquierdo;
			public Nodo? Derecho;

			public Nodo(NodoCategoria categoria)
			{
				Categoria = categoria;
			}
		}

		private Nodo? raiz;

		public BSTCategorias()
		{
			raiz = null;
		}

		public bool EstaVacio()
		{
			return raiz == null;
		}

		public void Insertar(NodoCategoria nuevaCategoria)
		{
			raiz = InsertarRecursivo(raiz, nuevaCategoria);
		}

		private Nodo InsertarRecursivo(Nodo? actual, NodoCategoria nuevaCategoria)
		{
			if (actual == null)
			{
				return new Nodo(nuevaCategoria);
			}

			int comparacion = string.Compare(nuevaCategoria.Nombre, actual.Categoria.Nombre,
				CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);

			if (comparacion < 0)
			{
				actual.Izquierdo = InsertarRecursivo(actual.Izquierdo, nuevaCategoria);
			}
			else if (comparacion > 0)
			{
				actual.Derecho = InsertarRecursivo(actual.Derecho, nuevaCategoria);
			}
			else
			{
				throw new InvalidOperationException(
					$"Ya existe una categoría llamada \"{nuevaCategoria.Nombre}\".");
			}

			return actual;
		}

		public NodoCategoria? BuscarPorNombre(string nombre)
		{
			Nodo? encontrado = BuscarRecursivo(raiz, nombre);
				return encontrado?.Categoria;
		}

		private Nodo? BuscarRecursivo(Nodo? actual, string nombre)
		{
			if (actual == null) return null;

			int comparacion = string.Compare(nombre, actual.Categoria.Nombre,
				CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace);

			if (comparacion == 0) 
			{
				return actual;
			}
			if (comparacion < 0) 
			{
				return BuscarRecursivo(actual.Izquierdo, nombre);
			}

			return BuscarRecursivo(actual.Derecho, nombre);
		}

		public void RecorridoInOrder(AccionCategoria accionPorCategoria)
		{
			RecorridoInOrderRecursivo(raiz, accionPorCategoria);
		}

		private void RecorridoInOrderRecursivo(Nodo? actual, AccionCategoria accionPorCategoria)
		{
			if (actual == null) return;

				RecorridoInOrderRecursivo(actual.Izquierdo, accionPorCategoria);
				accionPorCategoria(actual.Categoria);
				RecorridoInOrderRecursivo(actual.Derecho, accionPorCategoria);
		}
	}
}