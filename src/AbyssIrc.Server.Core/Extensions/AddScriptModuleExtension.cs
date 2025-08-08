using AbyssIrc.Core.Extensions;
using AbyssIrc.Server.Core.Data.Internal.ScriptEngine;
using DryIoc;

namespace AbyssIrc.Server.Core.Extensions;

public static class AddScriptModuleExtension
{
    public static IContainer AddScriptModule(this IContainer container, Type scriptModule)
    {
        if (scriptModule == null)
        {
            throw new ArgumentNullException(nameof(scriptModule), "Script module type cannot be null.");
        }

        container.AddToRegisterTypedList(new ScriptModuleData(scriptModule));

        container.Register(scriptModule, Reuse.Singleton);


        return container;
    }
}
