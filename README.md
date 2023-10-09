## FortifySkillsRedux
FortifySkillsRedux is a remake of the FortifySkills mod for Valheim that changes how skills are lost on death. Rather than being punished for dying by losing a flat 5% of every skill, you are instead rewarded for staying alive for longer. This is achieved by adding a new fortified skill level for each skill that is used when you die to reset your skills to their fortified skill level. This means that no matter how many times you die, your skills will never drop below their fortified skill levels.

## Acknowledgements
The original mod this one is based on was made by Merlyn42 and a patched version was made by Remeil. This mod is a complete rewrite of the original though as the original stopped working several game updates back and has no license. My thanks to Merlyn42 for the original idea though. Also, thanks to blaxxun-boop for their implementation of ServerSync.

## Installation
**Via Mod Manager (Recommended)**
- The best way to install the mod is using r2modman and installing it from Thunderstore.
- The next best way is to use Thunderstore Mod Manager.

**Manual**
- Download the BepInEx Pack https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/
- Download this mod and move the "FortifySkills.dll" into "<GameLocation>\BepInEx\plugins"

## Mechanics
The Fortify skill level increases very slowly at first but if you get your active level significantly higher that your fortified level it will level a little quicker giving you an incentive not to die. Because of this your fortified level may fall a long way behind your active level if you stay alive for a long time and you can lose more than you would with the vanilla 5% penalty. To make up for this your active level increases a little faster than Vanilla as well.

There are two major gameplay advantages to this:

- A string of deaths won't destroy your skill level. No need to worry about the No Skill Drain buff ending just before you die.
- Less used skills won't wither away completely from the occasional death. If you use one weapon type a lot early game but then switch to something else, now a few deaths without training the original weapon skill won't completely reset it.

Your Fortify skill level will be displayed in parenthesis in your skill list next to your active skill level.

## Config Settings

### Global Section
**EnableMod** [Restart Required]
- Globally enable or disable this mod.
- Default value: true

**LockConfiguration**
- If true, the configuration is locked and can be changed by server admins only.
- Default value: true

### Mechanics Section
**XPMult**
- Used to control the rate at which the active level increases, 1=base game, 1.5=50% bonus xp awarded, 0.8=20% less xp awarded.
- Default value: 1.5

**FortifyXPPerLevelRate**
                0.1f,
- "Used to control the rate at which the fortified skill XP increases PER LEVEL behind the active level. 0.1=Will gain 10% XP for every level behind the active level. Note that this is a percentage of the XP earned towards the active skill after the XPMult value has been applied.
- Default value: 0.1

**FortifyXPRateMax**
- Used to control the maximum rate of XP earned for the fortified skill. Caps FortifyXPPerLevelRate. Values less than 1 mean the fortify skill will always increase more slowly than the active level. 0.8=Will gain a max of 80% of the XP gained for the active skill.
- Default value: 0.8

**EnableIndividualSettings**
- Used to toggle whether the XPMult value from the Mechanics section is used for all skills or if the XPMult values from the IndividualSKills section are used for each vanilla skill (skills added by mods are always modified based on the XPMult value from the Mechanics section).
- Default value: false.

### IndividualSkills Section
There is one entry in this section for each skill in the Vanilla game.

**SkillName**
- XP Multiplier for {skillName} skill. Only used if EnableIndividualSettings is set to true.
- Default value: 1.5


**ModdedSkillXPMult**
- XP Multiplier for skills added by mods (default value is 1.0 since most skill mods have their own XP multipier settings). Only used if EnableIndividualSettings is set to true.
- Default value: 1.0


## Compatibility
- All skill mods by Smoothbrain.
  - The XP multiplier settings in this mod stacks multiplicatively with the XP multiplier in Smoothbrain's skill mods.
  - If you set EnableIndividualSettings to True and keep ModdedSkillXPMult set to 1.0 it will not impact the XP gain rates of Smoothbrain's skill mods while still letting you customize the skill gain rates for Vanilla skills via the IndividualSkills XP multiplier settings.

**Incompatibilities**
- May have issues with anything that changes the SkillsDialog text in-game.

## Notes
- You don't have to install this mod on the server you play on, it is able to work as a purely client-side mod. Installing the mod on the server is only necessary if you want to enforce the same configuration for all players.
- I recommend backing up your character file from "%appdata%\..\LocalLow\IronGate\Valheim\characters" as this mod changes how those files are written.
- Your Fortify skill level will be set to 95% of your current skill level when you first install it so dying immediately will have the same effect as the base game.
- If you remove this mod your character will be fine, the fortify skill level will disappear and the current skill level will stay the same (including levels gained due to the faster levelling from this mod).
- This mod requires BepInEx.

## Source Code
https://github.com/searica/FortifySkillsRedux

## Donations/Tips
My mods will always be free to use but if you feel like saying thanks you can tip/donate here: https://ko-fi.com/searica

### Contributions
If you would like to provide suggestions, make feature requests, or reports bugs and compatability issues you can either open an issue on the Github repository or tag me (@searica) with a message on the [Odin Plus discord](https://discord.gg/mbkPcvu9ax).

I'm a grad student and have a lot of personal responsibilities on top of that so I can't promise I will respond quickly, but I do intend to maintain and improve the mod in my free time.

