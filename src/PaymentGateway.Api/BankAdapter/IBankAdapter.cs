using System.Net;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.BankAdapter
{
    public class PaymentValidationException : Exception
    {
        public PaymentValidationException(string field, string message) :
            base(message) => Field = field;
        public string Field { get; set; }
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
        Task Connect();

        bool ValidateRequest(PostPaymentRequest request);

        Task<BankResponse> Pay(string cardNumber, int expMonth, int expYear, string currency, int amount, string cvv);
    }
}
