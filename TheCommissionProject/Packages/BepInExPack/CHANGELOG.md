-   **5.4.2113**

    -   RoR2 BepInExPack specific changes :
        -   BepInEx.GUI: Ensure remaining old dll in the patchers folder doesn't get loaded by BepInEx Preloader.

-   **5.4.2112**

    -   RoR2 BepInExPack specific changes :
        -   The thunderstore `BepInExPack` package no longer directly hosts the two BepInEx patchers, `BepInEx.GUI` and `FixPluginTypesSerialization`, and the `RoR2BepInExPack` plugin, instead, the thunderstore package now has them as dependencies in the thunderstore package manifest.

-   **5.4.2111**

    -   RoR2 BepInExPack specific changes :
        -   Fix the difficulty coefficient not being called at the start of a Run, causing the cost of chests to be incorrect for the first stage when resetting a run or in multiplayer.

-   **5.4.2110**

    -   RoR2 BepInExPack specific changes :
        -   Log all hook to the log file, this was previously done by R2API but made debugging harder in some cases where R2API was either initializing too late or for mods that wasn't depending on R2API.
        -   Add additional event to `SaferAchievementManager` AchievementAttribute collector for mod creators to run custom logic.

-   **5.4.2109**

    -   RoR2 BepInExPack specific changes :
        -   Fix a softlock related to Artifact of Metamorphosis with custom survivors that are locked behind custom expansions.
        -   Remove an unnecessary vanilla log line whenever expose is applied via the damage type.
        -   Fix NonLethal damage still killing when you have 1 max hp.

-   **5.4.2108**

    -   RoR2 BepInExPack specific changes :
        -   Add another config for `BepInEx` only meant to be modified by core maintainers / developers when the game updates. (Should fix some issues that some users have of BepInEx not loading any mods)

-   **5.4.2107**

    -   RoR2 BepInExPack specific changes :
        -   Fix another potential crash due to the ConVar change introduced on the previous BepInExPack update.
        -   Fix WWise crash for dedicated servers.

-   **5.4.2106**

    -   RoR2 BepInExPack specific changes :
        -   Fix potential crash due to the ConVar change introduced on the previous BepInExPack update.

-   **5.4.2105**

    -   RoR2 BepInExPack specific changes :
        -   Mod developers can now simply use `[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]` for adding ConVar to their mods without having to use `R2API.CommandHelper` modules or similar methods.
		-   The ProjectileCatalog logs an error if more than 256 projectiles are registered, despite the actual limit being much higher. The console log for that "fake warning" is now gone.

-   **5.4.2104**

    -   RoR2 BepInExPack specific changes :
        -   Swap to doorstop 4, this should make debugging mods for mod developers much easier.
		-   Default values for the `BepInEx.cfg` file are now RoR2 specific, meaning that even if an user deletes the config file by mistake BepInEx will still work correctly.

-   **5.4.2103**

    -   RoR2 BepInExPack specific changes :
        -   Fix for DynamicBones log spam.
        -   Fix for log spam on some deaths.

-   **5.4.2102**

    -   RoR2 BepInExPack specific changes :
        -   Fix FixPluginTypesSerialization sometimes making the game crash on exit.
        -   Fix Copy Log File button in the BepInEx GUI not working correctly.

-   **5.4.2101**

    -   RoR2 BepInExPack specific changes :
        -   Remove the console dev check in the BepInEx.GUI, replaced by a disclaimer instead: If you want #tech-support in the modding discord, please use the "Copy Log to Clipboard" button and then paste it in the discord channel.

-   **5.4.2100**

    -   Updated BepInEx to 5.4.21
    -   RoR2 BepInExPack specific changes :
        -   New BepInEx GUI, it should fix some performance issues the old one had + the .zip is now much smaller, you can still go back to the old console by setting to `true` the `Enables showing a console for log output` option in the `BepInEx/config/BepInEx.cfg` file
        -   Thunderstore Mod Manifests if available are printed in the log file to allow for better debugging.
        -   `FixPluginTypesSerialization` patcher is now shipped by default
        -   The `RoR2BepInExPack` (v1.1.0) plugin now contains a mod compatibility fix for when multiple corruption (void items) targets for an item are present, a config is available to determine which gets the new stack:
            -   Random -> (Default Option) picks randomly
            -   First -> Oldest Target Picked Up
            -   Last -> Newest Target Picked Up
            -   Rarest -> Rarest Target Picked Up (falls back to Newest on ambiguity)
            -   Alternate -> All targets get a turn in acquisition order

