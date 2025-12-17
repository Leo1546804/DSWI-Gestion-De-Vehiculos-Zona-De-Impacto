using Microsoft.AspNetCore.Mvc;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Models;
using ZonaDeImpacto.Filters;

namespace ZonaDeImpacto.Controllers
{
    [ValidarSesion]
    [ValidarAdmin]
    public class TipoGastoController : Controller
    {

        private readonly TipoGastoRepository _repo;

        public TipoGastoController(TipoGastoRepository repo)
        {
            _repo = repo;
        }

        // Listar tipos de gasto
        public async Task<IActionResult> Index()
        {
            var lista = await _repo.ListarTiposGastoAsync();
            return View(lista);
        }

        // Crear - GET
        public IActionResult Crear()
        {
            return View();
        }

        // Crear - POST
        [HttpPost]
        public async Task<IActionResult> Crear(TipoGasto tipoGasto)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoGasto);
            }
            await _repo.RegistrarTipoGastoAsync(tipoGasto);
            TempData["Mensaje"] = "El tipo de gasto a sido registrado correctamente.";
            return RedirectToAction("Index");
        }

        // Editar - GET
        public async Task<IActionResult> Editar(int id)
        {
            var tipoGasto = await _repo.ObtenerTipoGastoAsync(id);
            if (tipoGasto == null) return NotFound();

            return View(tipoGasto);
        }

        // Editar - POST
        [HttpPost]
        public async Task<IActionResult> Editar(TipoGasto tipoGasto)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoGasto);
            }

            await _repo.EditarTipoGastoAsync(tipoGasto);
            TempData["Mensaje"] = "El tipo de gasto a sido actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // Detalles???

        // Eliminar
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repo.EliminarTipoGastoAsync(id);
            TempData["Mensaje"] = "El tipo de gasto a sido eliminado correctamente";
            return RedirectToAction("Index");
        }
    }
}
