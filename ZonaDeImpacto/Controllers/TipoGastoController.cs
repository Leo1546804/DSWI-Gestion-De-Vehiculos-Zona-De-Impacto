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

        // Listar tipos de gasto con filtros y paginación simple
        [HttpGet]
        public async Task<IActionResult> Index(
            string filtroNombre = null,
            string filtroEstado = "",
            int pagina = 1)
        {
            int pageSize = 6; // Cantidad de registros por página

            // Pasamos los filtros a la vista
            ViewBag.FiltroNombre = filtroNombre;
            ViewBag.FiltroEstado = filtroEstado;

            // Obtener datos con paginación
            var (tiposGasto, totalRegistros) = await _repo.ListarTiposGastoPaginadoAsync(
                filtroNombre: filtroNombre,
                filtroEstado: filtroEstado,
                pagina: pagina,
                tamanoPagina: pageSize);

            // Calcular total de páginas
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / pageSize);
            ViewBag.TotalRegistros = totalRegistros;

            return View(tiposGasto);
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
            TempData["Mensaje"] = "El tipo de gasto ha sido registrado correctamente.";
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
            TempData["Mensaje"] = "El tipo de gasto ha sido actualizado correctamente.";

            return RedirectToAction("Index");
        }

        // Eliminar o desactivamos
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repo.EliminarTipoGastoAsync(id);
            TempData["Mensaje"] = "El tipo de gasto ha sido eliminado/desactivado correctamente";
            return RedirectToAction("Index");
        }

        // Habilitamos o reactivamos
        [HttpGet]
        public async Task<IActionResult> Habilitar(int id)
        {
            await _repo.HabilitarTipoGastoAsync(id);
            TempData["Mensaje"] = "El tipo de gasto ha sido habilitado correctamente.";

            return RedirectToAction("Index");
        }

        // Detalles
        public async Task<IActionResult> Detalles(int id)
        {
            var tipoGasto = await _repo.ObtenerTipoGastoAsync(id);
            if (tipoGasto == null) return NotFound();

            return View(tipoGasto);
        }
    }
}