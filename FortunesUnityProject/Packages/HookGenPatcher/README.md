# BepInEx.MonoMod.HookGenPatcher

Generates [MonoMod.RuntimeDetour.HookGen's](https://github.com/MonoMod/MonoMod) `MMHOOK` file during the [BepInEx](https://github.com/BepInEx/BepInEx) preloader phase. 

Installation:
Copy config, patchers, and plugins folder to your BepInEx folder.
Make sure the `MonoMod.dll` and `MonoMod.RuntimeDetour.HookGen.dll` files are also present.

**This project is not officially linked with BepInEx nor with MonoMod.**

*Note, there's a mismatch between the thunderstore version number and the github version numbers. Don't mind it.*

## See also:
[LighterPatcher](https://thunderstore.io/package/Harb/LighterPatcher/) which is designed to easy the load on the game when having particularly large MMHOOK files by stripping unneeded types. LighterPatcher and HookGenPatcher work in conjunction with each other to prevent multiple runs.
