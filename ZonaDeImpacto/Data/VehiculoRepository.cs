using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
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

        //Listar con filtros
        public async Task<List<Vehiculo>> ListarVehiculosAsync(
            string filtroPlaca = null,
            string filtroMarca = null,
            int? filtroAnio = null,
            string filtroEstado = null,
            bool? soloActivos = true)
        {
            List<Vehiculo> lista = new List<Vehiculo>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarVehiculos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                //Agregamos parametros de filtros
                cmd.Parameters.AddWithValue("@filtroPlaca", string.IsNullOrEmpty(filtroPlaca) ? (object)DBNull.Value : filtroPlaca);
                cmd.Parameters.AddWithValue("@filtroMarca", string.IsNullOrEmpty(filtroMarca) ? (object)DBNull.Value : filtroMarca);
                cmd.Parameters.AddWithValue("@filtroAnio", filtroAnio.HasValue ? (object)filtroAnio.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@filtroEstado", string.IsNullOrEmpty(filtroEstado) ? (object)DBNull.Value : filtroEstado);
                cmd.Parameters.AddWithValue("@soloActivos", soloActivos.HasValue ? (object)soloActivos.Value : DBNull.Value);

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
                            tipo = dr.IsDBNull(4) ? null : dr.GetString(4),
                            anio = dr.IsDBNull(5) ? null : dr.GetInt32(5),
                            kilometraje = dr.IsDBNull(6) ? null : dr.GetInt32(6),
                            estado = dr.IsDBNull(7) ? null : dr.GetString(7),
                            estadoLogico = dr.GetBoolean(8)
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
            using (SqlCommand cmd = new SqlCommand("dbo.usp_InsertarVehiculo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@placa", v.placa);
                cmd.Parameters.AddWithValue("@marca", v.marca);
                cmd.Parameters.AddWithValue("@modelo", v.modelo);
                cmd.Parameters.AddWithValue("@tipo", v.tipo ?? (object)DBNull.Value);
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
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ObtenerVehiculo", conn))
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
                            tipo = dr.IsDBNull(4) ? null : dr.GetString(4),
                            anio = dr.IsDBNull(5) ? null : dr.GetInt32(5),
                            kilometraje = dr.IsDBNull(6) ? null : dr.GetInt32(6),
                            estado = dr.IsDBNull(7) ? null : dr.GetString(7),
                            estadoLogico = dr.GetBoolean(8)
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
            using (SqlCommand cmd = new SqlCommand("dbo.usp_EditarVehiculo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idVehiculo", v.idVehiculo);
                cmd.Parameters.AddWithValue("@placa", v.placa);
                cmd.Parameters.AddWithValue("@marca", v.marca);
                cmd.Parameters.AddWithValue("@modelo", v.modelo);
                cmd.Parameters.AddWithValue("@tipo", v.tipo ?? (object)DBNull.Value);
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
            using (SqlCommand cmd = new SqlCommand("dbo.usp_EliminarVehiculo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idVehiculo", idVehiculo);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Método para obtener los tipos de vehículos disponibles
        public List<string> ObtenerTiposVehiculos()
        {
            return new List<string>
            {
                "Bus",
                "Compacto",
                "Convertible",
                "CrossOver",
                "Cupe",
                "Furgoneta",
                "HatchBack",
                "Limusina",
                "MicroBus",
                "Pick-Up",
                "Roadster",
                "Sedan",
                "SUV",
                "TodoTerreno"
            };
        }

        //Metodo para habilitar
        public async Task HabilitarVehiculoAsync(int idVehiculo)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("dbo.usp_HabilitarVehiculo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idVehiculo", idVehiculo);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        //Obtener marcas para filtro
        public async Task<List<string>> ObtenerMarcasAsync()
        {
            var vehiculos = await ListarVehiculosAsync(soloActivos: null);
            return vehiculos.Select(v => v.marca).Distinct().OrderBy(m=>m).ToList();
        }

        //Obtener estados para filtro
        public async Task<List<string>> ObtenerEstadosAsync()
        {
            var vehiculos = await ListarVehiculosAsync(soloActivos: null);
            return vehiculos.Where(v => !string.IsNullOrEmpty(v.estado)).Select(v => v.estado).Distinct().OrderBy(e =>e).ToList();
        }
    }
}