# Undercooked - Project Context

## Project Overview

**Undercooked** is a fan-made vertical slice recreation of the popular game *Overcooked*. It focuses on core kitchen mechanics like chopping, cooking, plating, and serving orders under time pressure. The project is built with Unity and emphasizes a tight game loop, clean code architecture, and faithful recreation of the original game's feel.

## Technical Details

- **Engine:** Unity 6000.3.24f1
- **Language:** C#
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Input:** Unity Input System (Supports Keyboard & Gamepads)
- **Key Packages:**
  - `com.unity.cinemachine`: Dynamic camera control.
  - `com.unity.inputsystem`: Local multiplayer input handling.
  - `com.unity.probuilder`: In-editor level modeling.
  - `com.unity.ai.navigation`: NavMesh agents.
  - `com.unity.formats.fbx`: FBX export support.

## Architecture & Code Structure

The main game logic resides in `Assets/Scripts` and is wrapped in the `Undercooked` assembly definition (`Undercooked.asmdef`).

### Key Directories

- **`Assets/Scripts/Player/`**:
  - `PlayerController.cs`: Main player logic (movement, state).
  - `InputController.cs`: Bridges Unity Input System events to player actions.
  - `InteractableController.cs`: Handles picking up, dropping, and interacting with objects.
- **`Assets/Scripts/Managers/`**:
  - `GameManager.cs`: Controls the game loop (start, timer, end).
  - `OrderManager.cs`: Manages recipe generation, delivery validation, and scoring.
  - `CameraManager.cs`: Interfaces with Cinemachine.
- **`Assets/Scripts/Appliances/`**: Logic for kitchen tools (Stoves, Cutting Boards, Sinks, Trash).
- **`Assets/Scripts/Data/`**: ScriptableObjects for Ingredients, Recipes, and Levels.
- **`Assets/Scripts/UI/`**: UI logic, likely utilizing LeanGUI/LeanTransition.

## Development Conventions

- **Asynchronous Logic:** Uses Coroutines and async patterns.
- **Interaction System:** Heavily relies on `IPickable` interface and `Interactable` abstract class.
- **Pattern Matching:** Used extensively for handling interactions between different item types.
- **Assets:** Custom assets located in `Assets/Art`, `Assets/Audio`, etc.

## Building and Running

1.  **Open Project:** Open the root directory in Unity 6000.3.24f1.
2.  **Scene:** The main gameplay scene is likely located in `Assets/Scenes` (e.g., related to `LevelData-1-1`).
3.  **Play:** Enter Play Mode in the Editor to test.
4.  **Build:** Standard Unity Build Settings (File > Build Settings) for the target platform.
