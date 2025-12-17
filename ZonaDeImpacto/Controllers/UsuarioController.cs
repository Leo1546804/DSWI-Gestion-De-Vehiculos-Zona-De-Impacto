using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Filters;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Controllers
{
    [ValidarSesion]
    [ValidarAdmin]
    public class UsuarioController : Controller
    {
        private readonly UsuarioRepository _repo;
        public UsuarioController(UsuarioRepository _repo)
        {
            this._repo = _repo;
        }


        [HttpGet]
        public async Task<IActionResult> Index(
            string filtroNombre = null,
            string filtroRol = null,
            string filtroEstado = null)
        {
            // Convertimos filtroEstado a int?
            int? estadoFiltro = null;
            if (filtroEstado == "activos") estadoFiltro = 1;
            else if (filtroEstado == "inactivos") estadoFiltro = 0;

            //Pasamos los filtros a la vista
            ViewBag.FiltroNombre = filtroNombre;
            ViewBag.FiltroRol = filtroRol;
            ViewBag.FiltroEstado = filtroEstado;

            //Obtenemos lista de roles unicos para el dropdown
            var todosUsuarios = await _repo.ListarUsuariosAsync();
            ViewBag.Roles = todosUsuarios.Select(u => u.rol).Distinct().ToList();

            // Obtenemos usuarios filtrados
                List<Usuario> lista = await _repo.ListarUsuariosAsync(
                    filtroNombre, filtroRol, estadoFiltro);
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
            if (usuario == null)
            {
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

        // Desabilitamos al usuario
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id, string filtroEstado = null)
        {
            await _repo.EliminarUsuarioAsync(id);
            //Redirigimos manteniendo filtros
            return RedirectToAction("Index", new
            {
                filtroEstado = filtroEstado
            });
        }

        // Habilitamos al usuario
        [HttpGet]
        public async Task<IActionResult> Habilitar(int id, string filtroEstado = null)
        {
            await _repo.HabilitarUsuarioAsync(id);
            //Redirigir manteniendo los filtros
            return RedirectToAction("Index", new
            {
                filtroEstado = filtroEstado
            });
        }

        // Detalles
        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            Usuario usuario = await _repo.ObtenerUsuarioAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

    }
}
