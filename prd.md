Hey,# INKSHOT
Working Title PRD
Version: Vertical Slice / First Playable
Genre: Mobile portrait physics-based precision roguelite
Target Platform: iPhone first
Session Length Goal: 20-35 minute full runs
Core Fantasy: Start with plain darts and end with a broken paint-combo engine that turns a carnival balloon wall into a neon, dripping disaster.

## 1. Product Vision

INKSHOT is a portrait-mode mobile roguelite where the player throws darts at a wall of balloons in short score-based rooms. The player must hit escalating score thresholds with a limited number of darts. Between rooms, they choose upgrades that either improve the dart, modify paint behavior, improve economy, or seed future walls with more valuable balloon types.

The game should feel like:
fast enough to be replayable
readable enough to support skill
juicy enough to feel physically satisfying
systemic enough to support roguelite build expression

The core run fantasy is not “I popped balloons.”
It is:
“I built a ridiculous paint engine and learned enough trick-shot finesse to exploit it.”

## 2. Design Pillars

### Readable First
The player should always understand why a shot worked or failed.
Balloon identity must be readable through color + symbol + tie shape/pattern.
Paint can get messy, but the board must never become visually incomprehensible.

### Skill Matters, But Does Not Dominate
The player should improve at angle, power, ricochet setup, and later spin.
The game should reward execution, but builds should matter more than raw aim alone.

### Score Is the Boss
For the first version, score thresholds are the primary challenge.
Do not build traditional enemies or boss characters yet.
The wall is the opponent.

### Paint Is the Signature
Base paint is visual juice.
Perks convert paint into mechanics.
Paint should become the player’s combo engine over the course of a run.

### Misses Can Become Setup
Missed darts sticking into the wall is a major differentiator.
A miss should sometimes be a future opportunity, not just a failure.

## 3. MVP Scope Summary

The first playable should include:
static balloon walls
score-threshold rooms
3 to 5 darts per room, baseline 4
sticky missed darts
basic dart physics with collision
paint splashes as visual effects
a small set of perks that make paint mechanical
weighted special balloon spawning
post-room reward selection
run failure when a room threshold is not met by the end of available darts

Do not include in the first version:
moving balloons
boss fights
full meta progression outside the current run
complex map navigation
slow-motion mid-flight steering by default
heavy story or world systems
multiplayer

## 4. Core Run Loop

Start run
Enter room with balloon wall and target score
Throw limited darts
Pop balloons, trigger score, paint, chain reactions
Reach threshold before darts run out
Get reward selection
Continue to next room
Fail if threshold not reached

Stretch goal after vertical slice:
after reaching threshold, player may “Bank & Leave” or continue throwing for bonus score at increased risk

## 5. Core Room Structure

Each room contains:
one balloon wall
one target score
one limited dart count
optional side hardware like bumpers or edge rails
a weighted distribution of common and special balloons

Recommended first-version defaults:
wall grid: 8 columns x 9 rows
baseline darts per room: 4
starting threshold: 3000
threshold scaling: +22% per room
first run target length: 10 to 12 rooms

Room clear rule:
the room clears as soon as the current shot fully resolves and the threshold has been reached

Failure rule:
if all darts are spent and threshold is not reached, the run ends

## 6. Controls

### Recommended Control Scheme
Portrait mobile
Player touches near the bottom launch area
Drags to set angle and power
A visible aim guide shows the initial projected path
Release throws the dart

### Why This Approach
It preserves readability and supports deliberate shots, bank shots, and future geometry play
It is more dependable than a raw flick
It is less visually noisy and less toy-like than a full slingshot pullback

### Post-Release Finesse
Not available by default
Unlocked through a perk family
When unlocked, player may apply one subtle swipe during the first 150-250 ms of flight to add curve/spin

This keeps the game fast while still supporting advanced mastery

## 7. Physics Feel Targets

Do not simulate real darts realistically
Use heroic, readable, slightly forgiving physics

Desired feel:
shots feel snappy and responsive
ricochets are predictable enough to learn
collision resolution is visually satisfying
dart travel speed is fast enough to stay exciting but slow enough that the player can read outcomes

Recommended feel tuning:
slight auto-stabilization on release
very small amount of aim forgiveness on direct balloon collisions
clear audio-visual feedback on impact
sticky misses lodge firmly into the board at visible angles

## 8. Balloon System

### Common Balloons
Standard colored balloons
Base value: 100 score
One hit to pop

Recommended common colors:
red
blue
yellow
green
purple

Each balloon should also carry:
a simple symbol or sticker
a tie shape or tie pattern for readability/accessibility

