using System.ComponentModel.DataAnnotations;

namespace TelaLoginCrud.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome")]
        public string Nome { get; set; }

        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [Required]
        [Display(Name = "Endereço")]
        public string Endereco { get; set; }

        [Display(Name = "Bairro")]
        public string? Bairro { get; set; }

        [Required]
        [Display(Name = "Cidade")]
        public string Cidade { get; set; }

        [Display(Name = "CEP")]
        public string? Cep { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [Display(Name = "Cadastrado em")]
        public DateTime DataCadastro { get; set; }

        public List<Pacote> Pacotes { get; set; } = new();
    }
}
