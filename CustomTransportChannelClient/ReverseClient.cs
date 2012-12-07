using System.ServiceModel;
using System.ServiceModel.Channels;
using SharedContracts;

namespace CustomTransportChannelClient 
{
    class ReverseClient : ClientBase<IReverse>, IReverse
    {
        public ReverseClient(Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        public string ReverseString(string text)
        {
            return base.Channel.ReverseString(text);
        }
    }
}
