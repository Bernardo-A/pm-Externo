
using Externo.API.Services;
using Externo.API.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace Externo.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ExternoController : ControllerBase
{
    private readonly ILogger<ExternoController> _logger;
    private readonly ICobrancaService _cobrancaService;

    public ExternoController(ILogger<ExternoController> logger, ICobrancaService cobrancaService)
    {
        _logger = logger;
        _cobrancaService = cobrancaService;
    }

    [HttpPost]
    [Route("/enviarEmail")]
    public IActionResult EnviarEmail([FromBody] EmailInsertViewModel email) {

        _logger.LogInformation("Enviando Email...");

        MailMessage mail = new()
        { 
            From = new MailAddress("scbexterno@gmail.com")
        };

        mail.To.Add(email.Email);


        mail.Subject = email.Assunto;
        mail.Body = email.Mensagem;

        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        
        smtp.EnableSsl = true;
        smtp.UseDefaultCredentials = false;
        smtp.Credentials = new NetworkCredential("scbexterno@gmail.com", "MinhaSenhaDificil123#@!");
        try {
            smtp.Send(mail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
         

        var result = email;
        return Ok(result);
    }



    [HttpPost]
    [Route("/filaCobranca")]
    public IActionResult AdicionarCobrancaNaFila([FromBody] CobrancaNovaViewModel cobranca)
    {

        _logger.LogInformation("Adicionando na fila de cobranças");

        var result = _cobrancaService.AdicionarCobrancaNaFila(cobranca);

        return Ok(result);

    }

    [HttpPost]
    [Route("/cobranca")]
    public IActionResult RealizarCobranca([FromBody] CobrancaNovaViewModel cobranca)
    {
        _logger.LogInformation("Realizando a cobrança...");

        var cartao = _cobrancaService.GetCartao(cobranca.Ciclista);


        

        if (cartao.Numero != null && _cobrancaService.ValidateCreditCardNumber(cartao.Numero)) {
            _cobrancaService.RealizarCobrancaAsync(cartao, cobranca.Valor);
            var result = _cobrancaService.RegistrarCobranca(cobranca, cartao);
            return Ok(result);
        }
        return BadRequest();
    }

    [HttpGet]
    [Route("/cobranca/{id}")]
    public IActionResult BuscarCobranca(int id)
    {
        _logger.LogInformation("Buscando cobranca");

        if (_cobrancaService.GetCobranca(id) != null)
        {

            var cobranca = _cobrancaService.GetCobranca(id);
            return Ok(cobranca);
        }

        return NotFound();

    }


    [HttpPost]
    [Route("/processaCobrancasEmFila")]
    public IActionResult ProcessarCobrancasEmFila()
    {
        _logger.LogInformation("Processando fila de cobranças...");

        Queue<CobrancaViewModel> filaCobrancas = _cobrancaService.BuscarCobrancasDaFila();

        while (filaCobrancas.Count > 0) {
            var cobranca = filaCobrancas.Dequeue();
            var cartao = _cobrancaService.GetCartao(cobranca.Ciclista);

            if (cartao.Numero != null && _cobrancaService.ValidateCreditCardNumber(cartao.Numero))
            {
                _cobrancaService.RealizarCobrancaAsync(cartao, cobranca.Valor);
                return Ok();
            }

        }

        return ValidationProblem();
    }

    
    [HttpPost]
    [Route("/validaCartaoDeCredito")]
    public IActionResult ValidarCartaoCredito([FromBody] CartaoViewModel cartao)
    {
        _logger.LogInformation("Validando cartão...");

        if (cartao.Numero != null && _cobrancaService.ValidateCreditCardNumber(cartao.Numero))
        {
            return Ok();
        }
        return ValidationProblem();   
           
    }


}
