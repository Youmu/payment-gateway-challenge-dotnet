using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using PaymentGateway.Api.BankAdapter;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

#pragma warning disable CS8602
public class PaymentsControllerTests
{
    private readonly Random _random = new();

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            AuthorizationCode = Guid.NewGuid().ToString(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999).ToString(),
            Currency = "GBP"
        };

        var mockFactory = new Mock<IBankAdapterFactory>();

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => {
                ((ServiceCollection)services).AddSingleton(paymentsRepository);
                ((ServiceCollection)services).AddSingleton(mockFactory.Object);
            }))
            .CreateClient();

        // Act
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());

        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>(options);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RejectIfValidationFails()
    {
        // Mock
        var mockFactory = new Mock<IBankAdapterFactory>();
        var mockBank = new Mock<IBankAdapter>();
        mockBank
            .Setup(b=>b.ValidateRequest(It.IsAny<PostPaymentRequest>()))
            .Throws(new PaymentValidationException("Mock","Expected"));
        mockFactory
            .Setup(f=>f.GetAdapter(It.IsAny<string>(), It.IsAny<ILogger>()))
            .Returns(mockBank.Object);
        var paymentsRepository = new PaymentsRepository();

        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => {
                ((ServiceCollection)services).AddSingleton(paymentsRepository);
                ((ServiceCollection)services).AddSingleton(mockFactory.Object);
            }))
            .CreateClient();

        // Act
        PostPaymentRequest req = new PostPaymentRequest()
        {
            CardNumber = "1234567812345678",
            ExpiryMonth = 1,
            ExpiryYear = 2099,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };
        var response = await client.PostAsJsonAsync<PostPaymentRequest>($"/api/Payments", req);
        var paymentRep = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentRep);
        Assert.Equal(PaymentStatus.Rejected, paymentRep.Status);

        // Retrieve previous payment
        var getResponse = await client.GetAsync($"/api/Payments/{paymentRep.Id}");
        var getPayment = await getResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getPayment);
        Assert.Equal(paymentRep.Status, getPayment.Status);
    }

    [Fact]
    public async Task Returns200WithAuthorized()
    {
        PostPaymentRequest req = new PostPaymentRequest()
        {
            CardNumber = "1234567812345678",
            ExpiryMonth = 1,
            ExpiryYear = 2099,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };
        // Mock
        var mockFactory = new Mock<IBankAdapterFactory>();
        var mockBank = new Mock<IBankAdapter>();

        mockBank.Setup(b => b.ValidateRequest(It.IsAny<PostPaymentRequest>()))
            .Returns(true);

        mockBank.Setup(b => b.Pay(
            It.IsAny<Guid>(),
            req.CardNumber,
            req.ExpiryMonth.Value,
            req.ExpiryYear.Value,
            req.Currency,
            req.Amount.Value,
            req.Cvv
            ))
            .ReturnsAsync(new BankResponse()
            {
                AuthorizationCode = Guid.NewGuid().ToString(),
                Authorized = true
            });

        mockFactory.Setup(f => f.GetAdapter(It.IsAny<string>(), It.IsAny<ILogger>()))
            .Returns(mockBank.Object);

        var paymentsRepository = new PaymentsRepository();

        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => {
                ((ServiceCollection)services).AddSingleton(paymentsRepository);
                ((ServiceCollection)services).AddSingleton(mockFactory.Object);
            }))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync<PostPaymentRequest>($"/api/Payments", req);
        var paymentRep = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentRep);
        Assert.Equal("5678", paymentRep.CardNumberLastFour);
        Assert.Equal(PaymentStatus.Authorized, paymentRep.Status);
        Assert.Equal(req.ExpiryMonth, paymentRep.ExpiryMonth);
        Assert.Equal(req.ExpiryYear, paymentRep.ExpiryYear);
        Assert.Equal(req.Currency, paymentRep.Currency);
        Assert.Equal(req.Amount, paymentRep.Amount);

        // Retrieve previous payment
        var getResponse = await client.GetAsync($"/api/Payments/{paymentRep.Id}");
        var getPayment = await getResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getPayment);
        Assert.Equal(paymentRep.AuthorizationCode, getPayment.AuthorizationCode);
    }


    [Fact]
    public async Task Returns200WithUnauthorized()
    {
        PostPaymentRequest req = new PostPaymentRequest()
        {
            CardNumber = "1234567812345678",
            ExpiryMonth = 1,
            ExpiryYear = 2099,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };

        // Mock
        var mockFactory = new Mock<IBankAdapterFactory>();
        var mockBank = new Mock<IBankAdapter>();

        mockBank.Setup(b => b.ValidateRequest(It.IsAny<PostPaymentRequest>()))
            .Returns(true);

        mockBank.Setup(b => b.Pay(
            It.IsAny<Guid>(),
            req.CardNumber,
            req.ExpiryMonth.Value,
            req.ExpiryYear.Value,
            req.Currency,
            req.Amount.Value,
            req.Cvv
            ))
            .ReturnsAsync(new BankResponse()
            {
                AuthorizationCode = Guid.NewGuid().ToString(),
                Authorized = false
            });

        mockFactory.Setup(f => f.GetAdapter(It.IsAny<string>(), It.IsAny<ILogger>()))
            .Returns(mockBank.Object);

        var paymentsRepository = new PaymentsRepository();

        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => {
                ((ServiceCollection)services).AddSingleton(paymentsRepository);
                ((ServiceCollection)services).AddSingleton(mockFactory.Object);
            }))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync<PostPaymentRequest>($"/api/Payments", req);
        var paymentRep = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentRep);
        Assert.Equal("5678", paymentRep.CardNumberLastFour);
        Assert.Equal(PaymentStatus.Declined, paymentRep.Status);
        Assert.Equal(req.ExpiryMonth, paymentRep.ExpiryMonth);
        Assert.Equal(req.ExpiryYear, paymentRep.ExpiryYear);
        Assert.Equal(req.Currency, paymentRep.Currency);
        Assert.Equal(req.Amount, paymentRep.Amount);

        // Retrieve previous payment
        var getResponse = await client.GetAsync($"/api/Payments/{paymentRep.Id}");
        var getPayment = await getResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(getPayment);
        Assert.Equal(paymentRep.Status, getPayment.Status);
    }
}