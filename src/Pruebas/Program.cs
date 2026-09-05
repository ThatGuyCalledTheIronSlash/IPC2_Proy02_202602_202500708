using Backend.TDA.Libros;

var bst = new BSTLibros();
bst.Insertar(new NodoLibro(500, "El Quijote", "Cervantes"));
bst.Insertar(new NodoLibro(200, "Cien años de soledad", "Garcia Marquez"));
bst.Insertar(new NodoLibro(800, "1984", "Orwell"));
bst.Insertar(new NodoLibro(100, "La Odisea", "Homero"));
bst.Insertar(new NodoLibro(300, "Rayuela", "Cortazar"));

Console.WriteLine("Recorrido ascendente:");
bst.RecorridoInOrder(libro => Console.WriteLine($"  ISBN {libro.ISBN}: {libro.Titulo}"));

Console.WriteLine($"Minimo ISBN: {bst.ObtenerMinimo().ISBN}");
Console.WriteLine($"Maximo ISBN: {bst.ObtenerMaximo().ISBN}");

bst.EliminarPorISBN(500);
Console.WriteLine("Despues de eliminar ISBN 500:");
bst.RecorridoInOrder(libro => Console.WriteLine($"  ISBN {libro.ISBN}: {libro.Titulo}"));