### Special Balloons for MVP

#### Gold Balloon
High-value target
Base value: 350
No extra paint effect by default

#### Paint Balloon
When popped, emits a thick splash of its paint color into nearby balloons and wall space
Base value: 125
Paint is cosmetic by default unless modified by perks

#### Prize Balloon
Awards room currency used for rerolls or future systems
Base value: 150
Also grants 1 prize currency

#### Hazard Balloon
Punishes greed or poor aim
Base value: 0
On pop:
remove 1 remaining dart if any remain
clear current chain bonus for the shot
strong negative feedback effect

Hazards should be visually unmistakable:
dark body
warning symbol
distinct tie shape

## 9. Paint System

### Base Rule
Paint is mostly visual at baseline
It splashes onto nearby balloons and the wall
It does not do anything mechanical on its own in the first room of a run

### Upgrade Rule
Perks convert paint colors into mechanics
This is where the combo engine lives

### Paint State
Each balloon can hold one or more paint tags
For the first version, keep this simple:
store up to 2 paint tags per balloon
store order of most recent applications
visualize splashed paint clearly

### Why This Matters
The player can deliberately create future value
Bad paint placement can create tradeoffs
The board becomes a strategic mess, not just a visual mess

## 10. Sticky Dart System

When a dart misses or finishes its flight without being destroyed, it may stick into the wall or frame
Stuck darts become physical geometry

First-version behavior:
future darts may ricochet off stuck darts
stuck darts remain for the current room only
cap active stuck darts at 3 to avoid chaos

This is a signature mechanic and should be present in v1

## 11. Scoring System

### Baseline Values
Common balloon: 100
Gold balloon: 350
Paint balloon: 125
Prize balloon: 150
Hazard balloon: 0

### Chain Bonus
Within a single shot resolution, each additional pop after the first grants increasing bonus score

Recommended first-version formula:
1st pop = base value
2nd pop = base + 25
3rd pop = base + 50
4th+ pops = base + 75 each

This makes cascades feel important without requiring a deep combo model yet

### End-of-Room
If threshold reached, room clears
Any excess score simply carries into the run total

Stretch goal:
convert excess score above threshold into bonus currency or optional greed value

## 12. Weighted Balloon Seeding

This is the slower strategic layer
Perks may change the likelihood that certain balloon types appear in future rooms

Do not implement this as literal deck drawing
Implement it as weighted board generation

Example weights:
common balloons always fill most slots
gold balloon weight
paint balloon weight by color
prize balloon weight
hazard balloon weight

Perks modify these weights for future rooms

Example:
Golden Mix: future walls are more likely to contain gold balloons
Cobalt Batch: future walls are more likely to contain blue paint balloons
Skull Deal: future walls contain more hazards, but specials are worth more

## 13. Perk Archetypes

These are the major bonus families the game should eventually support

### A. Throw Mods
Change how the dart travels or collides

Examples:
Needle Tip
Dart pierces 1 additional balloon

Split Fletching
On first balloon pop each shot, split into 2 mini-darts at slight angles

Curve Thumb
Unlock one subtle post-release curve swipe per shot

Banker’s Edge
First ricochet each shot grants +50% score on the next balloon hit

Heavy Steel
Dart hits harder and can pop reinforced targets in later versions, but curve control is reduced

### B. Paint Engine
Turn paint into mechanics

Examples:
Blue Drip
Blue-painted balloons grant +75 score and splash one extra neighbor when popped

Yellow Bleed
Yellow-painted balloons grant +150 score

Crimson Chain
Red-painted balloons trigger a tiny radial pop burst when popped

Blackwash
Any balloon with 2 paint tags bursts automatically after a short delay

Paint Tank
All paint splash radius increased by 25%

### C. Geometry / Board Control
Use the board itself as part of the build

Examples:
Pin Cushion
Missed darts always stick and are more bouncy

Rail Mouths
Edge gutters can catch and fire the dart sideways once per room

Echo Board
Your next shot inherits a weak bounce from the most recently stuck dart

String Theory
Adjacent painted balloons count as linked for combo purposes

### D. Economy / Greed
Convert performance into more value

Examples:
Greed Ticket
Excess score above threshold converts 20% into prize currency

Jackpot Trim
Gold balloon spawn weight increases significantly

Clean Finish
Clear the top row during a room for bonus prize currency

Prize Hunger
Prize balloons are worth more, but hazards are slightly more common

### E. Balloon Seeding
Modify what the future wall contains

Examples:
Golden Mix
Increase gold balloon weight in future rooms

