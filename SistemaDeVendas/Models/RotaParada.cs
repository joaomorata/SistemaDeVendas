using System.ComponentModel.DataAnnotations;

namespace SistemaDeVendas.Models
{
    public class RotaParada
    {
        public int Id { get; set; }

        public int RotaId { get; set; }

        public Rota? Rota { get; set; }

        public int ClienteId { get; set; }

        public Cliente? Cliente { get; set; }

        [Display(Name = "Ordem")]
        public int Ordem { get; set; }

        [Display(Name = "Visitada")]
        public bool Visitada { get; set; }
    }
}
