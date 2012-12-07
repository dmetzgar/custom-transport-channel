using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CustomTransportChannelLibrary
{
    abstract class FileChannelBase : ChannelBase
    {
        const int MaxBufferSize = 64 * 1024;
        const int MaxSizeOfHeaders = 4 * 1024;

        readonly EndpointAddress address;
        readonly BufferManager bufferManager;
        readonly MessageEncoder encoder;
        readonly long maxReceivedMessageSize;

        public EndpointAddress RemoteAddress
        {
            get { return this.address; }
        }

        public FileChannelBase(BufferManager bufferManager, MessageEncoderFactory encoderFactory, EndpointAddress address, ChannelManagerBase parent,
         long maxReceivedMessageSize)
            : base(parent)
        {
            this.address = address;
            this.bufferManager = bufferManager;
            this.encoder = encoderFactory.CreateSessionEncoder();
            this.maxReceivedMessageSize = maxReceivedMessageSize;
        }

        protected static Exception ConvertException(Exception exception)
        {
            Type exceptionType = exception.GetType();
            if (exceptionType == typeof(System.IO.DirectoryNotFoundException) ||
                exceptionType == typeof(System.IO.FileNotFoundException) ||
                exceptionType == typeof(System.IO.PathTooLongException))
            {
                return new EndpointNotFoundException(exception.Message, exception);
            }
            return new CommunicationException(exception.Message, exception);
        }

        protected static string PathToFile(Uri path, String name)
        {
            UriBuilder address = new UriBuilder(path);
            address.Scheme = "file";
            address.Path = Path.Combine(path.AbsolutePath, name);
            return address.Uri.AbsolutePath;
        }

        protected override void OnAbort()
        {
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        protected Message ReadMessage(string path)
        {
            return this.BufferedReadMessage(path);
        }

        protected void WriteMessage(string path, Message message)
        {
            this.BufferedWriteMessage(path, message);
        }

        Message BufferedReadMessage(string path)
        {
            byte[] data;
            long bytesTotal;
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    bytesTotal = stream.Length;
                    if (bytesTotal > int.MaxValue)
                    {
                        throw new CommunicationException(
                           String.Format("Message of size {0} bytes is too large to buffer. Use a streamed transfer instead.", bytesTotal)
                        );
                    }
                    if (bytesTotal > this.maxReceivedMessageSize)
                    {
                        throw new CommunicationException(String.Format("Message exceeds maximum size: {0} > {1}.", bytesTotal, maxReceivedMessageSize));
                    }
                    data = this.bufferManager.TakeBuffer((int)bytesTotal);
                    int bytesRead = 0;
                    while (bytesRead < bytesTotal)
                    {
                        int count = stream.Read(data, bytesRead, (int)bytesTotal - bytesRead);
                        if (count == 0)
                        {
                            throw new CommunicationException(String.Format("Unexpected end of message after {0} of {1} bytes.", bytesRead, bytesTotal));
                        }
                        bytesRead += count;
                    }
                }
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
            ArraySegment<byte> buffer = new ArraySegment<byte>(data, 0, (int)bytesTotal);
            Message message = this.encoder.ReadMessage(buffer, this.bufferManager);
            this.bufferManager.ReturnBuffer(data);
            return message;
        }

        void BufferedWriteMessage(string path, Message message)
        {
            ArraySegment<byte> buffer;
            using (message)
            {
                this.address.ApplyTo(message);
                buffer = this.encoder.WriteMessage(message, MaxBufferSize, this.bufferManager);
            }
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    stream.Write(buffer.Array, buffer.Offset, buffer.Count);
                }
                this.bufferManager.ReturnBuffer(buffer.Array);
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
        }
    }
}
