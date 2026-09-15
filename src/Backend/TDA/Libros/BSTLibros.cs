using System;

namespace Backend.TDA.Libros
{
    public class BSTLibros
    {
        private class Nodo
        {
            public NodoLibro Libro;
            public Nodo? Izquierdo;
            public Nodo? Derecho;

            public Nodo(NodoLibro libro)
            {
                Libro = libro;
            }
        }

        private Nodo? raiz;

        public BSTLibros()
        {
            raiz = null;
        }

        public bool EstaVacio()
        {
            return raiz == null;
        }

//Insercion
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
                throw new InvalidOperationException(
                    $"Ya existe un libro registrado con el ISBN {nuevoLibro.ISBN}.");
            }

            return actual;
        }

//Busqueda
        public NodoLibro? BuscarPorISBN(int isbn)
        {
            Nodo? encontrado = BuscarRecursivo(raiz, isbn);
            return encontrado?.Libro;
        }

        private Nodo? BuscarRecursivo(Nodo? actual, int isbn)
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

//Minimo y Maximo
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

// Eliminación de un libro por ISBN
        public void EliminarPorISBN(int isbn)
        {
            raiz = EliminarRecursivo(raiz, isbn);
        }

        private Nodo? EliminarRecursivo(Nodo? actual, int isbn)
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

            return actual;
        }

//Reccorrido in-order para procesar todos los libros en orden de ISBN
        public void RecorridoInOrder(Action<NodoLibro> accionPorLibro)
        {
            RecorridoInOrderRecursivo(raiz, accionPorLibro);
        }

        private void RecorridoInOrderRecursivo(Nodo? actual, Action<NodoLibro> accionPorLibro)
        {
            if (actual == null) return;

            RecorridoInOrderRecursivo(actual.Izquierdo, accionPorLibro);
            accionPorLibro(actual.Libro);
            RecorridoInOrderRecursivo(actual.Derecho, accionPorLibro);
        }
    }
}