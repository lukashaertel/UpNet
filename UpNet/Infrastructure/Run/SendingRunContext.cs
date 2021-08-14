using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using UpNet.Data;
using UpNet.Infrastructure.Communication;
using UpNet.Infrastructure.Db;
using UpNet.Infrastructure.Messages;
using UpNet.Infrastructure.Proxy;
using UpNet.Infrastructure.Undo;

namespace UpNet.Infrastructure.Run
{
    public class SendingRunContext : IRunContext
    {
        private IContextAccessor ContextAccessor { get; }

        private IProxyGenerator ProxyGenerator { get; }

        private IContextUndoGenerator UndoGenerator { get; }

        private IMessageRepository MessageRepository { get; }

        private ICommunicator Communicator { get; }

        public Instant RunningAsInstant { get; set; } = Instant.MaxValue;

        public SendingRunContext(
            IContextAccessor contextAccessor,
            IProxyGenerator proxyGenerator,
            IContextUndoGenerator undoGenerator,
            IMessageRepository messageRepository,
            ICommunicator communicator)
        {
            ContextAccessor = contextAccessor;
            ProxyGenerator = proxyGenerator;
            UndoGenerator = undoGenerator;
            MessageRepository = messageRepository;
            Communicator = communicator;
        }

        private int Depth { get; set; }

        private IPrincipal? OriginalPrincipal { get; set; }

        public void StartAction()
        {
            if (0 != Depth++)
                return;

            // Assign principal. TODO ??
            OriginalPrincipal = Thread.CurrentPrincipal;
            Thread.CurrentPrincipal = new GenericPrincipal(new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.Name, RunningAsInstant.Player.ToString()),
                new(ClaimTypes.AuthenticationInstant, RunningAsInstant.At.ToString(CultureInfo.InvariantCulture)),
            }, "Local"), null);
        }

        public void CompleteAction(MethodInfo action, params object[] args)
        {
            // Decrement depth, only action on returning to initial depth.
            if (0 != --Depth)
                return;

            // Get relevant parameters from dependencies.
            var instant = RunningAsInstant;
            var argumentProxies = ProxyGenerator.GenerateProxyObjects(args);
            var undo = UndoGenerator.CreateUndo(ContextAccessor.Context);
            var message = new Message(instant, action, argumentProxies, undo);

            // Insert message, communicate action.
            MessageRepository.Insert(message);
            Communicator.Send(message);

            // Reset principal. TODO ??
            Thread.CurrentPrincipal = OriginalPrincipal;
            OriginalPrincipal = null;
        }
    }
}