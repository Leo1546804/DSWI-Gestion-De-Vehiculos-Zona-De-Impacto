using System.Data;
using Microsoft.Data.SqlClient;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Data
{
    public class GastoRepository
    {
        private readonly string _connectionString;

        public GastoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexion");
        }

        // Listar gastos con filtros Y PAGINACIÓN
        public async Task<(List<Gasto> Gastos, int TotalRegistros)> ListarGastosPaginadoAsync(
            int pagina = 1,
            int tamanoPagina = 6,
            string filtroMantenimientoCodigo = null,
            int? filtroTipoGasto = null,
            int? filtroUsuario = null,  // Cambiado de string a int?
            DateTime? filtroFechaDesde = null,
            DateTime? filtroFechaHasta = null,
            int? idUsuarioFiltro = null)
        {
            List<Gasto> lista = new List<Gasto>();
            int totalRegistros = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarGastosPaginado", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Parámetros de paginación
                cmd.Parameters.AddWithValue("@Pagina", pagina);
                cmd.Parameters.AddWithValue("@TamanoPagina", tamanoPagina);

                // Parámetros de filtro
                cmd.Parameters.AddWithValue("@filtroMantenimientoCodigo",
                    string.IsNullOrEmpty(filtroMantenimientoCodigo) ? (object)DBNull.Value : filtroMantenimientoCodigo);

                cmd.Parameters.AddWithValue("@filtroTipoGasto",
                    filtroTipoGasto.HasValue ? (object)filtroTipoGasto.Value : DBNull.Value);

                // CORRECCIÓN: Ahora filtroUsuario es int?
                cmd.Parameters.AddWithValue("@filtroUsuario",
                    filtroUsuario.HasValue ? (object)filtroUsuario.Value : DBNull.Value);

                cmd.Parameters.AddWithValue("@filtroFechaDesde",
                    filtroFechaDesde.HasValue ? (object)filtroFechaDesde.Value : DBNull.Value);

                cmd.Parameters.AddWithValue("@filtroFechaHasta",
                    filtroFechaHasta.HasValue ? (object)filtroFechaHasta.Value : DBNull.Value);

                cmd.Parameters.AddWithValue("@idUsuarioFiltro",
                    idUsuarioFiltro.HasValue ? (object)idUsuarioFiltro.Value : DBNull.Value);

                // Parámetro de salida para total de registros
                SqlParameter totalRegistrosParam = new SqlParameter("@TotalRegistros", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(totalRegistrosParam);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new Gasto
                        {
                            idGasto = dr.GetInt32(0),
                            codigoMantenimiento = dr.GetString(1),
                            tipoGastoNombre = dr.GetString(2),
                            usuarioNombre = dr.GetString(3),
                            idUsuario = dr.GetInt32(4),
                            monto = dr.GetDecimal(5),
                            fecha = dr.GetDateTime(6),
                            descripcion = dr.IsDBNull(7) ? null : dr.GetString(7),
                            idMantenimiento = dr.GetInt32(8),
                            idTipoGasto = dr.GetInt32(9),
                            mantenimientoActivo = dr.GetBoolean(10)
                        });
                    }
                }

                // Obtener el valor del parámetro de salida
                totalRegistros = totalRegistrosParam.Value != DBNull.Value ? (int)totalRegistrosParam.Value : 0;
            }

            return (lista, totalRegistros);
        }

        // Listar gastos con filtros (sin paginación - para compatibilidad)
        public async Task<List<Gasto>> ListarGastosAsync(
            string filtroMantenimientoCodigo = null,
            int? filtroTipoGasto = null,
            int? filtroUsuario = null,  // Cambiado de string a int?
            DateTime? filtroFechaDesde = null,
            DateTime? filtroFechaHasta = null,
            int? idUsuarioFiltro = null)
        {
            // Usar paginación con tamaño máximo para simular "todos"
            var (gastos, _) = await ListarGastosPaginadoAsync(
                pagina: 1,
                tamanoPagina: int.MaxValue,
                filtroMantenimientoCodigo: filtroMantenimientoCodigo,
                filtroTipoGasto: filtroTipoGasto,
                filtroUsuario: filtroUsuario,  // Ahora es int?
                filtroFechaDesde: filtroFechaDesde,
                filtroFechaHasta: filtroFechaHasta,
                idUsuarioFiltro: idUsuarioFiltro);

            return gastos;
        }

        // Obtener mantenimientos activos para dropdown
        public async Task<List<Mantenimiento>> ObtenerMantenimientosAsync()
        {
            List<Mantenimiento> lista = new List<Mantenimiento>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarMantenimientosActivos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

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
                            usuarioNombre = dr.GetString(3)
                        });
                    }
                }
            }
            return lista;
        }

        // Obtener tipos de gasto activos para dropdown
        public async Task<List<TipoGasto>> ObtenerTiposGastoAsync()
        {
            List<TipoGasto> lista = new List<TipoGasto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarTiposGastoActivos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                await conn.OpenAsync();
                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new TipoGasto
                        {
                            idTipoGasto = dr.GetInt32(0),
                            nombre = dr.GetString(1)
                        });
                    }
                }
            }
            return lista;
        }

        // Obtener usuarios para filtro
        public async Task<List<Usuario>> ObtenerUsuariosAsync()
        {
            List<Usuario> lista = new List<Usuario>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarUsuariosParaFiltro", conn))
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
                            nombreCompleto = dr.GetString(1)
                        });
                    }
                }
            }
            return lista;
        }

        // Registrar nuevo gasto
        public async Task RegistrarGastoAsync(Gasto gasto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_InsertarGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idMantenimiento", gasto.idMantenimiento);
                cmd.Parameters.AddWithValue("@idTipoGasto", gasto.idTipoGasto);
                cmd.Parameters.AddWithValue("@monto", gasto.monto);
                cmd.Parameters.AddWithValue("@fecha", gasto.fecha);
                cmd.Parameters.AddWithValue("@descripcion",
                    string.IsNullOrEmpty(gasto.descripcion) ? (object)DBNull.Value : gasto.descripcion);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Obtener gasto por ID
        public async Task<Gasto> ObtenerGastoAsync(int id)
        {
            Gasto gasto = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ObtenerGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idGasto", id);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        gasto = new Gasto
                        {
                            idGasto = dr.GetInt32(0),
                            idMantenimiento = dr.GetInt32(1),
                            idTipoGasto = dr.GetInt32(2),
                            monto = dr.GetDecimal(3),
                            fecha = dr.GetDateTime(4),
                            descripcion = dr.IsDBNull(5) ? null : dr.GetString(5),
                            codigoMantenimiento = dr.GetString(6),
                            tipoGastoNombre = dr.GetString(7),
                            usuarioNombre = dr.GetString(8),
                            idUsuario = dr.GetInt32(9),
                            mantenimientoActivo = dr.GetBoolean(10)
                        };
                    }
                }
            }
            return gasto;
        }

        // Editar gasto
        public async Task EditarGastoAsync(Gasto gasto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_EditarGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idGasto", gasto.idGasto);
                cmd.Parameters.AddWithValue("@idTipoGasto", gasto.idTipoGasto);
                cmd.Parameters.AddWithValue("@monto", gasto.monto);
                cmd.Parameters.AddWithValue("@fecha", gasto.fecha);
                cmd.Parameters.AddWithValue("@descripcion",
                    string.IsNullOrEmpty(gasto.descripcion) ? (object)DBNull.Value : gasto.descripcion);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Eliminar gasto
        public async Task EliminarGastoAsync(int idGasto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_EliminarGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idGasto", idGasto);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // para el drop de trabajador
        public async Task<List<Mantenimiento>> ObtenerMantenimientosPorTrabajadorAsync(int idUsuario)
        {
            List<Mantenimiento> lista = new List<Mantenimiento>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var query = @"
            SELECT 
                m.idMantenimiento,
                m.codigoMantenimiento,
                v.placa,
                u.nombreCompleto AS responsable
            FROM Mantenimientos m
            INNER JOIN Vehiculos v ON m.idVehiculo = v.idVehiculo
            INNER JOIN Usuarios u ON m.idUsuario = u.idUsuario
            WHERE m.estadoLogico = 1
            AND m.idUsuario = @idUsuario
            ORDER BY m.codigoMantenimiento DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
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
                                usuarioNombre = dr.GetString(3)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // Método adicional: Obtener total de gastos para verificar funcionamiento
        public async Task<int> ObtenerTotalGastosAsync(
            string filtroMantenimientoCodigo = null,
            int? filtroTipoGasto = null,
            int? filtroUsuario = null,
            DateTime? filtroFechaDesde = null,
            DateTime? filtroFechaHasta = null,
            int? idUsuarioFiltro = null)
        {
            int total = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ObtenerTotalGastos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Parámetros de filtro
                cmd.Parameters.AddWithValue("@filtroMantenimientoCodigo",
                    string.IsNullOrEmpty(filtroMantenimientoCodigo) ? (object)DBNull.Value : filtroMantenimientoCodigo);

                cmd.Parameters.AddWithValue("@filtroTipoGasto",
                    filtroTipoGasto.HasValue ? (object)filtroTipoGasto.Value : DBNull.Value);

                cmd.Parameters.AddWithValue("@filtroUsuario",
                    filtroUsuario.HasValue ? (object)filtroUsuario.Value : DBNull.Value);

                cmd.Parameters.AddWithValue("@filtroFechaDesde",
                    filtroFechaDesde.HasValue ? (object)filtroFechaDesde.Value : DBNull.Value);

                cmd.Parameters.AddWithValue("@filtroFechaHasta",
                    filtroFechaHasta.HasValue ? (object)filtroFechaHasta.Value : DBNull.Value);

                cmd.Parameters.AddWithValue("@idUsuarioFiltro",
                    idUsuarioFiltro.HasValue ? (object)idUsuarioFiltro.Value : DBNull.Value);

                SqlParameter totalParam = new SqlParameter("@Total", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(totalParam);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                total = totalParam.Value != DBNull.Value ? (int)totalParam.Value : 0;
            }

            return total;
        }
    }
}