using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using CustomTransportChannelLibrary;

namespace CustomTransportChannelClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Binding binding = new FileTransportBinding();
            Uri uri = new Uri("my.file://localhost/x");
            ReverseClient client = new ReverseClient(binding, new EndpointAddress(uri));

            while (true)
            {
                Console.Write("Enter some text (Ctrl-Z to quit): ");
                String text = Console.ReadLine();
                if (text == null)
                    break;
                string response = client.ReverseString(text);
                Console.WriteLine("Reply: {0}", response);
            }

            client.Close();
        }
    }
}
