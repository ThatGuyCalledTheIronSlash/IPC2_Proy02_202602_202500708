using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Backend.TDA.Categoria;
using Backend.Servicios;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var catalogoGlobal = new Catalogo();

app.UseDefaultFiles(); 
app.UseStaticFiles();  

// Endpoint: Inicializar catalogo y limpiar memoria
app.MapPost("/api/inicializar", () => 
{
	catalogoGlobal.InicializarCatalogo();
	return Results.Ok(new { mensaje = "Catálogo inicializado con éxito y memoria limpia." });
});

// Endpoint: Subir y procesar archivo XML
app.MapPost("/api/cargar-xml", async (IFormFile archivo) =>
{
	if (archivo == null || archivo.Length == 0)
	{
		return Results.BadRequest(new { mensaje = "No se recibió ningún archivo." });
	}

	var rutaTemporal = Path.GetTempFileName();

	try
	{
		using (var stream = new FileStream(rutaTemporal, FileMode.Create))
		{
			await archivo.CopyToAsync(stream);
		}

		var res = CargarXML.LeerArchivo(rutaTemporal, catalogoGlobal);

		return Results.Ok(new { 
			mensaje = $"¡Archivo XML procesado con éxito! Se ingresaron {res.CategoriasAgregadas} categorías y {res.LibrosAgregados} libros.",
			avisos = res.Avisos.Trim(),
			categorias = res.CategoriasAgregadas,
			libros = res.LibrosAgregados
		});
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { mensaje = $"Error al procesar el XML: {ex.Message}" });
	}
	finally
	{
		if (File.Exists(rutaTemporal))
		{
			try { File.Delete(rutaTemporal); } catch { }
		}
	}
}).DisableAntiforgery(); 

// Endpoint: Grafica de libros por categoria en Graphviz (incluye subcategorias)
app.MapGet("/api/grafica/{categoria}", (string categoria) =>
{
	try
	{
		var librosTotales = catalogoGlobal.ObtenerLibrosDeCategoriaYSubcategorias(categoria);
		if (librosTotales.EstaVacio())
		{
			return Results.BadRequest(new { mensaje = $"La categoría '{categoria}' (y sus subcategorías) no contiene libros para graficar." });
		}

		var webRoot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
		var carpetaImg = Path.Combine(webRoot, "img");
		if (!Directory.Exists(carpetaImg))
		{
			Directory.CreateDirectory(carpetaImg);
		}

		string nombreSeguro = ReporteGraphviz.SanitizarNombreArchivo(categoria);
		string nombreArchivo = $"grafica_{nombreSeguro}.png";
		var rutaSalida = Path.Combine(carpetaImg, nombreArchivo);

		ReporteGraphviz.GenerarGraficaLibros(librosTotales, rutaSalida);
		string urlImagen = $"/img/{nombreArchivo}?t={DateTime.Now.Ticks}";
		return Results.Ok(new { url = urlImagen, mensaje = "¡Gráfica generada con éxito (incluye libros de subcategorías)!" });
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { mensaje = ex.Message });
	}
});

// Endpoint: Obtener libro con menor ISBN
app.MapGet("/api/libro-menor", () =>
{
	var libro = catalogoGlobal.ObtenerLibroMenorISBN();
	if (libro == null)
	{
		return Results.NotFound(new { mensaje = "No hay libros en el catálogo." });
	}

	return Results.Ok(new { 
		isbn = libro.ISBN, 
		titulo = libro.Titulo, 
		autor = libro.Autor, 
		categoria = libro.Categoria?.Nombre ?? ""
	});
});

// Endpoint: Obtener libro con mayor ISBN
app.MapGet("/api/libro-mayor", () =>
{
	var libro = catalogoGlobal.ObtenerLibroMayorISBN();
	if (libro == null)
	{
		return Results.NotFound(new { mensaje = "No hay libros en el catálogo." });
	}

	return Results.Ok(new { 
		isbn = libro.ISBN, 
		titulo = libro.Titulo, 
		autor = libro.Autor, 
		categoria = libro.Categoria?.Nombre ?? "" 
	});
});

// Endpoint: Registrar un nuevo libro manualmente
app.MapPost("/api/registrar-libro", (DatosLibro nuevoLibro) =>
{
	try
	{
		catalogoGlobal.RegistrarLibro(nuevoLibro.Isbn, nuevoLibro.Titulo, nuevoLibro.Autor, nuevoLibro.Categoria);
		return Results.Ok(new { mensaje = $"¡El libro '{nuevoLibro.Titulo}' fue guardado con éxito!" });
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { mensaje = ex.Message });
	}
});

