# FortifySkillsRedux
FortifySkillsRedux is a remake of the FortifySkills mod for Valheim that changes how skills are lost on death. Rather than being punished for dying by losing a flat 5% of every skill, you are instead rewarded for staying alive for longer. This is achieved by adding a new fortified skill level for each skill that is used when you die to reset your skills to their fortified skill level. This means that no matter how many times you die, your skills will never drop below their fortified skill levels.

**Server-Side Info**: This mod does work as a client-side only mod and only needs to be installed on the server if you wish to enforce configuration settings.

## Version 1.4.0 Notice
Configuration settings have changed! Please delete your config file and let the mod regenerate if if you're having any issues.

## Installation
**Via Mod Manager (Recommended)**
- The best way to install the mod is using r2modman and installing it from Thunderstore.
- The next best way is to use Thunderstore Mod Manager.

**Manual**
- Download and install [BepInEx Pack](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)
- Download and install [Jotunn](https://valheim.thunderstore.io/package/ValheimModding/Jotunn/)
- Download this mod and move the "FortifySkills.dll" into "<GameLocation>\BepInEx\plugins"

## Mechanics
The Fortify skill level increases very slowly at first but if you get your active level significantly higher that your fortified level it will level a little quicker giving you an incentive not to die. Because of this your fortified level may fall a long way behind your active level if you stay alive for a long time and you can lose more than you would with the vanilla 5% penalty. To make up for this your active level increases a little faster than Vanilla as well.

There are two major gameplay advantages to this:

- A string of deaths won't destroy your skill level. No need to worry about the No Skill Drain buff ending just before you die.
- Less used skills won't wither away completely from the occasional death. If you use one weapon type a lot early game but then switch to something else, now a few deaths without training the original weapon skill won't completely reset it.

Your Fortify skill level will be displayed in parenthesis in your skill list next to your active skill level.

## Configuration
Changes made to the configuration settings will be reflected in-game immediately (no restart required) and they will also sync to clients if the mod is on the server. The mod also has a built in file watcher so you can edit settings via an in-game configuration manager (changes applied upon closing the in-game configuration manager) or by changing values in the file via a text editor or mod manager.

### Global Section
**Verbosity**
- Low will log basic information about the mod. Medium will log information that is useful for troubleshooting. High will log a lot of information, do not set it to this without good reason as it will slow down your game.
    - Acceptable values: Low, Medium, High
    - Default value: Low.

**Use Inidividual Settings [Synced with Server]**
- If enabled, use the config settings for each individual Vanilla skill and the Modded skill config settings for all skills added by mods. If disabled use the config setting from the Mechanics section for all skills.
    - Acceptable values: False, True
    - Default value: false.

### Mechanics
**Active Skill XP Multiplier [Synced with Server]**
- Controls XP gained for the active skill level. 1 = base game XP, 1.5 = 50% bonus XP, 0.8 = 20% less XP.
    - Default value: 1.5

**Max Fortify Skill XP Rate [Synced with Server]**
- Controls maximum rate of XP earned for the fortified skill as a percentage of vanilla XP rates. Values below 1 mean that fortified skills will always increase slower than vanilla skills. Values above 1 mean that fortified skills can increase faster than vanilla skills if your active skill level is high enough.
    - Default value: 0.8

**Fortify Skill XP Per Level [Synced with Server]**
- Controls XP gained for the fortified skill. For every level the active skill is above the fortified skill increase the percentage of XP gained for the fortified skill by this amount up to Max Fortify Skill XP Rate.
    - Default value: 0.1

### IndividualSkills Section
There is a section with the same config settigns for each skill in the Vanilla game and one additional section for all skills added by mods. 
These settings are only used if Use Individual Settings is Enabled and they allow you to customize the XP gains for each individual skill.


## Compatibility
**All skill mods by Smoothbrain**
  - The XP multiplier settings in this mod stacks multiplicatively with the XP multiplier in Smoothbrain's skill mods.
  - If you set EnableIndividualSettings to True and keep ModdedSkillXPMult set to 1.0 it will not impact the XP gain rates of Smoothbrain's skill mods while still letting you customize the skill gain rates for Vanilla skills via the IndividualSkills XP multiplier settings.

**Incompatibilities**
- May have issues with anything that changes the SkillsDialog text in-game.

## Notes
- If you want to be extra cautious you can back up your character file from "%appdata%\..\LocalLow\IronGate\Valheim\characters" as this mod changes how those files are written.
- Your Fortify skill level will be set to 95% of your current skill level when you first install it so dying immediately will have the same effect as the base game.
- If you remove this mod your character will be fine, the fortify skill level will disappear and the current skill level will stay the same (including levels gained due to the faster leveling from this mod).

## Donations/Tips
My mods will always be free to use but if you feel like saying thanks you can tip/donate.

| My Ko-fi: | [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/searica) |
|-----------|---------------|

## Source Code
Source code is available on Github.

| Github Repository: | <img height="18" src="https://github.githubassets.com/favicons/favicon-dark.svg"></img><a href="https://github.com/searica/FortifySkillsRedux"> FortifySkillsRedux</a>|
|-----------|---------------|


### Contributions
If you would like to provide suggestions, make feature requests, or reports bugs and compatibility issues you can either open an issue on the Github repository or tag me (@searica) with a message on my discord [Searica's Mods](https://discord.gg/sFmGTBYN6n).

I'm a grad student and have a lot of personal responsibilities on top of that so I can't promise I will respond quickly, but I do intend to maintain and improve the mod in my free time.

### Credits
This mod is based on the original one made by Merlyn42 and the patched version was made by Remeil. This mod is a complete rewrite of the original though as the original stopped working several game updates back and has no license. My thanks to Merlyn42 for the original idea though! Also, many thanks to the developers of Jotunn for all their work making the library.

### Shameless Self Plug (Other Mods By Me)
If you like this mod you might like some of my other ones.

#### Building Mods
- [Extra Snap Points Made Easy](https://thunderstore.io/c/valheim/p/Searica/Extra_Snap_Points_Made_Easy/)
- [AdvancedTerrainModifiers](https://thunderstore.io/c/valheim/p/Searica/AdvancedTerrainModifiers/)
- [BuildRestrictionTweaksSync](https://thunderstore.io/c/valheim/p/Searica/BuildRestrictionTweaksSync/)
- [ToolTweaks](https://thunderstore.io/c/valheim/p/Searica/ToolTweaks/)

#### Gameplay Mods
- [CameraTweaks](https://thunderstore.io/c/valheim/p/Searica/CameraTweaks/)
- [DodgeShortcut](https://thunderstore.io/c/valheim/p/Searica/DodgeShortcut/)
- [DiscoveryPins](https://thunderstore.io/c/valheim/p/Searica/DiscoveryPins/)
- [ProjectileTweaks](https://thunderstore.io/c/valheim/p/Searica/ProjectileTweaks/)
- [SkilledCarryWeight](https://thunderstore.io/c/valheim/p/Searica/SkilledCarryWeight/)
- [SafetyStatus](https://thunderstore.io/c/valheim/p/Searica/SafetyStatus/)
- [WatchWhereYouStab](https://thunderstore.io/c/valheim/p/Searica/WatchWhereYouStab/)

#### Networking Mods
- [NetworkTweaks](https://thunderstore.io/c/valheim/p/Searica/NetworkTweaks/)
- [OpenSesame](https://thunderstore.io/c/valheim/p/Searica/OpenSesame/)