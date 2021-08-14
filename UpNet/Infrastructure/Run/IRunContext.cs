using System.Reflection;

namespace UpNet.Infrastructure.Run
{
    public interface IRunContext
    {
        public void StartAction();

        public void CompleteAction(MethodInfo action, params object[] args);
    }
}