# Echo Protocol - Game Design Document

## 1. High Concept

**Echo Protocol** is a mobile auto-battler built in Unity. The player controls a main unit named **Echo**, who automatically fights waves of enemy bots. During battle, the player can spend crystals on a gacha-style banner to recruit allies or upgrade Echo, creating a simple loop of fighting, pulling, powering up, and progressing through levels.

## 2. Platform and Orientation

- **Platform:** Android mobile
- **Build Type:** APK
- **Screen Orientation:** Landscape
- **Input Style:** Touch UI only
- **Battle Input:** No direct unit control. Combat runs automatically.

## 3. Core Gameplay Loop

1. Player starts from the Main Menu.
2. Login button loads the Game Scene.
3. Echo spawns on the left side of the arena.
4. Enemy bots spawn on the right side.
5. Units automatically move, target, and attack.
6. Enemy kills reward crystals.
7. Player can open the Banner menu and spend crystals.
8. Banner rewards can upgrade Echo, add allies, or fail.
9. After each wave is cleared, Echo and allies return to their formation positions.
10. After all waves and the final boss are defeated, the player advances to the next level.

## 4. Game Modes and Scenes

### Main Menu Scene

- Contains the game title and Login button.
- Login button loads `GameScene`.

### Game Scene

- Main auto-battler scene.
- Contains battle, waves, gacha banner, pause, victory, defeat, and level progression.
- Runtime systems build the battle scene if needed.

## 5. Player Unit

### Echo

Echo is the main player unit.

Base stats:

- HP: `100`
- Attack Damage: `20`
- Attack Cooldown: `1.5s`

Special effects:

- Echo auto-targets the nearest living enemy.
- Echo attacks automatically when in range.
- When Echo kills an enemy, Echo heals for **10% of max HP**.
- Echo gains permanent HP/ATK buffs from banner upgrades during the run.

## 6. Ally Units

Allies can be gained from the banner system.

Base stats:

- HP: `60`
- Attack Damage: `15`
- Attack Cooldown: `2s`

Rules:

- Allies auto-target nearest enemies.
- Allies can earn crystals by killing enemies.
- Allies return to formation positions after each wave.
- Allies persist between levels as long as they survive.
- Maximum allies: `4`

## 7. Enemy Units

Enemies are automated bot units.

Base Level 1 stats:

- HP: `40`
- Attack Damage: `10`
- Attack Cooldown: `2s`

Rules:

- Enemies auto-target the nearest player-side unit, including Echo or allies.
- Enemy stats scale upward each level.
- Enemies have health bars above their heads.
- Damage taken is shown as floating damage numbers.

## 8. Final Boss

Each level ends with a final boss encounter after the normal waves.

Boss rules:

- Boss spawns after the final normal wave.
- Boss is larger than regular enemies.
- Boss has higher HP and damage.
- Boss has its own health bar.
- Defeating the boss completes the level.

## 9. Combat System

Each unit uses a simple FSM:

- **Idle:** Look for nearest valid target.
- **Attack:** Move into attack range and attack on cooldown.
- **Dead:** Disable the unit object.

Combat behavior:

- Units move automatically.
- Units stop near attack range instead of directly overlapping targets.
- Friendly units keep distance from each other to avoid stacking.
- No direct player combat input is required.

## 10. Health and Damage Feedback

### Player HP Bar

- Echo HP is shown at the bottom center of the screen.
- HP bar uses a UI Slider.
- HP value is displayed numerically.
- HP changes tween smoothly with DOTween.

### Enemy HP Bars

- Enemy health bars appear above enemy heads.
- Enemy HP is shown numerically.
- Health bars face the camera.

### Floating Damage Numbers

When a unit takes damage, a number appears on the damaged unit.

- Player-side damage numbers are blue.
- Enemy damage numbers are red.
- Numbers float upward and fade out over about one second using DOTween.

## 11. Gacha Banner System

The player can open the Banner menu during battle.

### Currency

Currency is called **Crystals**.

Sources:

- Starting crystals at the beginning of the run.
- Crystals gained when Echo or allies kill enemies.
- Bonus crystals after clearing each level.

