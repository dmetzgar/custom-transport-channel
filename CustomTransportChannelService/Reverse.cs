using SharedContracts;

namespace CustomTransportChannelService
{
    class Reverse : IReverse
    {
        public string ReverseString(string text)
        {
            return Program.ProcessReflectRequest(text);
        }
    }
}
