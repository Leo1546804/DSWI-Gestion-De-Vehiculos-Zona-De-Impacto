namespace ZonaDeImpacto.Models.Reportes
{
    public class ReporteActividadUsuario
    {
        public List<UsuarioActividad> Usuarios { get; set; } = new();
        public List<MantenimientoUsuarioDetalle> DetalleMantenimientos { get; set; } = new();
        public int? UsuarioSeleccionadoId { get; set; }
        public string UsuarioSeleccionadoNombre { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class UsuarioActividad
    {
        public int idUsuario { get; set; }
        public string nombreCompleto { get; set; }
        public string usuario { get; set; }
        public string rol { get; set; }
        public int vehiculosAtendidos { get; set; }
        public int totalMantenimientos { get; set; }
        public decimal totalGastado { get; set; }
        public int mantenimientosPreventivos { get; set; }
        public int mantenimientosCorrectivos { get; set; }
        public decimal gastoPreventivo { get; set; }
        public decimal gastoCorrectivo { get; set; }

        // Propiedades calculadas
        public decimal GastoPromedio => totalMantenimientos > 0
            ? totalGastado / totalMantenimientos
            : 0;

        public string Eficiencia
        {
            get
            {
                if (totalMantenimientos == 0) return "Sin actividad";
                var ratio = (decimal)mantenimientosPreventivos / totalMantenimientos * 100;
                return ratio >= 70 ? "Alta ⭐⭐⭐" :
                       ratio >= 40 ? "Media ⭐⭐" : "Baja ⭐";
            }
        }
        // Propiedad para mostrar badge según rol
        public string RolBadgeClass
        {
            get
            {
                return rol == "Admin" ? "bg-danger" : "bg-primary";
            }
        }

        public string RolIcon
        {
            get
            {
                return rol == "Admin" ? "fas fa-user-shield" : "fas fa-user-cog";
            }
        }
    }

    public class MantenimientoUsuarioDetalle
    {
        public string placa { get; set; }
        public string marca { get; set; }
        public string modelo { get; set; }
        public string tipoMantenimiento { get; set; }
        public DateTime fecha { get; set; }
        public string descripcion { get; set; }
        public decimal totalMantenimiento { get; set; }
        public string detalleGastos { get; set; }
    }
}