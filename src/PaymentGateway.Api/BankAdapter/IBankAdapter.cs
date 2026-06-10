using System.Net;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.BankAdapter
{
    /// <summary>
    /// Throws when payment validation fails.
    /// </summary>
    /// <param name="field">The field that failed the validation</param>
    /// <param name="message">The error message</param>
    public class PaymentValidationException(string field, string message) : Exception(message)
    {
        public string Field { get; set; } = field;
    }

    /// <summary>
    /// Throws when the bank service is unavailable.
    /// </summary>
    public class BankException: Exception
    {
        public HttpStatusCode StatusCode { get; set; }
    }

    /// <summary>
    /// The payment response from the bank
    /// </summary>
    public class BankResponse
    {
        public bool Authorized {  get; set; }
        public required string AuthorizationCode { get; set; }
    }

    /// <summary>
    /// The adapter that implements the payment protocol of the bank.
    /// </summary>
    public interface IBankAdapter
    {
        /// <summary>
        /// Validates the payment request to the validation rules.
        /// Throws PaymentValidationException on validation failure.
        /// </summary>
        /// <param name="request">The payment request.</param>
        /// <returns>Returns true if passed.</returns>
        bool ValidateRequest(PostPaymentRequest request);

        /// <summary>
        /// Process the payment
        /// </summary>
        /// <param name="id">The payment ID.</param>
        /// <param name="cardNumber">The card number</param>
        /// <param name="expMonth">Exp Month</param>
        /// <param name="expYear">Exp Year</param>
        /// <param name="currency">The Currency Code</param>
        /// <param name="amount">The amount</param>
        /// <param name="cvv">CVV code</param>
        /// <returns>Retruns the payment response from the bank.</returns>
        Task<BankResponse> Pay(Guid id, string cardNumber, int expMonth, int expYear, string currency, int amount, string cvv);
    }
}
