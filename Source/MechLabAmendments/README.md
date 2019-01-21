# MechLab Amendments

[Battletech Mod][ModTek][BETA] Campaign MechLab features save/load of mech definitions as well as a reset to stock loadout.

## Gameplay changes
- Implemented saving/loading Mech loadouts from Campaign MechLab
  - Current Mech is exported as a valid mechdef_xxx.json directly to "MechDefs/" in mod directory
  - After next game start these can be re-applied in MechLab
  - Loading of stock loadout is possible too
  - All saved MechDefs can potentially be fielded as enemies on subsequent missions
- Removed validation of the input to make custom command chars possible
  - It is in your resposibility to not risky chars for your Mechs name 

## How to
* Save/Load functionality can be triggered via the Nickname-Input of the current Mech
  * "/stock" will try to set current Mech to its stock loadout
  * "/save XXX" or "/export XXX" will try to save the current MechDef as "mechdef_{$mechname}_{$variant}_XXX.json"
  * "/load XXX" or "/apply XXX" will try to load the specified MechDef and apply its loadout

## Known issues
* The inventory of the loaded MechDef does not properly implement the work-order meta
  * That means that if a programmatically placed component is removed via drag/drop you will pay again (instead of getting the money back)
  * You pay for removal/install of a component even though it could have stayed at a specific location

## Thanks
* CptMoore
* CWolf
* mpstark
* Morphyum
* pardeike
* HBS
