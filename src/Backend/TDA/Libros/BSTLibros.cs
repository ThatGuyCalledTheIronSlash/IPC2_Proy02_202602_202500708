using System;

namespace Backend.TDA.Libros
{
    /// <summary>
    /// Árbol binario de búsqueda (TDA propio) que organiza libros
    /// por su ISBN. Permite inserción, búsqueda, eliminación,
    /// obtención de mínimo/máximo y recorrido ordenado, todo en
    /// tiempo logarítmico promedio.
    /// </summary>
    public class BSTLibros
    {
        private NodoLibro raiz;

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

        private NodoLibro InsertarRecursivo(NodoLibro actual, NodoLibro nuevoLibro)
        {
            if (actual == null)
            {
                return nuevoLibro;
            }

            if (nuevoLibro.ISBN < actual.ISBN)
            {
                actual.Izquierdo = InsertarRecursivo(actual.Izquierdo, nuevoLibro);
            }
            else if (nuevoLibro.ISBN > actual.ISBN)
            {
                actual.Derecho = InsertarRecursivo(actual.Derecho, nuevoLibro);
            }
            else
            {
                // ISBN duplicado: la restricción indica que el ISBN es único.
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
            return BuscarRecursivo(raiz, isbn);
        }

        private NodoLibro BuscarRecursivo(NodoLibro actual, int isbn)
        {
            if (actual == null || actual.ISBN == isbn)
            {
                return actual;
            }

            if (isbn < actual.ISBN)
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
            return ObtenerMinimoRecursivo(raiz);
        }

        private NodoLibro ObtenerMinimoRecursivo(NodoLibro actual)
        {
            if (actual.Izquierdo == null)
            {
                return actual;
            }
            return ObtenerMinimoRecursivo(actual.Izquierdo);
        }

        public NodoLibro ObtenerMaximo()
        {
            if (raiz == null) return null;
            return ObtenerMaximoRecursivo(raiz);
        }

        private NodoLibro ObtenerMaximoRecursivo(NodoLibro actual)
        {
            if (actual.Derecho == null)
            {
                return actual;
            }
            return ObtenerMaximoRecursivo(actual.Derecho);
        }

        // ---------------------------------------------------------
        // ELIMINACIÓN (3 casos clásicos de BST)
        // ---------------------------------------------------------
        public void EliminarPorISBN(int isbn)
        {
            raiz = EliminarRecursivo(raiz, isbn);
        }

        private NodoLibro EliminarRecursivo(NodoLibro actual, int isbn)
        {
            if (actual == null)
            {
                // No existe un libro con ese ISBN; no hay nada que eliminar.
                return null;
            }

            if (isbn < actual.ISBN)
            {
                actual.Izquierdo = EliminarRecursivo(actual.Izquierdo, isbn);
            }
            else if (isbn > actual.ISBN)
            {
                actual.Derecho = EliminarRecursivo(actual.Derecho, isbn);
            }
            else
            {
                // Encontramos el nodo a eliminar.

                // Caso 1: sin hijos, o solo un hijo.
                if (actual.Izquierdo == null)
                {
                    return actual.Derecho;
                }
                if (actual.Derecho == null)
                {
                    return actual.Izquierdo;
                }

                // Caso 2: dos hijos.
                // Buscamos el sucesor in-order (el mínimo del subárbol derecho),
                // copiamos sus datos al nodo actual, y eliminamos el sucesor.
                NodoLibro sucesor = ObtenerMinimoRecursivo(actual.Derecho);

                actual.ISBN = sucesor.ISBN;
                actual.Titulo = sucesor.Titulo;
                actual.Autor = sucesor.Autor;
                actual.Categoria = sucesor.Categoria;

                actual.Derecho = EliminarRecursivo(actual.Derecho, sucesor.ISBN);
            }

            return actual;
        }

        // ---------------------------------------------------------
        // RECORRIDO IN-ORDER (orden ascendente por ISBN)
        // ---------------------------------------------------------
        // Se usa un callback (Action) en vez de una colección propia
        // de C#, para no violar la restricción de no usar List/Queue/etc.
        // Quien llame a este método decide qué hacer con cada libro
        // (imprimirlo, agregarlo a un TDA propio, generar un nodo .dot, etc.).
        public void RecorridoInOrder(Action<NodoLibro> accionPorLibro)
        {
            RecorridoInOrderRecursivo(raiz, accionPorLibro);
        }

        private void RecorridoInOrderRecursivo(NodoLibro actual, Action<NodoLibro> accionPorLibro)
        {
            if (actual == null) return;

            RecorridoInOrderRecursivo(actual.Izquierdo, accionPorLibro);
            accionPorLibro(actual);
            RecorridoInOrderRecursivo(actual.Derecho, accionPorLibro);
        }
    }
}