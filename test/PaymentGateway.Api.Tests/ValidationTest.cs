using PaymentGateway.Api.BankAdapter;
using PaymentGateway.Api.MounteBank;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests;

public class ValidationTest
{
    private readonly MounteBankAdapter _validator = new();

    private static PostPaymentRequest CreateValidRequest() => new()
    {
        CardNumber = "4242424242424242",
        ExpiryMonth = DateTime.UtcNow.Month == 12 ? 1 : DateTime.UtcNow.Month + 1,
        ExpiryYear = DateTime.UtcNow.Month == 12 ? DateTime.UtcNow.Year + 1 : DateTime.UtcNow.Year,
        Currency = "GBP",
        Amount = 100,
        Cvv = 123
    };

    private static void AssertValidationFails(string expectedField, Action validate)
    {
        var ex = Assert.Throws<PaymentValidationException>(validate);
        Assert.Equal(expectedField, ex.Field);
    }

    [Fact]
    public void ValidateRequest_ReturnsTrue_WhenRequestIsValid()
    {
        var result = _validator.ValidateRequest(CreateValidRequest());

        Assert.True(result);
    }

    [Fact]
    public void ValidateRequest_ReturnsTrue_WhenCardNumberIs14Characters()
    {
        var request = CreateValidRequest();
        request.CardNumber = "42424242424242";

        Assert.True(_validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_ReturnsTrue_WhenCardNumberIs19Characters()
    {
        var request = CreateValidRequest();
        request.CardNumber = "4242424242424242424";

        Assert.True(_validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_ReturnsTrue_WhenCardNumberHasLeadingAndTrailingWhitespace()
    {
        var request = CreateValidRequest();
        request.CardNumber = "  4242424242424242  ";

        Assert.True(_validator.ValidateRequest(request));
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("gbp")]
    [InlineData("CNY")]
    [InlineData("cny")]
    [InlineData("EUR")]
    [InlineData("eur")]
    public void ValidateRequest_ReturnsTrue_WhenCurrencyIsSupportedCaseInsensitive(string currency)
    {
        var request = CreateValidRequest();
        request.Currency = currency;

        Assert.True(_validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_ReturnsTrue_WhenCurrencyHasLeadingAndTrailingWhitespace()
    {
        var request = CreateValidRequest();
        request.Currency = "  GBP  ";

        Assert.True(_validator.ValidateRequest(request));
    }

    [Theory]
    [InlineData(123)]
    [InlineData(1234)]
    public void ValidateRequest_ReturnsTrue_WhenCvvHasValidLength(int cvv)
    {
        var request = CreateValidRequest();
        request.Cvv = cvv;

        Assert.True(_validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenRequestIsNull()
    {
        AssertValidationFails("Request", () => _validator.ValidateRequest(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRequest_Throws_WhenCardNumberIsMissing(string? cardNumber)
    {
        // 1.1 Required
        var request = CreateValidRequest();
        request.CardNumber = cardNumber;

        AssertValidationFails("CardNumber", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenCardNumberIsTooShort()
    {
        // 1.2 The characters count MUST >= 14 and <= 19
        var request = CreateValidRequest();
        request.CardNumber = "4242424242424";

        AssertValidationFails("CardNumber", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenCardNumberIsTooLong()
    {
        // 1.2 The characters count MUST >= 14 and <= 19
        var request = CreateValidRequest();
        request.CardNumber = "42424242424242424242";

        AssertValidationFails("CardNumber", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenCardNumberContainsNonNumericCharacters()
    {
        // 1.3 MUST only contain numeric characters
        var request = CreateValidRequest();
        request.CardNumber = "42424242424242A2";

        AssertValidationFails("CardNumber", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenExpiryMonthIsMissing()
    {
        // 2.1 Required
        var request = CreateValidRequest();
        request.ExpiryMonth = null;

        AssertValidationFails("ExpiryMonth", () => _validator.ValidateRequest(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void ValidateRequest_Throws_WhenExpiryMonthIsOutOfRange(int expiryMonth)
    {
        // 2.2 Value must between 1 - 12
        var request = CreateValidRequest();
        request.ExpiryMonth = expiryMonth;

        AssertValidationFails("ExpiryMonth", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenExpiryYearIsMissing()
    {
        // 3.1 Required
        var request = CreateValidRequest();
        request.ExpiryYear = null;

        AssertValidationFails("ExpiryYear", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenExpiryDateIsInPastYear()
    {
        // 3.2 The date ExpiryMonth/ExpiryYear MUST be in the future. The current month is excluded.
        var request = CreateValidRequest();
        request.ExpiryYear = DateTime.UtcNow.Year - 1;
        request.ExpiryMonth = 12;

        AssertValidationFails("ExpiryYear", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenExpiryDateIsCurrentMonth()
    {
        // 3.2 The date ExpiryMonth/ExpiryYear MUST be in the future. The current month is excluded.
        var request = CreateValidRequest();
        request.ExpiryYear = DateTime.UtcNow.Year;
        request.ExpiryMonth = DateTime.UtcNow.Month;

        AssertValidationFails("ExpiryYear", () => _validator.ValidateRequest(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRequest_Throws_WhenCurrencyIsMissing(string? currency)
    {
        // 4.1 Required
        var request = CreateValidRequest();
        request.Currency = currency;

        AssertValidationFails("Currency", () => _validator.ValidateRequest(request));
    }

    [Theory]
    [InlineData("GB")]
    [InlineData("GBPP")]
    public void ValidateRequest_Throws_WhenCurrencyLengthIsNotThreeCharacters(string currency)
    {
        // 4.1 MUST have exactly 3 characters.
        var request = CreateValidRequest();
        request.Currency = currency;

        AssertValidationFails("Currency", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenCurrencyIsNotSupported()
    {
        // 4.2 MUST be in the SupportedCurrencies list, case insensitive.
        var request = CreateValidRequest();
        request.Currency = "USD";

        AssertValidationFails("Currency", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenAmountIsMissing()
    {
        // 5.1 Required
        var request = CreateValidRequest();
        request.Amount = null;

        AssertValidationFails("Amount", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenCvvIsMissing()
    {
        // 5.1 Required
        var request = CreateValidRequest();
        request.Cvv = null;

        AssertValidationFails("Cvv", () => _validator.ValidateRequest(request));
    }

    [Theory]
    [InlineData(12)]
    [InlineData(1)]
    public void ValidateRequest_Throws_WhenCvvIsTooShort(int cvv)
    {
        // 5.2 The characters count MUST >= 3 and <= 4
        var request = CreateValidRequest();
        request.Cvv = cvv;

        AssertValidationFails("Cvv", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenCvvIsTooLong()
    {
        // 5.2 The characters count MUST >= 3 and <= 4
        var request = CreateValidRequest();
        request.Cvv = 12345;

        AssertValidationFails("Cvv", () => _validator.ValidateRequest(request));
    }

    [Fact]
    public void ValidateRequest_Throws_WhenCvvContainsNonNumericCharacters()
    {
        // 5.3 MUST only contain numeric characters
        var request = CreateValidRequest();
        request.Cvv = -123;

        AssertValidationFails("Cvv", () => _validator.ValidateRequest(request));
    }
}
