using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using U_Tech.Data;

namespace U_Tech.Controllers
{
    public class TecnicoController : Controller
    {
        private readonly Conexion _conexion;

        public TecnicoController(Conexion conexion)
        {
            _conexion = conexion;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "tecnico")
                return RedirectToAction("Index", "Login");

            var sucursal = HttpContext.Session.GetString("Sucursal_ID");
            DataTable tabla = new DataTable();

            try
            {
                using (var conn = _conexion.GetConexion())
                {
                    conn.Open();
                    using (var da = new SqlDataAdapter(
                        // 🔥 Corregí aquí: Tu consulta original era vulnerable a Inyección SQL.
                        // Ahora usa parámetros para seguridad.
                        "SELECT * FROM vw_Solicitud_Detalle WHERE Estado = 'Recibido' AND Sucursal_ID = @Sucursal", conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@Sucursal", sucursal);
                        da.Fill(tabla);
                    }
                }
            }
            catch (Exception ex)
            {
                // Es mejor mostrar un error en la vista
                TempData["Error"] = "Error al cargar solicitudes: " + ex.Message;
            }

            return View(tabla);
        }


        //crear accion para diagnostico
        public IActionResult Diagnosticar(int id)
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "tecnico")
                return RedirectToAction("Index", "Login");

            ViewBag.SolicitudID = id;
            return View();
        }

        //accion para guardar diagnostico (ESTE MÉTODO YA ESTÁ BIEN)
        [HttpPost]
        public IActionResult RegistrarDiagnostico(int Solicitud_ID, string Descripcion_Diagnostico, decimal Costo_Estimacion)
        {
            try
            {
                // 🔥 VALIDACIONES ANTES de insertar
                if (Solicitud_ID <= 0)
                {
                    TempData["Error"] = "Solicitud no válida";
                    ViewBag.SolicitudID = Solicitud_ID;
                    ViewBag.Descripcion_Diagnostico = Descripcion_Diagnostico;
                    ViewBag.Costo_Estimacion = Costo_Estimacion;
                    return View("Diagnosticar");
                }

                if (string.IsNullOrWhiteSpace(Descripcion_Diagnostico))
                {
                    TempData["Error"] = "La descripción del diagnóstico es obligatoria";
                    ViewBag.SolicitudID = Solicitud_ID;
                    ViewBag.Descripcion_Diagnostico = Descripcion_Diagnostico;
                    ViewBag.Costo_Estimacion = Costo_Estimacion;
                    return View("Diagnosticar");
                }

                if (Descripcion_Diagnostico.Length < 10)
                {
                    TempData["Error"] = "La descripción debe tener al menos 10 caracteres";
                    ViewBag.SolicitudID = Solicitud_ID;
                    ViewBag.Descripcion_Diagnostico = Descripcion_Diagnostico;
                    ViewBag.Costo_Estimacion = Costo_Estimacion;
                    return View("Diagnosticar");
                }

                if (Costo_Estimacion <= 0)
                {
                    TempData["Error"] = "El costo estimado debe ser mayor a 0";
                    ViewBag.SolicitudID = Solicitud_ID;
                    ViewBag.Descripcion_Diagnostico = Descripcion_Diagnostico;
                    ViewBag.Costo_Estimacion = Costo_Estimacion;
                    return View("Diagnosticar");
                }

                if (Costo_Estimacion > 100000) // Ajusta este límite según necesites
                {
                    TempData["Error"] = "El costo estimado es demasiado alto";
                    ViewBag.SolicitudID = Solicitud_ID;
                    ViewBag.Descripcion_Diagnostico = Descripcion_Diagnostico;
                    ViewBag.Costo_Estimacion = Costo_Estimacion;
                    return View("Diagnosticar");
                }

                var personal = HttpContext.Session.GetString("Personal_ID");

                // Validar sesión
                if (string.IsNullOrEmpty(personal))
                {
                    TempData["Error"] = "Error de sesión. Por favor, inicie sesión nuevamente";
                    ViewBag.SolicitudID = Solicitud_ID;
                    ViewBag.Descripcion_Diagnostico = Descripcion_Diagnostico;
                    ViewBag.Costo_Estimacion = Costo_Estimacion;
                    return View("Diagnosticar");
                }

                using (var conn = _conexion.GetConexion())
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                INSERT INTO Diagnostico (Descripcion_Diagnostico, Costo_Estimacion, Fecha, Personal_ID, Solicitud_ID)
                VALUES (@desc, @costo, GETDATE(), @per, @sol);

                UPDATE Solicitud SET Estado = 'Diagnosticado'
                WHERE Solicitud_ID = @sol;
            ", conn);

                    cmd.Parameters.AddWithValue("@desc", Descripcion_Diagnostico.Trim());
                    cmd.Parameters.AddWithValue("@costo", Costo_Estimacion);
                    cmd.Parameters.AddWithValue("@per", personal);
                    cmd.Parameters.AddWithValue("@sol", Solicitud_ID);
                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Diagnóstico registrado correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar el diagnóstico: " + ex.Message;
                ViewBag.SolicitudID = Solicitud_ID;
                ViewBag.Descripcion_Diagnostico = Descripcion_Diagnostico;
                ViewBag.Costo_Estimacion = Costo_Estimacion;
                return View("Diagnosticar");
            }
        }



        //metodo para ver solcitudes en reparacion
        public IActionResult EnReparacion()
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "tecnico")
                return RedirectToAction("Index", "Login");

            var sucursal = HttpContext.Session.GetString("Sucursal_ID");
            DataTable tabla = new DataTable();

            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    // 🔥 También parametericé esta consulta
                    "SELECT * FROM vw_Solicitud_Detalle WHERE Estado = 'En Reparacion' AND Sucursal_ID = @Sucursal", conn);

                da.SelectCommand.Parameters.AddWithValue("@Sucursal", sucursal);
                da.Fill(tabla);
            }

            return View(tabla);
        }



        // ==============================================================
        // MÉTODOS DE REPARACIÓN ACTUALIZADOS CON VALIDACIONES
        // ==============================================================

        // 1. GET: Mostrar el formulario de reparación
        public IActionResult Reparar(int id)
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "tecnico")
                return RedirectToAction("Index", "Login");

            ViewBag.SolicitudID = id;
            return View(); // Esto busca la vista Reparar.cshtml
        }

        // 2. POST: Guardar la reparación con validaciones
        [HttpPost]
        public IActionResult GuardarReparacion(int Solicitud_ID, string Detalle, decimal Costo)
        {
            try
            {
                // --- 1. VALIDACIONES ---
                if (string.IsNullOrWhiteSpace(Detalle))
                {
                    TempData["Error"] = "El detalle del trabajo es obligatorio.";
                    ViewBag.SolicitudID = Solicitud_ID; // Repoblar
                    ViewBag.Detalle = Detalle;          // Repoblar
                    ViewBag.Costo = Costo;              // Repoblar
                    return View("Reparar"); // Devuelve a la vista original con datos
                }

                if (Costo <= 0)
                {
                    TempData["Error"] = "El costo final debe ser un valor positivo (mayor a 0).";
                    ViewBag.SolicitudID = Solicitud_ID; // Repoblar
                    ViewBag.Detalle = Detalle;          // Repoblar
                    ViewBag.Costo = Costo;              // Repoblar
                    return View("Reparar");
                }

                var personal = HttpContext.Session.GetString("Personal_ID");

                // Validar sesión
                if (string.IsNullOrEmpty(personal))
                {
                    TempData["Error"] = "Error de sesión. Por favor, inicie sesión nuevamente";
                    ViewBag.SolicitudID = Solicitud_ID;
                    ViewBag.Detalle = Detalle;
                    ViewBag.Costo = Costo;
                    return View("Reparar");
                }

                // --- 2. LÓGICA DE GUARDADO (Tu consulta SQL está perfecta) ---
                using (var conn = _conexion.GetConexion())
                {
                    conn.Open();

                    // Tu lógica SQL compleja (está bien)
                    var cmd = new SqlCommand(@"
                INSERT INTO Reparacion (Fecha_Inicio, Fecha_Final, Detalle_trabajo, Costo_Final, Estado_Final, Diagnostico_ID)
                SELECT GETDATE(), GETDATE(), @detalle, @costo, 'Finalizado', d.Diagnostico_ID
                FROM Diagnostico d
                WHERE d.Solicitud_ID = @sol;

                UPDATE Solicitud 
                SET Estado = 'Finalizado'
                WHERE Solicitud_ID = @sol;

                INSERT INTO Expediente (Descripcion, Reparacion_ID, Tecnico_ID, PC_ID, Cliente_ID)
                SELECT 
                    @detalle, 
                    r.Reparacion_ID, 
                    d.Personal_ID, 
                    pc.PC_ID, 
                    c.Cliente_ID
                FROM Reparacion r
                INNER JOIN Diagnostico d ON r.Diagnostico_ID = d.Diagnostico_ID
                INNER JOIN Solicitud s ON d.Solicitud_ID = s.Solicitud_ID
                INNER JOIN Computadora pc ON s.PC_ID = pc.PC_ID
                INNER JOIN Cliente c ON pc.Cliente_ID = c.Cliente_ID
                WHERE s.Solicitud_ID = @sol;
            ", conn);

                    // Parámetros seguros
                    cmd.Parameters.AddWithValue("@detalle", Detalle.Trim());
                    cmd.Parameters.AddWithValue("@costo", Costo);
                    cmd.Parameters.AddWithValue("@sol", Solicitud_ID);

                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Reparación guardada y finalizada con éxito.";
                return RedirectToAction("EnReparacion");
            }
            catch (Exception ex)
            {
                // --- 3. MANEJO DE EXCEPCIONES ---
                TempData["Error"] = "Error inesperado al guardar la reparación: " + ex.Message;
                ViewBag.SolicitudID = Solicitud_ID;
                ViewBag.Detalle = Detalle;
                ViewBag.Costo = Costo;
                return View("Reparar");
            }
        }


    } // Fin de la clase TecnicoController
}