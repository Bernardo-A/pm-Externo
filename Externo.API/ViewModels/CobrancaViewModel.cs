namespace Externo.API.ViewModels
{
    public class Cobranca
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? HoraSolicitacao { get; set; }
        public string? HoraFinalizacao { get; set; }
        public decimal? Valor { get; set; }
        public int? Ciclista { get; set; }
    }
    public class CobrancaNova
    {
        public decimal Valor { get; set; }
        public int Ciclista { get; set; }
    }

}
