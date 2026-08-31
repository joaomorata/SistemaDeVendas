using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TelaLoginCrud.Areas.Identity.Data;
using TelaLoginCrud.Models;

namespace TelaLoginCrud.Controllers
{
    [Authorize]
    public class PacoteController : Controller
    {
        private readonly TelaLoginContext _context;

        public PacoteController(TelaLoginContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pacotes = _context.Pacote
                .Include(p => p.Cliente)
                .OrderBy(p => p.Coletado)
                .ThenBy(p => p.Cliente!.Nome);

            return View(await pacotes.ToListAsync());
        }

        public IActionResult Create()
        {
            CarregarClientes();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pacote pacote)
        {
            if (ModelState.IsValid)
            {
                pacote.DataCadastro = DateTime.Now;
                _context.Add(pacote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            CarregarClientes(pacote.ClienteId);
            return View(pacote);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var pacote = await _context.Pacote.FindAsync(id);
            if (pacote == null)
                return NotFound();

            CarregarClientes(pacote.ClienteId);
            return View(pacote);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pacote pacote)
        {
            if (id != pacote.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(pacote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            CarregarClientes(pacote.ClienteId);
            return View(pacote);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var pacote = await _context.Pacote
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pacote == null)
                return NotFound();

            return View(pacote);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pacote = await _context.Pacote.FindAsync(id);
            if (pacote != null)
            {
                _context.Pacote.Remove(pacote);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void CarregarClientes(int? selecionado = null)
        {
            ViewBag.Clientes = new SelectList(_context.Cliente.OrderBy(c => c.Nome).ToList(), "Id", "Nome", selecionado);
        }
    }
}
