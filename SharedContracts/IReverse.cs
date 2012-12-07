using System.ServiceModel;

namespace SharedContracts
{
    [ServiceContract]
    public interface IReverse
    {
        [OperationContract]
        string ReverseString(string text);
    }
}
