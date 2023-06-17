using AutoMapper;
using Externo.API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Externo.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ExternoController : ControllerBase
{
    private readonly ILogger<ExternoController> _logger;
    private readonly IMapper _mapper;

    public ExternoController(ILogger<ExternoController> logger, IMapper mapper)
    {
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost]
    [Route("/enviarEmail")]
    public IActionResult EnviarEmail([FromBody] EmailInsertViewModel email) {

        _logger.LogInformation("Enviando Email...");

        var result = _mapper.Map<EmailViewModel>(email);
        return Ok(result);

    }
}