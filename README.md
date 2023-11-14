## FortifySkillsRedux
FortifySkillsRedux is a remake of the FortifySkills mod for Valheim that changes how skills are lost on death. Rather than being punished for dying by losing a flat 5% of every skill, you are instead rewarded for staying alive for longer. This is achieved by adding a new fortified skill level for each skill that is used when you die to reset your skills to their fortified skill level. This means that no matter how many times you die, your skills will never drop below their fortified skill levels.

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

## Configuration Settings

### Global Section
**Verbosity**
- Low will log basic information about the mod. Medium will log information that is useful for troubleshooting. High will log a lot of information, do not set it to this without good reason as it will slow down your game.
    - Acceptable values: Low, Medium, High
    - Default value: Low.

**EnableIndividualSettings [Synced with Server]**
- Used to toggle whether the XPMult value from the Mechanics section is used for all skills or if the XPMult values from the IndividualSKills section are used for each vanilla skill (skills added by mods are always modified based on the XPMult value from the Mechanics section).
    - Acceptable values: False, True
    - Default value: false.

### Mechanics
**XPMult [Synced with Server]**
- Used to control the rate at which the active level increases, 1=base game, 1.5=50% bonus xp awarded, 0.8=20% less xp awarded.
    - Default value: 1.5

**FortifyXPPerLevelRate [Synced with Server]**
- "Used to control the rate at which the fortified skill XP increases PER LEVEL behind the active level. 0.1=Will gain 10% XP for every level behind the active level. Note that this is a percentage of the XP earned towards the active skill before any XP multipliers have been applied.
    - Default value: 0.1

**FortifyXPRateMax [Synced with Server]**
- Used to control the maximum rate of XP earned for the fortified skill. Caps FortifyXPPerLevelRate. Values less than 1 mean the fortify skill will always increase more slowly than the active level. 0.8=Will gain a max of 80% of the XP gained for the active skill.
    - Default value: 0.8

### IndividualSkills Section
There is one entry in this section for each skill in the Vanilla game.

**SkillName_XPMult [Synced with Server]**
- XP Multiplier for {skillName} skill. Only used if EnableIndividualSettings is set to true.
    - Default value: 1.5

**ModdedSkill_XPMult [Synced with Server]**
- XP Multiplier for skills added by mods (default value is 1.0 since most skill mods have their own XP multipier settings). Only used if EnableIndividualSettings is set to true.
    - Default value: 1.0


## Compatibility
- All skill mods by Smoothbrain.
  - The XP multiplier settings in this mod stacks multiplicatively with the XP multiplier in Smoothbrain's skill mods.
  - If you set EnableIndividualSettings to True and keep ModdedSkillXPMult set to 1.0 it will not impact the XP gain rates of Smoothbrain's skill mods while still letting you customize the skill gain rates for Vanilla skills via the IndividualSkills XP multiplier settings.

**Incompatibilities**
- May have issues with anything that changes the SkillsDialog text in-game.

## Notes
- You don't have to install this mod on the server you play on, it works as a purely client-side mod. Installing the mod on the server is only necessary if you want to enforce the same configuration for all players.
- If you want to be extra cautious you can back up your character file from "%appdata%\..\LocalLow\IronGate\Valheim\characters" as this mod changes how those files are written.
- Your Fortify skill level will be set to 95% of your current skill level when you first install it so dying immediately will have the same effect as the base game.
- If you remove this mod your character will be fine, the fortify skill level will disappear and the current skill level will stay the same (including levels gained due to the faster leveling from this mod).

## Donations/Tips
My mods will always be free to use but if you feel like saying thanks you can tip/donate.

| My Ko-fi: | [![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/searica) |
|-----------|---------------|

## Source Code
Source code is available on Github.

| Github Repository: | <button style="font-size:20px"><img height="18" src="https://github.githubassets.com/favicons/favicon-dark.svg"></img><a href="https://https://github.com/searica/FortifySkillsRedux"> FortifySkillsRedux</button> |
|-----------|---------------|

### Contributions
If you would like to provide suggestions, make feature requests, or reports bugs and compatibility issues you can either open an issue on the Github repository or tag me (@searica) with a message on my discord [Searica's Mods](https://discord.gg/sFmGTBYN6n).

I'm a grad student and have a lot of personal responsibilities on top of that so I can't promise I will respond quickly, but I do intend to maintain and improve the mod in my free time.

### Credits
This mod is based on the original one made by Merlyn42 and the patched version was made by Remeil. This mod is a complete rewrite of the original though as the original stopped working several game updates back and has no license. My thanks to Merlyn42 for the original idea though! Also, many thanks to the developers of J�tunn for all their work making the library.

### Shameless Self Plug (Other Mods By Me)
If you like this mod you might like some of my other ones.

#### Building Mods
- [More Vanilla Build Prefabs](https://valheim.thunderstore.io/package/Searica/More_Vanilla_Build_Prefabs/)
- [Extra Snap Points Made Easy](https://valheim.thunderstore.io/package/Searica/Extra_Snap_Points_Made_Easy/)
- [BuildRestrictionTweaksSync](https://valheim.thunderstore.io/package/Searica/BuildRestrictionTweaksSync/)

#### Gameplay Mods
- [CameraTweaks](https://valheim.thunderstore.io/package/Searica/CameraTweaks/)
- [DodgeShortcut](https://valheim.thunderstore.io/package/Searica/DodgeShortcut/)
- [ProjectileTweaks](https://github.com/searica/ProjectileTweaks/)
- [SkilledCarryWeight](https://github.com/searica/SkilledCarryWeight/)
- [SafetyStatus](https://valheim.thunderstore.io/package/Searica/SafetyStatus/)