using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace CustomTransportChannelLibrary
{
    partial class FileReplyChannel
    {
        class FileRequestContext : RequestContext
        {
            bool aborted;
            readonly Message message;
            readonly FileReplyChannel parent;
            CommunicationState state;
            readonly object thisLock;
            readonly object writeLock;

            public override Message RequestMessage
            {
                get { return this.message; }
            }

            public FileRequestContext(Message message, FileReplyChannel parent)
            {
                this.aborted = false;
                this.message = message;
                this.parent = parent;
                this.state = CommunicationState.Opened;
                this.thisLock = new object();
                this.writeLock = new object();
            }

            public override void Abort()
            {
                lock (thisLock)
                {
                    if (this.aborted)
                    {
                        return;
                    }
                    this.aborted = true;
                    this.state = CommunicationState.Faulted;
                }
            }

            public override void Close(TimeSpan timeout)
            {
                lock (thisLock)
                {
                    this.state = CommunicationState.Closed;
                }
            }

            public override void Close()
            {
                this.Close(this.parent.DefaultCloseTimeout);
            }

            public override void Reply(Message message, TimeSpan timeout)
            {
                lock (thisLock)
                {
                    if (this.aborted)
                    {
                        throw new CommunicationObjectAbortedException();
                    }
                    if (this.state == CommunicationState.Faulted)
                    {
                        throw new CommunicationObjectFaultedException();
                    }
                    if (this.state == CommunicationState.Closed)
                    {
                        throw new ObjectDisposedException("this");
                    }
                }
                this.parent.ThrowIfDisposedOrNotOpen();
                lock (writeLock)
                {
                    this.parent.WriteMessage(FileChannelBase.PathToFile(this.parent.LocalAddress.Uri, "reply"), message);
                }
            }

            public override void Reply(Message message)
            {
                this.Reply(message, this.parent.DefaultSendTimeout);
            }

            public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public override void EndReply(IAsyncResult result)
            {
                throw new NotImplementedException();
            }
        }
    }
}