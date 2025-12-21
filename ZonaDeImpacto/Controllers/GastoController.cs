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
            int? filtroUsuario = null,
            string filtroFechaDesde = null,
            string filtroFechaHasta = null)
        {
            // Siempre restablecer a página 1 cuando se aplican filtros (excepto si ya se está en una página específica)
            if ((!string.IsNullOrEmpty(filtroMantenimientoCodigo) ||
                 filtroTipoGasto.HasValue ||
                 filtroUsuario.HasValue ||
                 !string.IsNullOrEmpty(filtroFechaDesde) ||
                 !string.IsNullOrEmpty(filtroFechaHasta)) &&
                pagina == 1)
            {
                // Si hay filtros y es página 1, mantener página 1
            }
            else if ((!string.IsNullOrEmpty(filtroMantenimientoCodigo) ||
                      filtroTipoGasto.HasValue ||
                      filtroUsuario.HasValue ||
                      !string.IsNullOrEmpty(filtroFechaDesde) ||
                      !string.IsNullOrEmpty(filtroFechaHasta)) &&
                     pagina > 1)
            {
                // Si hay filtros y página > 1, verificar si hay resultados
                // La verificación se hace después de obtener los datos
            }

            int pageSize = 6; // Cantidad de gastos por página

            // Obtener datos de sesión
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            var rol = HttpContext.Session.GetString("rol");
            var nombreUsuario = HttpContext.Session.GetString("nombre");

            ViewBag.Rol = rol;
            ViewBag.NombreUsuario = nombreUsuario;
            ViewBag.IdUsuario = idUsuario;

            // Si es Trabajador, solo puede ver sus gastos
            int? idUsuarioFiltro = null;
            if (rol == "Trabajador" && idUsuario.HasValue)
            {
                idUsuarioFiltro = idUsuario.Value;
                // Para trabajador, forzamos el filtroUsuario a su ID
                filtroUsuario = idUsuario.Value;
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

            // Convertir filtroUsuario a string para el stored procedure
            string filtroUsuarioStr = filtroUsuario?.ToString();

            // Obtener datos con paginación
            var (gastos, totalRegistros) = await _repo.ListarGastosPaginadoAsync(
                pagina: pagina,
                tamanoPagina: pageSize,
                filtroMantenimientoCodigo: filtroMantenimientoCodigo,
                filtroTipoGasto: filtroTipoGasto,
                filtroUsuario: filtroUsuarioStr,
                filtroFechaDesde: fechaDesde,
                filtroFechaHasta: fechaHasta,
                idUsuarioFiltro: idUsuarioFiltro);

            // Si no hay resultados y no estamos en la página 1, redirigir a la página 1
            if (!gastos.Any() && pagina > 1)
            {
                return RedirectToAction("Index", new
                {
                    pagina = 1,
                    filtroMantenimientoCodigo,
                    filtroTipoGasto,
                    filtroUsuario,
                    filtroFechaDesde,
                    filtroFechaHasta
                });
            }

            // Calcular total de páginas
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / pageSize);
            ViewBag.TotalRegistros = totalRegistros;

            // Pasar filtros a la vista
            ViewBag.FiltroMantenimientoCodigo = filtroMantenimientoCodigo;
            ViewBag.FiltroTipoGasto = filtroTipoGasto;
            ViewBag.FiltroUsuario = filtroUsuario;
            ViewBag.FiltroFechaDesde = filtroFechaDesde;
            ViewBag.FiltroFechaHasta = filtroFechaHasta;

            // Datos para los dropdowns de filtros
            ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
            ViewBag.Usuarios = await _repo.ObtenerUsuariosAsync();

            return View(gastos);
        }

        // Resto del código se mantiene igual...
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
            ViewBag.IdUsuario = idUsuario;
            return View();
        }

        // Crear - POST 
        [HttpPost]
        public async Task<IActionResult> Crear(Gasto gasto)
        {
            var rol = HttpContext.Session.GetString("rol");

            if (!ModelState.IsValid)
            {
                // Recargar datos según rol
                var idUsuario = HttpContext.Session.GetInt32("idUsuario");
                if (rol == "Trabajador" && idUsuario.HasValue)
                {
                    ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosPorTrabajadorAsync(idUsuario.Value);
                }
                else
                {
                    ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
                }

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

                // Recargar datos según rol
                var idUsuario = HttpContext.Session.GetInt32("idUsuario");
                if (rol == "Trabajador" && idUsuario.HasValue)
                {
                    ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosPorTrabajadorAsync(idUsuario.Value);
                }
                else
                {
                    ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
                }

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

            // Construir la returnUrl con los filtros actuales si no se proporciona
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            // Cargar datos según rol
            if (rol == "Trabajador" && idUsuario.HasValue)
            {
                ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosPorTrabajadorAsync(idUsuario.Value);
            }
            else
            {
                ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
            }

            ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Rol = rol;
            ViewBag.IdUsuario = idUsuario;

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
                // Cargar datos según rol
                if (rol == "Trabajador" && idUsuario.HasValue)
                {
                    ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosPorTrabajadorAsync(idUsuario.Value);
                }
                else
                {
                    ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
                }

                ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
                ViewBag.Rol = rol;
                return View(gasto);
            }

            try
            {
                await _repo.EditarGastoAsync(gasto);
                TempData["Mensaje"] = "Gasto actualizado correctamente.";

                // Si no hay returnUrl, construir una con los filtros actuales
                if (string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = BuildReturnUrl();
                }

                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar: {ex.Message}";

                // Cargar datos según rol
                if (rol == "Trabajador" && idUsuario.HasValue)
                {
                    ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosPorTrabajadorAsync(idUsuario.Value);
                }
                else
                {
                    ViewBag.Mantenimientos = await _repo.ObtenerMantenimientosAsync();
                }

                ViewBag.TiposGasto = await _repo.ObtenerTiposGastoAsync();
                ViewBag.Rol = rol;
                return View(gasto);
            }
        }

        // Detalles
        public async Task<IActionResult> Detalles(int id, string returnUrl = null)
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

            // Construir la returnUrl con los filtros actuales si no se proporciona
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = BuildReturnUrl();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(gasto);
        }

        // Eliminar - SOLO ADMIN
        [HttpPost]
        [ValidarAdmin]
        public async Task<IActionResult> Eliminar(int id, string returnUrl = null)
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

            if (query.ContainsKey("filtroMantenimientoCodigo"))
            {
                queryParams.Add($"filtroMantenimientoCodigo={query["filtroMantenimientoCodigo"]}");
            }

            if (query.ContainsKey("filtroTipoGasto"))
            {
                queryParams.Add($"filtroTipoGasto={query["filtroTipoGasto"]}");
            }

            if (query.ContainsKey("filtroUsuario"))
            {
                queryParams.Add($"filtroUsuario={query["filtroUsuario"]}");
            }

            if (query.ContainsKey("filtroFechaDesde"))
            {
                queryParams.Add($"filtroFechaDesde={query["filtroFechaDesde"]}");
            }

            if (query.ContainsKey("filtroFechaHasta"))
            {
                queryParams.Add($"filtroFechaHasta={query["filtroFechaHasta"]}");
            }

            var queryString = queryParams.Any() ? $"?{string.Join("&", queryParams)}" : "";
            var returnUrl = Url.Action("Index") + queryString;

            return returnUrl;
        }
    }
}