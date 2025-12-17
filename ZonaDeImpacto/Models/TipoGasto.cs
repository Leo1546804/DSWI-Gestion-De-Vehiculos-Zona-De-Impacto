using System.ComponentModel.DataAnnotations;

namespace ZonaDeImpacto.Models
{
    public class TipoGasto
    {
        public int idTipoGasto { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        [Display(Name = "Nombre")]
        public string nombre { get; set; }

        [StringLength(200, ErrorMessage = "La descripcion no puede exceder de 200 caracteres")]
        [Display(Name = "Descripción")]
        public string descripcion { get; set; }

    }
}
