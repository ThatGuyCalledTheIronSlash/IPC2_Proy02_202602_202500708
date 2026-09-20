using Backend.TDA.Categoria;

namespace Backend.TDA.Libros
{
	public class NodoLibro
	{
		public long ISBN { get; set; }
		public string Titulo { get; set; }
		public string Autor { get; set; }
		public NodoCategoria? Categoria { get; set; }

		public NodoLibro(long isbn, string titulo, string autor)
		{
			ISBN = isbn;
			Titulo = titulo;
			Autor = autor;
		}
	}
}