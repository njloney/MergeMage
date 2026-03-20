# Merge Mage

First-person spell-combat prototype focused on automatic crystal merging and a multi-phase boss encounter.

## Overview

Merge Mage is a systems-driven gameplay prototype built in Unity. The project focuses on first-person combat, modular gameplay systems, and encounter design rather than content scale.

Core gameplay revolves around merging spell crystals to create stronger abilities while navigating a boss encounter gated by player-activated pylons and wave-based pressure.

## Core Systems

- **Item & Spell System**  
  Modular system supporting spell crystals, consumables, cooldowns, and scaling values.

- **Hit Registration**  
  Unified system handling projectile and area-of-effect damage across enemies and boss entities.

- **Merge System**  
  Compatibility-based automatic merging of crystals when in merge mode.

- **Enemy AI**  
  Behavior-driven enemy system supporting combat states and crowd-control responses.

- **Boss Encounter**  
  Multi-phase encounter featuring a shield gated by proximity-based pylons and wave pressure.

- **Pylon System**  
  Player proximity charges pylons, disabling boss shields and driving encounter flow.

- **Wave Spawner**  
  Controls enemy spawn timing, pacing, and escalation during encounters.

- **Audio & UI Integration**  
  Gameplay-driven audio triggers and UI updates reflecting player state and actions.

## Project Structure

## Key Design Decisions

- Gameplay systems are modular and designed for extensibility
- Merge compatibility is evaluated independently of player timing
- Boss shield state is controlled through pylons rather than health thresholds
- Hit registration is shared across all damage sources

## Known Limitations

- Single-level prototype focused on system development
- Turret enemies may float due to grounding edge cases
- Environmental colliders are not fully aligned with visual geometry
- Dungeon areas lack unique mechanics beyond consumable rewards
- Minor mesh artifacts present in boss model

## What I’d Improve

- Expand pylon system into reusable encounter mechanics
- Add multi-encounter level progression
- Improve turret grounding using better surface detection
- Introduce more complex merge compatibility rules
- Add debugging tools for combat interactions