### Pull Cost

- Single pull cost: `100 crystals`

### Possible Results

- Echo Upgrade: Adds HP and ATK.
- Rare Echo Upgrade: Adds larger HP and ATK bonuses.
- New Ally: Adds an ally to the player team.
- Nothing: Player receives no reward.

The "You receive nothing" result gives the banner a fail chance.

### Team Full Rule

If the player already has the maximum number of allies, a New Ally result becomes an Echo upgrade instead.

## 12. Level Progression

The game progresses up to **Level 5**.

Each level contains:

- Multiple enemy waves.
- One final boss.
- Level clear panel.
- Crystal reward for the next level.

After clearing a level:

- Echo and living allies keep their buffs.
- Echo and living allies reset to formation positions.
- Player receives a crystal bonus.
- Player can continue to the next level.

## 13. User Interface

### HUD

- Level and wave text at top left.
- Crystal count at top center.
- Pause button at top right.
- Banner button near top right.
- Echo HP bar at bottom center.

### Pause Panel

Buttons:

- Resume
- Main Menu

Pause behavior:

- Battle time freezes.
- UI buttons remain usable.

### Banner Panel

Buttons:

- Pull x1
- Close

Shows:

- Pull cost
- Pull result
- Fail chance messaging

### Level Clear / Victory Panel

Shows:

- Level clear message.
- Next level button if more levels remain.
- Restart button.
- Main Menu button.

### Lose Panel

Shows:

- Defeated title.
- Restart button.
- Main Menu button.

## 14. Visual Direction

Current placeholder visuals:

- Echo: blue capsule
- Allies: green capsules
- Enemies: red capsules
- Boss: larger enemy capsule
- Arena: simple sci-fi floor/wall/background

Asset replacement workflow:

- Visual prefabs are assigned in `Assets/Resources/GameVisualSettings.asset`.
- Echo, enemies, allies, and final bosses each have their own prefab slot.
- Each visual has editable local position, rotation, and scale offsets.

## 15. Technical Notes

Engine and tools:

- Unity
- DOTween
- TextMeshPro
- Unity UI

Important scripts:

- `GameManager.cs`: Waves, levels, spawning, combat rewards, win/lose flow.
- `Unit.cs`: HP, damage, cooldown, damage events, healing, death.
- `UnitFSM.cs`: Idle, Attack, Dead behavior.
- `UIManager.cs`: HUD, panels, HP bar, banner UI, DOTween UI animation.
- `GachaManager.cs`: Banner pull logic.
- `PlayerWallet.cs`: Crystal currency.
- `BattleBackground.cs`: Runtime arena background.
- `GameVisualSettings.cs`: Editable asset slots for imported visual prefabs.
- `MobileScreenSetup.cs`: Mobile orientation setup.

## 16. Mobile Build Requirements

- Game should run in landscape orientation.
- Portrait autorotation should be disabled.
- Camera should remain consistent between Editor and Android build.
- Runtime background materials use mobile-safe shaders to avoid purple materials.

## 17. Current Scope

Included:

- Main Menu to Game Scene flow.
- Auto-battle combat.
- Echo, enemies, allies, final bosses.
- Five-level progression.
- Gacha banner.
- Currency economy.
- DOTween UI and damage feedback.
- Mobile orientation setup.
- Replaceable visual asset settings.

Not currently included:

- Manual player movement.
- Inventory screen.
- Persistent save system.
- Audio system.
- Advanced enemy abilities.
- Full art polish.

## 18. Reports from Adnan

What Changed and Why?

-Animations are almost the most valuable thing in this genre and I couldnt add even basic character animations on this game. I think it's my own incompetence.
-I also couldnt implement the reward system I wanted to create in the project because I was undecided about what to offer the player as a reward.
-Changes to the core mechanics were necessary because the flow of the game could have been disrupted.
-In the prototype demo, the goals we set beforehand were further increased with the changes to the basic mechanics; the game consists of 3 waves and 1 boss per level and a total of 5 levels.

Usage of AI

-GameScene directly comes from script and made by my prompts with Cursor. Except Main Menu and Assets from asset store, I got assisted by AI.
