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

        // Listar tipos de gasto con filtros
        [HttpGet]
        public async Task<IActionResult> Index(
            string filtroNombre = null,
            string filtroEstado ="")
        {
            //Pasamos los filtros a la vista
            ViewBag.FiltroNombre = filtroNombre;
            ViewBag.FiltroEstado = filtroEstado;

            //obtenemos la lista filtrada
            var lista = await _repo.ListarTiposGastoAsync(filtroNombre, filtroEstado);
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
        public async Task<IActionResult> Editar(int id, string filtroEstado ="")
        {
            var tipoGasto = await _repo.ObtenerTipoGastoAsync(id);
            if (tipoGasto == null) return NotFound();

            //Pasamos el filtro actual
            ViewBag.FiltroEstado = filtroEstado;
            return View(tipoGasto);
        }

        // Editar - POST
        [HttpPost]
        public async Task<IActionResult> Editar(TipoGasto tipoGasto, string filtroEstado ="")
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FiltroEstado = filtroEstado;
                return View(tipoGasto);
            }

            await _repo.EditarTipoGastoAsync(tipoGasto);
            TempData["Mensaje"] = "El tipo de gasto a sido actualizado correctamente.";

            // Redirigimos manteniendo el filtro
            return RedirectToAction("Index", new {filtroEstado = filtroEstado});
        }

        // Eliminar o desactivamos
        public async Task<IActionResult> Eliminar(int id, string filtroEstado = null)
        {
            await _repo.EliminarTipoGastoAsync(id);
            TempData["Mensaje"] = "El tipo de gasto a sido eliminado/desactivado correctamente";
            return RedirectToAction("Index", new {filtroEstado = filtroEstado});
        }

        //Habilitamos o reactivamos
        [HttpGet]
        public async Task<IActionResult> Habilitar(int id, string filtroEstado = null)
        {
            await _repo.HabilitarTipoGastoAsync(id);
            TempData["Mensaje"] = "El tipo de gasto a sido habilitado correctamente.";

            return RedirectToAction("Index", new {filtroEstado=filtroEstado});
        }
        //Porsiacaso detalles
        public async Task<IActionResult> Detalles(int id, string filtroEstado="")
        {
            var tipoGasto = await _repo.ObtenerTipoGastoAsync(id);
            if (tipoGasto == null) return NotFound();

            ViewBag.FiltroEstado = filtroEstado;
            return View(tipoGasto);
        }
    }
}
