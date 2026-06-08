using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.BankAdapter;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;


[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly IBankAdapterFactory _bankAdapterFactory;

    public PaymentsController(PaymentsRepository paymentsRepository, IBankAdapterFactory bankAdapterFactory)
    {
        _paymentsRepository = paymentsRepository;
        _bankAdapterFactory = bankAdapterFactory;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);
        if (payment == null)
        {
            return new NotFoundResult();
        }
        return new OkObjectResult(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse?>> PostPaymentAsync(PostPaymentRequest request)
    {
        var paymentId = Guid.NewGuid();
        PostPaymentResponse postPaymentResponse;
        try
        {
            var adapter = _bankAdapterFactory.GetAdapter("MounteBank");
            adapter.ValidateRequest(request);
#pragma warning disable CS8604, CS8629
            var result = await adapter.Pay(
                request.CardNumber,
                request.ExpiryMonth.Value,
                request.ExpiryYear.Value,
                request.Currency,
                request.Amount.Value,
                request.Cvv
                );
#pragma warning restore CS8604, CS8629
            postPaymentResponse = new PostPaymentResponse()
            {
                Id = paymentId,
                AuthorizationCode = result.AuthorizationCode,
                Status = result.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
                CardNumberLastFour = request.CardNumber[^4..],
                ExpiryMonth = request.ExpiryMonth.Value,
                ExpiryYear = request.ExpiryYear.Value,
                Currency = request.Currency,
                Amount = request.Amount.Value,
            };
        }
        catch (PaymentValidationException ex)
        {
            postPaymentResponse = new PostPaymentResponse()
            {
                Id = paymentId,
                AuthorizationCode = Guid.Empty.ToString(),
                Status = PaymentStatus.Rejected,
                CardNumberLastFour = string.IsNullOrEmpty(request.CardNumber) ? "" : request.CardNumber[^4..],
                ExpiryMonth = request.ExpiryMonth ?? 0,
                ExpiryYear = request.ExpiryYear ?? 0,
                Currency = string.IsNullOrEmpty(request.Currency) ? "" : request.Currency[..3],
                Amount = request.Amount ?? 0,
            };
        }
        catch (BankException ex)
        {
            postPaymentResponse = new PostPaymentResponse()
            {
                Id = paymentId,
                AuthorizationCode = Guid.Empty.ToString(),
                Status = PaymentStatus.Rejected,
                CardNumberLastFour = string.IsNullOrEmpty(request.CardNumber) ? "" : request.CardNumber[^4..],
                ExpiryMonth = request.ExpiryMonth ?? 0,
                ExpiryYear = request.ExpiryYear ?? 0,
                Currency = string.IsNullOrEmpty(request.Currency) ? "" : request.Currency[..3],
                Amount = request.Amount ?? 0,
            };
        }
        _paymentsRepository.Add(postPaymentResponse);
        return new OkObjectResult(postPaymentResponse);
    }
}