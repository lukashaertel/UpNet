using System.Reflection;

namespace UpNet.Infrastructure.Run
{
    public class NopRunContext : IRunContext
    {
        public void StartAction()
        {
            // Does nothing, stub for inbound run.
        }

        public void CompleteAction(MethodInfo action, params object[] args)
        {
            // Does nothing, stub for inbound run.
        }
    }
}