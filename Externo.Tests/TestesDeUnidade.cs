using Externo.API.Controllers;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Externo.API.Services;
using Externo.API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using PostmarkDotNet;

namespace Externo.Tests
{
    public class TestesDeUnidade
    {
        private readonly Mock<ILogger<ExternoController>> _logger = new();
        private readonly Mock<ILogger<CobrancaService>> _loggerService = new();
        private readonly Mock<HttpClient> _client = new();

        [Fact]
        public void AdicionaNaFilaRetornaCobranca()
        {
            var MockCobrancaService = new Mock<ICobrancaService>();

            MockCobrancaService.Setup(service => service.AdicionarCobrancaNaFila(It.IsAny<CobrancaViewModel>())).Returns(new CobrancaViewModel());

            var sut = new ExternoController(_logger.Object, MockCobrancaService.Object);

            var result = (OkObjectResult)sut.AdicionarCobrancaNaFila(new CobrancaNovaViewModel());

            result.StatusCode.Should().Be(200);

        }

        [Fact]
        public void CobrancaOnSucessRetorna200()
        {
            var MockCobrancaService = new Mock<ICobrancaService>();

            MockCobrancaService.Setup(service => service.RealizarCobrancaAsync(It.IsAny<decimal>(), It.IsAny<int>())).ReturnsAsync(new CobrancaViewModel()); 

            var sut = new ExternoController(_logger.Object, MockCobrancaService.Object);

            var cobrancaTeste = new CobrancaNovaViewModel
            {
                Valor = 100,
                Ciclista = 0
            };

            var result = (OkObjectResult)sut.RealizarCobranca(cobrancaTeste).Result;

            result.StatusCode.Should().Be(200);

        }

        [Fact]
        public void BuscarCobrancaOnSucessRetorna200() {

            var MockCobrancaService = new Mock<ICobrancaService>();

            MockCobrancaService.Setup(service => service.GetCobranca(It.IsAny<int>())).Returns(new CobrancaViewModel());

            var sut = new ExternoController(_logger.Object, MockCobrancaService.Object);

            var result = (OkObjectResult)sut.BuscarCobranca(0);

            result.StatusCode.Should().Be(200);

        }

        [Fact]
        public void EnviarEmailOnSucessRetorna200()
        {
            var MockCobrancaService = new Mock<ICobrancaService>();
            var MockPostmark = new Mock<PostmarkClient>();

            MockPostmark.Setup(service => service.SendMessageAsync(It.IsAny<PostmarkMessage>())).ReturnsAsync(new PostmarkResponse());

            var sut = new ExternoController(_logger.Object, MockCobrancaService.Object);

            var result = (OkObjectResult)sut.EnviarEmail(new EmailInsertViewModel()).Result;

            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public void ProcessarFilaCobrancasOnSucessRetorna200() 
        {
            var MockCobrancaService = new Mock<ICobrancaService>();

            var retornoIdeal = new List<CobrancaViewModel>();
            retornoIdeal.Add(new CobrancaViewModel());

            MockCobrancaService.Setup(service => service.ProcessarFilaCobrancas()).ReturnsAsync(retornoIdeal);

            var sut = new ExternoController(_logger.Object, MockCobrancaService.Object);

            var result = (OkObjectResult)sut.ProcessarCobrancasEmFila().Result;

            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public void ValidaCartaoOnSucessReturn200()
        {
            var MockCobrancaService = new Mock<ICobrancaService>();

            MockCobrancaService.Setup(service => service.ValidateCreditCardNumber(It.IsAny<CartaoViewModel>())).ReturnsAsync(true);

            var sut = new ExternoController(_logger.Object, MockCobrancaService.Object);

            var result = (OkResult)sut.ValidarCartaoCreditoAsync(new CartaoViewModel()).Result;

            result.StatusCode.Should().Be(200);
        }


        [Fact]
        public void ServicoAdicionaNaFilaRetornaCobranca()
        {

            var sut = new CobrancaService(_client.Object, _loggerService.Object);

            var result = sut.AdicionarCobrancaNaFila(new CobrancaViewModel());

            Assert.Equal(typeof(CobrancaViewModel), result.GetType());
        }
    }
}