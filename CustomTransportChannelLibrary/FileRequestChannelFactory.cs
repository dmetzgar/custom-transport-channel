using System;
using System.ServiceModel.Channels;

namespace CustomTransportChannelLibrary
{
    class FileRequestChannelFactory : ChannelFactoryBase<IRequestChannel>
    {
        public readonly long MaxReceivedMessageSize;
        readonly BufferManager bufferManager;
        readonly MessageEncoderFactory encoderFactory;

        public FileRequestChannelFactory(FileTransportBindingElement transportElement, BindingContext context)
            : base(context.Binding)
        {
            MessageEncodingBindingElement messageElement = context.BindingParameters.Remove<MessageEncodingBindingElement>();
            this.MaxReceivedMessageSize = transportElement.MaxReceivedMessageSize;
            this.bufferManager = BufferManager.CreateBufferManager(transportElement.MaxBufferPoolSize, (int)this.MaxReceivedMessageSize);
            this.encoderFactory = messageElement.CreateMessageEncoderFactory();
        }

        protected override IRequestChannel OnCreateChannel(System.ServiceModel.EndpointAddress address, Uri via)
        {
            return new FileRequestChannel(this.bufferManager, this.encoderFactory, address, this, via);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }
    }
}
