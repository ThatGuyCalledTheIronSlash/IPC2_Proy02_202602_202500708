using System;
using System.Globalization;

namespace Backend.TDA.Categoria
{
	public delegate void AccionCategoria(NodoCategoria categoria);

	public class AVLCategorias
	{
		private class Nodo
		{
			public NodoCategoria Categoria;
			public Nodo? Izquierdo;
			public Nodo? Derecho;
			public int Altura;

			public Nodo(NodoCategoria categoria)
			{
				Categoria = categoria;
				Altura = 1;
			}
		}

		private Nodo? raiz;

		public AVLCategorias()
		{
			raiz = null;
		}

		public bool EstaVacio()
		{
			return raiz == null;
		}

		private int ObtenerAltura(Nodo? n) => n?.Altura ?? 0;

		private int ObtenerFactorBalance(Nodo? n)
		{
			if (n == null) return 0;
			return ObtenerAltura(n.Izquierdo) - ObtenerAltura(n.Derecho);
		}

		private void ActualizarAltura(Nodo n)
		{
			int altIzq = ObtenerAltura(n.Izquierdo);
			int altDer = ObtenerAltura(n.Derecho);
			n.Altura = 1 + (altIzq > altDer ? altIzq : altDer);
		}

		private Nodo RotacionDerecha(Nodo y)
		{
			Nodo x = y.Izquierdo!;
			Nodo? t2 = x.Derecho;

			x.Derecho = y;
			y.Izquierdo = t2;

			ActualizarAltura(y);
			ActualizarAltura(x);

			return x;
		}

		private Nodo RotacionIzquierda(Nodo x)
		{
			Nodo y = x.Derecho!;
			Nodo? t2 = y.Izquierdo;

			y.Izquierdo = x;
			x.Derecho = t2;

			ActualizarAltura(x);
			ActualizarAltura(y);

			return y;
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

			ActualizarAltura(actual);

			int balance = ObtenerFactorBalance(actual);

			// Izquierda-Izquierda
			if (balance > 1 && ObtenerFactorBalance(actual.Izquierdo) >= 0)
			{
				return RotacionDerecha(actual);
			}

			// Izquierda-Derecha
			if (balance > 1 && ObtenerFactorBalance(actual.Izquierdo) < 0)
			{
				actual.Izquierdo = RotacionIzquierda(actual.Izquierdo!);
				return RotacionDerecha(actual);
			}

			// Derecha-Derecha
			if (balance < -1 && ObtenerFactorBalance(actual.Derecho) <= 0)
			{
				return RotacionIzquierda(actual);
			}

			// Derecha-Izquierda
			if (balance < -1 && ObtenerFactorBalance(actual.Derecho) > 0)
			{
				actual.Derecho = RotacionDerecha(actual.Derecho!);
				return RotacionIzquierda(actual);
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