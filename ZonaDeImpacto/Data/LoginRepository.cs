using Microsoft.Data.SqlClient;
using System.Data;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Data
{
    public class LoginRepository
    {
        private readonly string _connectionString;

        public LoginRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexion");
        }

        public async Task<Usuario> LoginAsync(string usuario, string password)
        {
            Usuario user = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_LoginUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@password", password);

                await conn.OpenAsync();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        user = new Usuario
                        {
                            idUsuario = dr.GetInt32(0),
                            nombreCompleto = dr.GetString(1),
                            usuario = dr.GetString(2),
                            rol = dr.GetString(3),
                            estado = dr.GetBoolean(4)
                        };
                    }
                }
            }
            return user;
        }

    }
}
