
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
        if (email.Email != null)
        {
            mail.To.Add(email.Email);
        }
        else {
            return BadRequest();
        }

        mail.Subject = email.Assunto;
        mail.Body = email.Mensagem;

        SmtpClient smtp = new("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential("scbexterno@gmail.com", "MinhaSenhaDificil123#@!")
        };
        try {
            smtp.Send(mail);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return StatusCode(500);
        }
         

        var result = email;
        return Ok(result);
    }



    [HttpPost]
    [Route("/filaCobranca")]
    public IActionResult AdicionarCobrancaNaFila([FromBody] CobrancaNovaViewModel cobranca)
    {
        _logger.LogInformation("Adicionando na fila de cobranças");

        var result = _cobrancaService.AdicionarCobrancaNaFila(new CobrancaViewModel {
            Id = 11,
            Status = "Pendente",
            Valor = cobranca.Valor,
            Ciclista = cobranca.Ciclista,
        });

        return Ok(result);
    }

    [HttpPost]
    [Route("/cobranca")]
    public async Task<IActionResult> RealizarCobranca([FromBody] CobrancaNovaViewModel cobranca)
    {
        _logger.LogInformation("Realizando a cobrança...");
        try
        {
            var resposta = await _cobrancaService.RealizarCobrancaAsync(cobranca);
            return Ok();
        }
        catch(Exception ex) {
            _logger.LogError("Erro: ", ex.Message);
            return StatusCode(422);
        }
        

    }

    [HttpGet]
    [Route("/cobranca/{id}")]
    public IActionResult BuscarCobranca(int id)
    {
        _logger.LogInformation("Buscando cobranca");

        var cobranca = _cobrancaService.GetCobranca(id);

        if(cobranca != null)
        {
            return Ok(cobranca);
        }

        return NotFound();

    }


    [HttpPost]
    [Route("/processaCobrancasEmFila")]
    public async Task<IActionResult> ProcessarCobrancasEmFila()
    {
        _logger.LogInformation("Processando fila de cobranças...");

        return ValidationProblem();
    }

    
    [HttpPost]
    [Route("/validaCartaoDeCredito")]
    public async Task<IActionResult> ValidarCartaoCreditoAsync([FromBody] CartaoViewModel cartao)
    {
        _logger.LogInformation("Validando cartão...");

        if (await _cobrancaService.ValidateCreditCardNumber(cartao))
        {
            return Ok();
        }
        return ValidationProblem();   
    }
}
