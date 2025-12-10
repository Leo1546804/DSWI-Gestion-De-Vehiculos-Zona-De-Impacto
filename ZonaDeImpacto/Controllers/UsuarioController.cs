using Microsoft.AspNetCore.Mvc;
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
            TempData["Success"] = "Usuario creado correctamente";
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

            // Obtener el usuario original de la base de datos
            var usuarioOriginal = await _repo.ObtenerUsuarioAsync(usuarioModelo.idUsuario);

            if (usuarioOriginal == null)
            {
                return NotFound();
            }

            // Manejar la contraseña:
            // Si el campo password está vacío o nulo, mantener la contraseña original
            // Si tiene valor, usar la nueva
            if (string.IsNullOrWhiteSpace(usuarioModelo.password))
            {
                // Mantener la contraseña actual
                usuarioModelo.password = usuarioOriginal.password;
            }
            // Si el campo tiene valor, se usa esa 
            // Actualizar el usuario
            await _repo.EditarUsuarioAsync(usuarioModelo);
            TempData["Success"] = "Usuario actualizado correctamente";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repo.EliminarUsuarioAsync(id);
            TempData["Success"] = "Usuario eliminado permanentemente";
            return RedirectToAction("Index");
        }

        /*   Metodo para activar/desactivar   */
        [HttpGet]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuario = await _repo.ObtenerUsuarioAsync(id);
            if (usuario != null)
            {
                // Cambiar estado (true->false, false->true)
                usuario.estado = !usuario.estado;
                await _repo.EditarUsuarioAsync(usuario);

                TempData["Success"] = usuario.estado ?
                    "Usuario activado correctamente" :
                    "Usuario desactivado correctamente";
            }
            return RedirectToAction("Index");
        }
    }
}