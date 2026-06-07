namespace PaymentGateway.Api.BankAdapter
{
    public interface IBankAdapterFactory
    {
        public IBankAdapter GetAdapter(string bankName);
    }
}
