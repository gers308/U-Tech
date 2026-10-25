using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using U_Tech.Data;
using U_Tech.Models;
using System.Data;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http; // Necesario para CookieOptions

namespace U_Tech.Controllers
{
    public class LoginController : Controller
    {
        private readonly Conexion _conexion;
        private readonly ILogger<LoginController> _logger;

        public LoginController(Conexion conexion, ILogger<LoginController> logger)
        {
            _conexion = conexion;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Rol") != null)
            {
                return RedirectToDashboard();
            }

            // Asegura que la vista NUNCA reciba un modelo nulo.
            return View(new LoginModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using (var conn = _conexion.GetConexion())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("sp_Login", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Usuario", model.Usuario.Trim());
                        cmd.Parameters.AddWithValue("@Contrasena", model.Contrasena);

                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                // Lógica de la sesión (exitosa)
                                HttpContext.Session.SetString("Rol", dr["Nombre_Rol"].ToString());
                                HttpContext.Session.SetString("Sucursal_ID", dr["Sucursal_ID"].ToString());
                                HttpContext.Session.SetString("Personal_ID", dr["Personal_ID"].ToString());
                                HttpContext.Session.SetString("Usuario", model.Usuario);

                                // Lógica de la Cookie "Recordarme"
                                if (model.Recordarme)
                                {
                                    CookieOptions option = new CookieOptions
                                    {
                                        Expires = DateTime.Now.AddDays(30),
                                        Path = "/"
                                    };
                                    Response.Cookies.Append("UsuarioRecordado", model.Usuario.Trim(), option);
                                }
                                else
                                {
                                    Response.Cookies.Delete("UsuarioRecordado", new CookieOptions { Path = "/" });
                                }

                                _logger.LogInformation($"Login exitoso para el usuario: {model.Usuario}");
                                return RedirectToDashboard();
                            }
                            else
                            {
                                _logger.LogWarning($"Intento de login fallido para el usuario: {model.Usuario}");
                                ModelState.AddModelError("", "Credenciales incorrectas");
                                return View(model);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado durante el login para usuario: {model.Usuario}");
                ModelState.AddModelError("", "Error inesperado. Por favor, contacte al administrador.");
                return View(model);
            }
        }

        // ==========================================================
        // MÉTODO LOGOUT
        // ==========================================================
        public IActionResult Logout()
        {
            var usuario = HttpContext.Session.GetString("Usuario");
            _logger.LogInformation($"Logout del usuario: {usuario}");
            HttpContext.Session.Clear();
            return View("LogoutView");
        }

        private IActionResult RedirectToDashboard()
        {
            string rol = HttpContext.Session.GetString("Rol")?.ToLower() ?? "";
            return rol switch
            {
                "administrador" => RedirectToAction("Index", "Admin"),
                "gerentesucursal" => RedirectToAction("Index", "Gerente"),
                "personal" => RedirectToAction("Index", "Recepcionista"),
                "tecnico" => RedirectToAction("Index", "Tecnico"),
                _ => RedirectToAction("Logout")
            };
        }

        // ==========================================================
        // MÉTODOS DE RECUPERACIÓN (CORRECCIÓN APLICADA)
        // ==========================================================

        public IActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SolicitarRecuperacion(string identificador)
        {
            if (string.IsNullOrWhiteSpace(identificador))
            {
                TempData["Error"] = "Debe ingresar su nombre de usuario o correo electrónico.";
                return View("RecuperarContrasena");
            }

            int personalId = ObtenerPersonalIdPorUsuario(identificador);

            if (personalId > 0)
            {
                TempData["PersonalIdRecuperacion"] = personalId;
                TempData["Success"] = "Usuario verificado. Proceda a cambiar su contraseña.";
                return RedirectToAction("RestablecerContrasena");
            }
            else
            {
                TempData["Error"] = "El usuario o correo ingresado no existe en nuestro sistema.";
                return View("RecuperarContrasena");
            }
        }

        public IActionResult RestablecerContrasena()
        {
            if (TempData["PersonalIdRecuperacion"] == null)
            {
                TempData["Error"] = "Acceso denegado. Debe iniciar el proceso de recuperación.";
                return RedirectToAction("RecuperarContrasena");
            }

            ViewBag.PersonalId = TempData["PersonalIdRecuperacion"];
            return View();
        }

        [HttpPost]
        public IActionResult GuardarNuevaContrasena(int Personal_ID, string NuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(NuevaContrasena) || NuevaContrasena.Length < 4)
            {
                TempData["Error"] = "La contraseña debe tener al menos 4 caracteres.";
                ViewBag.PersonalId = Personal_ID;
                return View("RestablecerContrasena");
            }

            try
            {
                using (var conn = _conexion.GetConexion())
                {
                    conn.Open();
                    // 🔥 CORRECCIÓN 1: Apuntar a la tabla 'Usuarios' (plural)
                    using (var cmd = new SqlCommand(
                        "UPDATE Usuarios SET Contrasena = @clave WHERE Personal_ID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@clave", NuevaContrasena);
                        cmd.Parameters.AddWithValue("@id", Personal_ID);
                        cmd.ExecuteNonQuery();
                    }
                }
                TempData["Success"] = "¡Contraseña restablecida con éxito! Ya puede iniciar sesión.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar la contraseña: " + ex.Message;
                ViewBag.PersonalId = Personal_ID;
                return View("RestablecerContrasena");
            }
        }

        // 🔥 CORRECCIÓN 2: Apuntar a la tabla 'Usuarios' (plural)
        private int ObtenerPersonalIdPorUsuario(string identificador)
        {
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                // Usamos la tabla 'Usuarios' que es la que tiene esa columna
                using (var cmd = new SqlCommand(
                    "SELECT Personal_ID FROM Usuarios WHERE Usuario = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", identificador);
                    var result = cmd.ExecuteScalar();
                    return result != null ? (int)result : 0;
                }
            }
        }
        // ==========================================================
    }
}