using AutoMapper;
using Castle.Core.Logging;
using Externo.API.Controllers;
using Externo.Tests.MockData;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.Contracts;
using Xunit;

namespace Externo.Tests;

public class TesteExternoController
{
    [Fact]
    public void EnviarEmailOnSuccessReturnStatusCode200()
    {
        var mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<ExternoController>>();

        var sut = new ExternoController(mockLogger.Object, mockMapper.Object);

        var result = (OkObjectResult)sut.EnviarEmail(EmailFake.GetEmailFake());

        result.StatusCode.Should().Be(200);

    }
}