using PaymentGateway.Api.MounteBank;

namespace PaymentGateway.Api.BankAdapter
{
    public class BankAdapterFactory : IBankAdapterFactory
    {
        private MounteBankAdapter? Adapter { get; set; }

        public BankAdapterFactory()
        {
        }

        public IBankAdapter GetAdapter(string bankName, ILogger logger)
        {
            if (string.Equals(bankName, "MounteBank", StringComparison.InvariantCultureIgnoreCase))
            {
                Adapter ??= new MounteBankAdapter(logger);
                return Adapter;
            }
            throw new NotImplementedException();
        }
    }
}
