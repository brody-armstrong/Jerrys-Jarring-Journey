# Jerry's Jarring Journey 

A 2D endless runner game where you ski downhill while being chased by an avalanche.

## How to Play

- **HOLD SPACEBAR** - Tuck and accelerate (build speed)
- **RELEASE SPACEBAR** - Slow down
- **Stay ahead of the avalanche!**

## Game Features

- **Procedurally generated hills** - Infinite terrain with varied slopes
- **Avalanche chase mechanic** - Dynamic threat that adapts to player speed
- **Tutorial system** - In-game prompts teach mechanics
- **Score system** - Distance-based scoring with persistent high scores
- **Visual feedback** - Color-coded crest detection, sprite swapping
- **Audio** - Avalanche rumble sound creates tension

## Technical Details

**Built with:** Unity 2022.3+ (2D)

**Key Scripts:**
- `PlayerController.cs` - Player movement, speed, and physics
- `AvalancheController.cs` - Chase AI and dynamic speed adjustment
- `HillGenerator.cs` - Procedural terrain generation
- `ScoreManager.cs` - Scoring and persistence system
- `TutorialManager.cs` - Tutorial prompt system
- `GameOverUI.cs` - Game over screen and scene transitions

## Project Structure

```
Assets/
├── Art/               # Sprites and visual assets
├── Audio/             # Sound effects
├── Scenes/            # Game scenes (TitleScene, SampleScene)
└── *.cs               # Game scripts
```

## Gameplay Flow

1. **Title Screen** - Press SPACE to start
2. **Tutorial** - Learn controls as you play
3. **Chase** - Build speed and stay ahead of the avalanche
4. **Game Over** - View score and try again

## Credits

**Developer:** Brody Armstrong
**Course:** Game Development CS 583 @SDSU
**Year:** 2025

Made with Unity 

