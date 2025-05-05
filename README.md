# Auto Scrapper

![Version 0.2.0](https://img.shields.io/badge/version-0.2.0-blue)
![QoL](https://img.shields.io/badge/Quality%20of%20Life-blue)
![Client and Server Side](https://img.shields.io/badge/Client%20and%20Server%20Side-blue)

## Currently in beta testing!

Ever felt tired of scrapping the same item over an over again at every scrapper?  
Well fret no more, as **AutoScrapper** is here to save your time.  
Simply set a limit for every item in the config and let the automagic happen.

While [Risk of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) is not required, it is highly recommended as all config text is styled for the display in-game.

## Dependencies
- [BepInExPack](https://thunderstore.io/package/bbepis/BepInExPack/)
- [HookGenPatcher](https://thunderstore.io/package/RiskofThunder/HookGenPatcher/)
- [R2API Items](https://thunderstore.io/package/RiskofThunder/R2API_Items/)
- [R2API Language](https://thunderstore.io/package/RiskofThunder/R2API_Language/)
- [R2API Networking](https://thunderstore.io/package/RiskofThunder/R2API_Networking/)

### Optional
- [Risk of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) - for in-game config


## Installation
You can install the mod using the [Thunderstore Mod Manager](https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager) (Overwolf) or [R2 Modman](https://thunderstore.io/package/ebkr/r2modman/) (standalone) by following [this](https://thunderstore.io/package/TheAshenWolf/AutoScrapper/) link and clicking on "Install with Mod Manager".

If you wish to install the mod manually, you can download it [here](https://thunderstore.io/package/TheAshenWolf/AutoScrapper/). After downloading and installing all dependencies, extract the zip file into the `BepInEx/plugins` folder of your game directory.

## How to use

1. Set limit for each item in the config
2. Obtain more than the limit of an item
3. Interact with a scrapper

#### Example:
Tri-tip dagger's limit is set to 10. If I have 12 tri-tip daggers in my inventory, 2 of them will get scrapped upon interaction with a scrapper.  
Setting the limit to 0 will scrap all items, setting it to -1 will disable scrapping.

### With Risk of Options (recommended)
- Launch the game and open the settings
- Navigate to the Mod Settings tab and select AutoScrapper
- All items are grouped by their tier/color
- Set the limits for each item according to you needs and desires

### Without Risk of Options
- The Config is located in the `BepInEx/config` folder of your game directory
- You can open the config file with any text editor
- Note: The item descriptions are styled for display in-game, so there are several &lt;style&gt; and &lt;color&gt; tags present in the text making it harder to read.
- Set the limits for each item according to you needs and desires

## F&Q
- **Q: Does the mod work with modded items?**
- A: The support for modded items is currently very limited and was not yet thoroughly tested. We are already aware of some minor issues.
- **Q: Does the mod work with multiplayer?**
- A: Yes. Both the host and the client need to have the mod installed.

## Bugs? Issues? Feedback?
No mod exists without its bugs. No mod has everything.  
Feel free to report all issues you encounter right [here](https://github.com/TheAshenWolf/RoR2-AutoScrapper/issues).

## Thanks and credits
**TheAshenWolf** - programming, optimization  
**Danquo** - constantly breaking the mod  
**Holytepps** - initial mod plan, multiplayer testing