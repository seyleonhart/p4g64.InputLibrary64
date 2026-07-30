# Persona 4 Golden Input Library 64

A Reloaded-II dependency mod that exposes Persona 4 Golden's 64-bit keyboard and controller input as a simple shared API for other mods. Made as a port of rirurin's mod of the same name, minus the 64. This mod does not add visible gameplay features by itself. It provides input events for other Reloaded-II mods.

## What it does

The library reads the game's processed input state and publishes logical Persona 4 Golden actions through Reloaded-II's controller system.

It supports:

- Keyboard input
- Controller buttons
- Controller D-pad input
- Press and release events
- Distinguishing keyboard input from controller input
- Multiple simultaneous logical inputs in one bitmask

The library reports **logical game actions**, not physical key names.

For example, pressing the keyboard key currently assigned to Confirm is reported as `Input.Cross`. It is not reported as `Space`, `Enter`, or another physical keyboard key. This allows consuming mods to follow the player's in-game bindings.

## Supported inputs

| Input | Value | Typical meaning |
|---|---:|---|
| `Input.Select` | `0x0001` | Select / Back |
| `Input.Start` | `0x0008` | Start / Menu |
| `Input.Up` | `0x0010` | D-pad or menu Up |
| `Input.Right` | `0x0020` | D-pad or menu Right |
| `Input.Down` | `0x0040` | D-pad or menu Down |
| `Input.Left` | `0x0080` | D-pad or menu Left |
| `Input.LB` | `0x0400` | Left shoulder |
| `Input.RB` | `0x0800` | Right shoulder |
| `Input.Triangle` | `0x1000` | Triangle action |
| `Input.Circle` | `0x2000` | Circle action |
| `Input.Cross` | `0x4000` | Cross action |
| `Input.Square` | `0x8000` | Square action |

The values are flags and may be combined in one event.

## Requirements

### For players

- Persona 4 Golden 64-bit
- Reloaded-II
- Reloaded.Hooks
- A mod that depends on this library

### For mod developers

- A Reloaded-II mod project
- A reference to `p4g64.InputLibrary64.Interfaces.dll`, or a project reference to `p4g64.InputLibrary64.Interfaces`
- `p4g64.InputLibrary64` listed as a Reloaded-II dependency

## Installation

Install this mod through Reloaded-II like any other mod, then enable it for Persona 4 Golden.

Because this is a dependency library, it will normally be installed automatically or listed as a requirement by another mod.

There are no user-configurable settings.

## Using the library in another mod

### 1. Reference the interface assembly

Reference:

```text
p4g64.InputLibrary64.Interfaces.dll
```

A project reference can be used during development:

```xml
<ItemGroup>
  <ProjectReference Include="..\p4g64.InputLibrary64.Interfaces\p4g64.InputLibrary64.Interfaces.csproj" />
</ItemGroup>
```

A direct DLL reference can also be used:

```xml
<ItemGroup>
  <Reference Include="p4g64.InputLibrary64.Interfaces">
    <HintPath>Libraries\p4g64.InputLibrary64.Interfaces.dll</HintPath>
    <Private>false</Private>
  </Reference>
</ItemGroup>
```

`Private` is set to `false` because the installed Input Library supplies the interface assembly at runtime.

### 2. Add the Reloaded-II dependency

Add the library's mod ID to your mod's `ModConfig.json`:

```json
{
  "ModDependencies": [
    "p4g64.InputLibrary64"
  ]
}
```

Keep any other dependencies already required by your mod:

```json
{
  "ModDependencies": [
    "reloaded.sharedlib.hooks",
    "p4g64.InputLibrary64"
  ]
}
```

This ensures the Input Library is loaded before your mod attempts to retrieve its controller.

### 3. Retrieve `IInputHook`

