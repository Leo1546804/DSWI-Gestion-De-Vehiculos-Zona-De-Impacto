using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace ZonaDeImpacto.Models
{
    public class Usuario
    {
        public int idUsuario { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre Completo")]
        public string nombreCompleto { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50)]
        [Display(Name = "Usuario")]
        public string usuario { get; set; }

        [Display(Name = "Contraseña")]
        public string password { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [StringLength(20)]
        [Display(Name = "Rol")]
        public string rol { get; set; }

        [Display(Name = "Estado")]
        public bool estado { get; set; } = true;
    }
}
