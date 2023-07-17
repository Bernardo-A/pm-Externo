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
        public Task<CobrancaViewModel> RealizarCobrancaAsync(CobrancaNovaViewModel cobranca);
        public Task<bool> ValidateCreditCardNumber(CartaoViewModel cardNumber);
        public CobrancaViewModel GetCobranca(int idCobranca);

    }

    public class CobrancaService : ICobrancaService
    {
        private static readonly Queue<CobrancaViewModel> FilaCobrancas = new();

        private static readonly Dictionary<int, CobrancaViewModel> DicionarioCobrancas = new();

        private readonly ILogger<CobrancaService> _logger;

        private readonly HttpClient HttpClient = new();

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

        public int LibSize() {
            return DicionarioCobrancas.Count();
        }

        public Queue<CobrancaViewModel> BuscarCobrancasDaFila() { 
            return FilaCobrancas;
        }


        public CobrancaViewModel RegistrarCobranca(CobrancaViewModel Cobranca) {
            DicionarioCobrancas.Add(Cobranca.Id, Cobranca);

            return Cobranca;
        }

        public async Task<CobrancaViewModel> RealizarCobrancaAsync(CobrancaNovaViewModel cobranca) {

            var cobrancaCompleta = new CobrancaViewModel();

            cobrancaCompleta.HoraSolicitacao = DateTime.Now;
            cobrancaCompleta.Id = DicionarioCobrancas.Count;
            cobrancaCompleta.Valor = cobranca.Valor;

            try {
                var cartao = await GetCartao(cobranca.Ciclista);

                var pagamento = new PagamentoDTO()
                {
                    MerchantOrderId = DicionarioCobrancas.Count,

                    Payment = new Payment
                    {
                        Amount = cobranca.Valor,
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

                cobrancaCompleta.Status = (pagamentoResponse?.Payment?.Status == 4 || pagamentoResponse?.Payment?.Status == 6) ? "PAGA" : "FALHA";
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
                AdicionarCobrancaNaFila(cobrancaCompleta);
                return cobrancaCompleta;
            }

        }
           
         

        private static async Task<HttpResponseMessage> ConectarApiCobranca(string uri)
        {
            using HttpClient client = new HttpClient();
            string base64 = "OTM3YzAyNzQtMzgxOC00NmYwLWJiNGUtOWUxN2IyODRkZGVjOjU4N2RkNWM1LWFhMTktNGRkMy1iMDRjLTc1NGFlMzc0NDdhNA==";
            

            var formData = new Dictionary<string, string>
                {
                    { "scope", "oob" },
                    { "grant_type", "client_credentials" }
                };

            var content = new FormUrlEncodedContent(formData);
                
            var request = new HttpRequestMessage(HttpMethod.Post, uri + "/auth/oauth/v2/token");
            request.Content = content;
            request.Headers.Add("Authorization", "Basic " + base64);


            HttpResponseMessage response = await client.SendAsync(request);

            //response.EnsureSuccessStatusCode();

            return response;
            
        }

        //private static async Task<HttpResponseMessage> TokenizarCartao(string cartao,string token, string uri) {
        //    string sellerId = "aef59803-18a3-4495-a048-274105ad65ec";
        //    string costumerId = "14221";
        //    using HttpClient client = new HttpClient();

        //    var request = new HttpRequestMessage(HttpMethod.Post, uri + "/v1/tokens/card");

        //    var requestData = new
        //    {
        //        card_number = cartao,
        //        customer_id = costumerId
        //    };

        //    request.Content = JsonContent.Create(requestData);
        //    request.Headers.Add("Authorization", "Bearer " + token);
        //    request.Headers.Add("seller_id", sellerId);

        //    HttpResponseMessage response = await client.SendAsync(request);

        //    var responseBody = await response.Content.ReadAsStringAsync();
        //    using JsonDocument document = JsonDocument.Parse(responseBody);
               
        //    return response;
        //}
            

        

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

        public CobrancaViewModel GetCobranca(int idCobranca)
        {

            if (DicionarioCobrancas.ContainsKey(idCobranca))
            { 
                return DicionarioCobrancas.ElementAt(idCobranca).Value;
            }
            else {
                return null;
            }
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
