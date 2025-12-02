using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ZonaDeImpacto.Filters
{
    public class ValidarSesionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // si es que no existe una sesion, redirigir al login
            if (context.HttpContext.Session.GetString("usuario") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Login", null);
            }

            base.OnActionExecuted(context);
        }
    }
}
