using Microsoft.AspNetCore.Mvc;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Controllers
{
    public class LoginController : Controller
    {
        private readonly LoginRepository _repo;

        public LoginController(LoginRepository _repo)
        {
            this._repo = _repo;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string usuario, string password)
        {
            Usuario user = await _repo.LoginAsync(usuario, password);

            if(user == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            HttpContext.Session.SetString("usuario", user.usuario);
            HttpContext.Session.SetString("nombre", user.nombreCompleto);
            HttpContext.Session.SetString("rol",user.rol);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
