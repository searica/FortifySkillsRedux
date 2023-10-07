
## Acknowledgements
This mod was originally made by Merlyn42, and the original can be found here: https://valheim.thunderstore.io/package/Merlyn42/FortifySkills/

This fork fixes one specific crash that I ran into while using the original, as well as implements ServerSync to sync the config file between the client and server, and will be taken down upon the original author's request, or if they fix the bug on the original mod.

Do not install both this and the original mod at the same time. This mod replaces the original.

Thanks to blaxxun-boop for their implementation of ServerSync.

## Mechanics

The Fortify skill level increases very slowly at first but if you get your current level significantly higher that your fortified level it will level a little quicker giving you an incentive not to die. Because of this your fortified level may fall a long way behind your current level if you stay alive for a long time and you can lose more than you would with the vanilla 5% penalty. To make up for this your current level increases a little faster than Vanilla as well.

There are two major gameplay advantages to this:

- A string of deaths won't destroy your skill level. No need to worry about the No Skill Drain buff ending just before you die.
- Less used skills won't wither away completely from the occasional death. If you use one weapon type a lot early game but then switch to something else, now a few deaths without training the original weapon skill won't completely reset it.

Your Fortify skill level will be displayed in brackets on your skill list.

## Notes

- I recommend backing up your character file from "%appdata%\..\LocalLow\IronGate\Valheim\characters" as this mod changes how those files are written.
- Your Fortify skill level will be set to 95% of your current skill level when you first install it so dying immediately will have the same effect as the base game.
- If you remove this mod your character will be fine, the fortify skill level will disappear and the current skill level will stay the same (including levels gained due to the faster levelling from this mod).
- This mod requires BepInEx


## Config Settings

    BonusXPRate: Defaults to 1.5. Controls how fast skills level up. 1.5 = 50% bonus experience gained.
    FortifyXPPerLevelRate: Defaults to 0.1. Controls how much experience the fortified skill recieves for each level behind the regular skill it is. For example, at 0.1, the fortified skill recieves 40% experience when it is 4 levels behind the main skill (10% per level).
    FortifyMaxXPRate: Defaults to 0.8. Acts as a maximum cap to the FortifyXPPerLevelRate.

## Installation

- Download the BepInEx Pack https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/﻿
- Download this mod and move the "FortifySkills.dll" & "ServerSync.dll" into "<GameLocation>\BepInEx\plugins"

## Source Code
