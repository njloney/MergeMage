Merge Mage

First-person spell-combat prototype focused on automatic crystal merging and a pylon-gated boss encounter.

Core Systems Implemented

Modular item and spell-casting systems

Unified hit registration pipeline

Compatibility-based automatic merge system

Enemy AI and boss encounter logic

Proximity-based pylon shield mechanic

Wave spawning and pacing control

Architecture Overview

Gameplay logic is separated from tuning data using ScriptableObjects.
Combat resolution is unified across enemies and boss entities.
Encounter state is controlled through decoupled systems (boss logic does not directly manage waves or pylons).

Key Design Decisions

Merge compatibility evaluated independently of player timing

Boss shield state controlled via external pylon system

Hit registration shared across all damage sources

Known Limitations

(Short version of what we wrote earlier)

What I’d Improve

Expand pylon system into reusable encounter framework

Improve turret grounding via refined surface detection

Add deterministic combat replay tools
