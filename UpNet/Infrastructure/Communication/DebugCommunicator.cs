using System;
using System.Collections.Generic;
using System.Linq;
using UpNet.Infrastructure.Controller;
using UpNet.Infrastructure.Messages;

namespace UpNet.Infrastructure.Communication
{
    public class DebugCommunicator : ICommunicator
    {
        private IControllerLocator ControllerLocator { get; }

        public DebugCommunicator(IControllerLocator controllerLocator) =>
            ControllerLocator = controllerLocator;

        public void Send(Message message)
        {
            var actionName = ControllerLocator.ActionToName(message.Action);
            var args = string.Join(", ", message.ArgumentProxies);
            Console.WriteLine($"Sending {actionName}({args})");
        }

        public IEnumerable<Message> ReceiveAll() =>
            Enumerable.Empty<Message>();
    }
}