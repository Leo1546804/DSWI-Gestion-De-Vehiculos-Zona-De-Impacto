using System.Data;
using Microsoft.Data.SqlClient;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Data
{
    public class MantenimientoRepository
    {
        private readonly string _connectionString;

        public MantenimientoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexion");
        }

        // Listar con filtros
        public async Task<List<Mantenimiento>> ListarMantenimientosAsync(
            int? filtroCodigo = null,
            int? filtroVehiculo = null,
            string filtroTipo = null,
            DateTime? filtroFechaDesde = null,
            DateTime? filtroFechaHasta = null,
            string filtroEstado = null,
            int? idUsuarioFiltro = null)
        {
            List<Mantenimiento> lista = new List<Mantenimiento>();

            // Convertir filtroEstado a bool?
            bool? estadoFiltro = null;
            if (filtroEstado == "activos")
                estadoFiltro = true;
            else if (filtroEstado == "anulados")
                estadoFiltro = false;
            // else = null (muestra todos)

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarMantenimientos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@filtroCodigo",
                    filtroCodigo.HasValue ? (object)filtroCodigo.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@filtroVehiculo",
                    filtroVehiculo.HasValue ? (object)filtroVehiculo.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@filtroTipo",
                    string.IsNullOrEmpty(filtroTipo) ? (object)DBNull.Value : filtroTipo);
                cmd.Parameters.AddWithValue("@filtroFechaDesde",
                    filtroFechaDesde.HasValue ? (object)filtroFechaDesde.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@filtroFechaHasta",
                    filtroFechaHasta.HasValue ? (object)filtroFechaHasta.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@filtroEstado",
                    estadoFiltro.HasValue ? (object)estadoFiltro.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@idUsuario",
                    idUsuarioFiltro.HasValue ? (object)idUsuarioFiltro.Value : DBNull.Value);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new Mantenimiento
                        {
                            idMantenimiento = dr.GetInt32(0),
                            codigoMantenimiento = dr.GetString(1),
                            placa = dr.GetString(2),
                            marca = dr.GetString(3),
                            modelo = dr.GetString(4),
                            tipo = dr.GetString(5),
                            descripcion = dr.IsDBNull(6) ? null : dr.GetString(6),
                            fecha = dr.GetDateTime(7),
                            usuarioNombre = dr.GetString(8),
                            idUsuario = dr.GetInt32(9),
                            estadoLogico = dr.GetBoolean(10)
                        });
                    }
                }
            }
            return lista;
        }

        // Obtener vehículos para DROPDOWN (usando proc específico)
        public async Task<List<Vehiculo>> ObtenerVehiculosAsync()
        {
            List<Vehiculo> lista = new List<Vehiculo>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarVehiculosActivos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                await conn.OpenAsync();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new Vehiculo
                        {
                            idVehiculo = dr.GetInt32(0),
                            placa = dr.GetString(1),
                            marca = dr.GetString(2),
                            modelo = dr.GetString(3),
                            tipo = dr.IsDBNull(4) ? null : dr.GetString(4)
                        });
                    }
                }
            }
            return lista;
        }

        // Registrar nuevo mantenimiento
        public async Task RegistrarMantenimientoAsync(Mantenimiento mantenimiento)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("dbo.usp_InsertarMantenimiento", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@idVehiculo", mantenimiento.idVehiculo);
                    cmd.Parameters.AddWithValue("@tipo", mantenimiento.tipo);
                    cmd.Parameters.AddWithValue("@descripcion",
                        string.IsNullOrEmpty(mantenimiento.descripcion) ? (object)DBNull.Value : mantenimiento.descripcion);
                    cmd.Parameters.AddWithValue("@fecha", mantenimiento.fecha);
                    cmd.Parameters.AddWithValue("@idUsuario", mantenimiento.idUsuario);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Loguear el error
                Console.WriteLine($"Error en RegistrarMantenimientoAsync: {ex.Message}");
                throw; // Re-lanzar la excepción
            }
        }

        // Obtener mantenimiento por ID
        public async Task<Mantenimiento> ObtenerMantenimientoAsync(int id)
        {
            Mantenimiento mantenimiento = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ObtenerMantenimiento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idMantenimiento", id);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        mantenimiento = new Mantenimiento()
                        {
                            idMantenimiento = dr.GetInt32(0),
                            codigoMantenimiento = dr.GetString(1),
                            idVehiculo = dr.GetInt32(2),
                            tipo = dr.GetString(3),
                            descripcion = dr.IsDBNull(4) ? null : dr.GetString(4),
                            fecha = dr.GetDateTime(5),
                            idUsuario = dr.GetInt32(6),
                            placa = dr.GetString(7),
                            marca = dr.GetString(8),
                            modelo = dr.GetString(9),
                            tipoVehiculo = dr.IsDBNull(10) ? null : dr.GetString(10),
                            usuarioNombre = dr.GetString(11)
                        };
                    }
                }
            }
            return mantenimiento;
        }

        // Editar mantenimiento
        public async Task EditarMantenimientoAsync(Mantenimiento mantenimiento)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_EditarMantenimiento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idMantenimiento", mantenimiento.idMantenimiento);
                cmd.Parameters.AddWithValue("@idVehiculo", mantenimiento.idVehiculo);
                cmd.Parameters.AddWithValue("@tipo", mantenimiento.tipo);
                cmd.Parameters.AddWithValue("@descripcion",
                    string.IsNullOrEmpty(mantenimiento.descripcion) ? (object)DBNull.Value : mantenimiento.descripcion);
                cmd.Parameters.AddWithValue("@fecha", mantenimiento.fecha);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Eliminar/Anular mantenimiento
        public async Task EliminarMantenimientoAsync(int idMantenimiento)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_EliminarMantenimiento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idMantenimiento", idMantenimiento);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Habilitar mantenimiento
        public async Task HabilitarMantenimientoAsync(int idMantenimiento)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_HabilitarMantenimiento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idMantenimiento", idMantenimiento);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Obtener tipos de mantenimiento para dropdown
        public List<string> ObtenerTiposMantenimiento()
        {
            return new List<string> { "Preventivo", "Correctivo" };
        }

        // Obtener usuarios para dropdown (solo activos)
        public async Task<List<Usuario>> ObtenerUsuariosAsync()
        {
            List<Usuario> lista = new List<Usuario>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarUsuariosActivos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                await conn.OpenAsync();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new Usuario
                        {
                            idUsuario = dr.GetInt32(0),
                            nombreCompleto = dr.GetString(1),  // Usar nombreCompleto
                            rol = dr.GetString(2)
                        });
                    }
                }
            }
            return lista;
        }
    }
}