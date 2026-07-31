using System;
using p4g64.InputLibrary64.Interfaces;
using Reloaded.Mod.Interfaces;

namespace p4g64.InputLibrary64
{
    public sealed class Exports : IExports
    {
        public Type[] GetTypes()
        {
            return new[]
            {
                typeof(IInputHook)
            };
        }
    }
}