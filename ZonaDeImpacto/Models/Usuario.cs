using System.Security.Principal;

namespace ZonaDeImpacto.Models
{
    public class Usuario
    {

        public int idUsuario { get; set; }  
        public string nombreCompleto { get; set; }
        public string usuario { get; set; }
        public string password { get; set; }
        public string rol { get; set; }
        public bool estado { get; set; }


        
    }
}
