using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using U_Tech.Data;

namespace U_Tech.Controllers
{
    public class ExpedienteController : Controller
    {
        private readonly Conexion _conexion;
        public ExpedienteController(Conexion conexion) { _conexion = conexion; }

        public IActionResult Index()
        {
            DataTable tabla = new DataTable();

            using (var conn = _conexion.GetConexion())
            {
                conn.Open();
                var da = new SqlDataAdapter("SELECT * FROM vw_Expediente_Completo", conn);
                da.Fill(tabla);
            }

            return View(tabla);
        }
    }

}
