using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using CustomTransportChannelLibrary;
using SharedContracts;

namespace CustomTransportChannelService
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost serviceHost = new ServiceHost(typeof(Reverse)))
            {
                serviceHost.Open();

                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.ReadLine();
            }
        }

        internal static string ProcessReflectRequest(string request)
        {
            char[] output = new char[request.Length];
            for (int index = 0; index < request.Length; index++)
            {
                output[index] = request[request.Length - index - 1];
            }
            return new string(output);
        }
    }
}
