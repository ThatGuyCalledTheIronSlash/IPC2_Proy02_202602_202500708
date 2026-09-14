using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Backend.Servicios;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// 1. Instanciamos el Gestor Global (Catálogo)
// Esta variable vivirá en memoria mientras el servidor esté encendido.
var catalogoGlobal = new Catalogo();

// 2. Le decimos al servidor que busque y sirva "index.html" por defecto al entrar a la raíz "/"
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
            categoria = libro.Categoria.Nombre
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
        categoria = libro.Categoria.Nombre 
        });
    });
//8. Endpoint para registrar un libro manualmente
app.MapPost("/api/registrar-libro", (DatosLibro nuevoLibro) =>
{
    try
    {
        // Llamamos a tu método que ya tiene toda la validación
        catalogoGlobal.RegistrarLibro(nuevoLibro.Isbn, nuevoLibro.Titulo, nuevoLibro.Autor, nuevoLibro.Categoria);
        
        return Results.Ok(new { mensaje = $"¡El libro '{nuevoLibro.Titulo}' fue guardado con éxito!" });
    }
    catch (Exception ex)
    {
        // Si el libro ya existe o la categoría no existe, tu clase Catalogo tirará una Excepción y la atrapamos aquí
        return Results.BadRequest(new { mensaje = ex.Message });
    }
});

app.Run();

// Estructura temporal para recibir el JSON de la web
public record DatosLibro(int Isbn, string Titulo, string Autor, string Categoria);






