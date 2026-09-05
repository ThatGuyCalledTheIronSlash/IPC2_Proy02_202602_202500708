using System;

namespace Backend.TDA.Libros
{
    /// <summary>
    /// Árbol binario de búsqueda (TDA propio) indexado por ISBN.
    /// La estructura del árbol (izquierdo/derecho) vive en una clase
    /// interna privada (Nodo), separada de NodoLibro. Así, el mismo
    /// NodoLibro puede insertarse en varios BSTLibros distintos
    /// (por ejemplo: el índice global del catálogo y el índice local
    /// de su categoría) sin que ambos árboles se pisen entre sí.
    /// </summary>
    public class BSTLibros
    {
        private class Nodo
        {
            public NodoLibro Libro;
            public Nodo Izquierdo;
            public Nodo Derecho;

            public Nodo(NodoLibro libro)
            {
                Libro = libro;
            }
        }

        private Nodo raiz;

        public BSTLibros()
        {
            raiz = null;
        }

        public bool EstaVacio()
        {
            return raiz == null;
        }

        // ---------------------------------------------------------
        // INSERCIÓN
        // ---------------------------------------------------------
        public void Insertar(NodoLibro nuevoLibro)
        {
            raiz = InsertarRecursivo(raiz, nuevoLibro);
        }

        private Nodo InsertarRecursivo(Nodo actual, NodoLibro nuevoLibro)
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

        // ---------------------------------------------------------
        // BÚSQUEDA
        // ---------------------------------------------------------
        public NodoLibro BuscarPorISBN(int isbn)
        {
            Nodo encontrado = BuscarRecursivo(raiz, isbn);
            return encontrado?.Libro;
        }

        private Nodo BuscarRecursivo(Nodo actual, int isbn)
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

        // ---------------------------------------------------------
        // MÍNIMO Y MÁXIMO
        // ---------------------------------------------------------
        public NodoLibro ObtenerMinimo()
        {
            if (raiz == null) return null;
            return ObtenerMinimoRecursivo(raiz).Libro;
        }

        private Nodo ObtenerMinimoRecursivo(Nodo actual)
        {
            if (actual.Izquierdo == null) return actual;
            return ObtenerMinimoRecursivo(actual.Izquierdo);
        }

        public NodoLibro ObtenerMaximo()
        {
            if (raiz == null) return null;
            return ObtenerMaximoRecursivo(raiz).Libro;
        }

        private Nodo ObtenerMaximoRecursivo(Nodo actual)
        {
            if (actual.Derecho == null) return actual;
            return ObtenerMaximoRecursivo(actual.Derecho);
        }

        // ---------------------------------------------------------
        // ELIMINACIÓN (3 casos clásicos de BST)
        // ---------------------------------------------------------
        public void EliminarPorISBN(int isbn)
        {
            raiz = EliminarRecursivo(raiz, isbn);
        }

        private Nodo EliminarRecursivo(Nodo actual, int isbn)
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

        // ---------------------------------------------------------
        // RECORRIDO IN-ORDER (orden ascendente por ISBN)
        // ---------------------------------------------------------
        public void RecorridoInOrder(Action<NodoLibro> accionPorLibro)
        {
            RecorridoInOrderRecursivo(raiz, accionPorLibro);
        }

        private void RecorridoInOrderRecursivo(Nodo actual, Action<NodoLibro> accionPorLibro)
        {
            if (actual == null) return;

            RecorridoInOrderRecursivo(actual.Izquierdo, accionPorLibro);
            accionPorLibro(actual.Libro);
            RecorridoInOrderRecursivo(actual.Derecho, accionPorLibro);
        }
    }
}