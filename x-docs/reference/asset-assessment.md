# Inkshot Scene Composition — Asset Assessment

Cross-reference of the visual reference against the Unity Asset Library Index (4,119 assets across 26 packages). Organized by scene element with specific asset recommendations, gap analysis, and custom work required.

---

## Scene Breakdown (from visual reference)

The reference shows four screens of a dark, moody balloon-popping roguelite:

1. **Gameplay view** — grid of colorful metallic balloons, dark atmospheric background, neon paint splatter/drip effects, HUD overlay
2. **Perk selection** — dark card UI panels with icons and descriptions
3. **Pop explosion** — massive particle burst of colored fragments, sparks, and paint
4. **Room transition** — "ROOM 13 / THE NEON MAZE" title card, dark industrial floor, atmospheric fog

---

## Balloons (Grid Objects)

**Primary source: Balloons pack (Loafbrr) — already imported**

| Asset | Use | Notes |
|-------|-----|-------|
| `Balloon_Balloon` (prefab) | Base balloon mesh for the grid | Standard round balloon shape |
| `Balloon_Star` (prefab) | Special/bonus balloon type | Star-shaped variant |
| `Balloon_Heart` (prefab) | Special/bonus balloon type | Heart-shaped variant |
| `Balloons_Normal_1K` (texture) | Normal map for surface detail | Gives the glossy, reflective look |
| `Balloons_ORM_1K` (texture) | Occlusion/Roughness/Metallic map | Critical for the metallic sheen visible in the reference |
| `Balloons_Diffuse_1K_white` (texture) | Base diffuse — tint per-material | White base allows runtime color tinting |
| `Balloons_Diffuse_1K_Gold` (texture) | Gold balloon variant | For special/high-value balloons |
| `Balloons_Diffuse_1K_silver` (texture) | Silver balloon variant | For armored/special balloons |
| `Balloon_Gold` (material) | Gold material preset | Reference for metallic material setup |
| `Balloon_Silver` (material) | Silver material preset | Reference for metallic material setup |
| `Balloon_White` + Variants 1-5 (materials) | Color variant templates | Clone and retint for game colors |
| `ORM_Shader` (shader) | Pack's custom ORM shader | Evaluate vs URP Lit for balloon rendering |

**Custom work needed:**
- `BalloonLit.shader` — custom URP shader with emission for neon glow, rim lighting for the glossy look visible in the reference. The stock materials won't achieve the saturated, backlit neon appearance without emission.
- Per-type `BalloonTypeSO` color definitions — green, blue, red/orange, purple, gold, silver tints applied to the white diffuse base.
- Balloon pop/deflate animation (procedural or authored).

---

## Background & Environment

### Skybox / Atmospheric Background

**Primary source: AllSky Free — 10 Sky Skybox Set**

| Asset | Use | Notes |
|-------|-----|-------|
| `Deep Dusk` (6-face cubemap + material) | Dark moody sky backdrop | Best match for the dark blue-grey atmosphere in the reference |
| `Cold Night` (6-face cubemap + material) | Alternative dark sky | Cooler, darker variant |
| `Night Moon Burst` (6-face cubemap + material) | Dramatic dark sky with highlights | Good for rooms with more dramatic lighting |

**Also consider: FREE Skybox Extended Shader (BOXOPHOBIC)**

| Asset | Use | Notes |
|-------|-----|-------|
| `Skybox Cubemap Extended Blend` (material) | Procedural gradient skybox | Zero texture memory, can blend between two cubemaps at runtime for room transitions |
| `Skybox Cubemap Extended Day` (material) | Preset example | Reference for shader setup |

**Verdict:** The `Deep Dusk` cubemap from AllSky gets you 80% there. Layer the Extended Shader's blend capability on top for smooth room-to-room sky transitions. The visual reference background is extremely dark — you may want to darken the cubemap further via exposure or a custom tint.

### Floor / Ground Plane

**Primary source: New Dungeon Pack Modular Low Poly Free**

| Asset | Use | Notes |
|-------|-----|-------|
| `_Floor` (texture) | Dark stone floor | Matches the industrial/dungeon floor in the room transition screen |
| `_FloorRock` (prefab) | 3D floor mesh with texture | Ready-to-use floor piece |
| `_Wall` (texture + material) | Dark wall segments | If you need visible walls at screen edges |
| `_Beam` (texture + material + prefab) | Structural beams | Environmental framing |

**Also consider: Yughues Free Ground Materials**

| Asset | Use | Notes |
|-------|-----|-------|
| `T_YFGM_Mars` (texture set) | Dark reddish ground | Alternative moody ground texture |
| `T_YFGM_Lava_e` (texture) | Emissive lava texture | Could work for special room floor effects |