```csharp
using p4g64.InputLibrary64.Interfaces;
using Reloaded.Mod.Interfaces;
using System;

public class Mod : ModBase
{
    private readonly WeakReference<IInputHook> _inputHookController;

    public Mod(ModContext context)
    {
        _inputHookController =
            context.ModLoader.GetController<IInputHook>();

        IInputHook inputHook;

        if (!_inputHookController.TryGetTarget(out inputHook))
        {
            throw new InvalidOperationException(
                "Persona 4 Golden Input Library 64 could not be acquired.");
        }

        inputHook.OnInput += OnInput;
    }

    private void OnInput(
        int input,
        bool risingEdge,
        bool controlType)
    {
        // Handle input here.
    }
}
```

The public event contract is:

```csharp
public delegate void OnInputEvent(
    int input,
    bool risingEdge,
    bool controlType);
```

## Event parameters

### `input`

A bitmask containing the current logical input state reported for that event.

Use a bitwise check when combinations should be supported:

```csharp
if ((input & (int)Input.Triangle) != 0)
{
    // Triangle is active in this event.
}
```

An exact comparison is suitable only when the action must be the sole active input:

```csharp
if (input == (int)Input.Triangle)
{
    // Triangle is the only active logical input.
}
```

### `risingEdge`

Indicates whether the event represents an active nonzero input state or a release to no input:

```text
true  = a nonzero input state was entered or changed
false = the input state returned to zero
```

The library follows the event behavior of the original 32-bit Input Library:

```text
Press:                    input = current nonzero mask, risingEdge = true
Change held combination:  input = new nonzero mask,     risingEdge = true
Release all inputs:       input = 0,                    risingEdge = false
Remain held:              no repeated event
```

A release event therefore contains `input == 0`. It does not contain the button that was released.

For one-shot actions, check `risingEdge`:

```csharp
private void OnInput(
    int input,
    bool risingEdge,
    bool controlType)
{
    if (!risingEdge)
        return;

    if ((input & (int)Input.Down) != 0)
    {
        ActivateNextAction();
    }
}
```

### `controlType`

Identifies the source device:

```text
true  = keyboard
false = controller
```

Example:

```csharp
private void OnInput(
    int input,
    bool risingEdge,
    bool controlType)
{
    bool keyboard = controlType;

    if (!risingEdge)
        return;

    if (keyboard &&
        (input & (int)Input.Start) != 0)
    {
        // Keyboard Start action.
    }

    if (!keyboard &&
        (input & (int)Input.Start) != 0)
    {
        // Controller Start action.
    }
}
```

## Complete example

This example reacts to several game actions while supporting combinations and both input devices:

```csharp
using p4g64.InputLibrary64.Interfaces;
using Reloaded.Mod.Interfaces;
using System;

public class Mod : ModBase
{
    private readonly WeakReference<IInputHook> _inputHookController;

    public Mod(ModContext context)
    {
        _inputHookController =
            context.ModLoader.GetController<IInputHook>();

        IInputHook inputHook;

        if (!_inputHookController.TryGetTarget(out inputHook))
        {
            throw new InvalidOperationException(
                "Missing dependency: p4g64.InputLibrary64");
        }

        inputHook.OnInput += OnInput;
    }

    private void OnInput(
        int input,
        bool risingEdge,
        bool controlType)
    {
        if (!risingEdge)
            return;

        bool keyboard = controlType;

        if ((input & (int)Input.Triangle) != 0)
        {
            OpenPrimaryCommand();
        }

        if ((input & (int)Input.Square) != 0)
        {
            OpenSecondaryCommand();
        }

        if ((input & (int)Input.LB) != 0)
        {
            PreviousSelection();
        }

        if ((input & (int)Input.RB) != 0)
        {
            NextSelection();
        }

        if (keyboard)
        {
            // Optional keyboard-specific behavior.
        }
    }

    private void OpenPrimaryCommand() { }
    private void OpenSecondaryCommand() { }
    private void PreviousSelection() { }
    private void NextSelection() { }
}
```

## Tracking whether a button is held

The library does not send an event every frame while an input remains held. A consumer that needs held-state behavior should store the latest state itself.

Because a release event has a zero mask, one simple approach is:

