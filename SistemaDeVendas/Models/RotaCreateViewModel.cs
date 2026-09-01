using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SistemaDeVendas.Models
{
    public class RotaCreateViewModel
    {
        [Display(Name = "Nome da rota")]
        public string Nome { get; set; }

        [Display(Name = "Endereço de partida")]
        public string EnderecoPartida { get; set; }

        public List<int> ClientesSelecionados { get; set; } = new();

        [ValidateNever]
        public List<Cliente> ClientesDisponiveis { get; set; } = new();
    }
}
