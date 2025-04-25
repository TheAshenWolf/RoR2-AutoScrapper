# Auto Scrapper

![Version 0.0.1](https://img.shields.io/badge/version-0.0.1-blue)
![QoL](https://img.shields.io/badge/Quality%20of%20Life-blue)
![Client Side](https://img.shields.io/badge/Client%20Side-blue)
## Currently in beta testing!

---

Ever felt tired of scrapping the same item over an over again at every scrapper?  
Well fret no more, as **AutoScrapper** is here to save your time.  
Simply set a limit for every item in the config and let the automagic happen.

While [Risk of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) is not required, it is highly recommended as all config text is styled for the display in-game.

## Dependencies
- [BepInExPack](https://thunderstore.io/package/bbepis/BepInExPack/)
- [HookGenPatcher](https://thunderstore.io/package/RiskofThunder/HookGenPatcher/)
- [R2API Items](https://thunderstore.io/package/RiskofThunder/R2API_Items/)
- [R2API Language](https://thunderstore.io/package/RiskofThunder/R2API_Language/)

### Optional
- [Risk of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) - for in-game config


## Installation
You can install the mod using the [Thunderstore Mod Manager](https://www.overwolf.com/app/thunderstore-thunderstore_mod_manager) (Overwolf) or [R2 Modman](https://thunderstore.io/package/ebkr/r2modman/) (standalone) by following [this]() link and clicking on "Install with Mod Manager".

If you wish to install the mod manually, you can download it [here](). After downloading and installing all dependencies, extract the zip file into the `BepInEx/plugins` folder of your game directory.

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
- Note: The item descriptions are styled for display in-game, so there are severa &lt;style&gt; and &lt;color&gt; tags present in the text making it harder to read.
- Set the limits for each item according to you needs and desires

### F&Q
- **Q: Does the mod work with modded items?**
- A: Yes and no. The support for modded items is currently very limited. Luckily for you, the only thing "broken" about it is the settings. You can still set up limit for modded items, however, if the mod is using asset bundles (as it should be), the item names will most likely not load properly.

- **Q: Does the mod work with multiplayer?**
- A: [TBD: REQUIRES TESTING]

// In case it is a client-side mod
- **Q: Is this cheating?**
- A: The only advantage you gain against other players is that you don't need to mindlessly scroll through the scrapper every single time. While I would not consider it cheating, you should always make sure your friends are okay with you using the mod if you are playing in multiplayer.


## Thanks and credits
**TheAshenWolf** - programming, optimization  
**Danquo** - constantly breaking the mod  
**Holytepps** - initial mod plan, multiplayer testing  



# Test Checklist (Remove me)
- [ ] Multiplayer compatibility
- [ ] Resetting config to default using Risk of Options
- [ ] Setting limit to -2
- [ ] Modded items support
- [ ] Does the host need the item?