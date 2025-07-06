-   **1.26.0**

    -   Appends the exception to the error message of "Failed at InvokeHandler, probably malformed packet!", allowing for better and easier debugging.

-   **1.25.0**

    -   Fix lobby compatibility for DLC2 with additional difficulties.

-   **1.24.2**

    -   Properly log harmony patches.

-   **1.24.1**

    -   Fix `SaferSearchableAttribute` not working due to the game cctor firing before the hook has the chance to apply.

-   **1.24.0**

    -   Remove the `FixFrameRateDependantLogic` fix as it's now fixed by the new game patch.

-   **1.23.0**

    -   Re-add `SaferSearchableAttribute`.

-   **1.22.0**

    -   Continue the FixFrameRateDependantLogic experimentation. Better compat with mods hooking the same methods.

-   **1.21.0**

    -   Fix ConVar not getting scanned since the SOTS DLC2 Release patch.
    -   Continue the FixFrameRateDependantLogic experimentation. AI behaving weirdly should be fixed. Should also have better compat with mods that hooked HealthBar.Update and PlayerCharacterMasterController.Update methods.

-   **1.20.0**

    -   Add an optional fix that attempts to revert the changes made by Gearbox, which made most of the game's logic dependent on frame rate. To activate the fix, you need to run the game with this version at least once, exit the game and activate the new configuration option that is generated.

-   **1.19.0**

    -   Fix `SearchableAttribute` not working properly for assemblies other than `RoR2.dll`.

-   **1.18.0**

    -   Fix `SystemInitializerAttribute` not working for assemblies other than `RoR2.dll`.
    -   `FrankenMonoPrintStackOverflowException` works again.

-   **1.17.0**

    -   Initial update for DLC 2 Release.
	-   This package now also contains the Newtonsoft.Json.dll that have been removed this game update, just for back compat purposes.

-   **1.16.0**

    -   A better fix for a crash when exiting the game with some mods.

-   **1.15.0**

    -   Fix a crash when exiting the game with some mods.

-   **1.14.0**

    -   Fix HasEffectiveAuthority for host players.

-   **1.13.0**

    -   Better fix for stack overflow exception that does not require a native dll.

-   **1.12.0**

    -   Make frankenmono actually print stack overflow exception to the console instead of just closing.

-   **1.11.0**

    -   Fix dedicated servers with more than 4 max players not working.

-   **1.10.0**

    -   Make ResourceAvailable safer.

-   **1.9.0**

    -   For mod developers: Fix `CharacteracterBody.RemoveOldestTimedBuff` which didn't work if the oldest buff had index 0 in the `body.timedBuffs` array.

    -   Only init the wwise safety hooks if in dedicated server mode.

-   **1.8.0** - Shipped in BepInExPack **5.4.2111**

    -   Fix the difficulty coefficient not being called at the start of a `Run`, causing the cost of chests to be incorrect for the first stage when resetting a run or in multiplayer.

-   **1.7.0** - Shipped in BepInExPack **5.4.2110**

    -   Log all hook to the log file, this was previously done by `R2API` but made debugging harder in some cases where `R2API` was either initializing too late or for mods that wasn't depending on `R2API`.
    
    -   Add additional event to `SaferAchievementManager` AchievementAttribute collector for mod creators to run custom logic.

-   **1.6.0** - Shipped in BepInExPack **5.4.2109**

    -   Fix a softlock related to Artifact of Metamorphosis with custom survivors that are locked behind custom expansions.
    
    -   Remove an unnecessary vanilla log line whenever expose is applied via the damage type.
    
    -   Fix NonLethal damage still killing when you have 1 max hp.
        
-   **1.5.0** - Shipped in BepInExPack **5.4.2107**

    -   Fix another potential crash due to the ConVar change introduced on the previous BepInExPack update.
    
    -   Fix WWise crash for dedicated servers.

-   **1.4.1** - Shipped in BepInExPack **5.4.2106**

    -   Fix potential crash due to the ConVar change introduced on the previous BepInExPack update.

-   **1.4.0** - Shipped in BepInExPack **5.4.2105**

    -   Mod developers can now simply use `[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]` for adding ConVar to their mods without having to use `R2API.CommandHelper` modules or similar methods.
    
    -   The ProjectileCatalog logs an error if more than 256 projectiles are registered, despite the actual limit being much higher. The console log for that "fake warning" is now gone.

-   **1.3.0** - Shipped in BepInExPack **5.4.2104**

    -   Fix Eclipse button not being selectable for controllers.
    
    -   Add some `System.Reflection` safety by hooking `Assembly.GetTypes` and catching all potential `ReflectionTypeLoadException`

-   **1.2.0** - Shipped in BepInExPack **5.4.2103**

    -   Fix for DynamicBones log spam.
    
    -   Fix for log spam on some deaths.
        
-   **1.1.0** - Shipped in BepInExPack **5.4.2100**

    -   Now contains a mod compatibility fix for when multiple corruption (void items) targets for an item are present, a config is available to determine which gets the new stack:
    
        -   Random -> (Default Option) picks randomly.
        
        -   First -> Oldest Target Picked Up.
        
        -   Last -> Newest Target Picked Up.
        
        -   Rarest -> Rarest Target Picked Up (falls back to Newest on ambiguity).
        
        -   Alternate -> All targets get a turn in acquisition order.

-   **1.0.2** - Shipped in BepInExPack **5.4.1905**

    -   Fix achievements not working correctly. For real this time.

-   **1.0.1** - Shipped in BepInExPack **5.4.1904**

    -   Fix achievements not working correctly.

-   **1.0.0** - Shipped in BepInExPack **5.4.1900**

    -   Detour old Resources.Call to Addressable equivalent.
