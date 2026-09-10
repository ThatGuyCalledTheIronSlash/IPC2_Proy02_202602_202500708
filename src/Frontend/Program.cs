using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
// Importamos tu backend
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

// 5. Encender el servidor web
app.Run();