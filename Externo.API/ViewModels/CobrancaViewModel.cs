namespace Externo.API.ViewModels
{
    public class CobrancaViewModel
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? HoraSolicitacao { get; set; }
        public string? HoraFinalizacao { get; set; }
        public decimal Valor { get; set; }
        public int Ciclista { get; set; }
        public CartaoViewModel? Cartao { get; set; }
    }
    public class CobrancaNovaViewModel
    {
        public decimal Valor { get; set; }
        public int Ciclista { get; set; }
    }

    public class CartaoViewModel
    {
        public string? NomeTitular { get; set; }
        public string? Numero { get; set; }
        public string? Validade { get; set; }
        public string? CVV { get; set; }
    }
}
