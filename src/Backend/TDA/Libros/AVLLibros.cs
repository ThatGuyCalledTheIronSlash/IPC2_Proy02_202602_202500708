using System;

namespace Backend.TDA.Libros
{
	public delegate void AccionLibro(NodoLibro libro);

	public class AVLLibros
	{
		private class Nodo
		{
			public NodoLibro Libro;
			public Nodo? Izquierdo;
			public Nodo? Derecho;
			public int Altura;

			public Nodo(NodoLibro libro)
			{
				Libro = libro;
				Altura = 1;
			}
		}

		private Nodo? raiz;

		public AVLLibros()
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

		public void Insertar(NodoLibro nuevoLibro)
		{
			raiz = InsertarRecursivo(raiz, nuevoLibro);
		}

		private Nodo InsertarRecursivo(Nodo? actual, NodoLibro nuevoLibro)
		{
			if (actual == null)
			{
				return new Nodo(nuevoLibro);
			}

			if (nuevoLibro.ISBN < actual.Libro.ISBN)
			{
				actual.Izquierdo = InsertarRecursivo(actual.Izquierdo, nuevoLibro);
			}
			else if (nuevoLibro.ISBN > actual.Libro.ISBN)
			{
				actual.Derecho = InsertarRecursivo(actual.Derecho, nuevoLibro);
			}
			else
			{
				throw new InvalidOperationException($"Ya existe un libro registrado con el ISBN {nuevoLibro.ISBN}.");
			}

			ActualizarAltura(actual);

			int balance = ObtenerFactorBalance(actual);

			if (balance > 1 && nuevoLibro.ISBN < actual.Izquierdo!.Libro.ISBN)
			{
				return RotacionDerecha(actual);
			}

			if (balance < -1 && nuevoLibro.ISBN > actual.Derecho!.Libro.ISBN)
			{
				return RotacionIzquierda(actual);
			}

			if (balance > 1 && nuevoLibro.ISBN > actual.Izquierdo!.Libro.ISBN)
			{
				actual.Izquierdo = RotacionIzquierda(actual.Izquierdo!);
				return RotacionDerecha(actual);
			}

			if (balance < -1 && nuevoLibro.ISBN < actual.Derecho!.Libro.ISBN)
			{
				actual.Derecho = RotacionDerecha(actual.Derecho!);
				return RotacionIzquierda(actual);
			}

			return actual;
		}

		public NodoLibro? BuscarPorISBN(long isbn)
		{
			Nodo? encontrado = BuscarRecursivo(raiz, isbn);
			return encontrado?.Libro;
		}

		private Nodo? BuscarRecursivo(Nodo? actual, long isbn)
		{
			if (actual == null || actual.Libro.ISBN == isbn)
			{
				return actual;
			}

			if (isbn < actual.Libro.ISBN)
			{
				return BuscarRecursivo(actual.Izquierdo, isbn);
			}

			return BuscarRecursivo(actual.Derecho, isbn);
		}

		public NodoLibro? ObtenerMinimo()
		{
			if (raiz == null) return null;
			return ObtenerMinimoRecursivo(raiz).Libro;
		}

		private Nodo ObtenerMinimoRecursivo(Nodo actual)
		{
			if (actual.Izquierdo == null) return actual;
			return ObtenerMinimoRecursivo(actual.Izquierdo);
		}

		public NodoLibro? ObtenerMaximo()
		{
			if (raiz == null) return null;
			return ObtenerMaximoRecursivo(raiz).Libro;
		}

		private Nodo ObtenerMaximoRecursivo(Nodo actual)
		{
			if (actual.Derecho == null) return actual;
			return ObtenerMaximoRecursivo(actual.Derecho);
		}

		public void EliminarPorISBN(long isbn)
		{
			raiz = EliminarRecursivo(raiz, isbn);
		}

		private Nodo? EliminarRecursivo(Nodo? actual, long isbn)
		{
			if (actual == null)
			{
				return null;
			}

			if (isbn < actual.Libro.ISBN)
			{
				actual.Izquierdo = EliminarRecursivo(actual.Izquierdo, isbn);
			}
			else if (isbn > actual.Libro.ISBN)
			{
				actual.Derecho = EliminarRecursivo(actual.Derecho, isbn);
			}
			else
			{
				if (actual.Izquierdo == null) return actual.Derecho;
				if (actual.Derecho == null) return actual.Izquierdo;

				Nodo sucesor = ObtenerMinimoRecursivo(actual.Derecho);
				actual.Libro = sucesor.Libro;
				actual.Derecho = EliminarRecursivo(actual.Derecho, sucesor.Libro.ISBN);
			}

			ActualizarAltura(actual);

			int balance = ObtenerFactorBalance(actual);

			if (balance > 1 && ObtenerFactorBalance(actual.Izquierdo) >= 0)
				return RotacionDerecha(actual);

			if (balance > 1 && ObtenerFactorBalance(actual.Izquierdo) < 0)
			{
				actual.Izquierdo = RotacionIzquierda(actual.Izquierdo!);
				return RotacionDerecha(actual);
			}

			if (balance < -1 && ObtenerFactorBalance(actual.Derecho) <= 0)
				return RotacionIzquierda(actual);

			if (balance < -1 && ObtenerFactorBalance(actual.Derecho) > 0)
			{
				actual.Derecho = RotacionDerecha(actual.Derecho!);
				return RotacionIzquierda(actual);
			}

			return actual;
		}

		public void RecorridoInOrder(AccionLibro accionPorLibro)
		{
			RecorridoInOrderRecursivo(raiz, accionPorLibro);
		}

		private void RecorridoInOrderRecursivo(Nodo? actual, AccionLibro accionPorLibro)
		{
			if (actual == null) return;

			RecorridoInOrderRecursivo(actual.Izquierdo, accionPorLibro);
			accionPorLibro(actual.Libro);
			RecorridoInOrderRecursivo(actual.Derecho, accionPorLibro);
		}
	}
}