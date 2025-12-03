using System.Runtime.CompilerServices;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
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
        public  IActionResult Crear()
        {
            return View();
        }

        //creamos post
        [HttpPost]
        public async Task<IActionResult> Crear(Vehiculo modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }
            await _repo.RegistrarVehiculoAsync(modelo);
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
        public async Task<IActionResult> Editar(Vehiculo modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            await _repo.EditarVehiculoAsync(modelo);
            return RedirectToAction("Index");
        }

        // eliminamos
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repo.EliminarVehiculoAsync(id);
            return RedirectToAction("Index");
        }

    

        //reporte pdf
        [HttpGet]
        public async Task<IActionResult> ReportePDF()
        {
            List<Vehiculo> lista = await _repo.ListarVehiculosReporteAsync();

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                Paragraph titulo = new Paragraph("Reporte de Vehículos\n\n")
                {
                    Alignment = Element.ALIGN_CENTER
                };
                doc.Add(titulo);

                PdfPTable tabla = new PdfPTable(6);
                tabla.AddCell("Placa");
                tabla.AddCell("Marca");
                tabla.AddCell("Modelo");
                tabla.AddCell("Año");
                tabla.AddCell("Kilometraje");
                tabla.AddCell("Estado");

                foreach (var v in lista)
                {
                    tabla.AddCell(v.placa);
                    tabla.AddCell(v.marca);
                    tabla.AddCell(v.modelo);
                    tabla.AddCell(v.anio?.ToString() ?? "-");
                    tabla.AddCell(v.kilometraje?.ToString() ?? "-");
                    tabla.AddCell(v.estado ?? "-");
                }

                doc.Add(tabla);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", "ReporteVehiculos.pdf");
            }
        }

        //reporte excel
        [HttpGet]
        public async Task<IActionResult> ReporteExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            List<Vehiculo> lista = await _repo.ListarVehiculosReporteAsync();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Vehículos");

                ws.Cells["A1"].Value = "Placa";
                ws.Cells["B1"].Value = "Marca";
                ws.Cells["C1"].Value = "Modelo";
                ws.Cells["D1"].Value = "Año";
                ws.Cells["E1"].Value = "Kilometraje";
                ws.Cells["F1"].Value = "Estado";

                ws.Cells["A1:F1"].Style.Font.Bold = true;

                int fila = 2;

                foreach (var v in lista)
                {
                    ws.Cells[fila, 1].Value = v.placa;
                    ws.Cells[fila, 2].Value = v.marca;
                    ws.Cells[fila, 3].Value = v.modelo;
                    ws.Cells[fila, 4].Value = v.anio;
                    ws.Cells[fila, 5].Value = v.kilometraje;
                    ws.Cells[fila, 6].Value = v.estado;
                    fila++;
                }

                ws.Cells.AutoFitColumns();

                var bytes = package.GetAsByteArray();
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteVehiculos.xlsx");
            }
        }
    }
}