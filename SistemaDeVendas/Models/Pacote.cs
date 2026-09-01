using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeVendas.Models
{
    public class Pacote
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Display(Name = "Peso (kg)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Peso { get; set; }

        [Display(Name = "Coletado")]
        public bool Coletado { get; set; }

        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        public Cliente? Cliente { get; set; }

        [Display(Name = "Cadastrado em")]
        public DateTime DataCadastro { get; set; }
    }
}
