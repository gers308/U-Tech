using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Mail; // 👈 AÑADIDO PARA VALIDAR EMAIL
using U_Tech.Data;

namespace U_Tech.Controllers
{
    public class RecepcionistaController : Controller
    {
        private readonly Conexion _conexion;

        public RecepcionistaController(Conexion conexion)
        {
            _conexion = conexion;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "personal")
                return RedirectToAction("Index", "Login");

            return View();
        }


        // GET: Vista para registrar cliente
        public IActionResult RegistrarCliente()
        {
            return View();
        }

        // ==============================================================
        // MÉTODO 'RegistrarCliente' [POST] ACTUALIZADO CON VALIDACIONES
        // ==============================================================
        [HttpPost]
        public IActionResult RegistrarCliente(string Nombre, string Telefono, string Email)
        {
            // 🔥 CLAVE: Poner los datos en ViewBag INMEDIATAMENTE
            // Así, cualquier return View() los mantendrá.
            ViewBag.Nombre = Nombre;
            ViewBag.Telefono = Telefono;
            ViewBag.Email = Email;

            // --- SECCIÓN DE VALIDACIONES ---

            // 1. Validar campos vacíos
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Telefono) || string.IsNullOrWhiteSpace(Email))
            {
                TempData["Error"] = "Todos los campos son obligatorios.";
                return View();
            }

            // 2. NUEVA VALIDACIÓN: Formato de Email (con System.Net.Mail)
            try
            {
                // Esto intenta crear un objeto MailAddress.
                // Si el formato es inválido (ej: "texto", "texto@", "texto@.com"),
                // lanzará una excepción que el 'catch' capturará.
                var emailAddress = new MailAddress(Email);
            }
            catch (FormatException)
            {
                TempData["Error"] = "El formato del correo electrónico no es válido (ej: usuario@dominio.com).";
                return View();
            }

            // 3. Validar si el email ya existe (la que tenías)
            // (Solo se ejecuta si el formato es válido)
            if (EmailExiste(Email))
            {
                TempData["Error"] = "El correo electrónico ya está registrado. Por favor use otro.";
                return View();
            }

            // --- FIN DE VALIDACIONES ---

            // Si llegamos aquí, los datos son válidos. Procedemos a insertar.
            try
            {
                using (var conn = _conexion.GetConexion())
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO Cliente (Nombre, Telefono, Email) VALUES (@Nombre, @Telefono, @Email)", conn);

                    cmd.Parameters.AddWithValue("@Nombre", Nombre);
                    cmd.Parameters.AddWithValue("@Telefono", Telefono);
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Cliente registrado correctamente.";
                return RedirectToAction("Index"); // Solo redirige si fue exitoso
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "Error al registrar el cliente: " + ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error inesperado: " + ex.Message;
                return View();
            }
        }

        // 🔥 MÉTODO PARA VERIFICAR SI EL EMAIL EXISTE
        private bool EmailExiste(string email)
        {
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Cliente WHERE Email = @Email", conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    var count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }


        //antigua forma de registrar cliente con validaciones con errores
        /*
        ... (tu código comentado anterior)
        */
        //aca termina el registrar cliente con sus validaciones



        public IActionResult RegistrarPC(int? clienteId)
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "personal")
                return RedirectToAction("Index", "Login");

            ViewBag.ClienteSeleccionado = clienteId;
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarPC(int Cliente_ID, string Marca, string Tipo_computadora, string Descripcion)
        {
            try
            {
                // 🔥 VALIDACIONES ANTES de insertar
                if (Cliente_ID <= 0)
                {
                    TempData["Error"] = "Debe seleccionar un cliente válido";
                    ViewBag.ClienteSeleccionado = Cliente_ID;
                    ViewBag.Marca = Marca;
                    ViewBag.Tipo_computadora = Tipo_computadora;
                    ViewBag.Descripcion = Descripcion;
                    return View();
                }

                if (string.IsNullOrWhiteSpace(Marca))
                {
                    TempData["Error"] = "La marca es obligatoria";
                    ViewBag.ClienteSeleccionado = Cliente_ID;
                    ViewBag.Marca = Marca;
                    ViewBag.Tipo_computadora = Tipo_computadora;
                    ViewBag.Descripcion = Descripcion;
                    return View();
                }

                if (string.IsNullOrWhiteSpace(Tipo_computadora))
                {
                    TempData["Error"] = "El tipo de computadora es obligatorio";
                    ViewBag.ClienteSeleccionado = Cliente_ID;
                    ViewBag.Marca = Marca;
                    ViewBag.Tipo_computadora = Tipo_computadora;
                    ViewBag.Descripcion = Descripcion;
                    return View();
                }

                if (string.IsNullOrWhiteSpace(Descripcion))
                {
                    TempData["Error"] = "La descripción del problema es obligatoria";
                    ViewBag.ClienteSeleccionado = Cliente_ID;
                    ViewBag.Marca = Marca;
                    ViewBag.Tipo_computadora = Tipo_computadora;
                    ViewBag.Descripcion = Descripcion;
                    return View();
                }

                // Si pasa todas las validaciones, insertar en BD
                using (var conn = _conexion.GetConexion())
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO Computadora (Cliente_ID, Marca, Tipo_computadora, Descripcion) VALUES (@Cliente_ID, @Marca, @Tipo, @Descripcion)", conn);

                    cmd.Parameters.AddWithValue("@Cliente_ID", Cliente_ID);
                    cmd.Parameters.AddWithValue("@Marca", Marca);
                    cmd.Parameters.AddWithValue("@Tipo", Tipo_computadora);
                    cmd.Parameters.AddWithValue("@Descripcion", Descripcion);
                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Computadora registrada correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar la computadora: " + ex.Message;
                ViewBag.ClienteSeleccionado = Cliente_ID;
                ViewBag.Marca = Marca;
                ViewBag.Tipo_computadora = Tipo_computadora;
                ViewBag.Descripcion = Descripcion;
                return View();
            }
        }

        //antigua forma de registrar pc para cliente sin validaciones
        /*
        ... (tu código comentado anterior)
        */



        //solictud pc
        public IActionResult CrearSolicitud(int? pcId)
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "personal")
                return RedirectToAction("Index", "Login");

            ViewBag.PCSeleccionada = pcId;
            return View();
        }

        [HttpPost]
        public IActionResult CrearSolicitud(int PC_ID)
        {
            try
            {
                // 🔥 VALIDACIONES ANTES de insertar
                if (PC_ID <= 0)
                {
                    TempData["Error"] = "Debe seleccionar una computadora válida";
                    ViewBag.PCSeleccionada = PC_ID;
                    return View();
                }

                // Validar que la PC exista en la base de datos
                if (!PCExiste(PC_ID))
                {
                    TempData["Error"] = "La computadora seleccionada no existe";
                    ViewBag.PCSeleccionada = PC_ID;
                    return View();
                }

                // Validar que no tenga una solicitud activa
                if (TieneSolicitudActiva(PC_ID))
                {
                    TempData["Error"] = "Esta computadora ya tiene una solicitud activa";
                    ViewBag.PCSeleccionada = PC_ID;
                    return View();
                }

                var sucursal = HttpContext.Session.GetString("Sucursal_ID");
                var personal = HttpContext.Session.GetString("Personal_ID");

                // Validar sesión
                if (string.IsNullOrEmpty(sucursal) || string.IsNullOrEmpty(personal))
                {
                    TempData["Error"] = "Error de sesión. Por favor, inicie sesión nuevamente";
                    ViewBag.PCSeleccionada = PC_ID;
                    return View();
                }

                using (var conn = _conexion.GetConexion())
                {
                    conn.Open();
                    var cmd = new SqlCommand(@"
                INSERT INTO Solicitud (PC_ID, Cliente_ID, Sucursal_ID, Personal_ID, fecha_recibida, Estado)
                SELECT @PC_ID, Cliente_ID, @Sucursal_ID, @Personal_ID, GETDATE(), 'Recibido'
                FROM Computadora WHERE PC_ID = @PC_ID", conn);

                    cmd.Parameters.AddWithValue("@PC_ID", PC_ID);
                    cmd.Parameters.AddWithValue("@Sucursal_ID", sucursal);
                    cmd.Parameters.AddWithValue("@Personal_ID", personal);
                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Solicitud creada correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear la solicitud: " + ex.Message;
                ViewBag.PCSeleccionada = PC_ID;
                return View();
            }
        }

        // 🔥 MÉTODO PARA VERIFICAR SI LA PC EXISTE
        private bool PCExiste(int pcId)
        {
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Computadora WHERE PC_ID = @PC_ID", conn))
                {
                    cmd.Parameters.AddWithValue("@PC_ID", pcId);
                    var count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        // 🔥 MÉTODO PARA VERIFICAR SI TIENE SOLICITUD ACTIVA
        private bool TieneSolicitudActiva(int pcId)
        {
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Solicitud WHERE PC_ID = @PC_ID AND Estado IN ('Recibido', 'En Proceso', 'Diagnosticando')", conn))
                {
                    cmd.Parameters.AddWithValue("@PC_ID", pcId);
                    var count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }



        /* antigua forma de crear solicitud pc sin validaciones
        ... (tu código comentado anterior)
        */




        //lista de solicitudes diagnosticadas para que el cliente decida
        public IActionResult SolicitudesDiagnosticadas()
        {
            var sucursal = HttpContext.Session.GetString("Sucursal_ID");
            DataTable tabla = new DataTable();

            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT * FROM vw_Solicitud_Detalle WHERE Estado = 'Diagnosticado' AND Sucursal_ID = " + sucursal, conn);
                da.Fill(tabla);
            }

            return View(tabla);
        }




        //decidir si aceptar o rechazar reparacion
        public IActionResult DecidirCliente(int id)
        {
            ViewBag.SolicitudID = id;
            return View();
        }

        [HttpPost]
        public IActionResult GuardarDecision(int Solicitud_ID, string Decision)
        {
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var cmd = new SqlCommand(@"
                        UPDATE Solicitud 
                        SET Estado = CASE 
                                WHEN @d = 'Aceptar' THEN 'En Reparacion'
                                ELSE 'Finalizado'
                            END
                        WHERE Solicitud_ID = @id", conn);

                cmd.Parameters.AddWithValue("@d", Decision);
                cmd.Parameters.AddWithValue("@id", Solicitud_ID);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("SolicitudesDiagnosticadas");
        }




        //lista de clientes para registrar pc
        public IActionResult ListaClientes()
        {
            DataTable clientes = new DataTable();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT Cliente_ID, Nombre, Telefono, Email FROM Cliente", conn);
                da.Fill(clientes);
            }
            return View(clientes);
        }





        //lista de computadoras para crear solicitud
        public IActionResult ListaComputadoras()
        {
            var sucursal = HttpContext.Session.GetString("Sucursal_ID");

            DataTable pcs = new DataTable();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(@"
            SELECT 
                PC.PC_ID,
                C.Nombre AS Nombre_Cliente,
                PC.Marca,
                PC.Tipo_computadora,
                PC.Descripcion
            FROM Computadora PC
            INNER JOIN Cliente C ON PC.Cliente_ID = C.Cliente_ID
        ", conn);
                da.Fill(pcs);
            }

            return View(pcs);
        }












        // Ver computadoras finalizadas (lista que el recepcionista usa para entregar)
        public IActionResult ComputadorasFinalizadas()
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "personal")
                return RedirectToAction("Index", "Login");

            var sucursal = HttpContext.Session.GetString("Sucursal_ID");
            DataTable tabla = new DataTable();

            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
            @"SELECT R.Reparacion_ID, C.Cliente_ID, C.Nombre AS Cliente, 
                    PC.PC_ID, PC.Marca, PC.Tipo_computadora, 
                    R.Fecha_Final, R.Estado_Final, S.Sucursal_ID
            FROM Reparacion R
            INNER JOIN Diagnostico D ON R.Diagnostico_ID = D.Diagnostico_ID
            INNER JOIN Solicitud S ON D.Solicitud_ID = S.Solicitud_ID
            INNER JOIN Cliente C ON S.Cliente_ID = C.Cliente_ID
            INNER JOIN Computadora PC ON S.PC_ID = PC.PC_ID
            WHERE R.Estado_Final = 'Finalizado' AND S.Sucursal_ID = @Sucursal", conn);


                da.SelectCommand.Parameters.AddWithValue("@Sucursal", sucursal);
                da.Fill(tabla);
            }

            return View(tabla);
        }

        // Entregar computadora (marca como Entregado y registra en Entregas)


        // ===========================
        // ENTREGAR COMPUTADORA (SIN TABLA DE ENTREGAS)
        // ===========================
        [HttpPost]
        public IActionResult EntregarComputadora(int reparacionId)
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "personal")
                return RedirectToAction("Index", "Login");

            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "UPDATE Reparacion SET Estado_Final = 'Entregado' WHERE Reparacion_ID = @id", conn);
                cmd.Parameters.AddWithValue("@id", reparacionId);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("ComputadorasFinalizadas");
        }




    }
}