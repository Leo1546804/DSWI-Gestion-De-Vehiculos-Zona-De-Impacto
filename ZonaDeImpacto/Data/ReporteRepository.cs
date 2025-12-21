using System.Data;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Math;
using ZonaDeImpacto.Models;
using ZonaDeImpacto.Models.Reportes;

namespace ZonaDeImpacto.Data
{
    public class ReporteRepository
    {
        private readonly string _connectionString;

        public ReporteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexion");
        }

        // Reporte 1: Costos por Vehículo
        public async Task<List<ReporteCostosVehiculo>> ObtenerCostosPorVehiculoAsync(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
        {
            var lista = new List<ReporteCostosVehiculo>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ReporteCostosPorVehiculo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@fechaInicio",
                    fechaInicio.HasValue ? (object)fechaInicio.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaFin",
                    fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new ReporteCostosVehiculo
                        {
                            idVehiculo = dr.GetInt32(0),
                            placa = dr.GetString(1),
                            marca = dr.GetString(2),
                            modelo = dr.GetString(3),
                            tipoVehiculo = dr.IsDBNull(4) ? null : dr.GetString(4),
                            totalMantenimientos = dr.GetInt32(5),
                            totalGastado = dr.IsDBNull(6) ? 0 : dr.GetDecimal(6),
                            gastoPreventivo = dr.IsDBNull(7) ? 0 : dr.GetDecimal(7),
                            gastoCorrectivo = dr.IsDBNull(8) ? 0 : dr.GetDecimal(8)
                        });
                    }
                }
            }
            return lista;
        }

        // Reporte 2: Historial Completo de Vehículo
        public async Task<ReporteHistorialVehiculo> ObtenerHistorialVehiculoAsync(int idVehiculo)
        {
            var reporte = new ReporteHistorialVehiculo();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ReporteHistorialVehiculo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idVehiculo", idVehiculo);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    // Primera tabla: Información del vehículo
                    if (await dr.ReadAsync())
                    {
                        reporte.placa = dr.GetString(0);
                        reporte.marca = dr.GetString(1);
                        reporte.modelo = dr.GetString(2);
                        reporte.tipo = dr.IsDBNull(3) ? null : dr.GetString(3);
                        reporte.anio = dr.IsDBNull(4) ? null : dr.GetInt32(4);
                        reporte.kilometraje = dr.IsDBNull(5) ? null : dr.GetInt32(5);
                        reporte.estado = dr.IsDBNull(6) ? null : dr.GetString(6);
                    }

                    // Segunda tabla: Mantenimientos
                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            var mantenimiento = new MantenimientoDetalle
                            {
                                idMantenimiento = dr.GetInt32(0),
                                codigoMantenimiento = dr.GetString(1),
                                tipoMantenimiento = dr.GetString(2),
                                descripcionMantenimiento = dr.IsDBNull(3) ? null : dr.GetString(3),
                                fechaMantenimiento = dr.GetDateTime(4),
                                responsable = dr.GetString(5),
                                totalMantenimiento = dr.IsDBNull(6) ? 0 : dr.GetDecimal(6),
                                detalleGastos = dr.IsDBNull(7) ? "Sin gastos" : dr.GetString(7)
                            };
                            reporte.Mantenimientos.Add(mantenimiento);
                        }
                    }
                }
            }
            return reporte;
        }

        // Reporte 3: Dashboard
        public async Task<ReporteDashboard> ObtenerDashboardAsync(int? mes = null, int? anio = null)
        {
            var dashboard = new ReporteDashboard
            {
                mes = mes ?? DateTime.Now.Month,
                anio = anio ?? DateTime.Now.Year
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ReporteDashboard", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@mes", mes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@anio", anio ?? (object)DBNull.Value);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    // 1. Total gastado este mes
                    if (await dr.ReadAsync())
                    {
                        dashboard.totalGastadoMes = dr.IsDBNull(0) ? 0 : dr.GetDecimal(0);
                    }

                    // 2. Total por tipo de mantenimiento
                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            dashboard.tiposMantenimiento.Add(new TipoMantenimientoResumen
                            {
                                tipoMantenimiento = dr.GetString(0),
                                cantidad = dr.GetInt32(1),
                                total = dr.GetDecimal(2)
                            });
                        }
                    }

                    // 3. Top 3 vehículos
                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            dashboard.topVehiculos.Add(new TopVehiculo
                            {
                                placa = dr.GetString(0),
                                marca = dr.GetString(1),
                                modelo = dr.GetString(2),
                                totalGastado = dr.GetDecimal(3)
                            });
                        }
                    }

                    // 4. Vehículos por estado
                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            dashboard.estadosVehiculos.Add(new EstadoVehiculo
                            {
                                estado = dr.GetString(0),
                                cantidad = dr.GetInt32(1)
                            });
                        }
                    }
                }
            }
            return dashboard;
        }

        // Reporte 4: Actividad por Usuario
        public async Task<ReporteActividadUsuario> ObtenerActividadUsuarioAsync(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? idUsuario = null)
        {
            var reporte = new ReporteActividadUsuario
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                UsuarioSeleccionadoId = idUsuario
            };

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ReporteActividadUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@fechaInicio",
                    fechaInicio.HasValue ? (object)fechaInicio.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaFin",
                    fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@idUsuario",
                    idUsuario.HasValue ? (object)idUsuario.Value : DBNull.Value);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    // Primera tabla: Lista de usuarios con estadísticas
                    while (await dr.ReadAsync())
                    {
                        var usuario = new UsuarioActividad
                        {
                            idUsuario = dr.GetInt32(0),
                            nombreCompleto = dr.GetString(1),
                            usuario = dr.GetString(2),
                            rol = dr.GetString(3),
                            vehiculosAtendidos = dr.GetInt32(4),
                            totalMantenimientos = dr.GetInt32(5),
                            totalGastado = dr.IsDBNull(6) ? 0 : dr.GetDecimal(6),
                            mantenimientosPreventivos = dr.GetInt32(7),
                            mantenimientosCorrectivos = dr.GetInt32(8),
                            gastoPreventivo = dr.IsDBNull(9) ? 0 : dr.GetDecimal(9),
                            gastoCorrectivo = dr.IsDBNull(10) ? 0 : dr.GetDecimal(10)
                        };

                        reporte.Usuarios.Add(usuario);

                        // Guardar nombre del usuario seleccionado
                        if (idUsuario.HasValue && usuario.idUsuario == idUsuario.Value)
                        {
                            reporte.UsuarioSeleccionadoNombre = usuario.nombreCompleto;
                        }
                    }

                    // Segunda tabla: Detalle de mantenimientos (solo si se filtró por usuario)
                    if (idUsuario.HasValue && await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            var detalle = new MantenimientoUsuarioDetalle
                            {
                                placa = dr.GetString(0),
                                marca = dr.GetString(1),
                                modelo = dr.GetString(2),
                                tipoMantenimiento = dr.GetString(3),
                                fecha = dr.GetDateTime(4),
                                descripcion = dr.IsDBNull(5) ? null : dr.GetString(5),
                                totalMantenimiento = dr.IsDBNull(6) ? 0 : dr.GetDecimal(6),
                                detalleGastos = dr.IsDBNull(7) ? "Sin gastos" : dr.GetString(7)
                            };
                            reporte.DetalleMantenimientos.Add(detalle);
                        }
                    }
                }
            }

            return reporte;
        }

        // Obtener usuarios para dropdown (todos los que pueden hacer mantenimientos)
        public async Task<List<Usuario>> ObtenerUsuariosParaReporteAsync()
        {
            var lista = new List<Usuario>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var query = @"
            SELECT idUsuario, nombreCompleto, usuario, rol 
            FROM Usuarios 
            WHERE estado = 1 
                AND (rol = 'Trabajador' OR rol = 'Admin')  -- INCLUYE ADMINS
            ORDER BY rol DESC, nombreCompleto"; 
        

        using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Usuario
                            {
                                idUsuario = dr.GetInt32(0),
                                nombreCompleto = dr.GetString(1),
                                usuario = dr.GetString(2),
                                rol = dr.GetString(3)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        // Método para obtener vehículos activos (para dropdowns)
        public async Task<List<Vehiculo>> ObtenerVehiculosActivosAsync()
        {
            var lista = new List<Vehiculo>();

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
    }
}