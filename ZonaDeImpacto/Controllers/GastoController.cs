using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Filters;
using ZonaDeImpacto.Models;

namespace ZonaDeImpacto.Controllers
{
    [ValidarSesion]
    public class GastoController : Controller
    {
        private readonly GastoRepository _repo;
        private const int TAMANO_PAGINA = 6; // 6 registros por página

        public GastoController(GastoRepository repo)
        {
            _repo = repo;
        }

        // Listar Gastos con filtros Y PAGINACIÓN
        [HttpGet]
        public async Task<IActionResult> Index(
            int pagina = 1,
            string filtroMantenimientoCodigo = null,
            int? filtroTipoGasto = null,
            string filtroUsuario = null,
            DateTime? filtroFechaDesde = null,
            DateTime? filtroFechaHasta = null)
        {
            // Obtener datos de sesión
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            var rol = HttpContext.Session.GetString("rol");

            // Si es Trabajador, solo puede ver sus gastos
            int? idUsuarioFiltro = null;
            if (rol == "Trabajador" && idUsuario.HasValue)
            {
                idUsuarioFiltro = idUsuario.Value;
            }

            // Obtener datos con paginación
            var (gastos, totalRegistros) = await _repo.ListarGastosPaginadoAsync(
                pagina: pagina,
                tamanoPagina: TAMANO_PAGINA,
                filtroMantenimientoCodigo: filtroMantenimientoCodigo,
                filtroTipoGasto: filtroTipoGasto,
                filtroUsuario: filtroUsuario,
                filtroFechaDesde: filtroFechaDesde,
                filtroFechaHasta: filtroFechaHasta,
                idUsuarioFiltro: idUsuarioFiltro);

            // Calcular total de páginas
            int totalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / TAMANO_PAGINA) : 1;

            // Pasar datos de paginación a la vista
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            // Pasar filtros a la vista
            ViewBag.FiltroMantenimientoCodigo = filtroMantenimientoCodigo;
            ViewBag.FiltroTipoGasto = filtroTipoGasto;
            ViewBag.FiltroUsuario = filtroUsuario;
            ViewBag.FiltroFechaDesde = filtroFechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroFechaHasta = filtroFechaHasta?.ToString("yyyy-MM-dd");
            ViewBag.Rol = rol;

            // Datos para los dropdowns de filtros
            ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
            ViewBag.Usuarios = await _repo.ObtenerUsuariosAsync();

            return View(gastos);
        }

        
        // Crear - GET
        public async Task<IActionResult> Crear()
        {
            var rol = HttpContext.Session.GetString("rol");
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");

            // IF para rol
            if (rol == "Trabajador" && idUsuario.HasValue)
            {
                // Usar el nuevo método para trabajador
                ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosPorTrabajadorAsync(idUsuario.Value);
            }
            else
            {
                // Admin sigue usando el método normal
                ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
            }

            ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
            ViewBag.Rol = rol;
            return View();
        }

        // Crear - POST 
        [HttpPost]
        public async Task<IActionResult> Crear(Gasto gasto)
        {
            var rol = HttpContext.Session.GetString("rol");

            if (!ModelState.IsValid)
            {
                ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
                ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
                ViewBag.Rol = rol;
                return View(gasto);
            }

            try
            {
                await _repo.RegistrarGastoAsync(gasto);
                TempData["Mensaje"] = "Gasto registrado correctamente.";

                // Redirigir según rol
                return rol == "Trabajador"
                    ? RedirectToAction("Index", "Home")
                    : RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al registrar: {ex.Message}";
                ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
                ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
                ViewBag.Rol = rol;
                return View(gasto);
            }
        }

        // Editar - GET
        public async Task<IActionResult> Editar(int id, string returnUrl = null)
        {
            var gasto = await _repo.ObtenerGastoAsync(id);
            if (gasto == null) return NotFound();

            // Validación para Trabajador
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            var rol = HttpContext.Session.GetString("rol");

            if (rol == "Trabajador" && idUsuario.HasValue && gasto.idUsuario != idUsuario.Value)
            {
                TempData["Error"] = "No tiene permiso para editar este gasto.";
                return RedirectToAction("Index");
            }

            ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
            ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Rol = rol;

            return View(gasto);
        }

        // Editar - POST
        [HttpPost]
        public async Task<IActionResult> Editar(Gasto gasto, string returnUrl = null)
        {
            // Validación para Trabajador
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            var rol = HttpContext.Session.GetString("rol");

            if (rol == "Trabajador" && idUsuario.HasValue)
            {
                var gastoOriginal = await _repo.ObtenerGastoAsync(gasto.idGasto);
                if (gastoOriginal != null && gastoOriginal.idUsuario != idUsuario.Value)
                {
                    TempData["Error"] = "No tiene permiso para editar este gasto.";
                    return RedirectToAction("Index");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
                ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
                ViewBag.Rol = rol;
                return View(gasto);
            }

            try
            {
                await _repo.EditarGastoAsync(gasto);
                TempData["Mensaje"] = "Gasto actualizado correctamente.";

                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar: {ex.Message}";
                ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
                ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
                ViewBag.Rol = rol;
                return View(gasto);
            }
        }

        // Detalles
        public async Task<IActionResult> Detalles(int id)
        {
            var gasto = await _repo.ObtenerGastoAsync(id);
            if (gasto == null) return NotFound();

            // Validación para Trabajador
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            var rol = HttpContext.Session.GetString("rol");

            if (rol == "Trabajador" && idUsuario.HasValue && gasto.idUsuario != idUsuario.Value)
            {
                TempData["Error"] = "No tiene permiso para ver este gasto.";
                return RedirectToAction("Index");
            }

            return View(gasto);
        }

        // Eliminar - SOLO ADMIN
        [HttpPost]
        [ValidarAdmin]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _repo.EliminarGastoAsync(id);
                TempData["Mensaje"] = "Gasto eliminado correctamente.";
            }
            catch (SqlException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}