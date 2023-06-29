using Externo.API.ViewModels;
using System.Buffers.Text;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Unicode;

namespace Externo.API.Services
{
    public interface ICobrancaService
    {
        public CobrancaViewModel AdicionarCobrancaNaFila(CobrancaNovaViewModel Cobranca);
        public Queue<CobrancaViewModel> BuscarCobrancasDaFila();
        public CobrancaViewModel RegistrarCobranca(CobrancaNovaViewModel Cobranca, CartaoViewModel cartao);
        public void RealizarCobrancaAsync(CartaoViewModel cartao, decimal valor);
        public CartaoViewModel GetCartao(int ciclistaId);
        public bool ValidateCreditCardNumber(string cardNumber);
        public CobrancaViewModel GetCobranca(int idCobranca);

    }

    public class CobrancaService : ICobrancaService
    {
        private static readonly Queue<CobrancaViewModel> FilaCobrancas = new();

        private static readonly Dictionary<int, CobrancaViewModel> DicionarioCobrancas = new();

        public CobrancaViewModel AdicionarCobrancaNaFila(CobrancaNovaViewModel Cobranca)
        {
            var result = new CobrancaViewModel()
            {
                Id = FilaCobrancas.Count,
                Status = "nova",
                HoraSolicitacao = "agora",
                HoraFinalizacao = "depois",
                Valor = Cobranca.Valor,
                Ciclista = Cobranca.Ciclista
            };

            FilaCobrancas.Enqueue(result);

            return result;
        }


        public Queue<CobrancaViewModel> BuscarCobrancasDaFila() { 
            return FilaCobrancas;
        }


        public CobrancaViewModel RegistrarCobranca(CobrancaNovaViewModel Cobranca, CartaoViewModel cartao) {
            if (cartao is null)
            {
                throw new ArgumentNullException(nameof(cartao));
            }

            var result = new CobrancaViewModel()
            {
                Id = DicionarioCobrancas.Count,
                Status = "nova",
                HoraSolicitacao = "agora",
                HoraFinalizacao = "depois",
                Valor = Cobranca.Valor,
                Ciclista = Cobranca.Ciclista,
                Cartao = cartao
            };

            DicionarioCobrancas.Add(DicionarioCobrancas.Count, result);

            return result;
        }

        public async void RealizarCobrancaAsync(CartaoViewModel cartao, decimal valor) {
            string baseAddress = "https://api-sandbox.getnet.com.br";
            

            //conecta na api
            
            var response = await ConectarApiCobranca(baseAddress);
            var responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(responseBody);
            string accessToken = document.RootElement.GetProperty("access_token").GetString();

            //Tokeniza o cartão
            response = await TokenizarCartao(cartao.Numero, accessToken, baseAddress);
            responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
            string number_token = responseObject["number_token"];

            //Realiza a cobrança

        }
           
         

        private static async Task<HttpResponseMessage> ConectarApiCobranca( string uri)
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

            response.EnsureSuccessStatusCode();

            return response;
            
        }

        private static async Task<HttpResponseMessage> TokenizarCartao(string cartao,string token, string uri) {
            string sellerId = "aef59803-18a3-4495-a048-274105ad65ec";
            string costumerId = "14221";
            using HttpClient client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, uri + "/v1/tokens/card");

            var requestData = new
            {
                card_number = cartao,
                customer_id = costumerId
            };

            request.Content = JsonContent.Create(requestData);
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Headers.Add("seller_id", sellerId);

            HttpResponseMessage response = await client.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(responseBody);
               
            return response;
        }
            

        

        public CartaoViewModel GetCartao(int ciclistaId) {
            return new CartaoViewModel(){
                NomeTitular = "aroldo",
                Numero = "22445818436",
                Validade = "2023-06-25",
                CVV = "9905"
            };
        }

        public CobrancaViewModel GetCobranca(int idCobranca)
        {
            return DicionarioCobrancas.ElementAt(idCobranca).Value;
        }

        public bool ValidateCreditCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;
            System.Collections.Generic.IEnumerable<char> rev = cardNumber.Reverse();
            int sum = 0, i = 0;
            foreach (char c in rev)
            {
                if (c < '0' || c > '9')
                    return false;
                int tmp = c - '0';
                if ((i & 1) != 0)
                {
                    tmp <<= 1;
                    if (tmp > 9)
                        tmp -= 9;
                }
                sum += tmp;
                i++;
            }
            return ((sum % 10) == 0);
        }
    }
}
