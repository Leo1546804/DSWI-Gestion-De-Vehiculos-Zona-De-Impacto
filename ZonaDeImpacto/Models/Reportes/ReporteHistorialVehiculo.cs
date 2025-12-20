namespace ZonaDeImpacto.Models.Reportes
{
    public class ReporteHistorialVehiculo
    {
        // Información del vehículo
        public string placa { get; set; }
        public string marca { get; set; }
        public string modelo { get; set; }
        public string tipo { get; set; }
        public int? anio { get; set; }
        public int? kilometraje { get; set; }
        public string estado { get; set; }

        // Lista de mantenimientos
        public List<MantenimientoDetalle> Mantenimientos { get; set; } = new();
    }

    public class MantenimientoDetalle
    {
        public int idMantenimiento { get; set; }
        public string codigoMantenimiento { get; set; }
        public string tipoMantenimiento { get; set; }
        public string descripcionMantenimiento { get; set; }
        public DateTime fechaMantenimiento { get; set; }
        public string responsable { get; set; }
        public decimal totalMantenimiento { get; set; }
        public string detalleGastos { get; set; }
    }
}