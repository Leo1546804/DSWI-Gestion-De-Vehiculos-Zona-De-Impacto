using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Filters;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Controllers
{
    [ValidarSesion]
    public class MantenimientoController : Controller
    {
        private readonly MantenimientoRepository _repo;

        public MantenimientoController(MantenimientoRepository repo)
        {
            _repo = repo;
        }

        // Listar Mantenimientos con filtros
        [HttpGet]
        public async Task<IActionResult> Index(
            int? filtroCodigo = null,
            int? filtroVehiculo = null,
            string filtroTipo = null,
            DateTime? filtroFechaDesde = null,
            DateTime? filtroFechaHasta = null,
            string filtroEstado = null)
        {
            // Obtener datos de sesión
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            var rol = HttpContext.Session.GetString("rol");
            var nombreUsuario = HttpContext.Session.GetString("nombre");

            // Si es Trabajador, solo puede ver sus mantenimientos
            int? idUsuarioFiltro = null;
            if (rol == "Trabajador" && idUsuario.HasValue)
            {
                idUsuarioFiltro = idUsuario.Value;
            }

            // Pasar filtros a la vista
            ViewBag.FiltroCodigo = filtroCodigo;
            ViewBag.FiltroVehiculo = filtroVehiculo;
            ViewBag.FiltroTipo = filtroTipo;
            ViewBag.FiltroFechaDesde = filtroFechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroFechaHasta = filtroFechaHasta?.ToString("yyyy-MM-dd");
            ViewBag.FiltroEstado = filtroEstado;
            ViewBag.Rol = rol;
            ViewBag.NombreUsuario = nombreUsuario;

            // Datos para los dropdowns
            ViewBag.Vehiculos = await _repo.ObtenerVehiculosAsync();
            ViewBag.Tipos = _repo.ObtenerTiposMantenimiento();

            // Obtener lista filtrada
            var lista = await _repo.ListarMantenimientosAsync(
                filtroCodigo, filtroVehiculo, filtroTipo,
                filtroFechaDesde, filtroFechaHasta, filtroEstado, idUsuarioFiltro);

            return View(lista);
        }

        // Crear - GET
        public async Task<IActionResult> Crear()
        {
            // Obtener rol e ID de usuario
            var rol = HttpContext.Session.GetString("rol");
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            var nombreUsuario = HttpContext.Session.GetString("nombre");

            ViewBag.Vehiculos = await _repo.ObtenerVehiculosAsync();
            ViewBag.Tipos = _repo.ObtenerTiposMantenimiento();
            ViewBag.Rol = rol;

            // Pasar datos del usuario para la vista
            if (rol == "Trabajador")
            {
                ViewBag.IdUsuario = idUsuario;
                ViewBag.NombreUsuario = nombreUsuario;
            }
            else
            {
                ViewBag.Usuarios = await _repo.ObtenerUsuariosAsync();
            }

            return View();
        }

        // Crear - POST
        [HttpPost]
        public async Task<IActionResult> Crear(Mantenimiento mantenimiento)
        {
            var rol = HttpContext.Session.GetString("rol");

            if (!ModelState.IsValid)
            {
                // Recargar datos 
                ViewBag.Vehiculos = await _repo.ObtenerVehiculosAsync();
                ViewBag.Usuarios = await _repo.ObtenerUsuariosAsync();
                ViewBag.Tipos = _repo.ObtenerTiposMantenimiento();
                ViewBag.Rol = rol;
                TempData["Error"] = "Por favor corrija los errores del formulario.";
                return View(mantenimiento);
            }

            try
            {
                await _repo.RegistrarMantenimientoAsync(mantenimiento);
                TempData["Mensaje"] = "Mantenimiento registrado correctamente.";

                // Redirigir según rol
                return rol == "Trabajador"
                    ? RedirectToAction("Index", "Home")  // Trabajador => Home
                    : RedirectToAction("Index");         // Admin =z Index de Mantenimientos
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al guardar: {ex.Message}";
                ViewBag.Vehiculos = await _repo.ObtenerVehiculosAsync();
                ViewBag.Usuarios = await _repo.ObtenerUsuariosAsync();
                ViewBag.Tipos = _repo.ObtenerTiposMantenimiento();
                ViewBag.Rol = rol;
                return View(mantenimiento);
            }
        }

        // Editar - GET
        public async Task<IActionResult> Editar(int id, string returnUrl = null)
        {
            var mantenimiento = await _repo.ObtenerMantenimientoAsync(id);
            if (mantenimiento == null) return NotFound();

            await CargarDatosParaVista();
            ViewBag.ReturnUrl = returnUrl;

            return View(mantenimiento);
        }

        // Editar - POST
        [HttpPost]
        public async Task<IActionResult> Editar(Mantenimiento mantenimiento, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosParaVista();
                return View(mantenimiento);
            }

            await _repo.EditarMantenimientoAsync(mantenimiento);
            TempData["Mensaje"] = "Mantenimiento actualizado correctamente.";

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        // Detalles
        public async Task<IActionResult> Detalles(int id)
        {
            var mantenimiento = await _repo.ObtenerMantenimientoAsync(id);
            if (mantenimiento == null) return NotFound();

            return View(mantenimiento);
        }

        // Eliminar/Anular - SOLO ADMIN
        [HttpPost]
        [ValidarAdmin]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _repo.EliminarMantenimientoAsync(id);
                TempData["Mensaje"] = "Mantenimiento anulado correctamente.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Habilitar - SOLO ADMIN
        [HttpPost]
        [ValidarAdmin]
        public async Task<IActionResult> Habilitar(int id)
        {
            try
            {
                await _repo.HabilitarMantenimientoAsync(id);
                TempData["Mensaje"] = "Mantenimiento habilitado correctamente.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Método auxiliar para cargar datos comunes
        private async Task CargarDatosParaVista()
        {
            var vehiculos = await _repo.ObtenerVehiculosAsync();
            var usuarios = await _repo.ObtenerUsuariosAsync();

            ViewBag.Vehiculos = vehiculos;
            ViewBag.Usuarios = usuarios;
            ViewBag.Tipos = _repo.ObtenerTiposMantenimiento();

            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            var rol = HttpContext.Session.GetString("rol");
            var nombreUsuario = HttpContext.Session.GetString("nombre");

            ViewBag.Rol = rol;
            ViewBag.IdUsuario = idUsuario;
            ViewBag.NombreUsuario = nombreUsuario;
        }
    }
}