using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ZonaDeImpacto.Data;
using ZonaDeImpacto.Filters;
using ZonaDeImpacto.Models.Reportes;

namespace ZonaDeImpacto.Controllers
{
    [ValidarSesion]
    [ValidarAdmin]
    public class ReporteController : Controller
    {
        private readonly ReporteRepository _repo;
        private readonly VehiculoRepository _vehiculoRepo;

        public ReporteController(ReporteRepository repo, VehiculoRepository vehiculoRepo)
        {
            _repo = repo;
            _vehiculoRepo = vehiculoRepo;
        }

        // Página principal de reportes
        public IActionResult Index()
        {
            return View();
        }

        // Reporte 1: Costos por Vehículo
        [HttpGet]
        public async Task<IActionResult> CostosPorVehiculo(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
        {
            // Si no hay fechas, usar el mes actual por defecto
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                fechaFin = DateTime.Now;
            }

            ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");

            var reporte = await _repo.ObtenerCostosPorVehiculoAsync(fechaInicio, fechaFin);
            return View(reporte);
        }

        [HttpPost]
        public IActionResult CostosPorVehiculoPost(DateTime fechaInicio, DateTime fechaFin)
        {
            return RedirectToAction("CostosPorVehiculo", new { fechaInicio, fechaFin });
        }

        // Reporte 2: Historial de Vehículo
        [HttpGet]
        public async Task<IActionResult> HistorialVehiculo(int? idVehiculo = null)
        {
            if (idVehiculo.HasValue)
            {
                var historial = await _repo.ObtenerHistorialVehiculoAsync(idVehiculo.Value);
                if (historial == null)
                {
                    TempData["Error"] = "Vehículo no encontrado";
                    return RedirectToAction("HistorialVehiculo");
                }
                return View(historial);
            }

            // Si no hay id, mostrar solo el formulario de selección
            ViewBag.Vehiculos = await _repo.ObtenerVehiculosActivosAsync();
            return View();
        }

        [HttpPost]
        public IActionResult HistorialVehiculoPost(int idVehiculo)
        {
            return RedirectToAction("HistorialVehiculo", new { idVehiculo });
        }

        // Reporte 3: Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard(int? mes = null, int? anio = null)
        {
            var dashboard = await _repo.ObtenerDashboardAsync(mes, anio);

            // Lista de meses para dropdown
            ViewBag.Meses = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Enero" },
                new SelectListItem { Value = "2", Text = "Febrero" },
                new SelectListItem { Value = "3", Text = "Marzo" },
                new SelectListItem { Value = "4", Text = "Abril" },
                new SelectListItem { Value = "5", Text = "Mayo" },
                new SelectListItem { Value = "6", Text = "Junio" },
                new SelectListItem { Value = "7", Text = "Julio" },
                new SelectListItem { Value = "8", Text = "Agosto" },
                new SelectListItem { Value = "9", Text = "Septiembre" },
                new SelectListItem { Value = "10", Text = "Octubre" },
                new SelectListItem { Value = "11", Text = "Noviembre" },
                new SelectListItem { Value = "12", Text = "Diciembre" }
            };

            // Años (últimos 5 años)
            var anios = new List<SelectListItem>();
            for (int i = 0; i < 5; i++)
            {
                int a = DateTime.Now.Year - i;
                anios.Add(new SelectListItem { Value = a.ToString(), Text = a.ToString() });
            }
            ViewBag.Anios = anios;

            return View(dashboard);
        }

        // Reporte 4: Actividad por Usuario
        [HttpGet]
        public async Task<IActionResult> ActividadUsuario(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? idUsuario = null)
        {
            // Si no hay fechas, usar el mes actual por defecto
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                fechaFin = DateTime.Now;
            }

            var reporte = await _repo.ObtenerActividadUsuarioAsync(fechaInicio, fechaFin, idUsuario);

            // Pasar datos a la vista
            ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");
            ViewBag.Usuarios = await _repo.ObtenerUsuariosParaReporteAsync();

            return View(reporte);
        }

        [HttpPost]
        public IActionResult ActividadUsuarioPost(DateTime fechaInicio, DateTime fechaFin, int? idUsuario = null)
        {
            return RedirectToAction("ActividadUsuario", new { fechaInicio, fechaFin, idUsuario });
        }


        [HttpPost]
        public IActionResult DashboardPost(int mes, int anio)
        {
            return RedirectToAction("Dashboard", new { mes, anio });
        }

        // Exportar a CSV - Costos por Vehículo
        public async Task<IActionResult> ExportarCostosCSV(DateTime fechaInicio, DateTime fechaFin)
        {
            var reporte = await _repo.ObtenerCostosPorVehiculoAsync(fechaInicio, fechaFin);

            var csv = "Placa,Marca,Modelo,Tipo Vehiculo,Mantenimientos,Gasto Total,Gasto Preventivo,Gasto Correctivo\n";

            foreach (var item in reporte)
            {
                csv += $"\"{item.placa}\",\"{item.marca}\",\"{item.modelo}\",\"{item.tipoVehiculo}\","
                     + $"{item.totalMantenimientos},{item.totalGastado},{item.gastoPreventivo},{item.gastoCorrectivo}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"CostosVehiculos_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.csv");
        }

        // Exportar a CSV - Actividad por Usuario  
        public async Task<IActionResult> ExportarActividadCSV(DateTime fechaInicio, DateTime fechaFin, int? idUsuario = null)
        {
            var reporte = await _repo.ObtenerActividadUsuarioAsync(fechaInicio, fechaFin, idUsuario);

            var csv = "Usuario,Rol,Vehículos Atendidos,Mantenimientos Total,Preventivos,Correctivos,Gasto Total\n";

            foreach (var usuario in reporte.Usuarios)
            {
                csv += $"\"{usuario.nombreCompleto}\","
                     + $"\"{usuario.rol}\","
                     + $"{usuario.vehiculosAtendidos},{usuario.totalMantenimientos},"
                     + $"{usuario.mantenimientosPreventivos},{usuario.mantenimientosCorrectivos},"
                     + $"{usuario.totalGastado}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            string fileName = idUsuario.HasValue
                ? $"ActividadUsuario_{reporte.UsuarioSeleccionadoNombre}_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.csv"
                : $"ActividadTodosUsuarios_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}