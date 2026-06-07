namespace PaymentGateway.Api.Models.Responses;

using System.Text.Json.Serialization;

public class PostPaymentResponse
{
    public Guid Id { get; set; }
    public string AuthorizationCode { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentStatus Status { get; set; }
    public string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}
