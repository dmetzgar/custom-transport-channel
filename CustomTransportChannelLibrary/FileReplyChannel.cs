using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace CustomTransportChannelLibrary
{
    partial class FileReplyChannel : FileChannelBase, IReplyChannel
    {
        readonly EndpointAddress localAddress;
        readonly object readLock;

        public System.ServiceModel.EndpointAddress LocalAddress
        {
            get { return this.localAddress; }
        }

        public FileReplyChannel(BufferManager bufferManager, MessageEncoderFactory encoderFactory, EndpointAddress address,
            FileReplyChannelListener parent)
            : base(bufferManager, encoderFactory, address, parent, parent.MaxReceivedMessageSize)
        {
            this.localAddress = address;
            this.readLock = new object();
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();
            lock (readLock)
            {
                Message message = ReadMessage(PathToFile(LocalAddress.Uri, "request"));
                return new FileRequestContext(message, this);
            }
        }

        public RequestContext ReceiveRequest()
        {
            return ReceiveRequest(DefaultReceiveTimeout);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            context = null;
            bool complete = this.WaitForRequest(timeout);
            if (!complete)
                return false;
            context = this.ReceiveRequest(DefaultReceiveTimeout);
            return true;
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginWaitForRequest(timeout, callback, state);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            context = null;
            bool complete = this.EndWaitForRequest(result);
            if (!complete)
                return false;
            context = this.ReceiveRequest(DefaultReceiveTimeout);
            return true;
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();
            try
            {
                File.Delete(PathToFile(LocalAddress.Uri, "request"));
                using (FileSystemWatcher watcher = new FileSystemWatcher(LocalAddress.Uri.AbsolutePath, "request"))
                {
                    watcher.EnableRaisingEvents = true;
                    WaitForChangedResult result = watcher.WaitForChanged(WatcherChangeTypes.Changed, (int)timeout.TotalMilliseconds);
                    return !result.TimedOut;
                }
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfDisposedOrNotOpen();
            try
            {
                File.Delete(PathToFile(LocalAddress.Uri, "request"));
                FileSystemWatcher watcher = new FileSystemWatcher(LocalAddress.Uri.AbsolutePath, "request");
                watcher.EnableRaisingEvents = true;
                WaitForRequestAsyncResult asyncResult = new WaitForRequestAsyncResult(watcher, state, timeout);
                watcher.Changed += new FileSystemEventHandler((obj, ea) =>
                {
                    if (ea.ChangeType == WatcherChangeTypes.Changed)
                    {
                        asyncResult.Complete(false);
                        if (callback != null)
                            callback(asyncResult);
                    }
                });
                return asyncResult;
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
        }

        public bool EndWaitForRequest(IAsyncResult asyncResult)
        {
            bool result = (asyncResult as WaitForRequestAsyncResult).Result;
            (asyncResult as IDisposable).Dispose();
            return result;
        }

        class WaitForRequestAsyncResult : IAsyncResult, IDisposable
        {
            FileSystemWatcher watcher;
            ManualResetEvent waitHandle = new ManualResetEvent(false);

            public WaitForRequestAsyncResult(FileSystemWatcher watcher, object asyncState, TimeSpan timeout)
            {
                this.watcher = watcher;
                this.AsyncState = asyncState;
                if (timeout < TimeSpan.MaxValue)
                {
                    new Timer(new TimerCallback((obj) =>
                    {
                        this.Complete(true);
                    }),
                    null, (long)timeout.TotalMilliseconds, Timeout.Infinite);
                }
            }

            public object AsyncState
            {
                get;
                private set;
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return this.waitHandle; }
            }

            public bool CompletedSynchronously
            {
                get { return false; }
            }

            public bool IsCompleted
            {
                get;
                private set;
            }

            public bool Result
            {
                get;
                private set;
            }

            public void Dispose()
            {
                if (watcher != null)
                    watcher.Dispose();
            }

            public void Complete(bool timedOut)
            {
                this.waitHandle.Set();
                this.IsCompleted = true;
                this.Result = !timedOut;
            }
        }

    }
}
