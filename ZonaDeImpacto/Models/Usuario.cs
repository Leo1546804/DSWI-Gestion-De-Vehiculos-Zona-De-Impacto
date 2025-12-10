using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace ZonaDeImpacto.Models
{
    public class Usuario
    {
        public int idUsuario { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [Display(Name = "Nombre Completo")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string nombreCompleto { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Display(Name = "Nombre de Usuario")]
        [StringLength(50, ErrorMessage = "El usuario no puede exceder los 50 caracteres")]
        public string usuario { get; set; }

        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        [StringLength(255, ErrorMessage = "La contraseña no puede exceder los 50 caracteres")]
        public string password { get; set; }
        [Required(ErrorMessage = "El rol es obligatorio")]
        [Display(Name = "Rol del Usuario")]
        public string rol { get; set; }

        [Display(Name = "Estado")]
        public bool estado { get; set; }
    }
}