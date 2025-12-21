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

        // Listar con filtros y paginación (método que devuelve tupla como en Mantenimiento)
        public async Task<(List<TipoGasto>, int)> ListarTiposGastoPaginadoAsync(
            string filtroNombre = null,
            string filtroEstado = "",
            int pagina = 1,
            int tamanoPagina = 6)
        {
            List<TipoGasto> lista = new List<TipoGasto>();
            int totalRegistros = 0;

            // Convertir filtroEstado a bool?
            bool? soloActivos = null;
            if (filtroEstado == "activos")
                soloActivos = true;
            else if (filtroEstado == "inactivos")
                soloActivos = false;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_ListarTiposGastoPaginado", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@filtroNombre",
                    string.IsNullOrEmpty(filtroNombre) ? (object)DBNull.Value : filtroNombre);
                cmd.Parameters.AddWithValue("@soloActivos",
                    soloActivos.HasValue ? (object)soloActivos.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@pagina", pagina);
                cmd.Parameters.AddWithValue("@tamanoPagina", tamanoPagina);

                await conn.OpenAsync();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                {
                    // Primero leemos los datos
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new TipoGasto
                        {
                            idTipoGasto = dr.GetInt32(0),
                            nombre = dr.GetString(1),
                            descripcion = dr.IsDBNull(2) ? null : dr.GetString(2),
                            estadoLogico = dr.GetBoolean(3)
                        });
                    }

                    // Luego leemos el total de registros
                    if (await dr.NextResultAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            totalRegistros = dr.GetInt32(0);
                        }
                    }
                }
            }

            return (lista, totalRegistros);
        }

        // Registrar nuevo tipo de gasto
        public async Task RegistrarTipoGastoAsync(TipoGasto tipoGasto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_InsertarTipoGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@nombre", tipoGasto.nombre);
                cmd.Parameters.AddWithValue("@descripcion",
                    string.IsNullOrEmpty(tipoGasto.descripcion) ? (object)DBNull.Value : tipoGasto.descripcion);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Obtener Tipo de Gasto por ID
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
                            descripcion = dr.IsDBNull(2) ? null : dr.GetString(2),
                            estadoLogico = dr.GetBoolean(3)
                        };
                    }
                }
            }
            return tipoGasto;
        }

        // Editar el tipo de Gasto
        public async Task EditarTipoGastoAsync(TipoGasto tipoGasto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_EditarTipoGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@idTipoGasto", tipoGasto.idTipoGasto);
                cmd.Parameters.AddWithValue("@nombre", tipoGasto.nombre);
                cmd.Parameters.AddWithValue("@descripcion",
                    string.IsNullOrEmpty(tipoGasto.descripcion) ? (object)DBNull.Value : tipoGasto.descripcion);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Eliminar o deshabilitar Tipo de Gasto
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

        // Habilitar el tipo de gasto
        public async Task HabilitarTipoGastoAsync(int idTipoGasto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.usp_HabilitarTipoGasto", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idTipoGasto", idTipoGasto);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}