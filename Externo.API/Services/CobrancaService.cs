using Externo.API.ViewModels;
using System.Buffers.Text;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Unicode;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Dynamic;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata;
using Microsoft.Net.Http.Headers;
using Externo.API.Controllers;

namespace Externo.API.Services
{
    public interface ICobrancaService
    {
        public CobrancaViewModel AdicionarCobrancaNaFila(CobrancaViewModel cobranca);
        public Queue<CobrancaViewModel> BuscarCobrancasDaFila();
        public CobrancaViewModel RegistrarCobranca(CobrancaViewModel Cobranca);
        public Task<CobrancaViewModel> RealizarCobrancaAsync(decimal valor, int ciclistaId);
        public Task<bool> ValidateCreditCardNumber(CartaoViewModel cardNumber);
        public CobrancaViewModel? GetCobranca(int idCobranca);
        public Task<List<CobrancaViewModel>> ProcessarFilaCobrancas();

    }

    public class CobrancaService : ICobrancaService
    {
        private static readonly Queue<CobrancaViewModel> FilaCobrancas = new();

        private static readonly Dictionary<int, CobrancaViewModel> DicionarioCobrancas = new();

        private readonly ILogger<CobrancaService> _logger;

        private readonly HttpClient HttpClient;


        private const string MerchantId = "49a154cd-b990-4074-a9e9-7f79b70a4435";
        private const string MerchantKey = "YDUXUSDWLLJTZITLUSVOUTIIWBUIWKBLBTVEZSNC";
        private const string cieloAddress = "https://apisandbox.cieloecommerce.cielo.com.br";
        private const string aluguelAddress = "https://pmaluguel.herokuapp.com";


        public CobrancaService(HttpClient httpClient, ILogger<CobrancaService> logger)
        {
            HttpClient = httpClient;
            _logger = logger;
        }


        public CobrancaViewModel AdicionarCobrancaNaFila(CobrancaViewModel cobranca)
        {
            RegistrarCobranca(cobranca);
            FilaCobrancas.Enqueue(cobranca);
            return cobranca;
        }

        public Queue<CobrancaViewModel> BuscarCobrancasDaFila() { 
            return FilaCobrancas;
        }

        public CobrancaViewModel RegistrarCobranca(CobrancaViewModel Cobranca) {
            DicionarioCobrancas.Add(Cobranca.Id, Cobranca);

            return Cobranca;
        }

        public async Task<CobrancaViewModel> RealizarCobrancaAsync(decimal valor, int ciclistaId) {

            var cobrancaCompleta = new CobrancaViewModel();

            cobrancaCompleta.HoraSolicitacao = DateTime.Now;
            cobrancaCompleta.Id = DicionarioCobrancas.Count;
            cobrancaCompleta.Valor = valor;
            cobrancaCompleta.Ciclista = ciclistaId;

            try {
                var cartao = await GetCartao(ciclistaId);

                var pagamento = new PagamentoDTO()
                {
                    MerchantOrderId = DicionarioCobrancas.Count,

                    Payment = new Payment
                    {
                        Amount = valor,
                        CreditCard = new CartaoDTO()
                        {
                            CardNumber = cartao.Numero,
                            Holder = cartao.NomeTitular,
                            ExpirationDate = cartao.Validade,
                            SecurityCode = cartao.CVV
                        }
                    }
                };

                var requestBody = JsonConvert.SerializeObject(pagamento);

                HttpClient.DefaultRequestHeaders.Add("MerchantId", MerchantId);
                HttpClient.DefaultRequestHeaders.Add("MerchantKey", MerchantKey);

                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                var response = await HttpClient.PostAsync(cieloAddress + "/1/sales", content);

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                var pagamentoResponse = JsonConvert.DeserializeObject<PagamentoResponseDTO>(jsonString);

                cobrancaCompleta.Status = (pagamentoResponse?.Payment?.ReturnCode == 4 || pagamentoResponse?.Payment?.ReturnCode == 6) ? "PAGA" : "FALHA";
                cobrancaCompleta.HoraFinalizacao = pagamentoResponse?.Payment?.ReceivedDate;

                if (cobrancaCompleta.Status == "FALHA")
                {
                    AdicionarCobrancaNaFila(cobrancaCompleta);
                }
                else {
                    RegistrarCobranca(cobrancaCompleta);
                }

                return cobrancaCompleta;
            }
            catch (Exception ex) {
                _logger.LogError("Error: ", ex.Message);
                cobrancaCompleta.Status = "FALHA";
                AdicionarCobrancaNaFila(cobrancaCompleta);
                throw new Exception();
            }

        }

        private async Task<CartaoViewModel> GetCartao(int ciclistaId) {
            var response = await HttpClient.GetAsync(aluguelAddress + "/cartaoDeCredito/" + ciclistaId);

            response.EnsureSuccessStatusCode();
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) {
                return new CartaoViewModel();
            }
            var responseContent = response.Content;
            var cartao = await responseContent.ReadFromJsonAsync<CartaoViewModel>();

            if (cartao != null)
            {
                return cartao;
            }
            else { 
                return new CartaoViewModel();
            }
            
        }

        public CobrancaViewModel? GetCobranca(int idCobranca)
        {

            if (DicionarioCobrancas.ContainsKey(idCobranca))
            { 
                return DicionarioCobrancas.ElementAt(idCobranca).Value;
            }
            else {
                return null;
            }
        }


        public async Task<List<CobrancaViewModel>> ProcessarFilaCobrancas() {

            int tamanho = FilaCobrancas.Count;
            var lista =  new List<CobrancaViewModel>();
            
            for (int i = 0; i < tamanho; i++)
            {
                var cobranca = FilaCobrancas.Dequeue();
               
                try
                {
                    var result = await RealizarCobrancaAsync(cobranca.Valor, cobranca.Ciclista);
                    lista.Add(result);
                }
                catch {
                    continue;
                }

            }
            return lista;
        }

        public async Task<bool> ValidateCreditCardNumber(CartaoViewModel cartao)
        {
            var CreditCard = new CartaoDTO()
            {
                CardNumber = cartao.Numero,
                Holder = cartao.NomeTitular,
                ExpirationDate = cartao.Validade,
                SecurityCode = cartao.CVV
            };

            var requestBody = JsonConvert.SerializeObject(CreditCard);

            HttpClient.DefaultRequestHeaders.Add("MerchantId", MerchantId);
            HttpClient.DefaultRequestHeaders.Add("MerchantKey", MerchantKey);

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync(cieloAddress + "/1/zeroauth", content);

            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return false;
            }

            return true;
        }
    }
}
