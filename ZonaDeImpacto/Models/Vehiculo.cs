using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ZonaDeImpacto.Models
{
    public class Vehiculo
    {
        public int idVehiculo { get; set; }

        [Required(ErrorMessage="La placa es Obligatoria")]
        [StringLength(15)]
        [Display(Name ="Placa")]
        public string placa { get; set; }
        [Required(ErrorMessage = "La marca del Vehiculo es Obligatorio")]
        [StringLength(50)]
        [Display(Name ="Marca")]
        public string marca { get; set; }
        [Required(ErrorMessage = "El modelo es Obligatorio")]
        [StringLength(50)]
        [Display(Name ="Modelo")]
        public string modelo { get; set; }

        [Required(ErrorMessage = "El tipo es Obligatorio")]
        [StringLength(50)]
        [Display(Name ="Tipo")]
        public string tipo { get; set; }

        [Required(ErrorMessage = "El año es Obligatorio")]
        [Range(1900, 2100, ErrorMessage = "Ingrese un año valido entre 1900 y 2100")]
        [Display(Name = "Año")]
        public int? anio { get; set; }

        [Display(Name ="Kilometraje")]
        public int? kilometraje { get; set; }
        [StringLength(20)]
        [Display(Name ="Estado")]
        public string estado { get; set; }

    }
}
