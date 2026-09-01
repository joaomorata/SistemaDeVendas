using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using SistemaDeVendas.Areas.Identity.Data;
using SistemaDeVendas.Models;

namespace SistemaDeVendas.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SistemaDeVendasContext _context;

        public HomeController(SistemaDeVendasContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalClientes = await _context.Cliente.CountAsync();
            ViewBag.PacotesPendentes = await _context.Pacote.CountAsync(p => !p.Coletado);
            ViewBag.RotasAbertas = await _context.Rota.CountAsync(r => r.Status != StatusRota.Concluida);
            ViewBag.TotalVendas = await _context.Venda.CountAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
