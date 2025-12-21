using System.ComponentModel.DataAnnotations;

namespace ZonaDeImpacto.Models
{
    public class Mantenimiento
    {
        public int idMantenimiento { get; set; }

        public string? codigoMantenimiento { get; set; }

        [Required(ErrorMessage = "El Vehiculo es obligatorio")]
        [Display(Name = "Vehículo")]
        public int idVehiculo { get; set; }

        [Required(ErrorMessage = "El tipo de mantenimiento es obligatorio")]
        [Display(Name = "Tipo de Mantenimiento")]
        public string tipo { get; set; }

        [StringLength(200, ErrorMessage = " La descripcion no puede exceder los 200 caracteres")]
        [Display(Name = "Descripción")]
        public string descripcion { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime fecha { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public int? idUsuario { get; set; }

        //PROPÍEDADES DE LA INFORMACION RELACIONADA
        [Display(Name = "Placa")]
        public string? placa { get; set; }

        [Display(Name = "Marca")]
        public string? marca { get; set; }

        [Display(Name = "Modelo")]
        public string? modelo { get; set; }

        [Display(Name = "Tipo de Vehiculo")]
        public string? tipoVehiculo { get; set; }

        [Display(Name = "Usuario")]
        public string? usuarioNombre { get; set; }

        public bool estadoLogico { get; set; } = true;

    }
}
