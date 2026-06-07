using PaymentGateway.Api.BankAdapter;
using PaymentGateway.Api.Models.Requests;

using System.Linq;

namespace PaymentGateway.Api.MounteBank
{
    public partial class MounteBankAdapter : IBankAdapter
    {
        public static readonly string[] SupportedCurrencies = ["GBP", "CNY", "EUR"];

        public bool ValidateRequest(PostPaymentRequest request)
        {
            if (request is null)
            {
                throw new PaymentValidationException("Request", "Request is required.");
            }

            // 1.1 Required
            var cardNumber = request.CardNumber?.Trim();
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                throw new PaymentValidationException("CardNumber", "CardNumber is required.");
            }

            // 1.2 The characters count MUST >= 14 and <= 19
            if (cardNumber.Length < 14 || cardNumber.Length > 19)
            {
                throw new PaymentValidationException("CardNumber", "CardNumber must be between 14 and 19 characters.");
            }

            // 1.3 MUST only contain numeric characters
            if (!cardNumber.All(char.IsDigit))
            {
                throw new PaymentValidationException("CardNumber", "CardNumber must contain only numeric characters.");
            }

            // 2.1 Required
            if (!request.ExpiryMonth.HasValue)
            {
                throw new PaymentValidationException("ExpiryMonth", "ExpiryMonth is required.");
            }

            // 2.2 Value must between 1 - 12
            if (request.ExpiryMonth < 1 || request.ExpiryMonth > 12)
            {
                throw new PaymentValidationException("ExpiryMonth", "ExpiryMonth must be between 1 and 12.");
            }

            // 3.1 Required
            if (!request.ExpiryYear.HasValue)
            {
                throw new PaymentValidationException("ExpiryYear", "ExpiryYear is required.");
            }

            // 3.2 The date ExpiryMonth/ExpiryYear MUST be in the future. The current month is excluded.
            var now = DateTime.UtcNow;
            if (request.ExpiryYear < now.Year || (request.ExpiryYear == now.Year && request.ExpiryMonth <= now.Month))
            {
                throw new PaymentValidationException("ExpiryYear", "Expiry date must be in the future.");
            }

            // 4.1 Required
            var currency = request.Currency?.Trim();
            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new PaymentValidationException("Currency", "Currency is required.");
            }

            // 4.1 MUST have exactly 3 characters.
            if (currency.Length != 3)
            {
                throw new PaymentValidationException("Currency", "Currency must be exactly 3 characters.");
            }

            // 4.2 MUST be in the SupportedCurrencies list, case insensitive.
            if (!SupportedCurrencies.Any(c => string.Equals(c, currency, StringComparison.OrdinalIgnoreCase)))
            {
                throw new PaymentValidationException("Currency", "Currency is not supported.");
            }

            // 5.1 Required
            if (!request.Amount.HasValue)
            {
                throw new PaymentValidationException("Amount", "Amount is required.");
            }

            // 5.1 Required
            if (!request.Cvv.HasValue)
            {
                throw new PaymentValidationException("Cvv", "Cvv is required.");
            }

            var cvvText = request.Cvv.Value.ToString();

            // 5.2 The characters count MUST >= 3 and <= 4
            if (cvvText.Length < 3 || cvvText.Length > 4)
            {
                throw new PaymentValidationException("Cvv", "Cvv must be between 3 and 4 digits.");
            }

            // 5.3 MUST only contain numeric characters
            if (!cvvText.All(char.IsDigit))
            {
                throw new PaymentValidationException("Cvv", "Cvv must contain only numeric characters.");
            }

            return true;
        }
    }
}
