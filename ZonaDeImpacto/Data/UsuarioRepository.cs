using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Data
{
    public class UsuarioRepository
    {
        private readonly string _connectionString;
        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexion");
        }

        //listado de usuarios
        public async Task<List<Usuario>> ListarUsuariosAsync()
        {
            List<Usuario> lista = new List<Usuario>();

            using(SqlConnection conn = new SqlConnection(_connectionString))
            using(SqlCommand cmd = new SqlCommand("sp_ListarUsuarios", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while(await dr.ReadAsync())
                    {
                        lista.Add(new Usuario
                        {
                            idUsuario = dr.GetInt32(0),
                            nombreCompleto = dr.GetString(1),
                            usuario = dr.GetString(2),
                            rol = dr.GetString(3),
                            estado = dr.GetBoolean(4)
                        });
                    }
                }
            }
            return lista;
        }

        //registrar usuario
        public async Task RegistrarUsuarioAsync(Usuario usuario)
        {
            using(SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_InsertarUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@nombreCompleto", usuario.nombreCompleto);
                cmd.Parameters.AddWithValue("@usuario", usuario.usuario);
                cmd.Parameters.AddWithValue("@password", usuario.password);
                cmd.Parameters.AddWithValue("@rol", usuario.rol);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        //Buscar usuario por id
        public async Task<Usuario> ObtenerUsuarioAsync(int id)
        {
            Usuario usuario = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ObtenerUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idUsuario", id);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        usuario = new Usuario
                        {
                            idUsuario = dr.GetInt32(0),
                            nombreCompleto = dr.GetString(1),
                            usuario = dr.GetString(2),
                            password = dr.GetString(3),
                            rol = dr.GetString(4),
                            estado = dr.GetBoolean(5)
                        };
                    }
                }
            }
            return usuario;
        }

        public async Task EditarUsuarioAsync(Usuario usuario)
        {
            using(SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_EditarUsuario", conn))
            {
                cmd.CommandType= CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idUsuario", usuario.idUsuario);
                cmd.Parameters.AddWithValue("@nombreCompleto", usuario.nombreCompleto);
                cmd.Parameters.AddWithValue("@usuario", usuario.usuario);
                cmd.Parameters.AddWithValue("@password", usuario.password);
                cmd.Parameters.AddWithValue("@rol", usuario.rol);
                cmd.Parameters.AddWithValue("@estado", usuario.estado);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task EliminarUsuarioAsync(int idUsuario)
        {
            using(SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_EliminarUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

    }
}
