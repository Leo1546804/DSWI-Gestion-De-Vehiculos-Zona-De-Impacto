using System.ComponentModel.DataAnnotations;

namespace ZonaDeImpacto.Models
{
    public class Mantenimientos
    {
        public int IdMantenimiento { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un vehículo")]
        public int IdVehiculo { get; set; }

        // Para traer la placa por JOIN
        public string? PlacaVehiculo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo")]
        public string Tipo { get; set; }

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debe ingresar el costo")]
        public decimal Costo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar la fecha")]
        public DateTime Fecha { get; set; }

        public string? Evidencia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un usuario")]
        public int IdUsuario { get; set; }

        // Para mostrar el nombre del usuario
        public string? NombreUsuario { get; set; }
    }
}
