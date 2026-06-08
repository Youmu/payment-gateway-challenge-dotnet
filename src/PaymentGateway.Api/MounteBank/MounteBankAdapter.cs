using PaymentGateway.Api.BankAdapter;
using PaymentGateway.Api.Models.Requests;

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;


namespace PaymentGateway.Api.MounteBank
{
    static class MounteBankConfig
    {

        public static Uri GetBankEndpoint()
        {
            return new Uri("http://172.17.221.16:8080/payments");
        }
    }

    class MounteBankResponse
    {
        public bool authorized { get; set; }
        public required string authorization_code { get; set; }
    }

    public partial class MounteBankAdapter : IBankAdapter
    {
        readonly HttpClient _httpClient;
        readonly Uri _endpoint;

        public MounteBankAdapter() {
            _httpClient = new HttpClient();
            _endpoint = MounteBankConfig.GetBankEndpoint();
        }

        public async Task<BankResponse> Pay(string cardNumber, int expMonth, int expYear, string currency, int amount, string cvv)
        {
            var paymentData = new
            {

                card_number = cardNumber,
                expiry_date = string.Format("{0:D2}/{1:D4}", expMonth, expYear),
                currency = currency,
                amount = amount,
                cvv = cvv
            };
            var response = await _httpClient.PostAsJsonAsync( _endpoint, paymentData);
            if( response.IsSuccessStatusCode ) {
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
