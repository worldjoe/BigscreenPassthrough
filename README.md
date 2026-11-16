# Bigscreen Passthrough

Experimental MelonLoader mod that hides much of a room's scenery and replaces
the primary VR camera background with chroma green. It attempts to preserve
avatars, screens, presentation objects, and UI. Virtual Desktop can key out
that green to show Quest passthrough; this mod does not access the headset
camera itself. This is an unofficial community mod, not affiliated with
Bigscreen, Inc.

## Requirements

- Windows, Bigscreen PC VR, and IL2CPP-compatible MelonLoader. Launch the
  game once with MelonLoader to generate `MelonLoader\Il2CppAssemblies`.
- A compatible headset and Virtual Desktop's VR Passthrough for the tested
  chroma-key workflow.
- BigscreenBehind for the **Passthrough** item in its Mod Selection menu.
  F9 works without that menu.
- .NET 6 SDK or a later SDK capable of targeting .NET 6, for building.

Developed against Bigscreen `0.950.2.03f010-beta-perma-rooms-beta`,
MelonLoader `0.7.1` open beta, and Unity `2020.3.33f1`. Other versions may
require changes.

## Build

From this directory in PowerShell:

```powershell
dotnet build .\BigscreenPassthrough.csproj -c Release
```

The default Bigscreen path is
`C:\Program Files (x86)\Steam\steamapps\common\Bigscreen`. Override it if
needed:

```powershell
dotnet build .\BigscreenPassthrough.csproj -c Release "-p:GameDir=D:\SteamLibrary\steamapps\common\Bigscreen"
```

Output: `bin\Release\net6.0\BigscreenPassthrough.dll`. Required game
and MelonLoader assemblies are referenced from the installed game.

## Install and use

1. Close Bigscreen. If upgrading, remove `BigscreenChromaPrototype.dll` from
   the `Mods` folder, then copy `BigscreenPassthrough.dll` into it. Existing
   toggle preferences are retained.
2. Launch Bigscreen through Virtual Desktop and enter a room.
3. Enable Virtual Desktop **VR Passthrough** and configure its key color to
   **red 0, green 153, blue 51**. Adjust similarity and smoothness for your
   setup.
4. Use **Bigscreen Behind > Mod Selection > Passthrough** or focus the PC
   game window and press **F9** to toggle the filter.

The filter is enabled by default and remembers its setting. It restores
previously hidden renderers and camera background when switched off. While
the teleport arc is active, the filter temporarily turns off so the room and
destination are visible; it resumes when the arc closes if still enabled.
To uninstall, close Bigscreen and remove the DLL from `Mods`.

## Limitations

Room filtering uses object names and layers as heuristics. Some rooms may
retain scenery or hide wanted objects; matching green in avatars or movies
can also become transparent. This does not guarantee passthrough support
through Steam Link or other PC VR connections. Keep your Quest boundary
enabled and do not rely on the mod for obstacle detection.

Errors and activation messages appear in `MelonLoader\Latest.log`.
