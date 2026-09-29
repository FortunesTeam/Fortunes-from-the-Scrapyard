## 1.3.8

- Fixed ss2 (temp)
- Added redirect to the new SkinDef.BakeAsync

## 1.3.7

- Fixed common henry creation template error
- Fixed null itemDisplayRuleset prefab load error
- Included the dotParticleFix hooks

## 1.3.6

- Fix for UnityExplorer's all commando bug
- Fix for Item display rule instantiation
- Fix for SurvivorMannequinSlotController ApplyLoadoutToMannequinInstance

## 1.3.5

- Removed CharacterBody HandleDisableAllSkillsDebuff fix
- Removed fix for CHEF's "It's Getting Hot Here" achievement not unregistering an event upon completion

## 1.3.4

- Added fix for Xi Construct's laser attack not exploding at the end
- Updated HUD childloc to include springCanvas
- Removed convar fix for now.

## 1.3.3

- Added more childLocator entries and renamed some
- Added fix for Halcyonite Shrine being able to drain 0 gold and getting stuck

## 1.3.2

- Didn't happen

## 1.3.1

- Fixed 9 seperate possible NREs in CharacterBody_HandleDisableAllSkillsDebuff
- Added additional child locator entries to the Hud. These are difficult to find normally because they have no identifiable component attached.
> - UpperLeftCluster
> - NotificationArea
> - SkillIconContainer
> - BuffDisplayRoot
> - InventoryContainer

## 1.3.0

- Removed fixes that have been addressed by vanilla
- Rewrote the VineOrb.OnArrival patch and added an additional fix for null DotDefs
- Added fix for null HurtBoxes in CharacterModel.Awake
- Added fix for the multiple Lemurian Egg lock VFX
- Added fix for False Son not using Tainted Offering in phase 2
- Added fix for tether errors with the twisted elite
- Added fix for Longstanding Solitude NRE when spawning near a chest
- Added fix for the Elder Lemurian footstep sounds
- Added fix for ConVars with uppercase letters working
- Added fix for CHEF's "It's Getting Hot Here" achievement not unregistering an event upon completion
- Reworked a bunch of patches to use Harmony so there is no MMHOOK dependency
- Removed log spam for the Facepunch fix

## 1.2.8

- Changed false to true in OnBossGroupDefeated

## ugh

- ugh

## 1.2.6

- Goodbye tank, no longer need fixing
- Thats the last of the mod compats yay!

## 1.2.5

- Goodbye tyranitar, no longer need fixing

## 1.2.4

- Goodbye hunk, no longer need fixing

## 1.2.3

- Goodbye rifter, no longer need fixing

## 1.2.2

- Skip call to DotController.GetDotDef from new Noxious Thorns that existed for the sole purpose of throwing index out of bounds exceptions...
- This was vanilla, probably some partially commented out code or something idk

## 1.2.1

- Fixed FreeItemOnLevelup hook
- Updated changelog for previous patch

## 1.2.0

### **Added a big group of changes, thanks to Chinchi**
    * EvisDash.FixedUpdate
      - Merc eviserate no longer targets allies
    * Duplicating.DropDroplet
      - Printers use vfx again
    * DetachParticleOnDestroyAndEndEmission.OnDisable
      - Particle systems dont unparent inactive children
    * PositionIndicator.UpdatePositions
      - Position indicator NRE when hud is disabled
    * Indicator.SetVisibleInternal
      - IL only rewrite
    * CrosshairOverrideBehavior.OnDestroy
      - Collection modified error
    * RuleChoiceController.FindNetworkUser 
      - Quit to menu event system nre
    * RewiredIntegrationManager.RefreshJoystickAssignment
      - No rewired input on quitout
    * Frolic.TeleportAroundPlayer
      - Handles teleport with no available nodes
    * BurnEffectController.HandleDestroy
      - Prevents engine call on destroyed object
    * MeridianEventTriggerInteraction.Awake
      - Fixes test state spam
    * DamageIndicator.Awake
      - Loads asset into the main menu camera's damageindicator
    - Additionally added a ton of safeguards against calling server methods on client, instead of just letting those calls go through.
    - Fixes sale star collider being incorrectly configured
---
### Now onto my own fixes...
- Mods using the old TemporaryOverlay component will now properly initialize the new TempOverlayInstance
- Antler NREs are all gone now, sheesh
    - ElusiveAntlersPickup.Start, CharacterBody.OnShardDestroyed, ElusiveAntlersPickup.FixedUpdate
- Mysterious RouletteChestController.Idle NRE
- Minionleash OnDisable NRE
- ProjectileController.Start nre without ghost
- ProcessGoldEvent nre
- FreeItemOnLevelUp called on obj with null inventory
- Vineorb onArrival
- BossGroup Ondefeated calling event with no run instance

- Remove light flicker thing cuz ss2 is good now.
    - I still earn my keep by fixing the TetherVfxOrigin calling a null event.
    - Also fixed the flashbang menu scene
    - Thanks for continuing to boost my download count ily ss2 devs <3
- Made the Rifter fix actually fix Rifter

- **Todo:** update readme but euuuugh

## 1.1.3

- Forgot to make a hook actually do stuff

## 1.1.2

- Welcome Rifter to the club of mod fixes!
- Fixes a couple common errors
- Adjusts primary proc co-efficient to 1.0

- Fixed tether nre when invoking an event

- Tank: Fixed particles on genesis loop
- Tank: Enable overlays on renderers only if theyre enabled 

- Updated readme to reflect all changes
- Added mod specific config options

## 1.1.1

- Tank utility gives fuel again
- Added new optimization config option for tank
    - Tank no longer tanks fps hooray!
    - Default is disabled in case of bugs.

## 1.1.0

- Fixed tank save bug hooray!
- Removed damage indicator "fix" because it broke stuff when it didnt break lol

## 1.0.9

- Fixed the stupid SceneDirector.PopulateScene exception that the new stage 1 has
- Why are they using the wrong spawn cards
- Why is the nullchecking so inconsistent
- Why

## 1.0.8

- Fixed the fix for Hunk TVirus
- Moved stuff around

## 1.0.7

- Added git repo
- Fixed damage indicator startup exception

## 1.0.6

- Actually fixed the previously mentioned CheesePlayerHandler error

## 1.0.5

- Fixed incompletable void seed bug
- Fixed GoldenCoast chest interaction error
- Fixed more CheesePlayerHandler errors
- Fixed Hunk Urostep

## 1.0.4

- Fixed Celestial War Tank duplicating crosshair bug
- Fixed Celestial War Tank CheesePlayerHandler spam on stage start
- Adjusted some metadata

## 1.0.3

- Corrected mistake made with the tyranitar rock fix

## 1.0.2

- Fixed Steamworks loading error
- Prevented (harmless) exception when loading without Hunk

## 1.0.1

- Fixed some more vanilla bugs
- Added mod specific fixes for Hunk and Tyranitar

## 1.0.0

- Initial release