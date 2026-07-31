using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;
using p4g64.InputLibrary64.Template;
using p4g64.InputLibrary64.Configuration;
using p4g64.InputLibrary64.Interfaces;
// using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using System.Diagnostics;

namespace p4g64.InputLibrary64;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    private readonly Logging _utils;
    private readonly Inputs _inputs;
    // private readonly IStartupScanner _scanner;
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader
            ?? throw new InvalidOperationException(
                "Mod loader is unavailable.");

        if (!_modLoader
            .GetController<IReloadedHooks>()
            .TryGetTarget(out var hooks) ||
            hooks is null)
        {
            throw new InvalidOperationException(
                "Reloaded.Hooks is unavailable. " +
                "Make sure Reloaded.SharedLib.Hooks is listed as a dependency.");
        }

        // if (!_modLoader
        //     .GetController<IStartupScanner>()
        //     .TryGetTarget(out var scanner) ||
        //     scanner is null)
        // {
        //     throw new InvalidOperationException(
        //         "Reloaded startup scanner is unavailable.");
        // }

        _hooks = hooks;
        // _scanner = scanner;

        _modLoader = context.ModLoader;

        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;
        // #if DEBUG
        //         // Attaches debugger in debug mode; ignored in release.
        //         Debugger.Launch();
        // #endif

        // For more information about this template, please see
        // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

        // If you want to implement e.g. unload support in your mod,
        // and some other neat features, override the methods in ModBase.

        // TODO: Implement some mod logic
        _utils = new Logging(_configuration, _logger);
        _inputs = new Inputs(_hooks, _configuration, _utils); // IInputHook implementation
        _modLoader.AddOrReplaceController<IInputHook>(_owner, _inputs);

        _logger.WriteLine($"[{_modConfig.ModId}] Registered IInputHook controller.");
        
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}