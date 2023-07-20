using Externo.API.Controllers;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Externo.API.Services;
using Externo.API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Externo.Tests
{
    public class UnitTest1
    {
        private readonly ILogger<ExternoController> _logger;
        private readonly ILogger<CobrancaService> _loggerService;
        private readonly HttpClient _client;


        [Fact]
        public void Test1()
        {
            var MockCobrancaService = new Mock<CobrancaService>();

            MockCobrancaService.Setup(service => service.AdicionarCobrancaNaFila(new CobrancaViewModel())).Returns(new CobrancaViewModel());

            var sut = new ExternoController(_logger, MockCobrancaService.Object);

            var result = (OkObjectResult)sut.AdicionarCobrancaNaFila(new CobrancaNovaViewModel());

            result.StatusCode.Should().Be(200);

        }

        [Fact]
        public void Teste2() {

            var sut = new CobrancaService(_client, _loggerService);

            var result = sut.AdicionarCobrancaNaFila(new CobrancaViewModel());

            Assert.Equal(typeof(CobrancaViewModel), result.GetType());

        }

        public void TesteIntegracaoGetCartaoAluguel() {

            var sut = new CobrancaService(_client, _loggerService);

            var result = sut.GetCartao(0);
        }

        
    }
}