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
            Binding binding = new FileTransportBinding();
            Uri uri = new Uri("my.file://localhost/x");
            using (ServiceHost serviceHost = new ServiceHost(typeof(Reverse)))
            {
                serviceHost.AddServiceEndpoint(typeof(IReverse), binding, uri);
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