**Custom work needed:**
- The reference shows a perspective floor receding into darkness with atmospheric fog. This needs a simple ground plane mesh + URP fog or a gradient fade shader — not complex geometry.

---

## VFX & Particles

### Pop / Explosion Effects

**Primary source: Hit Impact Effects FREE**

| Asset | Use | Notes |
|-------|-----|-------|
| `Hit_04` (prefab) | Flash + shockwave on balloon pop | Best match — cool-styled flash with radial shockwave |
| `HIE_Hit_01_Flash_01` (material + texture) | Bright flash sprite | Core pop flash |
| `HIE_Hit_03_Shockwave_2x2_01` (material + texture) | Expanding ring shockwave | For combo pops |
| `HIE_Hit_01_PointGlow_01` (material) | Point glow effect | Balloon highlight before pop |
| `HIE_Hit_01_Smoke_01` (material) | Smoke puff | Post-pop smoke wisp |
| `HIE_PointBlur_01` (texture) | Soft glow texture | Reusable for custom particle materials |
| `HIE_Smoke_01` (texture) | Smoke sprite sheet | Atmospheric wisps |
| `HIE_Shockwave_01` (texture) | Shockwave ring texture | For expanding ring particles |

**Also needed but not in library (claimed, not yet imported):**

| Pack | What to grab | Use |
|------|-------------|-----|
| **Cartoon FX Remaster Free** | Explosion prefabs, confetti bursts | The massive color fragment burst in screen 3 of the reference — this is the closest match in the library |
| **Free Quick Effects Vol. 1** | Portal/projectile trails | Dart trail effects |
| **Hovl Studio Magic Effects Free** | Energy/sparkle bursts | Combo/multiplier celebration effects |

**Custom work needed:**
- The neon paint splatter/drip effect (green and yellow splatters running down the screen) is **not covered by any library asset**. This requires:
  - Custom splatter textures (paint drip sprites)
  - Particle system spawning decal-like quads on balloon pop
  - Possibly a screen-space drip shader for the running paint effect
- The massive fragment burst in screen 3 needs custom color-matched particles even with Cartoon FX as a base — tint particles to match the popped balloon's color.

### Atmospheric Effects

**Gap — needs custom work:**
- Fog/haze particles (simple soft sprites with slow drift)
- Ambient dust motes
- Vignette darkening at screen edges (URP post-processing, no asset needed)

The `HIE_Smoke_01` texture can be repurposed for ambient fog particles.

---

## UI Elements

### HUD (Score, Darts, Room Number)

**Primary source: Kenney UI Pack — already imported**

The Kenney UI Pack provides panel backgrounds, progress bars, and button shapes. The reference shows a minimal, dark HUD with:
- Room number (top left)
- Score + target score (top right)
- Darts remaining counter (top right)

**Also useful: Clean Vector Icons — claimed**

| Asset | Use | Notes |
|-------|-----|-------|
| `T_9_kunai_` | Dart/projectile icon | Closest match to a throwing weapon icon |
| Various weapon/tool icons | Perk icons | Browse the full icon set for perk card imagery |

**Also useful: Game GUI Buttons (BraveWarrior)**

| Asset | Use | Notes |
|-------|-----|-------|
| `Star` textures | Score/achievement icons | Multiple star variants available |
| `Trophy` textures | Completion/reward icons | Warm-toned trophy sprites |
| `WoodenBG-resized` | Panel background texture | Dark wood texture for card backgrounds (with tinting) |

### Perk/Reward Cards

**Custom work needed — no direct match in library.**

The reference shows dark translucent card panels with:
- Colored icon at top
- Title text
- Description text
- Subtle border glow

Build these with:
- Unity UI `Image` with dark semi-transparent sprite (Kenney UI panel, darkened)
- TextMeshPro for title + description
- Outline/glow via UI shader or secondary image layer
- Icons from Clean Vector Icons or Kenney Game Icons (already imported)

### Room Transition Screen

The "ROOM 13 / THE NEON MAZE" screen uses:
- Large display font (check Kenney Fonts — already imported)
- Subtitle/flavor text
- Dark atmospheric background (reuse skybox + floor setup)

---

## Audio

**Primary source: FREE Casual Game SFX Pack (DM-CGS series) — claimed**

50 casual game SFX clips. Likely covers:
- Balloon pop sounds (multiple "click/pop" variants)
- UI button press
- Score tick
- Combo chime
- Room clear jingle

**Gaps — need to claim/import:**

| Pack | What it covers |
|------|---------------|
| **Free Sound Effects Pack** | Broader SFX for impacts, whooshes (dart flight) |
| **UI SFX Free Pack** | Menu transitions, card reveals |
| **Free Casual & Relaxing Music** | Background music loops |

