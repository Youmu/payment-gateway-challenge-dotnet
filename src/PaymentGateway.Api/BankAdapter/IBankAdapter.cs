using System.Net;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.BankAdapter
{
    public class PaymentValidationException(string field, string message) : Exception(message)
    {
        public string Field { get; set; } = field;
    }

    public class BankException: Exception
    {
        public HttpStatusCode StatusCode { get; set; }
    }

    public class BankResponse
    {
        public bool Authorized {  get; set; }
        public required string AuthorizationCode { get; set; }
    }
    public interface IBankAdapter
    {
        bool ValidateRequest(PostPaymentRequest request);

        Task<BankResponse> Pay(Guid id, string cardNumber, int expMonth, int expYear, string currency, int amount, string cvv);
    }
}
