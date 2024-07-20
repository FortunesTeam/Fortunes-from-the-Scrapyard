### 1.4.0

- Updated to use Thunderkit 8.0.6
- Updated README.md
- Removed LegacyResourcePatcher extension, as it now causes an invalid DLL post meteorite patch
- R2API Submodule Installer now always installs the latest versions of the submodulef

### 1.3.7

- Fixed issue where the MMHook Generator would delete the MMHook Generated Assemblies
- Fixed issue where SetDeferredShading wouldnt properly apply the changes to the editor

### 1.3.6

- Fixed issue where the MMHook Generator cleanup step would throw an exception
- Moved the AppDomain cache into the MMHook Assembly Processor
- Removed message about not updating R2API submodule list
- Fixed issue where R2API Submodule Dependencies werent getting serialized
- Fixed issue where R2API Submodule Installer would install outdated versions of modules (Specifically the Core and ContentManagement submodules)
- Installing HookGenPatcher no longer causes an invalid project state due to MMHOOK_AssemblyCSharp

### 1.3.5

- Updated to use ThunderKit 7.0.0
- Now installs the R2API Submodules in one big batch instead of one by one

### 1.3.4

- Fixed an issue where executing the R2APISubmoduleInstaller would result in an invalid project state
- R2APISubmoduleInstaller now uninstalls old versions of Submodules that where added as dependencies if a newer version exists.

### 1.3.3

- Fixed an issue where NotSupportedException would throw when the MMHook Generator Configuration was trying to cache assemblies
- R2APISubmoduleInstaller doesnt destroy the Thunderstore Source if it has been ensured
- Fixed issue where R2APISubmoduleInstaller wouldnt properly install submodules and cause a loop spamming a warning message about no packages found.

### 1.3.1

- Added a method to serialize R2API submodule installation as hard dependencies.
	- These hard dependencies are managed by a serialized JSON file.
	- The hard dependencies will persist even if the import configuration asset is removed/deleted

### 1.3.0

- Updated Install R2API to the R2API Submodule Installer, updating the importer to properly support the Split Assemblies update.
- Added a safety catch to the MMHook generator so it doesnt accidentally create duplicate MMHook assemblies.

### 1.2.0

- FixPluginTypesSerialization is now in the BepInExPack by default, removed the option for installing the FixPluginTypesSerialization standalone
- Added the MMHook Generator, used for automatically generating MonoMod hook assemblies.

### 1.1.1

- Fix `README.md`

### 1.1.0

- Add SetDeferredShading to fix Graphics settings not collected by Import Project settings

### 1.0.2

- Add importer to fix graphics settings, enabling consistent rendering of shaders in the editor

### 1.0.1

- Fix legacy api patcher path by @PassivePicasso in #3

### 1.0.0

- Initial Release