# Mod Name — DECIDED

## ✅ Chosen Name
**Stop Shuffling, You Fools - Formation Manager**

- **Mod ID / folder**: `FormationManager`
- **Assembly name**: `FormationManager`
- Brand consistent with: PartyManager, EquipmentManager, FiefManager, SmithingOptimizer, TradeOptimizer

---

## Name Research Notes

### Existing mods to differentiate from
- **Better Troop Formations** — generic, abandoned
- **Partyscreen Formation Assignments** — clinical, unmemorable, no longer maintained
- **Army Formations Made Easy** — bland

### Rejected candidates (kept for reference)

| Name | Reason rejected |
|---|---|
| "You, Stand There!" | Great but lacks "formation" for searchability |
| "Hear Me!" | Authentic in-game bark, but too cryptic |
| "Listen to Me, Peasant" | Funny but slightly mean-spirited |
| "Order Has Left This Party" | Clever but no "formation" keyword |
| "Assigned Seating" | Best alt — explains itself, funny, but no "formation" keyword |
| "No One Leaves This Party" | Good double meaning, no "formation" keyword |
| "Stop Shuffling, You Fools" | Great but standalone — adding "Formation Manager" makes it searchable AND brand-consistent |

---

## Voice Line Research Notes

From `voice_definitions.xml` (`Native\ModuleData`), player character battle command voice types:
- **Identifiers**: `Infantry`, `Cavalry`, `Archers`, `HorseArchers`, `Everyone`, `Mixed`
- **Generic Orders**: `Move`, `Follow`, `Charge`, `Advance`, `FallBack`, `Stop`, `Retreat`, `Mount`, `Dismount`, `FireAtWill`, `HoldFire`, `FaceEnemy`, `FaceDirection`, `CommandDelegate`, `CommandUndelegate`
- **Formation Shape Orders**: `FormLine`, `FormShieldWall`, `FormLoose`, `FormCircle`, `FormSquare`, `FormSkein`, `FormColumn`, `FormScatter`

`CommandUndelegate` is likely the voice type that fires "Hear me!" — the player asserting direct command.
Actual audio is compiled binary; text not in human-readable XML.
