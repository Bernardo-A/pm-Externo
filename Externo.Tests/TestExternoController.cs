using AutoMapper;
using Castle.Core.Logging;
using Externo.API.Controllers;
using Externo.API.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.Contracts;
using Xunit;

namespace Externo.Tests;

public class TestExternoController
{
    private readonly Mock<ILogger<ExternoController>> _logger = new();
    private readonly Mock<IMapper> _mapper = new();

    [Fact]
    public void EnviarEmailOnSuccessReturnStatusCode200()
    {
        var sut = new ExternoController(_logger.Object, _mapper.Object);

        var result = (OkObjectResult)sut.EnviarEmail(new EmailInsertViewModel
        {
            Email = "meuEmail@email.com",
            Assunto = "Assunto Interessante",
            Mensagem = "Olá"
        });

        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public void TestSetEmailViewModel()
    {
        var emailViewModel = new EmailViewModel();

        emailViewModel = new EmailViewModel
        {
            Id = 0,
            Email = "meuEmail@email.com",
            Assunto = "Assunto Interessante",
            Mensagem = "Olá"
        };
        
        emailViewModel.Should().BeOfType<EmailViewModel>();
    }
}