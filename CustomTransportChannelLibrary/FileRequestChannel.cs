using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace CustomTransportChannelLibrary
{
    class FileRequestChannel : FileChannelBase, IRequestChannel
    {
        readonly Uri via;
        readonly object writeLock;

        public Uri Via
        {
            get { return this.via; }
        }

        public FileRequestChannel(BufferManager bufferManager, MessageEncoderFactory encoderFactory, EndpointAddress address,
            FileRequestChannelFactory parent, Uri via)
            : base(bufferManager, encoderFactory, address, parent, parent.MaxReceivedMessageSize)
        {
            this.via = via;
            this.writeLock = new object();
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();
            lock (this.writeLock)
            {
                try
                {
                    File.Delete(PathToFile(Via, "reply"));
                    using (FileSystemWatcher watcher = new FileSystemWatcher(Via.AbsolutePath, "reply"))
                    {
                        ManualResetEvent replyCreated = new ManualResetEvent(false);
                        watcher.Changed += new FileSystemEventHandler(
                           delegate(object sender, FileSystemEventArgs e) { replyCreated.Set(); }
                        );
                        watcher.EnableRaisingEvents = true;
                        WriteMessage(PathToFile(via, "request"), message);
                        if (!replyCreated.WaitOne(timeout, false))
                        {
                            throw new TimeoutException(timeout.ToString());
                        }
                    }
                }
                catch (IOException exception)
                {
                    throw ConvertException(exception);
                }
                return ReadMessage(PathToFile(Via, "reply"));
            }
        }

        public Message Request(Message message)
        {
            return this.Request(message, DefaultReceiveTimeout);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public Message EndRequest(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
    }
}
