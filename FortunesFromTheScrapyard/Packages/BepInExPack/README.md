## BepInEx Framework

This is the pack of all the things you need to both start using mods, and start making mods using the BepInEx framework.

To install, extract contents of the inner BepInExPack to the gamefolder, such that the `winhttp.dll` file sits right next to `RiskOfRain2.exe`.

The zip looks like:

```
\BepInExPack    <----- move the contents of this folder
manifest.json
readme.md
icon.png
```

### What each folder is for:

-   `BepInEx/plugins` - This is where normal mods/plugins are placed to be loaded. If a mod is just a `NAME.dll` file, it probably goes in here. For your own organisation, consider putting them inside folders, eg: `plugins/ActualRain/ActualRain.dll`

-   `BepInEx/patchers` - These are more advanced types of plugins that need to access Mono.Cecil to edit .dll files during runtime. Only copy paste your mods here if the author tells you to.

-   `BepInEx/config` - If your plugin has support for configuration, you can find the config file here to edit it.

-   `BepInEx/core` - Core BepInEx .dll files, you'll usually never want to touch these files (unless you're updating manually)

### What is included in this pack

**[Doorstop 4](https://github.com/NeighTools/UnityDoorstop)**

This is what loads BepInEx.

**[BepInEx 5.4](https://github.com/BepInEx/BepInEx)**

This is what loads all of your plugins/mods.

**Customized BepInEx configuration**

BepInEx config customized for use with RoR2.

**[BepInEx.GUI](https://github.com/risk-of-thunder/BepInEx.GUI)**

Graphical User Interface meant to replace the regular console host that is used by BepInEx when the config setting is enabled.

**[FixPluginTypesSerialization](https://github.com/xiaoxiao921/FixPluginTypesSerialization)**

Enables custom Serializable struct stored in plugin assemblies to be correctly deserialized by the Unity engine's Deserializer.

**[RoR2BepInExPack](https://github.com/risk-of-thunder/RoR2BepInExPack)**

Provides fixes that makes it easier for modders to create and maintain their mods while preventing harmful bugs.

### Writing your own mods

There's 2 documentation pages available:

-   [R2Wiki](https://risk-of-thunder.github.io/R2Wiki/)
-   [BepInEx docs](https://docs.bepinex.dev/)

Places to talk:

-   [RoR2 modding discord](https://discord.gg/5MbXZvd)
-   [General BepInEx discord](https://discord.gg/MpFEDAg)

BepInEx contains helper libraries like [MonoMod.RuntimeDetour](https://github.com/MonoMod/MonoMod/blob/master/README-RuntimeDetour.md) and [HarmonyX](https://github.com/BepInEx/HarmonyX/wiki)

### Changelog

Available in the `CHANGELOG.md` file