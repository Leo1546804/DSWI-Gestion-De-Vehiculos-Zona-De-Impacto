using System.ComponentModel.DataAnnotations;

namespace ZonaDeImpacto.Models
{
    public class Vehiculo
    {
        public int idVehiculo { get; set; }

        [Required(ErrorMessage = "La placa es Obligatoria")]
        [StringLength(15)]
        public string placa { get; set; }
        [Required(ErrorMessage = "La marca del Vehiculo es Obligatorio")]
        [StringLength(50)]
        public string marca { get; set; }
        [Required(ErrorMessage = "El modelo es Obligatorio")]
        [StringLength(50)]
        public string modelo { get; set; }
        [Required(ErrorMessage = "El año es Obligatorio")]
        [Range(1900, 2100, ErrorMessage = "Ingrese un año valido entre 1900 y 2100")]
        public int? anio { get; set; }
        public int? kilometraje { get; set; }
        [StringLength(20)]
        public string estado { get; set; }

    }
}