using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Backend.TDA.Categoria;
using Backend.Servicios;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. Instancia del Gestor Global (Catálogo)
var catalogoGlobal = new Catalogo();

// 2. Servidor busca y sirve "index.html" por defecto al entrar a la raíz "/"
app.UseDefaultFiles(); 

// 3. Habilitamos la carpeta "wwwroot" para que los navegadores puedan descargar el CSS
app.UseStaticFiles();  

// 4. Endpoint para el botón "Inicializar"
app.MapPost("/api/inicializar", () => 
{
    catalogoGlobal.InicializarCatalogo();
    return Results.Ok(new { mensaje = "Catálogo inicializado con éxito y memoria limpia." });
});

// 5. Endpoint para subir el XML
// Nota: DisableAntiforgery() es requerido en .NET modernos al subir archivos por Minimal APIs
app.MapPost("/api/cargar-xml", async (IFormFile archivo) =>
{
    if (archivo == null || archivo.Length == 0)
    {
        return Results.BadRequest(new { mensaje = "No se recibió ningún archivo." });
    }

    var rutaTemporal = Path.GetTempFileName();

    using (var stream = new FileStream(rutaTemporal, FileMode.Create))
    {
        await archivo.CopyToAsync(stream);
    }

    try
    {
        // LeerArchivo ahora devuelve los avisos acumulados
        string avisos = CargarXML.LeerArchivo(rutaTemporal, catalogoGlobal);

        File.Delete(rutaTemporal);

        // Si hay avisos, los incluimos en la respuesta para que el usuario los vea
        if (!string.IsNullOrWhiteSpace(avisos))
        {
            return Results.Ok(new { 
                mensaje = "XML procesado con algunos avisos.",
                avisos = avisos.Trim()
            });
        }

        return Results.Ok(new { 
            mensaje = "¡Archivo XML procesado e ingresado al catálogo con éxito!",
            avisos = "" 
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al procesar el XML: {ex.Message}");
    }
}).DisableAntiforgery(); 


// 6. Endpoint para generar y obtener la gráfica de Graphviz
app.MapGet("/api/grafica/{categoria}", (string categoria) =>
{
    // Usamos BuscarCategoria del Catálogo (que delega a ArbolCategorias)
    var nodoCategoria = catalogoGlobal.BuscarCategoria(categoria);
    if (nodoCategoria == null)
        {
            return Results.NotFound(new { mensaje = $"La categoría '{categoria}' no existe en el catálogo." });
        }

        var carpetaImg = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
        if (!Directory.Exists(carpetaImg))
        {
            Directory.CreateDirectory(carpetaImg);
        }

        // Nombre único por categoría para que no se sobreescriban las gráficas
        string nombreArchivo = $"grafica_{categoria}.png";
        var rutaSalida = Path.Combine(carpetaImg, nombreArchivo);

        try
            {
                ReporteGraphviz.GenerarGraficaLibros(nodoCategoria.LibrosDirectos, rutaSalida);
                string urlImagen = $"/img/{nombreArchivo}?t={DateTime.Now.Ticks}";
                return Results.Ok(new { url = urlImagen, mensaje = "¡Gráfica generada con éxito!" });
            }
        catch (Exception ex)
            {
                return Results.Problem($"Error al generar gráfica: {ex.Message}");
            }
    });

//7. Endpoint para obtener el Libro Mayor y Menor
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
//8. Endpoint para registrar un libro manualmente
app.MapPost("/api/registrar-libro", (DatosLibro nuevoLibro) =>
{
    try
    {
        // Llamamos a metodo del catálogo para registrar el libro, que internamente validará si la categoría existe y si el ISBN es único
        catalogoGlobal.RegistrarLibro(nuevoLibro.Isbn, nuevoLibro.Titulo, nuevoLibro.Autor, nuevoLibro.Categoria);
        
        return Results.Ok(new { mensaje = $"¡El libro '{nuevoLibro.Titulo}' fue guardado con éxito!" });
    }
    catch (Exception ex)
    {
        // Si el libro ya existe o la categoría no existe, tu clase Catalogo tirará una Excepción y la atrapamos aquí
        return Results.BadRequest(new { mensaje = ex.Message });
    }
});

//9. Endpoint para buscar libros de forma especifica por ISBN
app.MapGet("/api/libro/{isbn:int}", (int isbn) =>
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

//10. Endpoint para eliminar libros
app.MapDelete("/api/libro/{isbn:int}", (int isbn) =>
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

//11. Endpoint para obtener todos los libros de todas las categorías
app.MapGet("/api/arbol-categorias", () =>
{
    if (catalogoGlobal.Categorias.CategoriasPrincipales.EstaVacio())
    {
        return Results.Ok(new { html = "<p style='color:#7f8c8d;'>No hay categorías registradas en el catálogo.</p>" });
    }
    var sb = new System.Text.StringBuilder();
    sb.Append("<ul style='list-style-type: square; margin-left: 20px;'>");
    void ConstruirArbolHtml(BSTCategorias bst)
    {
        bst.RecorridoInOrder(cat =>
        {
            string nombreSeguro = System.Net.WebUtility.HtmlEncode(cat.Nombre);
            sb.Append($"<li style='margin-bottom: 5px; font-size: 16px;'><strong>{nombreSeguro}</strong>");
            if (!cat.Hijos.EstaVacio())
            {
                sb.Append("<ul style='list-style-type: circle; margin-left: 20px; color: #2980b9;'>");
                ConstruirArbolHtml(cat.Hijos);
                sb.Append("</ul>");
            }
            sb.Append("</li>");
        });
    }
    ConstruirArbolHtml(catalogoGlobal.Categorias.CategoriasPrincipales);
    sb.Append("</ul>");
    return Results.Ok(new { html = sb.ToString() });
});

//12. Registrar nueva Categoria
app.MapPost("/api/agregar-categoria", (DatosCategoria nuevaCat) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(nuevaCat.Nombre))
        {
            return Results.BadRequest(new { mensaje = "El nombre de la categoría es obligatorio." });
        }
        // Si mandaron el padre en blanco, lo volvemos null para que sea raíz
        string? padre = string.IsNullOrWhiteSpace(nuevaCat.Padre) ? null : nuevaCat.Padre.Trim();
        catalogoGlobal.AgregarCategoria(nuevaCat.Nombre.Trim(), padre);
        
        return Results.Ok(new { mensaje = $"¡La categoría '{nuevaCat.Nombre}' se agregó correctamente!" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { mensaje = ex.Message });
    }
});

//13. Endpoint para Dashboard: Cantidad de Libros y Categorias
app.MapGet("/api/dashboard", () =>
{
    int totalLibros = 0;
    int totalCategorias = 0;
        // Contar libros es fácil recorriendo el índice global
            catalogoGlobal.IndiceGlobalLibros.RecorridoInOrder(l => totalLibros++);
        // Contar categorías requiere una pequeña recursividad por el árbol
            void ContarCategorias(BSTCategorias bst)
                {
                    bst.RecorridoInOrder(c => {
                        totalCategorias++;
                        ContarCategorias(c.Hijos);
                    });
                }
    
        if (!catalogoGlobal.Categorias.CategoriasPrincipales.EstaVacio())
            {
                ContarCategorias(catalogoGlobal.Categorias.CategoriasPrincipales);
            }
             return Results.Ok(new { libros = totalLibros, categorias = totalCategorias });
});
//14.Endpoint para Arbol de Categorias en Graphviz
app.MapGet("/api/grafica-arbol", () =>
{
    var carpetaImg = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
    if (!Directory.Exists(carpetaImg))
    {
        Directory.CreateDirectory(carpetaImg);
    }
    var rutaSalida = Path.Combine(carpetaImg, "grafica_arbol.png");
    try
    {
        ReporteGraphviz.GenerarGraficaCategorias(
            catalogoGlobal.Categorias.CategoriasPrincipales, rutaSalida);
        return Results.Ok(new { url = $"/img/grafica_arbol.png?t={DateTime.Now.Ticks}" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { mensaje = ex.Message });
    }
});
//15. Endpoint para listar TODOS los libros en orden ascendente por ISBN
app.MapGet("/api/todos-libros", () =>
{
    if (catalogoGlobal.IndiceGlobalLibros.EstaVacio())
    {
        return Results.Content("[]", "application/json");
    }
    var sb = new System.Text.StringBuilder();
    sb.Append("[");
    bool primero = true;
    catalogoGlobal.IndiceGlobalLibros.RecorridoInOrder(libro =>
    {
        if (!primero) sb.Append(",");
        primero = false;
        string titulo = libro.Titulo.Replace("\\", "\\\\").Replace("\"", "\\\"");
        string autor = libro.Autor.Replace("\\", "\\\\").Replace("\"", "\\\"");
        string cat = (libro.Categoria?.Nombre ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
        sb.Append($"{{\"isbn\":{libro.ISBN},\"titulo\":\"{titulo}\",\"autor\":\"{autor}\",\"categoria\":\"{cat}\"}}");
    });
    sb.Append("]");
    return Results.Content(sb.ToString(), "application/json");
});



app.Run();

// Estructura temporal para recibir el JSON de la web
public record DatosLibro(int Isbn, string Titulo, string Autor, string Categoria);
public record DatosCategoria(string Nombre, string? Padre);






