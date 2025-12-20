namespace ZonaDeImpacto.Models.Reportes
{
    public class ReporteDashboard
    {
        public decimal totalGastadoMes { get; set; }
        public List<TipoMantenimientoResumen> tiposMantenimiento { get; set; } = new();
        public List<TopVehiculo> topVehiculos { get; set; } = new();
        public List<EstadoVehiculo> estadosVehiculos { get; set; } = new();
        public int mes { get; set; }
        public int anio { get; set; }
    }

    public class TipoMantenimientoResumen
    {
        public string tipoMantenimiento { get; set; }
        public int cantidad { get; set; }
        public decimal total { get; set; }
    }

    public class TopVehiculo
    {
        public string placa { get; set; }
        public string marca { get; set; }
        public string modelo { get; set; }
        public decimal totalGastado { get; set; }
    }

    public class EstadoVehiculo
    {
        public string estado { get; set; }
        public int cantidad { get; set; }
    }
}