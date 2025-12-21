using Microsoft.AspNetCore.Mvc;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Filters;
using ZonaDeImpacto.Models;
using System.Runtime.CompilerServices;

namespace ZonaDeImpacto.Controllers
{
    [ValidarSesion]
    [ValidarAdmin]
    public class VehiculoController : Controller
    {
        private readonly VehiculoRepository _repo;
        public VehiculoController(VehiculoRepository repo)
        {
            _repo = repo;
        }

        //listamos con filtros
        [HttpGet]
        public async Task<IActionResult> Index(
            string filtroPlaca = null,
            string filtroMarca = null,
            int? filtroAnio = null,
            string filtroEstado = null,
            string filtroEstadoLogico= "")
        {
            bool? soloActivos = null;
            if (filtroEstadoLogico == "activos")
                soloActivos = true;
            else if (filtroEstadoLogico == "inactivos")
                soloActivos = false;

            //Pasamos los filtros a la vista
            ViewBag.FiltroPlaca = filtroPlaca;
            ViewBag.FiltroMarca = filtroMarca;
            ViewBag.FiltroAnio = filtroAnio;
            ViewBag.FiltroEstado = filtroEstado;
            ViewBag.FiltroEstadoLogico = filtroEstadoLogico;

            // DATOS PARA LOS DROPDOWNS
            ViewBag.Marcas = await _repo.ObtenerMarcasAsync();    // Para filtro de marcas
            ViewBag.Estados = await _repo.ObtenerEstadosAsync();  // Para filtro de estados del vehículo
            ViewBag.Tipos = _repo.ObtenerTiposVehiculos();        // Por si acaso lo necesita otra vista

            //Obtener vehiculos filtrados
            var lista = await _repo.ListarVehiculosAsync(filtroPlaca, filtroMarca, filtroAnio, filtroEstado, soloActivos);

            //Ordenamos para que los activos se muestren primero y luego los inactivos
            lista = lista.OrderByDescending(v => v.estadoLogico).ThenBy(v => v.placa).ToList();
            return View(lista);
        }

        // creamos get
        public IActionResult Crear()
        {
            ViewBag.Tipos = _repo.ObtenerTiposVehiculos();
            return View();
        }

        //creamos post
        [HttpPost]
        public async Task<IActionResult> Crear(Vehiculo veh)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tipos = _repo.ObtenerTiposVehiculos();
                return View(veh);
            }
            await _repo.RegistrarVehiculoAsync(veh);
            TempData["Mensaje"] = "Vehículo registrado correctamente.";
            return RedirectToAction("Index");
        }

        // editamos get
        public async Task<IActionResult> Editar(int id, string filtroEstadoLogico = "activos")
        {
            var vehiculo = await _repo.ObtenerVehiculoAsync(id);
            if (vehiculo == null) return NotFound();

            // Pasar el filtro actual a la vista
            ViewBag.FiltroEstadoLogico = filtroEstadoLogico;
            ViewBag.Tipos = _repo.ObtenerTiposVehiculos();

            return View(vehiculo);
        }

        // editamos post
        [HttpPost]
        public async Task<IActionResult> Editar(Vehiculo veh, string filtroEstadoLogico = "activos")
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tipos = _repo.ObtenerTiposVehiculos();
                ViewBag.FiltroEstadoLogico = filtroEstadoLogico;
                return View(veh);
            }

            await _repo.EditarVehiculoAsync(veh);
            TempData["Mensaje"] = "Vehículo actualizado correctamente.";

            //  Redirigir manteniendo el filtro
            return RedirectToAction("Index", new
            {
                filtroEstadoLogico = filtroEstadoLogico
            });
        }

        // detalles get
        public async Task<IActionResult> Detalles(int id, string filtroEstadoLogico = "activos")
        {
            var vehiculo = await _repo.ObtenerVehiculoAsync(id);
            if (vehiculo == null) return NotFound();

            // Pasar el filtro actual a la vista
            ViewBag.FiltroEstadoLogico = filtroEstadoLogico;

            return View(vehiculo);
        }

        // eliminamos o desabilitados
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id, string filtroEstadoLogico = null)
        {
            await _repo.EliminarVehiculoAsync(id);
            TempData["Mensaje"] = "Vehículo Eliminado/Desabilitado correctamente.";

            return RedirectToAction("Index", new
            {
                filtroEstadoLogico = filtroEstadoLogico
            });
        }

        //Volver a habilitar vehiculo(reactivar)
        [HttpGet]
        public async Task<IActionResult> Habilitar(int id, string filtroEstadoLogico = null)
        {
            await _repo.HabilitarVehiculoAsync(id);
            TempData["Mensaje"] = "Vehiculo habilitado correctamente.";

                return RedirectToAction("Index", new
                {
                    filtroEstadoLogico = filtroEstadoLogico
                });
        }
    }
}