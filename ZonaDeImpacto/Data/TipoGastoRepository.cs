using System.Data;
using Microsoft.Data.SqlClient;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Data
{
    public class TipoGastoRepository
    {
        private readonly string _connectionString;

        public TipoGastoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("conexion");
        }

        //Listar todos los tipos de gasto
        public async Task<List<TipoGasto>> ListarTiposGastoAsync()
        {
            List<TipoGasto> lista = new List<TipoGasto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarTiposGasto", conn))
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
                            nombre = dr.GetString(1),
                            descripcion = dr.IsDBNull(2) ? null : dr.GetString(2)
                        });
                    }
                }
            }
            return lista;
        }

        //Registrar nuevo tipo de gasto
        public async Task RegistrarTipoGastoAsync(TipoGasto tipoGasto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_InsertarTipoGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@nombre", tipoGasto.nombre);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(tipoGasto.descripcion) ? (object)DBNull.Value : tipoGasto.descripcion);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        //Optener Tipos de Gasto pro ID
        public async Task<TipoGasto> ObtenerTipoGastoAsync(int id)
        {
            TipoGasto tipoGasto = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ObtenerTipoGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idTipoGasto", id);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    if (await dr.ReadAsync())
                    {
                        tipoGasto = new TipoGasto
                        {
                            idTipoGasto = dr.GetInt32(0),
                            nombre = dr.GetString(1),
                            descripcion = dr.IsDBNull(2) ? null : dr.GetString(2)
                        };
                    }
                }

            }
            return tipoGasto;
        }

        //Editar el tipo de Gasto
        public async Task EditarTipoGastoAsync(TipoGasto tipoGasto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_EditarTipoGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idTipoGasto", tipoGasto.idTipoGasto);
                cmd.Parameters.AddWithValue("@nombre", tipoGasto.nombre);
                cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrEmpty(tipoGasto.descripcion) ? (object)DBNull.Value : tipoGasto.descripcion);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        //Eliminar Tipo de Gasto
        public async Task EliminarTipoGastoAsync(int idTipoGasto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_EliminarTipoGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idTipoGasto", idTipoGasto);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
