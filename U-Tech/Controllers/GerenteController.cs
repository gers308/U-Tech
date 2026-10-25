using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using U_Tech.Data;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace U_Tech.Controllers
{
    public class GerenteController : Controller
    {
        private readonly Conexion _conexion;

        public GerenteController(Conexion conexion)
        {
            _conexion = conexion;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "gerentesucursal")
                return RedirectToAction("Index", "Login");

            return View();
        }

        public IActionResult EstadoSucursal()
        {
            if (HttpContext.Session.GetString("Rol")?.ToLower() != "gerentesucursal")
                return RedirectToAction("Index", "Login");

            var sucursal = HttpContext.Session.GetString("Sucursal_ID");
            DataTable tabla = new DataTable();

            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT * FROM vw_Solicitud_Detalle WHERE Sucursal_ID = " + sucursal, conn);
                da.Fill(tabla);
            }

            return View(tabla);
        }


        public IActionResult PersonalSucursal()
        {
            if (HttpContext.Session.GetString("Rol") != "GerenteSucursal")
                return RedirectToAction("Index", "Login");

            var sucursal = HttpContext.Session.GetString("Sucursal_ID");
            DataTable tabla = new DataTable();

            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT Personal_ID, Nombre, Apellido, Rol FROM Personal WHERE Sucursal_ID = " + sucursal, conn);
                da.Fill(tabla);
            }

            return View(tabla);
        }


        //rendimiento por tecnico de sus sucursal
        public IActionResult ReportesSucursal()
        {
            if (HttpContext.Session.GetString("Rol") != "GerenteSucursal")
                return RedirectToAction("Index", "Login");

            var sucursal = HttpContext.Session.GetString("Sucursal_ID");
            DataTable tabla = new DataTable();

            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(@"
            SELECT 
                P.Nombre + ' ' + P.Apellido AS Tecnico,
                COUNT(R.Reparacion_ID) AS ReparacionesRealizadas,
                ISNULL(SUM(R.Costo_Final),0) AS TotalGenerado
            FROM Reparacion R
            INNER JOIN Diagnostico D ON R.Diagnostico_ID = D.Diagnostico_ID
            INNER JOIN Solicitud S ON D.Solicitud_ID = S.Solicitud_ID
            INNER JOIN Personal P ON D.Personal_ID = P.Personal_ID
            WHERE S.Sucursal_ID = " + sucursal + @"
            GROUP BY P.Nombre, P.Apellido;", conn);

                da.Fill(tabla);
            }

            return View(tabla);
        }



        //probablemente no usar, esta en modo de prueba.
        //reporte sucursal en pdf
        public IActionResult DescargarReporteSucursalPDF()
        {
            if (HttpContext.Session.GetString("Rol") != "GerenteSucursal")
                return RedirectToAction("Index", "Login");

            var sucursal = HttpContext.Session.GetString("Sucursal_ID");

            // --- 1) TABLA: Clientes y Computadoras ---
            DataTable clientes = new DataTable();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT * FROM vw_Clientes_Computadoras WHERE Sucursal_ID = " + sucursal, conn);
                da.Fill(clientes);
            }

            // --- 2) TABLA: Solicitudes ---
            DataTable solicitudes = new DataTable();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT * FROM vw_Solicitud_Detalle WHERE Sucursal_ID = " + sucursal, conn);
                da.Fill(solicitudes);
            }

            // --- 3) TABLA: Diagnósticos ---
            DataTable diagnosticos = new DataTable();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT * FROM vw_Diagnosticos_Detalle WHERE Sucursal_ID = " + sucursal, conn);
                da.Fill(diagnosticos);
            }

            // --- 4) TABLA: Reparaciones Finalizadas ---
            DataTable reparaciones = new DataTable();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT * FROM vw_Reparaciones_Detalle WHERE Sucursal_ID = " + sucursal + " AND Estado_Final = 'Finalizado'", conn);
                da.Fill(reparaciones);
            }

            // --- 5) TABLA: Expedientes ---
            DataTable expedientes = new DataTable();
            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter(
                    "SELECT * FROM vw_Expediente_Completo WHERE Sucursal_ID = " + sucursal, conn);
                da.Fill(expedientes);
            }

            // --- Crear PDF ---
            MemoryStream ms = new MemoryStream();
            Document doc = new Document(PageSize.A4.Rotate(), 20, 20, 20, 20);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            void AgregarTabla(DataTable tabla, string titulo)
            {
                doc.Add(new Paragraph($"\n\n{titulo}\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));

                PdfPTable pdfTable = new PdfPTable(tabla.Columns.Count);
                pdfTable.WidthPercentage = 100;

                foreach (DataColumn col in tabla.Columns)
                {
                    PdfPCell header = new PdfPCell(new Phrase(col.ColumnName, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)));
                    header.BackgroundColor = BaseColor.LightGray;
                    pdfTable.AddCell(header);
                }

                foreach (DataRow row in tabla.Rows)
                    foreach (var item in row.ItemArray)
                        pdfTable.AddCell(new Phrase(item?.ToString() ?? ""));

                doc.Add(pdfTable);
            }

            doc.Add(new Paragraph($"REPORTE COMPLETO DE LA SUCURSAL #{sucursal}\n\n",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20))
            { Alignment = Element.ALIGN_CENTER });

            AgregarTabla(clientes, "CLIENTES Y COMPUTADORAS");
            AgregarTabla(solicitudes, "SOLICITUDES EN PROCESO");
            AgregarTabla(diagnosticos, "DIAGNÓSTICOS REALIZADOS");
            AgregarTabla(reparaciones, "REPARACIONES FINALIZADAS");
            AgregarTabla(expedientes, "EXPEDIENTES COMPLETOS");

            doc.Close();

            return File(ms.ToArray(), "application/pdf", "Reporte_Sucursal.pdf");
        }

    }

}
