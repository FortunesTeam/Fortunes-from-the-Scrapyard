# Bursts of Rain

Bursts of Rain is a lightweight assembly loader that's used to load Burst compiled unity assemblies, allowwing for the usage of Burst-Compiled Jobs within mods for Risk of Rain 2.

To allow for the full utilization of the Data Oriented Technology Stack (DOTS)'s systems, this Thunderstore Package redistributes the following unity assemblies, which are not found within the base game:

| Assembly | Unity Package Version |
|--|--|
| System.Runtime| Not Applicable |
| Unity.Burst | 1.8.18 |
| Unity.Collections | 1.5.1 |
| Unity.Mathematics | 1.2.6 |

These assemblies should not cause issues on existing profiles.

## For Developers

Bursts of Rain is used mainly as a front-end and ease of loading for Bust Assemblies utilizing ``BurstRuntime.LoadAdditionalLibrary``'s method, ``BurstsOfRainMain`` contains a method that can be used to simplify this process, by giving it your ``PluginInfo``, which serves as the start location, and a ``string``, which should be the name of the bursted assembly.

    //Example
    public class MyModWithBurstedAssemblies : BaseUnityPlugin
    {
        private void Awake()
        {
            //Load the assembly named "MyMod_Burst.dll" onto the Burst Runtime
            BurstsOfRainMain.AddBurstAssembly(Info, "MyMod_Burst.dll");
        }
    }

Below are the main entries for each of the unity packages this thunderstore package redistributes, alongside a brief description:

