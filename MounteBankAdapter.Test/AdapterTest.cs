using System.Net;

using Microsoft.Extensions.Logging.Abstractions;

using PaymentGateway.Api.BankAdapter;
using PaymentGateway.Api.MounteBank;

namespace MounteBank.Test
{
    public class AdapterTest
    {
        [Fact]
        public async void PaymentApprovedTest()
        {
            var bankAdapter = new MounteBankAdapter(NullLogger<AdapterTest>.Instance);
            var result = await bankAdapter.Pay(
                    Guid.NewGuid(),
                    "1001324581621231", 
                    12,
                    2099,
                    "GBP",
                    10,
                    "021"
                );
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AuthorizationCode));
            Assert.True(result.Authorized);
        }


        [Fact]
        public async void PaymentDeniedTest()
        {
            var bankAdapter = new MounteBankAdapter(NullLogger<AdapterTest>.Instance);
            var result = await bankAdapter.Pay(
                    Guid.NewGuid(),
                    "1001324581621232",
                    12,
                    2099,
                    "GBP",
                    10,
                    "021"
                );
            Assert.NotNull(result);
            Assert.False(result.Authorized);
        }

        [Fact]
        public async void ServiceUnavailableTest()
        {
            var bankAdapter = new MounteBankAdapter(NullLogger<AdapterTest>.Instance);
            var ex = await Assert.ThrowsAsync<BankException>(async () =>
            {
                var result = await bankAdapter.Pay(
                        Guid.NewGuid(),
                        "1001324581621230",
                        12,
                        2099,
                        "GBP",
                        10,
                        "021"
                    );
            });
            Assert.NotNull(ex);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        }
    }
}