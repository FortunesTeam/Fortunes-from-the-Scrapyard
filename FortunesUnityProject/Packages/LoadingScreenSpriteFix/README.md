# LoadingScreenSpriteFix -  An API for adding new Loading Screen Sprites

## What the fuck is this?

Do you like the funny little sprites  on the loading screen? do you find it "Soulful"?

Do you want to add even more sprites? All the sprites?! then this mod is for your.

This mod allows you to fairly easily add new sprite animations to the game, includes documentation and a class for creating SimpleSpriteAnimations using code with a variety of method options.

Also, this fixes an issue where sometimes the sprites and numbers would overlap each other until the splash art scene would load.

## Care to be more of a nerd?

The API's part of this mod was originally part of MSU 2.0's launch, i noticed it could be made to be lightweight and its own mod, so instead of making a new mod i added it to this original fix.

This mod contains a single AssetBundle which contains a pre-made prefab that works as a generic container for the animation and the animator itself.

The methods for adding the new animations are meant to be purely temporary and exist during the loading screen only, once the game reaches the main menu screen, the mod unhooks all the hooks done and destroys the given sprite animations, alongside unloading any bundles passed over. this is done for memory reasons. You can always just instantiate a clone of the animation and only pass that to avoid deleting the actual metadata of the sprite animation.

For the overlapping sprite issue, instead of actually figuring out why the hell it happens, this mod adds a black background behind the Numbers and the loading sprite.

As Soon as the splash screen appears, the black background is removed.

## Why?

Sprite animations in the loading screen are fucking soul, and i saw more modders wanted to add them themselves, so i proceeded to update this mod into the API itself.

Also, the overlapping fix was done because i hate seeing the spritework being mangled.

## Donations

Feel free to give me a coffee here

[![ko-fi](https://raw.githubusercontent.com/TeamMoonstorm/MoonstormSharedUtils/refs/heads/main/Docs/Readme/SupportNebby.png)](https://ko-fi.com/nebby1999)