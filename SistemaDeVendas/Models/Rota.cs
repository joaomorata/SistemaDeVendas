using System.ComponentModel.DataAnnotations;

namespace SistemaDeVendas.Models
{
    public class Rota
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome da rota")]
        public string Nome { get; set; }

        [Display(Name = "Ponto de partida")]
        public string EnderecoPartida { get; set; }

        public double LatitudePartida { get; set; }

        public double LongitudePartida { get; set; }

        [Display(Name = "Distância total (km)")]
        public double DistanciaKm { get; set; }

        [Display(Name = "Status")]
        public StatusRota Status { get; set; }

        [Display(Name = "Criada em")]
        public DateTime DataCriacao { get; set; }

        public List<RotaParada> Paradas { get; set; } = new();
    }
}
