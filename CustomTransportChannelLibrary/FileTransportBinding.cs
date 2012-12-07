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
