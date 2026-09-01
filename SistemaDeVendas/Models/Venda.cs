using System.ComponentModel.DataAnnotations;

namespace SistemaDeVendas.Models
{
    public class Venda
    {
        [Key]
        public int IdVenda { get; set; }

        public string NomeProd { get; set; }

        public int Quant { get; set; }

        public decimal Preco { get; set; }

        public DateTime DataVenda { get; set; }
    }
}