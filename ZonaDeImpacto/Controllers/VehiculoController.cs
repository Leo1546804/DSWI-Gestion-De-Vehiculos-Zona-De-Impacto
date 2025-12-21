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

        //listamos con filtros Y PAGINACIÓN
        [HttpGet]
        public async Task<IActionResult> Index(
            int pagina = 1,
            string filtroPlaca = null,
            string filtroMarca = null,
            int? filtroAnio = null,
            string filtroEstado = null,
            string filtroEstadoLogico = "")
        {
            bool? soloActivos = null;
            if (filtroEstadoLogico == "activos")
                soloActivos = true;
            else if (filtroEstadoLogico == "inactivos")
                soloActivos = false;

            int pageSize = 6; // Cantidad de vehículos por página

            // Obtener datos con paginación
            var (vehiculos, totalRegistros) = await _repo.ListarVehiculosPaginadoAsync(
                pagina: pagina,
                tamanoPagina: pageSize,
                filtroPlaca: filtroPlaca,
                filtroMarca: filtroMarca,
                filtroAnio: filtroAnio,
                filtroEstado: filtroEstado,
                soloActivos: soloActivos);

            // Calcular total de páginas
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / pageSize);
            ViewBag.TotalRegistros = totalRegistros;

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

            //Ordenamos para que los activos se muestren primero y luego los inactivos
            vehiculos = vehiculos.OrderByDescending(v => v.estadoLogico).ThenBy(v => v.placa).ToList();
            return View(vehiculos);
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
        public async Task<IActionResult> Editar(int id, string returnUrl = null)
        {
            var vehiculo = await _repo.ObtenerVehiculoAsync(id);
            if (vehiculo == null) return NotFound();

            // Construir la returnUrl con los filtros actuales si no se proporciona
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Tipos = _repo.ObtenerTiposVehiculos();

            return View(vehiculo);
        }

        // editamos post
        [HttpPost]
        public async Task<IActionResult> Editar(Vehiculo veh, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Tipos = _repo.ObtenerTiposVehiculos();
                ViewBag.ReturnUrl = returnUrl;
                return View(veh);
            }

            await _repo.EditarVehiculoAsync(veh);
            TempData["Mensaje"] = "Vehículo actualizado correctamente.";

            // Si no hay returnUrl, construir una con los filtros actuales
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            return Redirect(returnUrl);
        }

        // detalles get
        public async Task<IActionResult> Detalles(int id, string returnUrl = null)
        {
            var vehiculo = await _repo.ObtenerVehiculoAsync(id);
            if (vehiculo == null) return NotFound();

            // Construir la returnUrl con los filtros actuales si no se proporciona
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(vehiculo);
        }

        // eliminamos o desabilitados
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id, string returnUrl = null)
        {
            await _repo.EliminarVehiculoAsync(id);
            TempData["Mensaje"] = "Vehículo Eliminado/Desabilitado correctamente.";

            // Si no hay returnUrl, construir una con los filtros actuales
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            return Redirect(returnUrl);
        }

        //Volver a habilitar vehiculo(reactivar)
        [HttpGet]
        public async Task<IActionResult> Habilitar(int id, string returnUrl = null)
        {
            await _repo.HabilitarVehiculoAsync(id);
            TempData["Mensaje"] = "Vehiculo habilitado correctamente.";

            // Si no hay returnUrl, construir una con los filtros actuales
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            return Redirect(returnUrl);
        }

        // Método para construir la URL de retorno con los filtros actuales
        private string BuildReturnUrl()
        {
            var query = HttpContext.Request.Query;
            var queryParams = new List<string>();

            if (query.ContainsKey("pagina"))
            {
                queryParams.Add($"pagina={query["pagina"]}");
            }

            if (query.ContainsKey("filtroPlaca"))
            {
                queryParams.Add($"filtroPlaca={query["filtroPlaca"]}");
            }

            if (query.ContainsKey("filtroMarca"))
            {
                queryParams.Add($"filtroMarca={query["filtroMarca"]}");
            }

            if (query.ContainsKey("filtroAnio"))
            {
                queryParams.Add($"filtroAnio={query["filtroAnio"]}");
            }

            if (query.ContainsKey("filtroEstado"))
            {
                queryParams.Add($"filtroEstado={query["filtroEstado"]}");
            }

            if (query.ContainsKey("filtroEstadoLogico"))
            {
                queryParams.Add($"filtroEstadoLogico={query["filtroEstadoLogico"]}");
            }

            var queryString = queryParams.Any() ? $"?{string.Join("&", queryParams)}" : "";
            var returnUrl = Url.Action("Index") + queryString;

            return returnUrl;
        }
    }
}