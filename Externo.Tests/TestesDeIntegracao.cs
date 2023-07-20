using Externo.API.Controllers;
using Externo.API.Services;
using Externo.API.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
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

        private readonly Mock<ILogger<CobrancaService>> _loggerService = new();
        private readonly HttpClient _client = new();


        private const string MerchantId = "49a154cd-b990-4074-a9e9-7f79b70a4435";
        private const string MerchantKey = "YDUXUSDWLLJTZITLUSVOUTIIWBUIWKBLBTVEZSNC";
        private const string cieloAddress = "https://apisandbox.cieloecommerce.cielo.com.br";

        [Fact]
        public void TesteIntegracaoGetCartaoAluguel()
        {
            var sut = new CobrancaService(_client, _loggerService.Object);

            var result = sut.GetCartao(0);

            Assert.NotNull(result);
        }


        [Fact]
        public async void RealizaPagamentoRetornaStatusSucesso()
        {

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


        [Fact]
        public async void ValidateCartaoRetornaTrueCartaoCom13Digitos()
        {
            var CreditCard = new CartaoDTO()
            {
                CardNumber = "10102222212212",
                Holder = "Titular Cartao",
                ExpirationDate = "08/2024",
                SecurityCode = "111"
            };

            var requestBody = JsonConvert.SerializeObject(CreditCard);

            _client.DefaultRequestHeaders.Add("MerchantId", MerchantId);
            _client.DefaultRequestHeaders.Add("MerchantKey", MerchantKey);

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(cieloAddress + "/1/zeroauth", content);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

    }
}