| Package | Description |
|--|--|
| [Burst](https://docs.unity3d.com/2021.3/Documentation/Manual/com.unity.burst.html) | Burst is a compiler that translates from IL/.NET bytecode to highly optimized native code using LLVM. |
| [Collections](https://docs.unity3d.com/2021.3/Documentation/Manual/com.unity.collections.html) | A C# collections library providing data structures that can be used in jobs, and optimized by Burst compiler. |
| [Mathematics](https://docs.unity3d.com/2021.3/Documentation/Manual/com.unity.mathematics.html) | Unity’s C# SIMD math library providing vector types and math functions with a shader like syntax. Methods within are ***Job Safe*** |

While not redistributed by the mod since its part of the Core module, an understanding of the [Jobs System](https://docs.unity3d.com/2021.3/Documentation/Manual/JobSystem.html) is also required.

## Development of Mods using Burst

Development of mods utilizing burst follows very similar procedures as regular Burst-centered job development, utilization of Data oriented programming is recommended over Object oriented programming to allow for the full utilization of the Jobs and Burst system, an example on how to "Burstify" a complex behaviour can be found in this [Github Repository](https://github.com/Nebby1999/BarebonesMongerTrail/tree/main), courtesy of Nebby.

Utilizing [ThunderKit](https://github.com/PassivePicasso/ThunderKit) as your main mod development environment is recommended, as there you can utilize the direct packages of Burst, Collections and Mathematics.

## Burstifying an Assembly

Burstifying an existing assembly is done utilizing the ``BCL.exe`` file found within the Burst package, specifically in ``Burst/.Runtime``
For more information on how to utilize the ``BCL.exe`` file directly, look below at the "Non ThunderKit" section.

### ThunderKit:
For burstifying your assembly utilizing the ThunderKit development environment, it is recommended to utilize the [RoR2ThunderBurster]() package, which contains the following utilities for deploying your bursted assemblies

* Import Extension: for ensuring the installation of Burst, Collections and Mathematics.
* ``BurstAssemblyDefinitionsDatum``: A subclass of ThunderKit's ``AssemblyDefinitions`` manifest datum, which is used to mark that the Assembly Definitions within should be passed thru the Burst Compiler if possible.
* ``BurstStagedAssemblies``: A custom ``PipelineJob`` that requires a ``BurstAssemblyDefinitionsDatum``, it utilizes reflection to enqueue and compile the selected assemblies with BCL, mimicking as close as possible the actual compilation of Bursted Assemblies when a Player is Built.

Replacing your ``Manifest``'s ``AssemblyDefinitions`` datum with a ``BurstAssemblyDefinitionsDatum``, and running a ``BurstStagedAssemblies`` job after a ``StageAssemblies`` is enough to burst-compile your assembly.

### Non ThunderKit:

To burstify an assembly without ThunderKit you'll need to get access to the internal Burst compiler (BCL.exe), a download of the package can be found in this website: ``https://download.packages.unity.com/com.unity.burst/``. alternatively, if you trust the links in this readme, click [here](https://download.packages.unity.com/com.unity.burst/-/com.unity.burst-1.8.18.tgz) to download Burst version 1.8.18

Below is a small guide on how to use the compiler, this part may be moved to the R2Wiki at a later date.

* Open the Tarball (tgz) file and open the file within using 7zip or similar, its the actual .tar file which contains the package.

![IMG](https://raw.githubusercontent.com/risk-of-thunder/BurstsOfRain/refs/heads/main/Documentation/OpenTarball.png)

* Open the ``Package`` folder, and locate the ``.Runtime`` folder, this ``.Runtime`` folder contains the BCL.exe, which contains the compiler.

![IMG2](https://raw.githubusercontent.com/risk-of-thunder/BurstsOfRain/refs/heads/main/Documentation/dotRuntimeFolderLocation.png)

* Extract the contents of ``.Runtime`` to a folder you desire.

![IMG3](https://raw.githubusercontent.com/risk-of-thunder/BurstsOfRain/refs/heads/main/Documentation/ExtractedFolder.png)

You can write a .txt file that contains the necesary arguments for Burst compiling your assembly, below is an example of one.

    --key-folder=D:/ProgramFiles/UnityInstallations/2021.3.33f1/Editor/Data/PlaybackEngines/WindowsStandaloneSupport
    --decode-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Library\Burst
    --platform=Windows
    --assembly-defines=Starstorm2;UNITY_2021_3_33;UNITY_2021_3;UNITY_2021;UNITY_5_3_OR_NEWER;UNITY_5_4_OR_NEWER;UNITY_5_5_OR_NEWER;UNITY_5_6_OR_NEWER;UNITY_2017_1_OR_NEWER;UNITY_2017_2_OR_NEWER;UNITY_2017_3_OR_NEWER;UNITY_2017_4_OR_NEWER;UNITY_2018_1_OR_NEWER;UNITY_2018_2_OR_NEWER;UNITY_2018_3_OR_NEWER;UNITY_2018_4_OR_NEWER;UNITY_2019_1_OR_NEWER;UNITY_2019_2_OR_NEWER;UNITY_2019_3_OR_NEWER;UNITY_2019_4_OR_NEWER;UNITY_2020_1_OR_NEWER;UNITY_2020_2_OR_NEWER;UNITY_2020_3_OR_NEWER;UNITY_2021_1_OR_NEWER;UNITY_2021_2_OR_NEWER;UNITY_2021_3_OR_NEWER;ENABLE_AR;ENABLE_AUDIO;ENABLE_CACHING;ENABLE_CLOTH;ENABLE_EVENT_QUEUE;ENABLE_MICROPHONE;ENABLE_MULTIPLE_DISPLAYS;ENABLE_PHYSICS;ENABLE_TEXTURE_STREAMING;ENABLE_VIRTUALTEXTURING;ENABLE_UNET;ENABLE_LZMA;ENABLE_UNITYEVENTS;ENABLE_VR;ENABLE_WEBCAM;ENABLE_UNITYWEBREQUEST;ENABLE_WWW;ENABLE_CLOUD_SERVICES;ENABLE_CLOUD_SERVICES_ADS;ENABLE_CLOUD_SERVICES_USE_WEBREQUEST;ENABLE_CLOUD_SERVICES_CRASH_REPORTING;ENABLE_CLOUD_SERVICES_PURCHASING;ENABLE_CLOUD_SERVICES_ANALYTICS;ENABLE_CLOUD_SERVICES_UNET;ENABLE_CLOUD_SERVICES_BUILD;ENABLE_CLOUD_LICENSE;ENABLE_EDITOR_HUB_LICENSE;ENABLE_WEBSOCKET_CLIENT;ENABLE_DIRECTOR_AUDIO;ENABLE_DIRECTOR_TEXTURE;ENABLE_MANAGED_JOBS;ENABLE_MANAGED_TRANSFORM_JOBS;ENABLE_MANAGED_ANIMATION_JOBS;ENABLE_MANAGED_AUDIO_JOBS;ENABLE_MANAGED_UNITYTLS;INCLUDE_DYNAMIC_GI;ENABLE_SCRIPTING_GC_WBARRIERS;PLATFORM_SUPPORTS_MONO;RENDER_SOFTWARE_CURSOR;ENABLE_VIDEO;ENABLE_ACCELERATOR_CLIENT_DEBUGGING;PLATFORM_STANDALONE;TEXTCORE_1_0_OR_NEWER;PLATFORM_STANDALONE_WIN;UNITY_STANDALONE_WIN;UNITY_STANDALONE;ENABLE_RUNTIME_GI;ENABLE_MOVIES;ENABLE_NETWORK;ENABLE_NVIDIA;ENABLE_CRUNCH_TEXTURE_COMPRESSION;ENABLE_UNITY_GAME_SERVICES_ANALYTICS_SUPPORT;ENABLE_OUT_OF_PROCESS_CRASH_HANDLER;ENABLE_CLUSTER_SYNC;ENABLE_CLUSTERINPUT;PLATFORM_UPDATES_TIME_OUTSIDE_OF_PLAYER_LOOP;GFXDEVICE_WAITFOREVENT_MESSAGEPUMP;ENABLE_WEBSOCKET_HOST;ENABLE_MONO;NET_STANDARD_2_0;NET_STANDARD;NET_STANDARD_2_1;NETSTANDARD;NETSTANDARD2_1;DEVELOPMENT_BUILD;ENABLE_PROFILER;DEBUG;TRACE;UNITY_ASSERTIONS;ENABLE_CUSTOM_RENDER_TEXTURE;ENABLE_DIRECTOR;ENABLE_LOCALIZATION;ENABLE_SPRITES;ENABLE_TERRAIN;ENABLE_TILEMAP;ENABLE_TIMELINE;ENABLE_LEGACY_INPUT_MANAGER;TEXTCORE_FONT_ENGINE_1_5_OR_NEWER;RISKOFRAIN2;TK_ADDRESSABLE;RISKOFTHUNDER_BEPINEX_GUI;RISKOFTHUNDER_FIXPLUGINTYPESSERIALIZATION;RISKOFTHUNDER_ROR2BEPINEXPACK;BBEPIS_BEPINEXPACK;RISKOFTHUNDER_HOOKGENPATCHER;RISKOFTHUNDER_R2API_CORE;RISKOFTHUNDER_R2API_CONTENTMANAGEMENT;RISKOFTHUNDER_R2API_ADDRESSABLES;RISKOFTHUNDER_R2API_ARTIFACTCODE;RISKOFTHUNDER_R2API_COLORS;RISKOFTHUNDER_R2API_COMMANDHELPER;RISKOFTHUNDER_R2API_DAMAGETYPE;RISKOFTHUNDER_R2API_DEPLOYABLE;RISKOFTHUNDER_R2API_DIFFICULTY;RISKOFTHUNDER_R2API_DIRECTOR;RISKOFTHUNDER_R2API_DOT;RISKOFTHUNDER_R2API_ELITES;RISKOFTHUNDER_R2API_NETWORKING;RISKOFTHUNDER_R2API_PREFAB;RISKOFTHUNDER_R2API_RECALCULATESTATS;RISKOFTHUNDER_R2API_RULEBOOK;RISKOFTHUNDER_R2API_STAGES;RISKOFTHUNDER_R2API_STRINGSERIALIZEREXTENSIONS;RISKOFTHUNDER_R2API_TEMPVISUALEFFECT;RUNE580_RISK_OF_OPTIONS;SMOOTH_SALAD_SHADERSWAPPER;NEBBY_LOADINGSCREENSPRITEFIX;UNITY_POST_PROCESSING_STACK_V2;RISKOFTHUNDER_R2API_ITEMS;RISKOFTHUNDER_R2API_LANGUAGE;RISKOFTHUNDER_R2API_ORB;AMOGUS_LOVERS_STANDALONEANCIENTSCEPTER;SS2_GAME_IMPORTED;CSHARP_7_OR_LATER;CSHARP_7_3_OR_NEWER
    --root-assembly=D:\RoR2Related\Projects\Starstorm2\SS2-Project\ThunderKit\Libraries\Starstorm2.dll
    --output=D:\RoR2Related\Projects\Starstorm2\SS2-Project\ThunderKit\Libraries\Starstorm2_Burst
    --temp-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Temp\Burst
    --target=X64_SSE2
    --target=AVX2
    --linker-options=PdbAltPath="Starstorm2_Data/Plugins/x86_64/lib_burst_generated.pdb"
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\ThunderKit\Libraries
    --assembly-folder=D:\ProgramFiles\UnityInstallations\2021.3.33f1\Editor\Data\PlaybackEngines\WindowsStandaloneSupport\Variations\mono\Managed
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\BepInExPack\BepInExPack\BepInEx\core
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\Risk of Rain 2
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\mmhook-assemblies
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\StandaloneAncientScepter
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_ArtifactCode\plugins\R2API.ArtifactCode
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Colors\plugins\R2API.Colors
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_ContentManagement\plugins\R2API.ContentManagement
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Core\plugins\R2API.Core
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_DamageType\plugins\R2API.DamageType
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Director\plugins\R2API.Director
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Dot\plugins\R2API.Dot
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Elites\plugins\R2API.Elites
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Networking\plugins\R2API.Networking
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Prefab\plugins\R2API.Prefab
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_RecalculateStats\plugins\R2API.RecalculateStats
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Difficulty\plugins\R2API.Difficulty
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_TempVisualEffect\plugins\R2API.TempVisualEffect
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\Risk_Of_Options\plugins\RiskOfOptions
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Addressables\plugins\R2API.Addressables
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Deployable\plugins\R2API.Deployable
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Packages\R2API_Rulebook\plugins\R2API.Rulebook
    --assembly-folder=D:\ProgramFiles\UnityInstallations\2021.3.33f1\Editor\Data\NetStandard\ref\2.1.0
    --assembly-folder=D:\ProgramFiles\UnityInstallations\2021.3.33f1\Editor\Data\MonoBleedingEdge\lib\mono\unityaot-win32
    --assembly-folder=D:\ProgramFiles\UnityInstallations\2021.3.33f1\Editor\Data\MonoBleedingEdge\lib\mono\unityaot-win32\Facades
    --assembly-folder=D:\ProgramFiles\UnityInstallations\2021.3.33f1\Editor\Data\NetStandard\Extensions\2.0.0
    --assembly-folder=D:\ProgramFiles\r2modman\DataFolder\RiskOfRain2\profiles\MSU2Dev\BepInEx\plugins\RiskofThunder-Bursts_of_Rain\.RuntimeAssemblies
    --assembly-folder=D:\RoR2Related\Projects\Starstorm2\SS2-Project\Library\ScriptAssemblies
    --pdb-search-paths=Temp/ManagedSymbols/
    --generate-link-xml=Temp\burst.link.xml
    --debug=Full
    --opt-level=2

Afterwards you can specify the file directly on the command prompt to let the compiler use the data within the file as the arguments for compilation:

    bcl.exe +burstc @"PathToTheFileWithTheArguments"

Below is a list of the possible arguments to use with the compiler, alongside what they do, text in **bold** is not mentioned directly by the compiler when using ``bcl --help``

_Multiple values are separated by ``|``_

_Entries that end with ``=`` means they expect a value from the valid values list. If they lack a valid values entry its because the valid values are unkown at the time of writing_

_Entries without specific valid values means theyre toggles_

<details><summary>--platform=</summary>
<p>

* Valid Values

        Windows|macOs|Linux|Android|iOS|PS4|XboxOne_Deprecated|Wasm|UWP|Lumin|Switch|Stadia_Deprecated|tvOS|EmbeddedLinux|GameCoreXboxOne|GameCoreXboxSeries|PS5|QNX|visionOS|visionSimulator

* Notes:
    * Target platform, Defaults to Windows
    * **This is the platform to compile to, most of the time this should be Windows**

</p>
</details>

<details><summary>--backend=</summary>
<p>

* Notes:
    * The backend name. Default: "burst-llvm-16"
    * **Seems to be optional to specify**

</p>
</details>

<details><summary>--global-safety-checks-settings=</summary>
<p>

* Valid Values

        Off|On|ForceOn

* Notes:
    * Global safety checks setting. Default:Off

</p>
</details>

<details><summary>--disable-safety-checks</summary>
<p>

* Notes:
    * Disable safety checks. Default: Disabled

</p>
</details>

<details><summary>--disable-opt</summary>
<p>

* Notes:
    * Disable `ir-opt` and `cpu-opt` optimizations

</p>
</details>

<details><summary>--fastmath</summary>
<p>

* Notes:
    * Enable fast math optimizations

</p>
</details>

<details><summary>--meta-data-generation=</summary>
<p>

* Notes:
    * Disable `ir-opt` and `cpu-opt` optimizations. Default: Disabled

</p>
</details>

<details><summary>--target=</summary>
<p>

* Valid Values:

        X86_SSE2|X86_SSE4|X64_SSE2|X64_SSE4|AVX|AVX2|WASM32|ARMV7A_NEON32|ARMV8A_AARCH64|THUMB2_NEON32|ARMV8A_AARCH64_HALFFP|ARMV9A

* Notes:
    * Target CPU. Can be specified multiple times for enabling more than one target.
    * **This is the target instruction set.**
    * **To specify more than once, add more "--target" arguments to your file.**

</p>
</details>

<details><summary>--log-timings</summary>
<p>

* Notes:
    * Log timings. Default False

</p>
</details>

<details><summary>--opt-level=</summary>
<p>

* Valid Values

        1|2|3

* Notes:
    * Optimization level. Default: 3
    * **This is the optimization to use by default when a ``BurstCompile`` attribute does not specify an optimization level**
    * **The values here co-rrelate directly to the ``BurstCompile``'s ``OptimizeFor`` enum**

    |OptimizeFor|Equivalent Int|
    |--|--|
    | Default | 2 |
    | Balanced | 2 |
    | Performance | 3 |
    | Size | 3 |
    | Fast Compilation | 1 |

</p>
</details>

<details><summary>--opt-for-size</summary>
<p>

* Notes:
    * Optimizes for size instead of performance. Default: False

</p>
</details>

<details><summary>--float-precision=</summary>
<p>

* Valid Values

        Standard|High|Medium|Low

* Notes:
    * Precision CPU. Default: Standard

</p>
</details>

<details><summary>--float-mode=</summary>
<p>

* Valid Values

        Default|Strict|Deterministic|Fast

* Notes:
    * Math options. Default: Default

</p>
</details>

<details><summary>--branc-protection=</summary>
<p>

* Valid Values

        None|Standard

* Notes:
    * Branch protection (PAC/BTI). Default: None

</p>
</details>

<details><summary>--dump=</summary>
<p>

* Valid Values

        None|IL|Unused|IR|IROptimized|Asm|Function|Analysis|IRPassAnalysis|ILPre|IRPerEntryPoint|All

* Notes:
    * Dump flags. Default: Function

</p>
</details>

<details><summary>--format=</summary>
<p>

* Valid Values

        Elf|Coff|MachO|Wasm

* Notes:
    * Object Format. Default: Elf

</p>
</details>

<details><summary>--debugtrap</summary>
<p>

* Notes:
    * Inserts a debug trap on the first instruction of the entry point function. Default: False

</p>
</details>

<details><summary>--disable-vectors</summary>
<p>

* Notes:
    * DisableSIMD Vector types special codegen (float4, float2...). Default: False

</p>
</details>

<details><summary>--generate-link-xml=</summary>
<p>

* Valid Values

        string(path)

* Notes:
    * Generate a link.xml as part of the build process. Default: ""

</p>
</details>

<details><summary>--debug=</summary>
<p>

* Valid Values

        None|Full|LineOnly

* Notes:
    * Enables generation of debug info - PDB, DWARF -. Default: None

</p>
</details>

<details><summary>--debugMode</summary>
<p>

* Notes:
    * Enables debuggability for code generation using a native debugger. Default: False

</p>
</details>

<details><summary>--generate-static-linkage-methods=</summary>
<p>

* Notes:
    * Enables the generation of static linkage methods. Default: false

</p>
</details>

<details><summary>--generate-job-marshalling-methods</summary>
<p>

* Notes:
    * Enables the generation of job marshalling methods. Default: False

</p>
</details>

<details><summary>--temp-folder=</summary>
<p>

* Valid Values

        string(path)

* Notes:
    * The temporary directory to use. Defaults to ``C:\Users\USERNAME\AppData\LocalTemp\``

</p>
</details>

<details><summary>--disable-warnings=</summary>
<p>

* Valid Values

        string(Burst warning codes)

* Notes
    * Warnings to disable, (separated by ;) eg: ``BC1370; BC1322``

</p>
</details>

<details><summary>--assembly-defines=</summary>
<p>

* Valid Values

        string(Assembly name, followed by defines.)

* Notes
    * Defines to use for building the specified assemblies in ``--root-assembly``. (Separated by ;) eg: ``MyAssembly;UNITY_2020_1;NET_2_0``

</p>
</details>

<details><summary>--linker-options=</summary>
<p>

* Notes:
    * Additional settings to be consumed by the native linkers (separated by ;)

</p>
</details>

<details><summary>--enable-direct-external-linking</summary>
<p>

* Notes:
    * Link external calls directly instead of using burst.initialize. Default: False

</p>
</details>

<details><summary>--enable-autolayout-fallback-check=</summary>
<p>

* Notes: Enables validation that structs are managed-sequential. Default: False

</p>
</details>

<details><summary>--disable-string-interpolation-in-exception-messages=</summary>
<p>

* Notes: Disables string interpolation in exception messages.

</p>
</details>

<details><summary>--target-framework=</summary>
<p>

* Target framework that was used when compiling the input C#

</p>
</details>

<details><summary>--platform-configuration=</summary>
<p>

* Notes:
    * Platform specific configuration (SDK Version, for example)

</p>
</details>

<details><summary>--discard-assemblies=</summary>
<p>

* Notes:
    * Discard assemblies from consideration even though they may burst compile attributes

</p>
</details>

<details><summary>--save-extra-context</summary>
<p>

* Notes:
    * Save extra context state and recover at abort

</p>
</details>

<details><summary>--output=</summary>
<p>

* Valid Values

        string(path)

* Notes:
    * Output path for the generated shared library. Default: ""
    * **This is the path of the output of the compiler, the format is ``Directory/BaseFileName``**
    * **BaseFileName, in this case, is the base file name for all the files generated by the compiler, by default, this is ``lib_burst_generated``**
    * **For example, the compiler may output 3 different files with the same names, ``MyAssembly_Burst.dll``, ``MyAssembly_Burst.pdb``, ``MyAssembly_Burst.txt``**

</p>
</details>

<details><summary>--keep-intermediate-files</summary>
<p>

* Notes:
    * Keep intermediate files along the shared library generated final file. Default: False

</p>
</details>

<details><summary>--nolink</summary>
<p>

* Notes:
    * Dont link the final object file to a shared library, but let the object file to be the output. Default: False

</p>
</details>

<details><summary>--no-native-toolchain</summary>
<p>

* Notes
    * Dont look for a native toolchain. Useful if you want to provide your own

</p>
</details>

<details><summary>--emit-llvm-objects</summary>
<p>

* Notes:
    * Forces output of object files to be LLVM bitcode rather than native objects.

</p>
</details>

<details><summary>--key-folder=</summary>
<p>

* Valid Values

        string(path)

* Notes:
    * Key file folder location - required for some platforms. Default: ""
    * **This in the case of testing has proved to be the playback engine of the selected platform.**
    * **Example: ``D:/ProgramFiles/UnityInstallations/2021.3.33f1/Editor/Data/PlaybackEngines/WindowsStandaloneSupport``**

</p>
</details>

<details><summary>--decode-folder=</summary>
<p>

* Valid Values

        string(path)

* Decode folder location- required for some platforms. Default: Same location as BCL.exe

</p>
</details>

<details><summary>--cache-directory=</summary>
<p>

* Valid Values

        string(path)

* Cache Directory. Default: ""

</p>
</details>

<details><summary>--pdb-search-paths=</summary>
<p>

* Valid Values:

        string(path)

* Notes:
    * Path to search for pdbs, in addition to the same folder as the assembly. (specify multiple times for multiple folders)
    * **To specify more than once, add more "--pdb-search-paths" arguments to your file.**

</p>
</details>

<details><summary>--print-monopinvokecallbackmissing-message</summary>
<p>

* Notes
    * Print a warning if a compiled function pointer is missing MonoPInvokeCallbackAttribute (needed for IL2CPP). Default: false

</p>
</details>

<details><summary>--include-root-assembly-references=</summary>
<p>

* Valid Values:
        
        string(path)

* Notes:
    * Recursively scan root assembly references for target methods. If this is false, only target methods from the root assembly will be compiled. Default is False

</p>
</details>

<details><summary>--threads=</summary>
<p>

* Valid Values:
    
    Integer

* Notes:
    * Number of compiler threads working concurrently. Default is 11

</p>
</details>

<details><summary>--always-create-output=</summary>
<p>

* Valid Values:

    Boolean

* Notes:
    * Always create output library. If this is false and no target methods are found, no output library will be created. Default: True

</p>
</details>

<details><summary>--safety-checks</summary>
<p>

* Notes:
    * Enable safety checks. Default: disabled
</p>
</details>

<details><summary>--is-for-function-pointer</summary>
<p>

* Notes:
    * True if the requested compilation is for a function pointer. DEfault: False

</p>
</details>

<details><summary>--enable-interpreter</summary>
<p>

* Notes:
    * Allow the compiled method to be interpreted

</p>
</details>

<details><summary>--assembly-folder=</summary>
<p>

* Valid Values

        string(path)

* Notes:
    * Assembly folders (specify multiple times for multiple folders)
    * **Used during assembly recognition and parsing**
    * **Should point to folders where assemblies the ``--root-assembly`` references.
    * **To specify more than once, add more "--assembly-folder" arguments to your file.**

</p>
</details>

<details><summary>--method=</summary>
<p>
    
* Notes:
    * Full methodname with optional hash (separated by --)

</p>
</details>

<details><summary>--type=</summary>
<p>
    
* Notes:
    * A type to decompile all static public methods from. A hash will be generated for each method

</p>
</details>

<details><summary>--assembly=</summary>
<p>
    
* Notes:
    * An assembly path to look for the type (specify multiple times for multiple paths)
    * **To specify more than once, add more "--assembly" arguments to your file.**

</p>
</details>

<details><summary>--group</summary>
<p>

* Notes:
    * Start a new group of methods    

</p>
</details>

<details><summary>--verbose</summary>
<p>
    
* Notes:
    * Display methods being compiled. Default: False

</p>
</details>

<details><summary>--root-assembly=</summary>
<p>

* Valid Values:

        string(path)

* Notes:
    * Root assembly for finding compile target methods (specify multiple times for multiple roots)
    * **This is the actual assembly to be compiled with Burst**
    * **To specify more than once, add more "--root-assembly" arguments to your file.**

</p>
</details>

<details><summary>--validate-external-tool-chain</summary>
<p>

* Notes:
    * Dont attempt to build anything, just check that the current target and host are correctly configured for linking

</p>
</details>

<details><summary>--only-static-methods</summary>
<p>

* Notes:
    * Compile only static methods and not Execute methods of job producer interfaces
    
</p>
</details>

<details><summary>--method-prefix=</summary>
<p>

* Notes:
    * Add a prefix to the names of generated methods
    
</p>
</details>

<details><summary>--chunk-size=</summary>
<p>

* Valid Values:

        Integer

* Notes:
    * Number of methods to compile per threads working concurrently. Default is 64
    
</p>
</details>

<details><summary>--enable-guard</summary>
<p>

* Notes:
    * Enable guard asserts. Default: False

</p>
</details>

<details><summary>--output-mode=</summary>
<p>

* Valid Values:

        SingleLibrary|LibraryPerJob

* Notes:
    * Output mode. Default: SingleLibrary
    
</p>
</details>

<details><summary>--only-list-methods</summary>
<p>

* Notes:
    * Only list the methods to compile. Outputs like ``assembly.dll, method``. Default: False
    
</p>
</details>

<details><summary>--warmup</summary>
<p>

* Notes:
    * Run a warmup pass of the compile to amortize the cost of the JIT Compile. Default: False
    
</p>
</details>

<details><summary>--help</summary>
<p>

* Notes:
    * Show Help
    
</p>
</details>