# Shader Swapper
A lightweight library for upgrading stubbed shaders to actual shaders at runtime.

## Stubbed Shaders?
Stubbed shaders are dummy shaders that contain the properties of the real John Hopoo shaders. This means stubbed shaders are safe to include in your project and asset bundles. At runtime, the stubbed shaders are upgraded to the real shaders, and the material properties are preserved!

**If you are looking for the RoR2 Stubbed Shaders, you can find them [here](https://github.com/risk-of-thunder/RoR2StubbedShaders/releases).**

This mod is a general solution for swapping your stubbed shaders with real shaders at runtime.

## Examples
> **Make sure you are using the** `ShaderSwapper` **namespace!**

ShaderSwapper methods are asynchronous. You can start them as a coroutine from your plugin like so:
```csharp
base.StartCoroutine(myAssetBundle.UpgradeStubbedShadersAsync());
```
ShaderSwapper will run in the background to upgrade the shaders of every material in your asset bundle.

Alternatively, you could yield `UpgradeStubbedShadersAsync` in your `IContentPackProvider`:
```csharp
public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
{
    // ...
    
    var upgradeStubbedShaders = myAssetBundle.UpgradeStubbedShadersAsync();
    while (upgradeStubbedShaders.MoveNext())
    {
        yield return upgradeStubbedShaders.Current;
    }

    // ...
}
```

> ### Materials with stubbed shaders must be explicitly included in your asset bundles
> **Auto-included materials (partially greyed out) will NOT be found by ShaderSwapper.**

## Contact
For questions or bug reports, you can find me in the [RoR2 Modding Server](https://discord.gg/5MbXZvd) @Groove_Salad

Or, you can post issues on the [GitHub](https://github.com/Priscillalala/ShaderSwapper)

## See Also
* [RoR2StubbedShaders](https://github.com/risk-of-thunder/RoR2StubbedShaders): A curated collection of stubbed shaders for RoR2
* [RuntimeMaterialInspector](https://thunderstore.io/package/Groove_Salad/RuntimeMaterialInspector): A tool for editing material properties in-game
