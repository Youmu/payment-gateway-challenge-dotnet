using PaymentGateway.Api.BankAdapter;
using PaymentGateway.Api.Models.Requests;

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

using Microsoft.Extensions.Logging;


namespace PaymentGateway.Api.MounteBank
{
    static class MounteBankConfig
    {

        public static Uri GetBankEndpoint()
        {
            var endpoint = Environment.GetEnvironmentVariable("MOUNTEBANK_ENDPOINT");
            return new Uri(string.IsNullOrWhiteSpace(endpoint)
                ? "http://127.0.0.1:8080/payments"
                : endpoint);
        }
    }

    class MounteBankResponse
    {
        public bool authorized { get; set; }
        public required string authorization_code { get; set; }
    }

    public partial class MounteBankAdapter(ILogger logger) : IBankAdapter
    {
        readonly HttpClient _httpClient = new();
        readonly Uri _endpoint = MounteBankConfig.GetBankEndpoint();
        readonly ILogger _logger = logger;

        public async Task<BankResponse> Pay(Guid id, string cardNumber, int expMonth, int expYear, string currency, int amount, string cvv)
        {
            var paymentData = new
            {

                card_number = cardNumber,
                expiry_date = string.Format("{0:D2}/{1:D4}", expMonth, expYear),
                currency = currency,
                amount = amount,
                cvv = cvv
            };
            _logger.LogInformation("Payment ID:{Id} submitted to MounteBank bank.", id);
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(_endpoint, paymentData);
            }
            catch (Exception ex)
            {
                _logger.LogError("Payment ID:{Id} submission failed: {message}", id, ex.Message);
                throw new BankException()
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable
                };
            }
            if ( response.IsSuccessStatusCode ) {
                var mountBankResponse = await response.Content.ReadFromJsonAsync<MounteBankResponse>();
                return new BankResponse()
                {
                    AuthorizationCode = mountBankResponse.authorization_code,
                    Authorized = mountBankResponse.authorized
                };
            };
            throw new BankException()
            {
                StatusCode = response.StatusCode
            };
        }
    }
}
