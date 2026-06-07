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

    public PaymentsController(PaymentsRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository;
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
            var adapter = BankAdapterFactory.GetAdapter("MounteBank");
            adapter.ValidateRequest(request);
#pragma warning disable CS8604, CS8629
            var result = await adapter.Pay(
                request.CardNumber,
                request.ExpiryMonth.Value,
                request.ExpiryYear.Value,
                request.Currency,
                request.Amount.Value,
                request.Cvv.ToString()
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
            return new BadRequestObjectResult(string.Format("{0}: {1}", ex.Field, ex.Message));
        }
        catch (BankException ex)
        {
            return new StatusCodeResult((int)ex.StatusCode);
        }
        _paymentsRepository.Add(postPaymentResponse);
        return new OkObjectResult(postPaymentResponse);
    }
}