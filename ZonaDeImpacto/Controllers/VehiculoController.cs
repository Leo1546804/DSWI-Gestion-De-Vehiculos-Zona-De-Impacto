using Microsoft.AspNetCore.Mvc;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Controllers
{
    public class VehiculoController : Controller
    {
        private readonly VehiculoRepository _repo;
        public VehiculoController(VehiculoRepository repo)
        {
            _repo = repo;
        }
        //listamos
        public async Task<IActionResult> Index()
        {
            var lista = await _repo.ListarVehiculosAsync();
            return View(lista);
        }
        // craemos get
        public IActionResult Crear()
        {
            return View();
        }

        //creamos post
        [HttpPost]
        public async Task<IActionResult> Crear(Vehiculo veh)
        {
            if (!ModelState.IsValid)
            {
                return View(veh);
            }
            await _repo.RegistrarVehiculoAsync(veh);
            TempData["Mensaje"] = "Vehículo registrado correctamente.";
            return RedirectToAction("Index");
        }

        // editamos get
        public async Task<IActionResult> Editar(int id)
        {
            var vehiculo = await _repo.ObtenerVehiculoAsync(id);
            if (vehiculo == null) return NotFound();

            return View(vehiculo);
        }

        // editamos post
        [HttpPost]
        public async Task<IActionResult> Editar(Vehiculo veh)
        {
            if (!ModelState.IsValid)
            {
                return View(veh);
            }

            await _repo.EditarVehiculoAsync(veh);
            TempData["Mensaje"] = "Vehículo actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // eliminamos
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repo.EliminarVehiculoAsync(id);
            TempData["Mensaje"] = "Vehículo eliminado correctamente.";
            return RedirectToAction("Index");
        }


    }
}