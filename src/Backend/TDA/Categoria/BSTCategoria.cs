using System;

namespace Backend.TDA.Categoria
{
    /// <summary>
    /// Árbol binario de búsqueda (TDA propio) que organiza
    /// categorías por nombre. Igual que BSTLibros, la estructura
    /// del árbol vive en una clase interna privada (Nodo), separada
    /// de NodoCategoria, para que una misma categoría pueda
    /// insertarse tanto en el BST de "hijos" de su padre como en
    /// un eventual índice global por nombre, sin conflictos.
    ///
    /// Se usa en dos contextos distintos dentro del proyecto:
    ///  1) Como los "hijos" de un NodoCategoria (orden alfabético
    ///     entre subcategorías del mismo nivel).
    ///  2) Como índice global de todas las categorías por nombre,
    ///     para ubicar rápido a un padre al leer el XML.
    /// </summary>
    public class BSTCategorias
    {
        private class Nodo
        {
            public NodoCategoria Categoria;
            public Nodo Izquierdo;
            public Nodo Derecho;

            public Nodo(NodoCategoria categoria)
            {
                Categoria = categoria;
            }
        }

        private Nodo raiz;

        public BSTCategorias()
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
        public void Insertar(NodoCategoria nuevaCategoria)
        {
            raiz = InsertarRecursivo(raiz, nuevaCategoria);
        }

        private Nodo InsertarRecursivo(Nodo actual, NodoCategoria nuevaCategoria)
        {
            if (actual == null)
            {
                return new Nodo(nuevaCategoria);
            }

            int comparacion = string.Compare(nuevaCategoria.Nombre, actual.Categoria.Nombre, StringComparison.Ordinal);

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

        // ---------------------------------------------------------
        // BÚSQUEDA
        // ---------------------------------------------------------
        public NodoCategoria BuscarPorNombre(string nombre)
        {
            Nodo encontrado = BuscarRecursivo(raiz, nombre);
            return encontrado?.Categoria;
        }

        private Nodo BuscarRecursivo(Nodo actual, string nombre)
        {
            if (actual == null) return null;

            int comparacion = string.Compare(nombre, actual.Categoria.Nombre, StringComparison.Ordinal);

            if (comparacion == 0) return actual;
            if (comparacion < 0) return BuscarRecursivo(actual.Izquierdo, nombre);
            return BuscarRecursivo(actual.Derecho, nombre);
        }

        // ---------------------------------------------------------
        // RECORRIDO IN-ORDER (orden alfabético)
        // ---------------------------------------------------------
        public void RecorridoInOrder(Action<NodoCategoria> accionPorCategoria)
        {
            RecorridoInOrderRecursivo(raiz, accionPorCategoria);
        }

        private void RecorridoInOrderRecursivo(Nodo actual, Action<NodoCategoria> accionPorCategoria)
        {
            if (actual == null) return;

            RecorridoInOrderRecursivo(actual.Izquierdo, accionPorCategoria);
            accionPorCategoria(actual.Categoria);
            RecorridoInOrderRecursivo(actual.Derecho, accionPorCategoria);
        }
    }
}