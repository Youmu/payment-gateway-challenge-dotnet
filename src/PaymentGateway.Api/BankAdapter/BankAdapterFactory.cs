using PaymentGateway.Api.MounteBank;

namespace PaymentGateway.Api.BankAdapter
{
    public class BankAdapterFactory : IBankAdapterFactory
    {
        private MounteBankAdapter? Adapter { get; set; }

        public IBankAdapter GetAdapter(string bankName)
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
