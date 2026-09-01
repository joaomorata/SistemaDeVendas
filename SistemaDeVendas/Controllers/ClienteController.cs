using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.Areas.Identity.Data;
using SistemaDeVendas.Models;
using SistemaDeVendas.Services;

namespace SistemaDeVendas.Controllers
{
    [Authorize]
    public class ClienteController : Controller
    {
        private readonly SistemaDeVendasContext _context;
        private readonly GeocodingService _geocoding;

        public ClienteController(SistemaDeVendasContext context, GeocodingService geocoding)
        {
            _context = context;
            _geocoding = geocoding;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Cliente.OrderBy(c => c.Nome).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Cliente
                .Include(c => c.Pacotes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                await PreencherCoordenadas(cliente);
                cliente.DataCadastro = DateTime.Now;
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                await PreencherCoordenadas(cliente);
                _context.Update(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var cliente = await _context.Cliente.FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente != null)
            {
                try
                {
                    _context.Cliente.Remove(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Erro"] = "Não é possível excluir: o cliente está em uma rota.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PreencherCoordenadas(Cliente cliente)
        {
            var partes = new[] { cliente.Endereco, cliente.Bairro, cliente.Cidade, cliente.Cep };
            var endereco = string.Join(", ", partes.Where(p => !string.IsNullOrWhiteSpace(p)));

            var coordenadas = await _geocoding.BuscarCoordenadas(endereco);
            if (coordenadas != null)
            {
                cliente.Latitude = coordenadas.Value.Latitude;
                cliente.Longitude = coordenadas.Value.Longitude;
            }
        }
    }
}