---

## Dart (Projectile)

**No suitable dart model exists in the library.** The closest semantic matches (kunai icon, spear sprite) are 2D only.

**Options:**
1. **Model it** — a dart is geometrically simple (cone + cylinder + fins). ProBuilder or a quick Blender export.
2. **Procedural** — the current game may already use a procedural/primitive dart mesh. Check existing `Darts/` scripts.
3. **External** — Kenney's game assets or a free Sketchfab model.

---

## Slingshot / Launch Mechanism

**No slingshot model in the library.** This is a custom visual element:
- `SlingshotInput` already handles gameplay
- Visual band rendering is handled by `Visuals/` scripts
- The reference shows an aiming line / trajectory preview — purely code-driven

---

## Shaders & Rendering

**Available:**

| Asset | Use | Notes |
|-------|-----|-------|
| `(URP) Simple Toon Shader` — already imported | Cel-shading option | Could use for UI elements or stylized balloon rendering |
| `ORM_Shader` (Balloons pack) | ORM material rendering | Evaluate for balloon metallic look |

**Custom work needed:**
- `BalloonLit.shader` — emission + rim light + metallic for the neon-glow balloon appearance
- Paint drip/splatter shader (screen-space or decal)
- Possible bloom/glow post-processing profile (URP Volume — no asset needed, just configuration)

---

## Priority Import List

Assets to import into the Inkfall project, ordered by impact:

### Tier 1 — Import Now (blocking scene composition)

| Pack | Assets to grab | Reason |
|------|---------------|--------|
| **AllSky Free** | `Deep Dusk` cubemap + material, `Cold Night` cubemap | Background atmosphere |
| **Hit Impact Effects FREE** | All prefabs + textures | Balloon pop VFX base |
| **New Dungeon Pack** | `_Floor`, `_FloorRock`, `_Wall` textures + materials | Room environment |

### Tier 2 — Import Soon (polish & juice)

| Pack | Assets to grab | Reason |
|------|---------------|--------|
| **Cartoon FX Remaster Free** | Explosion/confetti prefabs | Big pop celebration effects |
| **FREE Casual Game SFX Pack** | Full pack (50 clips) | Pop, click, chime, jingle sounds |
| **FREE Skybox Extended Shader** | Blend shader + materials | Runtime sky transitions between rooms |
| **Game GUI Buttons** | Star, Trophy icons | Reward/perk card decoration |

### Tier 3 — Import Later (audio, extra polish)

| Pack | Assets to grab | Reason |
|------|---------------|--------|
| **Free Sound Effects Pack** | Impact/whoosh clips | Dart flight and collision audio |
| **Free Quick Effects Vol. 1** | Trail/projectile effects | Dart trails |
| **Hovl Studio Magic Effects Free** | Sparkle/energy bursts | Combo celebration |
| **Yughues Free Ground Materials** | Dark ground textures | Room floor variety |

---

## Custom Assets Required (Not in Library)

These elements from the visual reference have no library coverage and must be authored:

| Element | Complexity | Approach |
|---------|-----------|----------|
| Neon paint splatter/drip textures | Medium | Hand-paint 4-6 splatter sprites in Photoshop/Procreate, or generate with AI image tool |
| Paint drip screen effect | Medium | Screen-space particle system or scrolling UV shader |
| Dart 3D model | Low | ProBuilder or Blender — simple cone+cylinder geometry |
| Balloon glow shader | Medium | Custom URP Lit variant with emission, rim light, fresnel |
| Room transition animations | Low | DOTween sequences (already imported) |
| Perk card UI layout | Low | Unity UI + TextMeshPro composition |
| Atmospheric fog particles | Low | Simple soft-sprite particle system, reuse `HIE_Smoke_01` texture |

---

## Style Coherence Notes

The visual reference has a very specific aesthetic: **dark, moody atmosphere + hyper-saturated neon game objects**. Key considerations:

- **Color temperature contrast** — background is cool/neutral, game objects are warm/saturated. The library's balloon materials default warm, which works. Skybox assets default cool, which also works.
- **Metallic/glossy look** — the balloons in the reference have visible specular highlights and appear almost chrome-like. The Balloons pack includes ORM textures which support this, but you'll need careful material tuning (high metallic, low roughness) and good lighting.
- **Bloom is load-bearing** — the neon glow effect relies heavily on URP bloom post-processing. Without it, the balloons will look flat. Configure a URP Volume with bloom intensity tuned to the emission values in the balloon shader.
- **The paint splatter is the signature effect** — this is what makes the visual identity unique. No library asset covers it. Budget time for custom splatter textures and particle setup.
