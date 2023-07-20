
using Externo.API.Services;
using Externo.API.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PostmarkDotNet;
using PostmarkDotNet.Model;
using System.Net;
using System.Net.Mail;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

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
    public async Task<IActionResult> EnviarEmail([FromBody] EmailInsertViewModel email) {

        var message = new PostmarkMessage()
        {
            To = email.Email,
            From = "bernardo.agrelos@edu.unirio.br",
            TrackOpens = true,
            Subject = email.Assunto,
            TextBody = email.Mensagem,
            HtmlBody = email.Mensagem,
        };

        var client = new PostmarkClient("ac4655e9-9242-4bac-be28-a8d9568f9191");
        var sendResult = await client.SendMessageAsync(message);

        if (sendResult.Status == PostmarkStatus.Success) { return Ok(); }

        else return BadRequest();
    }



    [HttpPost]
    [Route("/filaCobranca")]
    public IActionResult AdicionarCobrancaNaFila([FromBody] CobrancaNovaViewModel cobranca)
    {
        _logger.LogInformation("Adicionando na fila de cobranças");

        var result = _cobrancaService.AdicionarCobrancaNaFila(new CobrancaViewModel {
            Status = "PENDENTE",
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
            var resposta = await _cobrancaService.RealizarCobrancaAsync(cobranca.Valor, cobranca.Ciclista);
            return Ok(resposta);
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

        var FilaCobrancas = await _cobrancaService.ProcessarFilaCobrancas();

        if (FilaCobrancas.Count != 0) {
            return Ok(FilaCobrancas);
        }

        return StatusCode(422);
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
