using System.ComponentModel.DataAnnotations;

namespace ZonaDeImpacto.Models
{
    public class Gasto
    {
        public int idGasto { get; set; }

        [Required(ErrorMessage = "El mantenimiento es obligatorio")]
        [Display(Name = "Mantenimiento")]
        public int idMantenimiento { get; set; }

        [Required(ErrorMessage = "El tipo de gasto es obligatorio")]
        [Display(Name = "Tipo de Gasto")]
        public int idTipoGasto { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 9999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto")]
        public decimal monto { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Display(Name = "Fecha")]
        [DataType(DataType.Date)]
        public DateTime fecha { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        [Display(Name = "Descripción")]
        public string? descripcion { get; set; }

        // PROPIEDADES DE RELACIONES (solo lectura)
        [Display(Name = "Código Mantenimiento")]
        public string? codigoMantenimiento { get; set; }

        [Display(Name = "Tipo de Gasto")]
        public string? tipoGastoNombre { get; set; }

        [Display(Name = "Responsable")]
        public string? usuarioNombre { get; set; }

        public int? idUsuario { get; set; }

        [Display(Name = "Mantenimiento Activo")]
        public bool mantenimientoActivo { get; set; } = true;
    }
}