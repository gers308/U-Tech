using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using U_Tech.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace U_Tech.Controllers
{
    public class AdminController : Controller
    {
        private readonly Conexion _conexion;

        public AdminController(Conexion conexion)
        {
            _conexion = conexion;
        }

        // === PANEL PRINCIPAL ===
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "administrador")
                return RedirectToAction("Index", "Login");

            return View();
        }

        // === VER EXPEDIENTES GLOBALES ===
        public IActionResult VerExpedientes()
        {
            DataTable tabla = new();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter("SELECT * FROM vw_Expediente_Completo", conn);
                da.Fill(tabla);
            }
            return View(tabla);
        }

        // === GESTIÓN DE SUCURSALES ===
        public IActionResult GestionSucursales()
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "administrador")
                return RedirectToAction("Index", "Login");

            DataTable tabla = new();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter("SELECT * FROM Sucursal", conn);
                da.Fill(tabla);
            }
            return View(tabla);
        }

        // === GESTIÓN DE USUARIOS ===
        public IActionResult GestionUsuarios()
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "administrador")
                return RedirectToAction("Index", "Login");

            DataTable tabla = new();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(@"
                    SELECT 
                        U.Usuario_ID, 
                        U.Usuario, 
                        R.Nombre_Rol, 
                        P.Nombre AS NombrePersonal, 
                        S.Nombre_Sucursal 
                    FROM Usuarios U
                    INNER JOIN Roles R ON U.Rol_ID = R.Rol_ID
                    LEFT JOIN Personal P ON U.Personal_ID = P.Personal_ID
                    LEFT JOIN Sucursal S ON P.Sucursal_ID = S.Sucursal_ID", conn);
                da.Fill(tabla);
            }

            return View(tabla);
        }

        // === REPORTES GLOBALES ===
        public IActionResult ReportesGlobales()
        {
            DataTable tabla = new();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(@"
                    SELECT 
                        S.Nombre_Sucursal, 
                        COUNT(R.Reparacion_ID) AS ReparacionesRealizadas
                    FROM Reparacion R
                    INNER JOIN Diagnostico D ON R.Diagnostico_ID = D.Diagnostico_ID
                    INNER JOIN Solicitud Sol ON D.Solicitud_ID = Sol.Solicitud_ID
                    INNER JOIN Sucursal S ON Sol.Sucursal_ID = S.Sucursal_ID
                    GROUP BY S.Nombre_Sucursal;", conn);
                da.Fill(tabla);
            }
            return View(tabla);
        }

        // === FUNCION AUXILIAR: CREAR PDF ===
        private byte[] CrearPDF(DataTable tabla, string titulo, string nombreArchivo)
        {
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            var header = new Paragraph($"{titulo}\n\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18))
            {
                Alignment = Element.ALIGN_CENTER
            };
            doc.Add(header);

            PdfPTable pdfTable = new(tabla.Columns.Count) { WidthPercentage = 100 };
            foreach (DataColumn col in tabla.Columns)
            {
                PdfPCell cell = new(new Phrase(col.ColumnName, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)))
                {
                    BackgroundColor = BaseColor.LightGray
                };
                pdfTable.AddCell(cell);
            }

            foreach (DataRow row in tabla.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    pdfTable.AddCell(new Phrase(item?.ToString() ?? ""));
                }
            }

            doc.Add(pdfTable);
            doc.Close();

            return ms.ToArray();
        }

        // === PDF CLIENTES Y COMPUTADORAS ===
        public IActionResult DescargarClientesComputadorasPDF()
        {
            DataTable tabla = new();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter("SELECT * FROM vw_Clientes_Computadoras", conn);
                da.Fill(tabla);
            }

            return File(CrearPDF(tabla, "REPORTE DE CLIENTES Y COMPUTADORAS", "Clientes_Computadoras.pdf"),
                "application/pdf", "Clientes_Computadoras.pdf");
        }

        // === PDF SOLICITUDES ===
        public IActionResult DescargarSolicitudesPDF()
        {
            DataTable tabla = new();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT * FROM vw_Solicitud_Detalle WHERE Estado <> 'Finalizado' ORDER BY fecha_recibida DESC", conn);
                da.Fill(tabla);
            }

            return File(CrearPDF(tabla, "REPORTE DE SOLICITUDES EN PROCESO", "Solicitudes.pdf"),
                "application/pdf", "Solicitudes_En_Proceso.pdf");
        }

        // === PDF DIAGNÓSTICOS ===
        public IActionResult DescargarDiagnosticosPDF()
        {
            DataTable tabla = new();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter("SELECT * FROM vw_Diagnosticos_Detalle ORDER BY Fecha DESC", conn);
                da.Fill(tabla);
            }

            return File(CrearPDF(tabla, "REPORTE DE DIAGNÓSTICOS", "Diagnosticos.pdf"),
                "application/pdf", "Diagnosticos.pdf");
        }

        // === PDF REPARACIONES (Finalizadas + Entregadas) ===
        public IActionResult DescargarReparacionesPDF()
        {
            DataTable tabla = new();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();

                var da = new SqlDataAdapter(
                    "SELECT * FROM vw_Reparaciones_Detalle WHERE (Estado_Final = 'Finalizado' OR Estado_Final = 'Entregado')", conn);

                da.Fill(tabla);
            }

            return File(CrearPDF(tabla, "REPORTE DE REPARACIONES FINALIZADAS Y ENTREGADAS", "Reparaciones.pdf"),
                "application/pdf", "Reparaciones_Finalizadas.pdf");
        }

        // === PDF EXPEDIENTES ===
        public IActionResult DescargarExpedientesPDF()
        {
            DataTable tabla = new();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter("SELECT * FROM vw_Expediente_Completo ORDER BY Expediente_ID DESC", conn);
                da.Fill(tabla);
            }

            return File(CrearPDF(tabla, "EXPEDIENTES COMPLETOS DEL SISTEMA", "Expedientes.pdf"),
                "application/pdf", "Expedientes_Completos.pdf");
        }
    }
}
