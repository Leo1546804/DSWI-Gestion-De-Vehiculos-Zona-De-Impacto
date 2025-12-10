using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
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

        // Listado de usuarios
        public async Task<List<Usuario>> ListarUsuariosAsync()
        {
            var lista = new List<Usuario>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_ListarUsuarios", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Usuario
                            {
                                idUsuario = dr.GetInt32(dr.GetOrdinal("idUsuario")),
                                nombreCompleto = dr.GetString(dr.GetOrdinal("nombreCompleto")),
                                usuario = dr.GetString(dr.GetOrdinal("usuario")),
                                rol = dr.GetString(dr.GetOrdinal("rol")),
                                estado = dr.GetBoolean(dr.GetOrdinal("estado"))
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // Registrar usuario
        public async Task RegistrarUsuarioAsync(Usuario usuario)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_InsertarUsuario", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nombreCompleto", usuario.nombreCompleto);
                    cmd.Parameters.AddWithValue("@usuario", usuario.usuario);
                    cmd.Parameters.AddWithValue("@password", usuario.password);
                    cmd.Parameters.AddWithValue("@rol", usuario.rol);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Buscar usuario por id
        public async Task<Usuario> ObtenerUsuarioAsync(int id)
        {
            Usuario usuario = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_ObtenerUsuario", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idUsuario", id);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            usuario = new Usuario
                            {
                                idUsuario = dr.GetInt32(dr.GetOrdinal("idUsuario")),
                                nombreCompleto = dr.GetString(dr.GetOrdinal("nombreCompleto")),
                                usuario = dr.GetString(dr.GetOrdinal("usuario")),
                                password = dr.GetString(dr.GetOrdinal("password")),
                                rol = dr.GetString(dr.GetOrdinal("rol")),
                                estado = dr.GetBoolean(dr.GetOrdinal("estado"))
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        // Editar usuario
        public async Task EditarUsuarioAsync(Usuario usuario)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_EditarUsuario", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@idUsuario", usuario.idUsuario);
                    cmd.Parameters.AddWithValue("@nombreCompleto", usuario.nombreCompleto);
                    cmd.Parameters.AddWithValue("@usuario", usuario.usuario);
                    cmd.Parameters.AddWithValue("@password", usuario.password);
                    cmd.Parameters.AddWithValue("@rol", usuario.rol);
                    cmd.Parameters.AddWithValue("@estado", usuario.estado);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Eliminar usuario (cambia estado a 0)
        public async Task EliminarUsuarioAsync(int idUsuario)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("sp_EliminarUsuario", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Método adicional para verificar si usuario existe
        public async Task<bool> ExisteUsuarioAsync(string nombreUsuario)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE usuario = @usuario", conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", nombreUsuario);
                    var count = (int)await cmd.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        // Método para cambiar solo el estado
        public async Task CambiarEstadoAsync(int idUsuario, bool nuevoEstado)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("UPDATE Usuarios SET estado = @estado WHERE idUsuario = @idUsuario", conn))
                {
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@estado", nuevoEstado);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

    }
}