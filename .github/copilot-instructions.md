# GitHub Copilot Instructions - Beergeon

This repository contains project-specific instructions for GitHub Copilot. 

## Context Trigger
When the user includes the string `@codeContext:` in their prompt, prioritize the following project details and standards to ensure all generated code and advice is aligned with the project's architecture and vision.

---

# Project Context: Beergeon

## Project Overview
**Beergeon** is a first-person dungeon crawler set in a surreal, decaying urban underworld. The game combines 3D environments with 2D billboard sprites, creating a unique aesthetic inspired by '90s and early 2000s underground games and animation.

## Technical Pillars

### Visual Style
- **Hybrid Rendering**: 3D level geometry paired with 2D pixel-art sprites (billboarding) for characters and items.
- **Aesthetic**: Gritty, neon-lit, "humor meets horror."
- **Atmosphere**: Starts as a decaying metro system and descends into nightmarish, surreal hellscapes.

### Core Gameplay Loop
- **Exploration**: Navigating interconnected, procedural-like tunnels and chambers.
- **Combat**: First-person real-time combat against exaggerated enemies.
- **Progression**: Collecting loot, upgrading gear, and trading with mysterious merchants.

### Inventory & Items
- **Structure**:
  - `ScriptableObjects` (`EquippableItem`, `WeaponItem`, `PotionItem`) define item data.
  - `Inventory` (Singleton) manages the list of items and the **Active Slot**.
- **Player Hand**:
  - `PlayerHand` listens to `Inventory` events.
  - Visuals change dynamically based on the active item (Weapon vs Potion vs Empty Hand).
  - Empty Hand defaults to "Open Hand" (idle) and "Fist" (attack).
- **Inputs**:
  - **Scroll**: Changes active slot.
  - **Q**: Drops the active item (instantiates `Pickup Prefab`).
  - **E**: Interacts/Pickups items.
  - **Click**: Uses the active item (Attack/Consume).
- **Pickups**:
  - World items use `ItemPickup` component and `IInteractable` interface.
  - Auto-scaling is handled via `OnValidate` in `ItemPickup`.

## Programming Standards & Best Practices

### Code Style
- **Naming Conventions**:
  - `PascalCase` for classes, methods, properties, and public fields.
  - `camelCase` for local variables and private fields (prefix private fields with `_` is optional but consistent usage is key).
- **Comments**: Use XML documentation (`///`) for public APIs and complex logic. Keep comments concise and focused on *why*, not *what*.

### Architecture
- **Modularity**: Keep systems decoupled.
  - `Player` logic should be separate from `UI` and `Inventory`.
  - Use events or signals for cross-system communication where possible to reduce dependencies.
- **ScriptableObjects**: Use `ScriptableObject` for data definitions (Items, Enemy Stats, Skills) to keep data separate from logic.

### File Structure
Maintain the existing `Assets/` directory structure:
- `Scripts/`: Logic, organized by feature (e.g., `Player/`, `Enemy/`, `Systems/`).
- `Art/`: All visual assets.
- `Prefabs/`: Pre-configured GameObjects.
- `Data/`: ScriptableObject assets.

### Git Workflow
- **Branches**: Use `feature/` or `bug/` prefixes for branches.
- **Commits**: Write clear, descriptive commit messages in the present tense (e.g., "Add player jump mechanic" not "Added jumping").

## Goal
The primary engineering goal is to build a scalable, maintainable codebase that supports rapid iteration on gameplay features while maintaining high performance and stability.
