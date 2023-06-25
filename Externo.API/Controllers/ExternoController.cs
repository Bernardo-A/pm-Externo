using AutoMapper;
using Externo.API.Services;
using Externo.API.ViewModels;
using Microsoft.AspNetCore.Mvc;

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

    //[HttpPost]
    //[Route("/enviarEmail")]
    //public IActionResult EnviarEmail([FromBody] EmailInsertViewModel email) {

    //    _logger.LogInformation("Enviando Email...");

    //    var result = Map<EmailViewModel>(email);
    //    return Ok(result);
    //}



    [HttpPost]
    [Route("/filaCobranca")]
    public IActionResult AdicionarCobrancaNaFila([FromBody] CobrancaNova cobranca)
    {

        _logger.LogInformation("Adicionando na fila de cobranças");

        var result = _cobrancaService.AdicionarCobrancaNaLista(cobranca);

        return Ok(result);

    }

}
