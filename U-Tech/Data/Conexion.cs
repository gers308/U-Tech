using Microsoft.Data.SqlClient;

namespace U_Tech.Data
{
    public class Conexion
    {
        private readonly IConfiguration _config;

        public Conexion(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection GetConexion()
        {
            return new SqlConnection(_config.GetConnectionString("cnn"));
        }
    }
}