```csharp
private int _currentInput;

private void OnInput(
    int input,
    bool risingEdge,
    bool controlType)
{
    _currentInput = risingEdge ? input : 0;
}

private bool IsHeld(Input input)
{
    return (_currentInput & (int)input) != 0;
}
```

When keyboard and controller state must be tracked separately:

```csharp
private int _keyboardInput;
private int _controllerInput;

private void OnInput(
    int input,
    bool risingEdge,
    bool controlType)
{
    int current = risingEdge ? input : 0;

    if (controlType)
        _keyboardInput = current;
    else
        _controllerInput = current;
}
```

A mod that needs repeat behavior while held should run its own timer or game-update hook and query the stored state there.

## Keyboard behavior

Keyboard events use the same `Input` values as controller events. The reported value represents the game's logical action after its key bindings have been processed.

Examples:

```text
Keyboard key bound to Confirm  -> Input.Cross
Keyboard key bound to Cancel   -> Input.Circle
Keyboard key bound to Up       -> Input.Up
```

The library intentionally does not expose physical keyboard scan codes. This keeps consuming mods compatible with custom keyboard bindings.

## Controller behavior

The library currently exposes:

- Face buttons
- Left and right shoulder buttons
- Select and Start
- D-pad directions, including diagonals as combined direction flags

Analog sticks, triggers, L3, and R3 are not part of the public `Input` enum and are not emitted by this API.

## Porting a mod from the 32-bit Input Library

A mod originally using the 32-bit library normally needs only these dependency-facing changes.

Change the namespace:

```csharp
// Old
using p4gpc.inputlibrary.interfaces;

// 64-bit
using p4g64.InputLibrary64.Interfaces;
```

Change the referenced interface assembly:

```text
p4gpc.inputlibrary.interfaces.dll
```

to:

```text
p4g64.InputLibrary64.Interfaces.dll
```

Change the Reloaded-II dependency ID:

```text
p4gpc.inputlibrary
```

to:

```text
p4g64.InputLibrary64
```

The following API remains the same:

```csharp
GetController<IInputHook>()
inputHook.OnInput += OnInput
OnInput(int input, bool risingEdge, bool controlType)
```

Existing numeric input checks can remain unchanged because the logical bit values are preserved. New code can use the public `Input` enum instead.

## Important implementation notes

- Treat `input` as a bitmask.
- Prefer bitwise checks over exact equality when combinations are possible.
- Use `risingEdge` for one-shot actions.
- A release event has `input == 0`.
- No event is repeatedly emitted while an unchanged input remains held.
- `controlType` is `true` for keyboard and `false` for controller.
- Keyboard events represent logical actions, not physical keys.
- Do not bundle the main implementation DLL inside a consuming mod.
- Declare `p4g64.InputLibrary64` as a Reloaded-II dependency.

## Troubleshooting

### `GetController<IInputHook>()` has no target

Check that:

1. `p4g64.InputLibrary64` is installed and enabled.
2. Your mod lists `p4g64.InputLibrary64` in `ModDependencies`.
3. Your project references the matching `p4g64.InputLibrary64.Interfaces.dll`.
4. Your code imports `p4g64.InputLibrary64.Interfaces`.
5. You are not bundling a conflicting copy of the interface assembly.

### The callback reports `Input.Cross` for a keyboard key

This is expected. The API reports the logical Confirm/Cross action, not the physical keyboard key.

### A release callback has `input == 0`

This is expected and preserves the original Input Library's observable event behavior.

### An action triggers when another button is held

Use a bitwise check if combinations are allowed:

```csharp
(input & (int)Input.Triangle) != 0
```

Use exact equality only when the input must be active alone:

```csharp
input == (int)Input.Triangle
```

### A held button does not repeat

The event is emitted when the state changes, not every frame. Store the latest state and implement repeat timing in the consuming mod.

## Credits

rirurin, for the original Input Library.

## License

MIT License. See the repository's license information for more details regarding usage and redistribution terms.
