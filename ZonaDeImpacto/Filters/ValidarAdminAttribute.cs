using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ZonaDeImpacto.Filters
{
    public class ValidarAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var rol = context.HttpContext.Session.GetString("rol");

            //si es que el usuario ingresado no es un admin bloquear acceso
            if(rol != "Admin" )
            {
                context.Result = new RedirectToActionResult("AccesoDenegado", "Login", null);
            }

            base.OnActionExecuting(context); 
        }

    }
}
