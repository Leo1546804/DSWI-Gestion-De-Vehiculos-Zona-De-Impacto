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

        // Listar todos los mantenimientos
        public async Task<List<Mantenimiento>> ListarMantenimientosAsync()
        {
            List<Mantenimiento> lista = new List<Mantenimiento>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ListarMantenimientos", conn))
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
                            placa = dr.GetString(1),
                            marca = dr.GetString(2),
                            modelo = dr.GetString(3),
                            tipo = dr.GetString(4),
                            descripcion = dr.IsDBNull(5) ? null : dr.GetString(5),
                            costo = dr.GetDecimal(6),
                            fecha = dr.GetDateTime(7),
                            usuarioNombre = dr.GetString(8)
                        });
                    }
                }
            }
            return lista;
        }

        // Obtener vehiculos para DROPDOWN
        public async Task<List<Vehiculo>> ObtenerVehiculosAsync()
        {
            List<Vehiculo> lista = new List<Vehiculo>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT idVehiculo, placa, marca, modelo, tipo FROM Vehiculos ORDER BY placa", conn))
            {
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
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_InsertarMantenimiento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idVehiculo", mantenimiento.idVehiculo);
                cmd.Parameters.AddWithValue("@tipo", mantenimiento.tipo);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(mantenimiento.descripcion) ? (object)DBNull.Value : mantenimiento.descripcion);
                cmd.Parameters.AddWithValue("@costo", mantenimiento.costo);
                cmd.Parameters.AddWithValue("@fecha", mantenimiento.fecha);
                cmd.Parameters.AddWithValue("@idUsuario", mantenimiento.idUsuario);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Obtener mantenimiento por ID con vehiculo
        public async Task<Mantenimiento> ObtenerMantenimientoAsync(int id)
        {
            Mantenimiento mantenimiento = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ObtenerMantenimiento", conn))
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
                            idVehiculo = dr.GetInt32(1),
                            tipo = dr.GetString(2), // tipoMantenimiento
                            descripcion = dr.IsDBNull(3) ? null : dr.GetString(3),
                            costo = dr.GetDecimal(4),
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
            using (SqlCommand cmd = new SqlCommand("sp_EditarMantenimiento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idMantenimiento", mantenimiento.idMantenimiento);
                cmd.Parameters.AddWithValue("@idVehiculo", mantenimiento.idVehiculo);
                cmd.Parameters.AddWithValue("@tipo", mantenimiento.tipo);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(mantenimiento.descripcion) ? (object)DBNull.Value : mantenimiento.descripcion);
                cmd.Parameters.AddWithValue("@costo", mantenimiento.costo);
                cmd.Parameters.AddWithValue("@fecha", mantenimiento.fecha);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Eliminar el mantenimiento
        public async Task EliminarMnatenimientoAsync(int idMantenimiento)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_EliminarMantenimiento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idMantenimiento", idMantenimiento);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
