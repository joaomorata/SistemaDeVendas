using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelaLoginCrud.Areas.Identity.Data;
using TelaLoginCrud.Models;
using TelaLoginCrud.Services;

namespace TelaLoginCrud.Controllers
{
    [Authorize]
    public class RotaController : Controller
    {
        private readonly TelaLoginContext _context;
        private readonly GeocodingService _geocoding;
        private readonly OtimizadorRota _otimizador;

        public RotaController(TelaLoginContext context, GeocodingService geocoding, OtimizadorRota otimizador)
        {
            _context = context;
            _geocoding = geocoding;
            _otimizador = otimizador;
        }

        public async Task<IActionResult> Index()
        {
            var rotas = _context.Rota
                .Include(r => r.Paradas)
                .OrderByDescending(r => r.DataCriacao);

            return View(await rotas.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var rota = await _context.Rota
                .Include(r => r.Paradas.OrderBy(p => p.Ordem))
                    .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rota == null)
                return NotFound();

            return View(rota);
        }

        public IActionResult Create()
        {
            var model = new RotaCreateViewModel
            {
                ClientesDisponiveis = ClientesComCoordenadas()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RotaCreateViewModel model)
        {
            var clientes = await _context.Cliente
                .Where(c => model.ClientesSelecionados.Contains(c.Id) && c.Latitude != null && c.Longitude != null)
                .ToListAsync();

            if (clientes.Count == 0)
                ModelState.AddModelError(nameof(model.ClientesSelecionados), "Selecione pelo menos um cliente com localização.");

            (double Latitude, double Longitude)? partida = null;
            if (ModelState.IsValid)
            {
                partida = await _geocoding.BuscarCoordenadas(model.EnderecoPartida);
                if (partida == null)
                    ModelState.AddModelError(nameof(model.EnderecoPartida), "Não foi possível localizar o endereço de partida.");
            }

            if (!ModelState.IsValid || partida == null)
            {
                model.ClientesDisponiveis = ClientesComCoordenadas();
                return View(model);
            }

            var pontos = clientes.Select(c => new OtimizadorRota.Ponto
            {
                ClienteId = c.Id,
                Latitude = c.Latitude!.Value,
                Longitude = c.Longitude!.Value
            }).ToList();

            var resultado = _otimizador.Resolver(partida.Value.Latitude, partida.Value.Longitude, pontos);

            var rota = new Rota
            {
                Nome = model.Nome,
                EnderecoPartida = model.EnderecoPartida,
                LatitudePartida = partida.Value.Latitude,
                LongitudePartida = partida.Value.Longitude,
                DistanciaKm = Math.Round(resultado.DistanciaKm, 2),
                Status = StatusRota.Planejada,
                DataCriacao = DateTime.Now
            };

            var ordem = 1;
            foreach (var ponto in resultado.Ordem)
            {
                rota.Paradas.Add(new RotaParada
                {
                    ClienteId = ponto.ClienteId,
                    Ordem = ordem,
                    Visitada = false
                });
                ordem++;
            }

            _context.Add(rota);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = rota.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iniciar(int id)
        {
            var rota = await _context.Rota.FindAsync(id);
            if (rota == null)
                return NotFound();

            rota.Status = StatusRota.EmAndamento;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarColeta(int id)
        {
            var parada = await _context.RotaParada
                .Include(p => p.Cliente)
                    .ThenInclude(c => c!.Pacotes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parada == null)
                return NotFound();

            parada.Visitada = true;

            if (parada.Cliente != null)
            {
                foreach (var pacote in parada.Cliente.Pacotes)
                    pacote.Coletado = true;
            }

            var rota = await _context.Rota
                .Include(r => r.Paradas)
                .FirstAsync(r => r.Id == parada.RotaId);

            if (rota.Paradas.All(p => p.Visitada))
                rota.Status = StatusRota.Concluida;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = parada.RotaId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var rota = await _context.Rota
                .Include(r => r.Paradas)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rota == null)
                return NotFound();

            return View(rota);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rota = await _context.Rota
                .Include(r => r.Paradas)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rota != null)
            {
                _context.RotaParada.RemoveRange(rota.Paradas);
                _context.Rota.Remove(rota);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private List<Cliente> ClientesComCoordenadas()
        {
            return _context.Cliente
                .Where(c => c.Latitude != null && c.Longitude != null)
                .OrderBy(c => c.Nome)
                .ToList();
        }
    }
}
