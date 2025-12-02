using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Filters;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Controllers
{
    [ValidarAdmin]
    [ValidarSesion]
    public class UsuarioController : Controller
    {
        private readonly UsuarioRepository _repo;
        public UsuarioController(UsuarioRepository _repo)
        {
            this._repo = _repo;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Usuario> lista = await _repo.ListarUsuariosAsync();
            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Usuario usuarioModelo)
        {
            if (!ModelState.IsValid)
            {
                return View(usuarioModelo);
            }

            await _repo.RegistrarUsuarioAsync(usuarioModelo);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            Usuario usuario = await _repo.ObtenerUsuarioAsync(id);
            if(usuario == null) {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Usuario usuarioModelo)
        {
            ModelState.Remove("password");

            if (!ModelState.IsValid)
            {
                return View(usuarioModelo);
            }

            Usuario usuarioOriginal = await _repo.ObtenerUsuarioAsync(usuarioModelo.idUsuario);


            if (string.IsNullOrWhiteSpace(usuarioModelo.password))
            {
                usuarioModelo.password = usuarioOriginal.password;
            }

            await _repo.EditarUsuarioAsync(usuarioModelo);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repo.EliminarUsuarioAsync(id);
            return RedirectToAction("Index");
        }

    }
}
