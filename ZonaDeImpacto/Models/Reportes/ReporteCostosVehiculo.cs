namespace ZonaDeImpacto.Models.Reportes
{
    public class ReporteCostosVehiculo
    {
        public int idVehiculo { get; set; }
        public string placa { get; set; }
        public string marca { get; set; }
        public string modelo { get; set; }
        public string tipoVehiculo { get; set; }
        public int totalMantenimientos { get; set; }
        public decimal totalGastado { get; set; }
        public decimal gastoPreventivo { get; set; }
        public decimal gastoCorrectivo { get; set; }
    }
}