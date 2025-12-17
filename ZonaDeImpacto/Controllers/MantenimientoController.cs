using Microsoft.AspNetCore.Mvc;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Models;
using ZonaDeImpacto.Filters;

namespace ZonaDeImpacto.Controllers
{
    [ValidarSesion]
    [ValidarAdmin]
    public class MantenimientoController : Controller
    {
        private readonly MantenimientoRepository _repo;

        public MantenimientoController(MantenimientoRepository repo)
        {
            _repo = repo;
        }

        // Listar Mantenimientos
        public async Task<IActionResult> Index()
        {
            var lista = await _repo.ListarMantenimientosAsync();
            return View(lista);
        }

        // Crear - GET
        public async Task<IActionResult> Crear()
        {
            var vehiculos = await _repo.ObtenerVehiculosAsync();
            ViewBag.Vehiculos = vehiculos;

            // Obtener idUsuario de la sesión 
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            ViewBag.IdUsuario = idUsuario;

            return View();
        }

        // Crear - POST
        [HttpPost]
        public async Task<IActionResult> Crear(Mantenimiento mantenimiento)
        {
            if (!ModelState.IsValid)
            {
                var vehiculos = await _repo.ObtenerVehiculosAsync();
                ViewBag.Vehiculos = vehiculos;
                return View(mantenimiento);
            }

            await _repo.RegistrarMantenimientoAsync(mantenimiento);
            TempData["Mensaje"] = "Mantenimiento registrado correctamente.";
            return RedirectToAction("Index");
        }

        // Editar - GET
        public async Task<IActionResult> Editar(int id)
        {
            var mantenimiento = await _repo.ObtenerMantenimientoAsync(id);
            if (mantenimiento == null) return NotFound();

            var vehiculos = await _repo.ObtenerVehiculosAsync();
            ViewBag.Vehiculos = vehiculos;

            return View(mantenimiento);
        }

        // Editar - POST
        [HttpPost]
        public async Task<IActionResult> Editar(Mantenimiento mantenimiento)
        {
            if (!ModelState.IsValid)
            {
                var vehiculos = await _repo.ObtenerVehiculosAsync();
                ViewBag.Vehiculos = vehiculos;
                return View(mantenimiento);
            }

            await _repo.EditarMantenimientoAsync(mantenimiento);
            TempData["Mensaje"] = "Mantenimiento actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // Detalles 
        public async Task<IActionResult> Detalles(int id)
        {
            var mantenimiento = await _repo.ObtenerMantenimientoAsync(id);
            if (mantenimiento == null) return NotFound();

            return View(mantenimiento);
        }

        //Eliminar
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repo.EliminarMnatenimientoAsync(id);
            TempData["Mensaje"] = "Mantenimiento eliminado correctamente.";
            return RedirectToAction("Index");
        }
    }
}