using System.Collections.Generic;

namespace Externo.API.ViewModels
{
    public class CobrancaViewModel
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public DateTime? HoraSolicitacao { get; set; }
        public DateTime? HoraFinalizacao { get; set; }
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

    public class PagamentoDTO
    {
        public int? MerchantOrderId { get; set; }
        public Payment? Payment { get; set; }
        
    }

    public class PagamentoResponseDTO
    {
        public int? MerchantOrderId { get; set; }
        public Payment? Payment { get; set; }
    }

    public class Payment 
    {
        public string? Type { get; set; } = "CreditCard";
        public decimal? Amount { get; set; }
        public int? Installments { get; set; } = 1;
        public CartaoDTO? CreditCard { get; set; }
        public int? Status { get; set; }
        public DateTime? ReceivedDate { get; set; }
    }


    public class CartaoDTO
    {
        public string? CardNumber { get; set; }
        public string? Holder { get; set; }
        public string? ExpirationDate { get; set; }
        public string? SecurityCode { get; set; }
    }





}
