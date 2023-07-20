using Externo.API.Controllers;
using Externo.API.Services;
using Externo.API.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Externo.Tests
{
    public class TestesDeIntegracao
    {
        private readonly ILogger<ExternoController> _logger;
        private readonly ILogger<CobrancaService> _loggerService;
        private readonly HttpClient _client;

        
        private const string MerchantId = "49a154cd-b990-4074-a9e9-7f79b70a4435";
        private const string MerchantKey = "YDUXUSDWLLJTZITLUSVOUTIIWBUIWKBLBTVEZSNC";
        private const string cieloAddress = "https://apisandbox.cieloecommerce.cielo.com.br";

        [Fact]
        public void TesteIntegracaoGetCartaoAluguel()
        {
            //adicionar ciclista para encontrar o cartão dele.

            var sut = new CobrancaService(_client, _loggerService);

            var result = sut.GetCartao(0);

            Assert.NotNull(result);
        }


        [Fact]
        public async void RealizaPagamentoRetornaStatus200() {

            var pagamento = new PagamentoDTO()
            {
                MerchantOrderId = 0,

                Payment = new Payment
                {
                    Amount = 100,
                    CreditCard = new CartaoDTO()
                    {
                        CardNumber = "101022222121",
                        Holder = "Titular Cartao",
                        ExpirationDate = "08/2024",
                        SecurityCode = "111"
                    }
                }
            };

            var requestBody = JsonConvert.SerializeObject(pagamento);

            _client.DefaultRequestHeaders.Add("MerchantId", MerchantId);
            _client.DefaultRequestHeaders.Add("MerchantKey", MerchantKey);

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var result = await _client.PostAsync(cieloAddress + "/1/sales", content);

            Assert.True(result.IsSuccessStatusCode);
        }





    }

   


}