Cobalt Batch
Increase blue paint balloon weight in future rooms

Sticker Pack
Future rooms may contain “sticker” balloons that count as a wildcard for paint-related perks

Skull Deal
Increase hazard spawn weight, but all special balloon base scores increase

### F. Recovery / Utility
Help smooth out run volatility

Examples:
Pocket Dart
Gain +1 dart every room

Ghost Line
Show a clearer projected initial path on the first shot of each room

Lucky Angle
First direct miss each room refunds a small amount of score or aim assist on the next shot

Reset Wax
At room start, clear paint from 3 random balloons and gain a small bonus for each cleared tag

## 14. Suggested First-Version Perk Pool

Implement 12 to 15 perks total for the vertical slice

Recommended starting 12:
Needle Tip
Curve Thumb
Banker’s Edge
Pin Cushion
Blue Drip
Yellow Bleed
Crimson Chain
Blackwash
Paint Tank
Golden Mix
Pocket Dart
Ghost Line

These 12 are enough to create noticeably different runs without exploding scope

## 15. Challenge Rooms

These are not the primary loop
They are optional spice

For the first version, these may be skipped entirely
If included, only implement 1 or 2

Recommended types:

### Full Clear Tent
Clear all balloons within 5 darts
Reward: stronger perk choice or extra currency

### Jackpot Tent
Pop 3 marked prize balloons within 4 darts
Reward: guaranteed economy or seeding perk

### Ice Mode Tent
Some balloons require 2 hits and do not spread paint
Reward: high-value paint perk

## 16. Board Generation Rules

For each room:
generate base 8x9 grid
fill mostly with common balloons
place special balloons using current weighted spawn table
cap special count by room number
place side hardware only on certain board templates

Recommended caps early game:
1-2 special balloons in room 1
2-4 by room 5
4-6 by room 10

Hazards should start rare and become more common only if the player has actively taken greed/seeding perks that enable them

## 17. UI / UX Requirements

### Gameplay HUD
Top left:
room number

Top center:
current score and target threshold bar

Top right:
remaining darts and prize currency

Main center:
balloon wall occupying most of the screen

Bottom area:
launch zone
current dart stack
aim guide during drag

### Feedback
Every shot needs:
strong impact sound
clear pop sound
paint splash effect
score numbers that are readable but not overwhelming
short combo feedback for chain events

### Perk Choice Screen
After room clear, show 3 large perk cards
Each card contains:
big icon
short title
1-line effect text
small rarity color/frame

Cards should rise up from the bottom and feel premium
Player taps 1 to continue

### Readability Rules
Never rely on color alone
Use color + symbol + tie shape
Hazards must be unmistakable
Perk icons must be bold and simple

## 18. Art Direction Summary

Theme:
graffiti noir carnival
after-hours midway
wet asphalt
black corkboard / chalkboard wall textures
fluorescent thick paint
chrome darts
neon accent lighting
ruined, messy end-state after each room

The wall should look more vandalized as the room progresses

## 19. Data Model Suggestions

### Balloon
id
grid position
base color
balloon type
symbol type
tie type
paint tags array max 2
is popped
score value

### Dart
id
position
velocity
remaining pierce
can curve
is stuck
stuck angle
has bounced count

### Room State
room index
target score
current score
darts remaining
prize currency
spawn weights
active balloons
active stuck darts
selected perks

### Perk Definition
id
name
archetype
rarity
description
effect hooks
future weight modifiers if any

## 20. Implementation Order

### Milestone 1
One static wall
one dart
basic throw
basic collision
balloon popping
scoring

### Milestone 2
full room loop
4 darts
threshold clear
room restart / run restart

### Milestone 3
paint splash visuals
gold, paint, prize, and hazard balloons
chain scoring

### Milestone 4
sticky darts
ricochet off stuck darts
basic perk selection after rooms

### Milestone 5
12-perk pool
weighted future balloon spawning
10-room run progression

### Milestone 6
polish
juice
audio
screen transitions
balance tuning

## 21. Success Criteria for the Vertical Slice

The build is successful if:
a player can complete a 10-room run
different perk choices create visibly different runs
paint matters by mid-run
sticky darts create at least occasional “that was sick” moments
the player can understand balloon types at a glance
shots feel deliberate and satisfying
rooms resolve quickly enough that “one more run” feels natural

## 22. Non-Goals for This Build

Do not build:
bosses
moving balloons
deep account meta progression
live ops
PvP
complex narrative
dozens of balloon types
complex map branching

Keep the first version honest, playable, and juicy