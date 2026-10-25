using Microsoft.AspNetCore.Mvc.Filters;

namespace U_Tech.Filters
{
    public class NoCacheFilter : IActionFilter
    {
        // Se ejecuta antes de la acción del controlador. No necesitamos hacer nada aquí.
        public void OnActionExecuting(ActionExecutingContext context)
        {

        }

        // Se ejecuta DESPUÉS de que la acción del controlador ha terminado.
        // Aquí es donde agregamos los encabezados de control de caché.
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var response = context.HttpContext.Response;

            // 1. Evita que la página sea almacenada en caché por el navegador o proxies.
            response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

            // 2. Encabezado de compatibilidad con HTTP 1.0.
            response.Headers.Add("Pragma", "no-cache");

            // 3. Establece la expiración en una fecha ya pasada (0).
            response.Headers.Add("Expires", "0");
        }
    }
}