// Endpoint: Buscar un libro por ISBN
app.MapGet("/api/libro/{isbn:long}", (long isbn) =>
{
	var libro = catalogoGlobal.BuscarLibro(isbn);
	if (libro == null)
	{
		return Results.NotFound(new { mensaje = $"No se encontró ningún libro con el ISBN {isbn}." });
	}
	return Results.Ok(new {
		isbn = libro.ISBN,
		titulo = libro.Titulo,
		autor = libro.Autor,
		categoria = libro.Categoria?.Nombre ?? ""
	});
});

// Endpoint: Eliminar un libro por ISBN
app.MapDelete("/api/libro/{isbn:long}", (long isbn) =>
{
	try
	{
		catalogoGlobal.EliminarLibro(isbn);
		return Results.Ok(new { mensaje = $"El libro con ISBN {isbn} fue eliminado exitosamente del catálogo." });
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { mensaje = ex.Message });
	}
});

// Endpoint: Listar arbol en HTML hasta los libros (con opcion de subcategoria)
app.MapGet("/api/arbol-categorias", (string? subcat) =>
{
	try
	{
		string html = catalogoGlobal.GenerarArbolHtml(subcat);
		return Results.Ok(new { html });
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { mensaje = ex.Message });
	}
});

// Endpoint: Agregar una nueva categoria
app.MapPost("/api/agregar-categoria", (DatosCategoria nuevaCat) =>
{
	try
	{
		if (string.IsNullOrWhiteSpace(nuevaCat.Nombre))
		{
			return Results.BadRequest(new { mensaje = "El nombre de la categoría es obligatorio." });
		}
		string? padre = string.IsNullOrWhiteSpace(nuevaCat.Padre) ? null : nuevaCat.Padre.Trim();
		catalogoGlobal.AgregarCategoria(nuevaCat.Nombre.Trim(), padre);
		
		return Results.Ok(new { mensaje = $"¡La categoría '{nuevaCat.Nombre}' se agregó correctamente!" });
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { mensaje = ex.Message });
	}
});

// Endpoint: Estadisticas del Dashboard (totales de libros y categorias)
app.MapGet("/api/dashboard", () =>
{
	return Results.Ok(new { 
		libros = catalogoGlobal.ContarTotalLibros(), 
		categorias = catalogoGlobal.ContarTotalCategorias() 
	});
});

// Endpoint: Grafica del arbol en Graphviz hasta los libros (con opcion de subcategoria)
app.MapGet("/api/grafica-arbol", (string? subcat) =>
{
	try
	{
		NodoCategoria? subcatNodo = null;
		if (!string.IsNullOrWhiteSpace(subcat))
		{
			subcatNodo = catalogoGlobal.BuscarCategoria(subcat.Trim());
			if (subcatNodo == null)
			{
				return Results.BadRequest(new { mensaje = $"La subcategoría '{subcat}' no existe en el catálogo." });
			}
		}
		else if (catalogoGlobal.Categorias.CategoriasPrincipales.EstaVacio())
		{
			return Results.BadRequest(new { mensaje = "No hay categorías registradas para graficar." });
		}

		var webRoot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
		var carpetaImg = Path.Combine(webRoot, "img");
		if (!Directory.Exists(carpetaImg))
		{
			Directory.CreateDirectory(carpetaImg);
		}

		string sufijo = subcatNodo != null ? ReporteGraphviz.SanitizarNombreArchivo(subcatNodo.Nombre) : "completo";
		string nombreArchivo = $"grafica_arbol_{sufijo}.png";
		var rutaSalida = Path.Combine(carpetaImg, nombreArchivo);

		ReporteGraphviz.GenerarGraficaJerarquia(
			subcatNodo,
			catalogoGlobal.Categorias.CategoriasPrincipales,
			rutaSalida);

		return Results.Ok(new { url = $"/img/{nombreArchivo}?t={DateTime.Now.Ticks}" });
	}
	catch (Exception ex)
	{
		return Results.BadRequest(new { mensaje = ex.Message });
	}
});

// Endpoint: Listar todos los libros en orden ascendente por ISBN
app.MapGet("/api/todos-libros", () =>
{
	string json = catalogoGlobal.GenerarJsonTodosLibros();
	return Results.Content(json, "application/json");
});

app.Run();

public record DatosLibro(long Isbn, string Titulo, string Autor, string Categoria);
public record DatosCategoria(string Nombre, string? Padre);
