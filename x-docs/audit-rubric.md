# Visual & Mechanical Parity — Audit Rubric

Score each category 1-10. A score of 10 means perfect parity with the reference/spec.

## Categories

### 1. Physics Feel (weight: 25%)
- Do 30%, 60%, 100% pull produce distinct trajectories reaching different board rows?
- Is there a visible parabolic arc during dart flight?
- Does pull strength meaningfully change which balloons are reachable?
- Does it feel like lobbing a dart at a vertical wall (not a flat throw)?
- Is the trajectory preview accurate to actual dart path?

### 2. Balloon Visuals (weight: 15%)
- Are balloons glossy with vivid saturated colors and specular highlights?
- Do special types (Gold, Hazard, Paint, Shield) look visually distinct?
- Are emblems/symbols readable on special balloons?
- Does perspective scaling create a 3D depth feel?

### 3. Paint System (weight: 10%)
- Do balloon pops produce thick neon paint splatter particles?
- Are there paint drip streaks running down the board?
- Do persistent paint marks accumulate on the cork board?
- Does the board look increasingly "vandalized" as the room progresses?

### 4. Dart Visuals (weight: 10%)
- Are darts chrome/silver metallic with high smoothness?
- Is there visible fletching geometry on the dart mesh?
- Do darts have visible trails during flight?
- Is there clear impact feedback (sparks)?

### 5. Environment & Framing (weight: 15%)
- Dark cork board with weathered metal frame and corner bolts?
- Neon lighting rig (magenta, cyan, gold, green point lights)?
- Atmospheric fog/mist with color oscillation?
- Multi-color floor stripes in launch area?
- Dark moody backdrop?

### 6. HUD & UI (weight: 15%)
- Industrial noir aesthetic?
- Top bar: room badge (left), score/target (center), darts + currency (right)?
- Premium perk cards with rarity borders?
- Room intro with large room number?
- Combo text escalates in size and color?

### 7. Screen Effects / Juice (weight: 10%)
- Screen shake: Perlin noise, exponential decay, combo escalation?
- Slow motion on 3+ combos with smooth easing?
- Combo flash: screen-edge vignette with color escalation?
- Impact sparks: small, fast, chrome-colored?

## Scoring Guide
- **1-3**: Not implemented or fundamentally broken
- **4-5**: Basic structure exists but far from reference
- **6-7**: Functional and trending toward reference, notable gaps remain
- **8-9**: Close to reference, minor polish needed
- **10**: Indistinguishable from reference / spec-perfect

## Output Format
```
| Category | Score | Notes |
|----------|-------|-------|
| Physics Feel | X/10 | ... |
| Balloon Visuals | X/10 | ... |
| Paint System | X/10 | ... |
| Dart Visuals | X/10 | ... |
| Environment | X/10 | ... |
| HUD & UI | X/10 | ... |
| Screen Effects | X/10 | ... |
| **WEIGHTED TOTAL** | **X.X/10** | |
```
