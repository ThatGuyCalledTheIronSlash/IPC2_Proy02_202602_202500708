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

// 4. Creamos nuestra primera ruta (API Endpoint) para el botón "Inicializar"
app.MapPost("/api/inicializar", () => 
{
    catalogoGlobal.InicializarCatalogo(); // Llama al método que hiciste en Catalogo.cs
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
    // 1. Crear una ruta temporal segura en la PC para guardar el XML
    var rutaTemporal = Path.GetTempFileName();
    // 2. Descargar el archivo desde la web al disco duro
    using (var stream = new FileStream(rutaTemporal, FileMode.Create))
    {
        await archivo.CopyToAsync(stream);
    }
    try
    {
        // 3. Llamar a tu clase CargarXML que hicimos anteriormente
        CargarXML.LeerArchivo(rutaTemporal, catalogoGlobal);
        
        // 4. Borrar el archivo temporal porque ya lo metimos a nuestros árboles
        File.Delete(rutaTemporal);
        return Results.Ok(new { mensaje = "¡Archivo XML procesado e ingresado al catálogo con éxito!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al procesar el XML: {ex.Message}");
    }
}).DisableAntiforgery(); 


 app.Run();