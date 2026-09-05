using Backend.TDA.Libros;
using Backend.TDA.Categoria;

var categorias = new BSTCategorias();
categorias.Insertar(new NodoCategoria("Ficcion"));
categorias.Insertar(new NodoCategoria("Ciencia"));
categorias.Insertar(new NodoCategoria("Historia"));

Console.WriteLine("Categorias en orden alfabetico:");
categorias.RecorridoInOrder(cat => Console.WriteLine($"  {cat.Nombre}"));

var encontrada = categorias.BuscarPorNombre("Ciencia");
Console.WriteLine($"Buscar 'Ciencia': {(encontrada != null ? "encontrada" : "no encontrada")}");