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

        // Listar Mantenimientos con paginación y filtros
        [HttpGet]
        public async Task<IActionResult> Index(
            int pagina = 1,
            string filtroCodigo = null,
            int? filtroVehiculo = null,
            string filtroTipo = null,
            string filtroFechaDesde = null,
            string filtroFechaHasta = null,
            string filtroEstado = null)
        {
            int pageSize = 6; // Debe coincidir con el valor en la vista

            // Obtener datos de sesión
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            var rol = HttpContext.Session.GetString("rol");
            var nombreUsuario = HttpContext.Session.GetString("nombre");

            ViewBag.Rol = rol;
            ViewBag.NombreUsuario = nombreUsuario;

            // Si es Trabajador, solo puede ver sus mantenimientos
            int? idUsuarioFiltro = null;
            if (rol == "Trabajador" && idUsuario.HasValue)
            {
                idUsuarioFiltro = idUsuario.Value;
            }

            // Convertir fechas de string a DateTime?
            DateTime? fechaDesde = null;
            DateTime? fechaHasta = null;

            if (!string.IsNullOrEmpty(filtroFechaDesde) && DateTime.TryParse(filtroFechaDesde, out DateTime parsedDesde))
            {
                fechaDesde = parsedDesde;
            }

            if (!string.IsNullOrEmpty(filtroFechaHasta) && DateTime.TryParse(filtroFechaHasta, out DateTime parsedHasta))
            {
                fechaHasta = parsedHasta;
            }

            // Convertir filtroCodigo de string a int? si es posible
            int? codigoFiltroInt = null;
            if (!string.IsNullOrEmpty(filtroCodigo) && int.TryParse(filtroCodigo, out int codigo))
            {
                codigoFiltroInt = codigo;
            }

            // Obtener datos con paginación y filtros
            var (mantenimientos, totalRegistros) = await _repo.ListarMantenimientosPaginadoAsync(
                pagina: pagina,
                tamanoPagina: pageSize,
                filtroCodigo: codigoFiltroInt,
                filtroVehiculo: filtroVehiculo,
                filtroTipo: filtroTipo,
                filtroFechaDesde: fechaDesde,
                filtroFechaHasta: fechaHasta,
                filtroEstado: filtroEstado,
                idUsuarioFiltro: idUsuarioFiltro);

            // Calcular total de páginas
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / pageSize);
            ViewBag.TotalRegistros = totalRegistros;

            // Guardar los filtros en ViewBag para la vista
            ViewBag.FiltroCodigo = filtroCodigo;
            ViewBag.FiltroVehiculo = filtroVehiculo;
            ViewBag.FiltroTipo = filtroTipo;
            ViewBag.FiltroFechaDesde = filtroFechaDesde;
            ViewBag.FiltroFechaHasta = filtroFechaHasta;
            ViewBag.FiltroEstado = filtroEstado;

            // Datos para los dropdowns de filtros
            ViewBag.Vehiculos = await _repo.ObtenerVehiculosAsync();
            ViewBag.Tipos = _repo.ObtenerTiposMantenimiento();

            return View(mantenimientos);
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
                    : RedirectToAction("Index");         // Admin => Index de Mantenimientos
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

            // Construir la returnUrl con los filtros actuales si no se proporciona
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

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

            // Si no hay returnUrl, construir una con los filtros actuales
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            return Redirect(returnUrl);
        }

        // Detalles
        public async Task<IActionResult> Detalles(int id, string returnUrl = null)
        {
            var mantenimiento = await _repo.ObtenerMantenimientoAsync(id);
            if (mantenimiento == null) return NotFound();

            // Construir la returnUrl con los filtros actuales si no se proporciona
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(mantenimiento);
        }

        // Eliminar/Anular - SOLO ADMIN
        [HttpPost]
        [ValidarAdmin]
        public async Task<IActionResult> Eliminar(int id, string returnUrl = null)
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

            // Si no hay returnUrl, construir una con los filtros actuales
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            return Redirect(returnUrl);
        }

        // Habilitar - SOLO ADMIN
        [HttpPost]
        [ValidarAdmin]
        public async Task<IActionResult> Habilitar(int id, string returnUrl = null)
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

            // Si no hay returnUrl, construir una con los filtros actuales
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            return Redirect(returnUrl);
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

        // Método para construir la URL de retorno con los filtros actuales
        private string BuildReturnUrl()
        {
            var queryString = HttpContext.Request.Query;

            // Obtener la página actual, si existe
            int paginaActual = 1;
            if (queryString.ContainsKey("pagina") && int.TryParse(queryString["pagina"], out int pagina))
            {
                paginaActual = pagina;
            }

            // Obtener filtroVehiculo
            int? filtroVehiculo = null;
            if (queryString.ContainsKey("filtroVehiculo") && int.TryParse(queryString["filtroVehiculo"], out int vehiculoId))
            {
                filtroVehiculo = vehiculoId;
            }

            var returnUrl = Url.Action("Index", new
            {
                pagina = paginaActual,
                filtroCodigo = queryString.ContainsKey("filtroCodigo") ? (string)queryString["filtroCodigo"] : null,
                filtroVehiculo = filtroVehiculo,
                filtroTipo = queryString.ContainsKey("filtroTipo") ? (string)queryString["filtroTipo"] : null,
                filtroFechaDesde = queryString.ContainsKey("filtroFechaDesde") ? (string)queryString["filtroFechaDesde"] : null,
                filtroFechaHasta = queryString.ContainsKey("filtroFechaHasta") ? (string)queryString["filtroFechaHasta"] : null,
                filtroEstado = queryString.ContainsKey("filtroEstado") ? (string)queryString["filtroEstado"] : null
            });

            return returnUrl;
        }
    }
}