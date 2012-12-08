using System.Configuration;
using System.Globalization;
using System.ServiceModel.Channels;

namespace CustomTransportChannelLibrary
{
    public class FileTransportBinding : Binding
    {
        readonly MessageEncodingBindingElement messageElement;
        readonly FileTransportBindingElement transportElement;

        public FileTransportBinding()
        {
            this.messageElement = new TextMessageEncodingBindingElement();
            this.transportElement = new FileTransportBindingElement();
        }

        public FileTransportBinding(string configurationName)
            : this()
        {
            FileTransportBindingCollectionElement section = (FileTransportBindingCollectionElement)ConfigurationManager.GetSection(
                "system.serviceModel/bindings/fileTransportBinding");
            FileTransportBindingConfigurationElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture,
                    "There is no binding named {0} at {1}.", configurationName, section.BindingName));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        public override BindingElementCollection CreateBindingElements()
        {
            return new BindingElementCollection(new BindingElement[] {
                this.messageElement,
                this.transportElement
            });
        }

        public override string Scheme
        {
            get { return this.transportElement.Scheme; }
        }
    }
}
