using System.Data;
using Microsoft.Data.SqlClient;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Data
{
    public class VehiculoRepository
    {
        private readonly string _connectionString;
        public VehiculoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexion");
        }
        //Listar
        public async Task<List<Vehiculo>> ListarVehiculosAsync()
        {
            List<Vehiculo> lista = new List<Vehiculo>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ListarVehiculos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    while ( await dr.ReadAsync())
                    {
                        lista.Add(new Vehiculo
                        {
                            idVehiculo = dr.GetInt32(0),
                            placa = dr.GetString(1),
                            marca = dr.GetString(2),
                            modelo = dr.GetString(3),
                            anio = dr.IsDBNull(4) ? null : dr.GetInt32(4),
                            kilometraje = dr.IsDBNull(5) ? null : dr.GetInt32(5),
                            estado = dr.IsDBNull(6) ? null : dr.GetString(6)
                        });
                    }
                    
                        
                    }
                }
            return lista;
            
        }

        //Registar
        public async Task RegistrarVehiculoAsync(Vehiculo v)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_listarVehiculo", conn))
            {
                cmd.CommandType= CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@placa", v.placa);
                cmd.Parameters.AddWithValue("@marca", v.marca);
                cmd.Parameters.AddWithValue("@modelo", v.modelo);
                cmd.Parameters.AddWithValue("@anio", v.anio ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@kilometraje", v.kilometraje ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@estado", v.estado ?? (object)DBNull.Value);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
        // obtener por ID
        public async Task<Vehiculo> ObtenerVehiculoAsync(int id)
        {
            Vehiculo vehiculo = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_ObtenerVehiculo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idVehiculo", id);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        vehiculo = new Vehiculo
                        {
                            idVehiculo = dr.GetInt32(0),
                            placa = dr.GetString(1),
                            marca = dr.GetString(2),
                            modelo = dr.GetString(3),
                            anio = dr.IsDBNull(4) ? null : dr.GetInt32(4),
                            kilometraje = dr.IsDBNull(5) ? null : dr.GetInt32(5),
                            estado = dr.IsDBNull(6) ? null : dr.GetString(6)
                        };
                    }
                }
            }
            return vehiculo;
        }

        // editamos
        public async Task EditarVehiculoAsync(Vehiculo v)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_EditarVehiculo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idVehiculo", v.idVehiculo);
                cmd.Parameters.AddWithValue("@placa", v.placa);
                cmd.Parameters.AddWithValue("@marca", v.marca);
                cmd.Parameters.AddWithValue("@modelo", v.modelo);
                cmd.Parameters.AddWithValue("@anio", v.anio ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@kilometraje", v.kilometraje ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@estado", v.estado ?? (object)DBNull.Value);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // eliminamos
        public async Task EliminarVehiculoAsync(int idVehiculo)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_EliminarVehiculo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idVehiculo", idVehiculo);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }


        //traremos la lista para el reporte
        public async Task<List<Vehiculo>> ListarVehiculosReporteAsync()
        {
            List<Vehiculo> lista = new List<Vehiculo>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Vehiculos", conn))
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
                            anio = dr.IsDBNull(4) ? null : dr.GetInt32(4),
                            kilometraje = dr.IsDBNull(5) ? null : dr.GetInt32(5),
                            estado = dr.IsDBNull(6) ? null : dr.GetString(6)
                        });
                    }
                }
            }
            return lista;
        }

    }
}