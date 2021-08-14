using System.Collections.Generic;
using UpNet.Infrastructure.Messages;

namespace UpNet.Infrastructure.Communication
{
    public interface ICommunicator
    {
        public void Send(Message message);

        public IEnumerable<Message> ReceiveAll();
    }
}