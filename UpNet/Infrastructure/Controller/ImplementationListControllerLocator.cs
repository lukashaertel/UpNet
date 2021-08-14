using System;
using System.Collections.Generic;
using System.Reflection;

namespace UpNet.Infrastructure.Controller
{
    public class ImplementationListControllerLocator : IControllerLocator
    {
        private Dictionary<ushort, MethodInfo> NamesToActions { get; }
        private Dictionary<MethodInfo, ushort> ActionsToNames { get; }

        public ImplementationListControllerLocator(IEnumerable<Type> controllerTypes)
        {
            NamesToActions = new();
            ActionsToNames = new();

            var name = ushort.MinValue;
            foreach (var controllerType in controllerTypes)
            foreach (var action in controllerType.GetMethods())
            {
                // Needs public and non-abstract implementation.
                if (!action.IsPublic)
                    continue;
                if (action.IsAbstract)
                    continue;
                if (controllerType != action.DeclaringType)
                    continue;

                NamesToActions[name] = action;
                ActionsToNames[action] = name;
                name++;
            }
        }

        public MethodInfo NameToAction(ushort name) =>
            NamesToActions[name];

        public ushort ActionToName(MethodInfo action) =>
            ActionsToNames[action];
    }
}