using PaymentGateway.Api.MounteBank;

namespace PaymentGateway.Api.BankAdapter
{
    public static class BankAdapterFactory
    {
        private static MounteBankAdapter? Adapter { get; set; }

        public static IBankAdapter GetAdapter(string bankName)
        {
            if (string.Equals(bankName, "MounteBank", StringComparison.InvariantCultureIgnoreCase))
            {
                if (Adapter == null)
                {
                    Adapter = new MounteBankAdapter();
                    Adapter.Connect();
                }
                return Adapter;
            }
            throw new NotImplementedException();
        }
    }
}
