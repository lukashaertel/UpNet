using System.Reflection;

namespace UpNet.Infrastructure.Controller
{
    public interface IControllerLocator
    {
        public MethodInfo NameToAction(ushort name);

        public ushort ActionToName(MethodInfo action);
    }
}