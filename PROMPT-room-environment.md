# Task: Build a 3D Room/Booth Environment

## Context

The balloon board currently floats in front of a procedurally generated cork/frame/floor environment built from Unity primitives (Quads and Cubes). The project just pivoted to imported 3D assets — the perspective camera task (see `PROMPT-perspective-camera.md`) should be done first, since the room geometry should be designed around the camera's perspective view.

Read `CHANGELOG.md` for full context on the recent asset-based visual overhaul.

## The Concept

**"Frame on wall"**: The balloon grid is a panel/board mounted on the back wall of a carnival game booth or arcade alcove. The player looks into the booth from a few meters away and throws darts at the board. Moving between rooms = changing room dressing, lighting, and props.

The goal is to make the scene feel like you're looking into a real physical space, not at a flat 2D game screen.

## Approach Options (Evaluate and Choose)

1. **Minimal geometry approach**: 5 planes (back wall, two side walls, ceiling, floor) with imported or procedural textures. Fast to implement, gives depth with perspective camera. Start here.

2. **Free asset approach**: Find a free room/interior asset on Unity Asset Store or Sketchfab (CC-BY). Research from this session found:
   - Arcade Room Interior ($9.99) — retro enclosed room, closest match
   - Dartboard by studiolab ($4.99, URP compatible)
   - Various free interior assets on Sketchfab
   - User is willing to spend money on assets if needed

3. **Simple modeled booth**: A wooden carnival booth frame with counter, back panel for the balloon board, and side walls. Could be built from Unity primitives with better materials/textures.

## Key Files

- `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs` and its partials:
  - `.Frame.cs` — metal frame geometry (SetPanel helpers)
  - `.Materials.cs` — material creation (procedural textures + unlit variants)
  - `.Atmosphere.cs` — currently emptied (fog/vignette disabled)
- `Assets/Scripts/BalloonGame/Visuals/ProceduralTextures.cs` — generates cork, floor, frame textures at runtime. May be replaced by imported textures.
- `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs` — currently 3 neutral directional lights. Should be adjusted for room geometry (maybe add point lights in the booth).

## Requirements

- The balloon board should be mounted on the room's back wall
- Side walls should create natural framing and depth
- Floor should extend from the board toward the camera/player position
- A ceiling (even simple) helps sell the "enclosed space" feeling
- The room should work with the perspective camera (see companion task)
- Lighting should complement the room geometry — consider a warm overhead light in the booth
- The Loafbrr balloon materials use PBR (ORM shader) so they benefit from environment reflections and proper lighting

## What Exists Now

- Cork board: procedural texture on a Quad
- Metal frame: Cubes with procedural weathered texture
- Corner bolts: small metallic Cubes
- Floor: procedural wood texture on a Quad
- Lane stripes: 4 colored glowing Quads
- All created at runtime in `EnvironmentBuilder.Awake()`

## Asset Store Research Already Done

No ready-made "carnival dart booth" asset exists. The booth geometry is simple — a box with a back wall. The balloons and darts (the complex parts) are already handled. Focus on making a convincing room/space, not on finding a perfect pre-made asset.

The Loafbrr balloon pack is already imported at `Assets/LoafbrrAssets/Balloons/`. It contains 49 balloon variants, PBR materials, and textures that look good under proper lighting.