-   **5.4.1905**

    -   RoR2 BepInExPack specific changes :
        -   Fix achievements not working correctly. For real this time.

-   **5.4.1904**

    -   RoR2 BepInExPack specific changes :
        -   Fix achievements not working correctly

-   **5.4.1903**

    -   RoR2 BepInExPack specific changes :
        -   Fix the BepInEx GUI sometimes not being visible at all / full white

-   **5.4.1902**

    -   RoR2 BepInExPack specific changes :
        -   Plugin Entrypoint is now at RoR2.FlashWindow..ctor

-   **5.4.1901**

    -   RoR2 BepInExPack specific changes :
        -   HideAndDontSave set back to false in config
        -   Plugin Entrypoint is now at RoR2.RoR2Application.Awake

-   **5.4.1900**

    -   Added basic fix for cases where games try to ship their own Harmony
    -   Updated HarmonyX to 2.9.0
    -   Updated MonoMod to 22.01.29.01
    -   RoR2 BepInExPack specific changes :
        -   Added BepInEx.GUI which replace the default console (you can disable it in the settings)
        -   HideAndDontSave set to true in config
        -   Add a plugin which detour old Resources.Call to Addressable equivalent

-   **5.4.1801**

    -   Updated MonoMod to 22.01.04.03, fixing problems in some Unity environments (wine)

-   **5.4.18**

    -   Fixed some console messages being cut off (especially if using non-ASCII characters)
    -   Updated HarmonyX to 2.7.0
    -   Updated MonoMod to 21.12.13.01

-   **5.4.17**

    -   Fixed console not opening in Outer Wilds and other games that ship custom `user32.dll`

-   **5.4.16**

    -   Fixed when DumpAssemblies is enabled, dumped assemblies are now put to `BepInEx/DumpedAssemblies/<ProcessName>`. If assembly is in use (e.g. multiple game processes open), dumped assemblies will have a number postfix.
    -   Game executable timestamp is not included in console title now (fixes issue with some window managers)
    -   Updated HarmonyX to 2.5.5
    -   Updated MonoMod.RuntimeDetour to 21.9.19.1

-   **5.4.15**

    -   Update HarmonyX to 2.5.4
    -   Update MonoMod to 21.8.5.1

-   **5.4.14**

    -   Update HarmonyX to 2.5.3
    -   Update MonoMod to 21.7.22.3

-   **5.4.13**

    -   Update HarmonyX to 2.5.2
        -   Fixes an issue that prevented BepInEx from launching on certain games

-   **5.4.12**

    -   Log executable timestamp in preloader logs
    -   Fix BepInEx \*nix launch script
        -   Add support for the new Steam bootstrapper
        -   Add experimental fix for resolving symlinks
    -   Update Doorstop to 3.4.0.0
    -   Fix crash in paths with non-ASCII characters
    -   Experimental fix for HarmonyXInterop not working sometimes on first game launch
    -   Update HarmonyX to 2.5.1

-   **5.4.11**

    -   Update Bepin GOs to use HideAndDontSave flags
        -   Disable setting BepInEx manager to HideAndDontSave by default
        -   New config option: [Chainloader].HideManagerGameObject which enables HideAndDontSave for manager GameObjects
    -   Fix ChainLoader.HasBepinPlugins returning false when inheriting from class from another Assembly

-   **5.4.10**
    -   Updated HarmonyX to 2.4.2
    -   Updated UnityDoorstop.Unix to 1.5.1.0
    -   Marked BepInEx plugin manager and ThreadingHelper GameObjects with HideAndDontSave flag to prevent it from being destroyed in some games
    -   Converted configuration files to always use UTF-8 without BOM
    -   Fixed headless mode check throwing an exception in some games
