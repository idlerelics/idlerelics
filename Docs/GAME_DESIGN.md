# Lost Chambers: Idle Relics — Game Design Document

## 1. Game Overview

### Concept
An idle management game where you play as a lead archaeologist exploring ancient sites around the world. You manage expedition workers, deliver supplies, excavate sealed tomb chambers, and collect valuable artifacts. Inspired by My Perfect Hotel but with a genuine narrative reason for room unlocking: archaeological discovery.

### High Concept
"My Perfect Hotel meets Indiana Jones" — An idle arcade game where progressive room unlocking feels like genuine archaeological discovery, not just spending currency.

### Key Pillars
- **Discovery:** Every sealed chamber is a mystery. The excitement of cracking open a new room and finding out what artifact is inside is the emotional core.
- **Cozy Adventure:** Warm and inviting, not dark or intimidating. Saturday morning cartoon expedition, not survival horror tomb.
- **Satisfying Progression:** Start with a small dig site, expand deeper, unlock branching paths, move to new locations worldwide.
- **Accessible Depth:** Simple to pick up (walk, carry, deliver), layered enough to stay interesting (artifact rarity, supply management, site-specific mechanics).

### Target
- **Platform:** Mobile (iOS & Android), portrait orientation
- **Engine:** Unity 6 (URP)
- **Genre:** Idle / Arcade Management
- **Audience:** Casual mobile gamers, ages 13+
- **Session Length:** 2-5 minutes active, with offline idle progress
- **Monetization:** Free-to-play with ads and optional IAP

### Competitive Landscape
No existing game combines the MPH walk-around-and-manage loop with archaeology exploration. Existing archaeology idle games (Idle Archeology Tycoon, Idle Archeology: Idle Miner) are tap-to-dig clickers or basic number tycoons. The niche is empty.

---

## 2. Core Gameplay Loop

### Primary Loop
1. Workers arrive at Base Camp **with their own tools** and register at the desk
2. Workers are assigned to a tomb chamber and begin excavating
3. As they dig, **needs appear** (thirst, hunger) — shown by an icon above their head
4. Player (or an NPC supply runner) grabs the correct supply from the **storeroom** and delivers it
5. Worker continues digging — **gold/artifacts trickle out** at the chamber door over time
6. Periodically, workers carry accumulated findings to the **Findings Deposit Counter**
7. Player collects from the deposit counter (micro-reveal: what rarity did they find?)
8. Artifacts slot into the **Collection Album**; duplicates convert to currency
9. Currency earned is used to unlock new sealed chambers or upgrade existing ones
10. When excavation is complete, the worker leaves the site
11. Repeat, going deeper into the dig site

### Worker Flow (Detailed)
Workers arrive with their own pickaxe/tools — the player does NOT need to equip them. Registration at the base camp desk serves as a pacing gate (upgrade the desk to register faster). Once assigned, workers enter a chamber and start excavating immediately. The player's active involvement comes from **supply delivery**: when a need icon pops up (water canteen, food), the player runs to the storeroom, picks up the right item, and delivers it. If not delivered, the worker **pauses** (no penalty, just lost time — keeps it casual-friendly). An NPC supply runner can be unlocked later to automate deliveries.

### Multi-Worker Chambers
Chambers support multiple workers based on room level:
- **Level 1:** 1 worker slot
- **Level 2:** 2 worker slots
- **Level 3:** 3 worker slots (maximum)

More workers = faster excavation + faster artifact trickle, but also more supply needs popping up simultaneously. A level 3 chamber becomes a high-maintenance, high-reward engine. This creates a strategic choice: unlock new chambers (breadth) vs. upgrade existing ones (depth). Workers are visually staggered inside the chamber — one near the entrance, one mid-room, one at the back wall.

### Progressive Chamber Reveal
Chambers are **not fully visible** when first purchased. They start covered in dirt and rubble, and the artwork is revealed progressively as excavation work accumulates. Four visual stages:

1. **Sealed** — Completely covered in dirt and rubble. Cracks in stone, indistinct shapes. Player can barely tell what's underneath.
2. **Partially Cleared** — Outlines emerge. A pillar top, the edge of a wall carving. Workers have made a dent.
3. **Mostly Excavated** — Architecture clearly visible: walls, floor patterns, shelf niches, sarcophagus shapes. Room has identity. Still dusty corners and debris.
4. **Fully Revealed** — Clean, beautiful, detailed. The "wow" moment. Triggers a small celebration effect (camera nudge, sparkle, discovery sound).

The reveal is **permanent progress** — it never resets. Each worker cycle peels back more of the chamber. Multi-worker rooms with higher levels clear stages faster, giving a visible payoff for upgrade investment. Late-game, looking at a fully excavated site with every chamber revealed creates a satisfying visual trophy.

**Implementation:** Swap between prefab variants per stage, or toggle overlay meshes (dirt layer, rubble layer, dust layer) on/off. Build the clean room first, then add cover meshes on top.

### Findings Deposit Counter (Replaces MPH Bathroom)
The deposit counter replaces MPH's bathroom mechanic. Instead of a bathroom break, workers periodically carry their accumulated small finds (pottery shards, coins, bone fragments) to a **sorting table / collection crate** near base camp.

**How it works:**
- Workers' internal "findings inventory" fills up as they dig
- When full, they leave the chamber and walk to the deposit counter
- They drop off findings and return to excavating
- The deposit counter has **limited sorting slots** — when full, workers queue up and wait
- The player taps the counter to collect (this is the primary money collection point)
- Each collection triggers a **micro-reveal**: the artifact tumbles out, glows with its rarity color

**Upgrades:**
- More sorting slots (reduces worker queue times)
- Faster sorting speed
- Visual upgrade: from a basic crate to a proper cataloging desk

**Design rationale:** This replaces *both* the bathroom mechanic (secondary facility workers must visit) *and* partially replaces cash pile collection (centralized pickup point instead of running to every chamber door). It also feeds directly into the Collection Album and monetization systems.

### Mechanic Mapping from My Perfect Hotel

| MPH Mechanic | Lost Chambers Equivalent | Notes |
|---|---|---|
| Reception desk | Base Camp desk | Workers register and get assigned. Pacing gate. |
| Hotel rooms | Tomb chambers | Excavated over time, multi-worker (up to 3), progressive visual reveal |
| Guests | Expedition workers | Arrive WITH own tools, dig, develop needs (thirst/hunger). NO tourists. |
| Room cleaning | Supply delivery (mid-dig) | Needs pop up as icons above workers' heads. Player delivers from storeroom. |
| Cash piles | Findings deposit counter | Workers carry findings to a central counter. Player collects there. |
| Toilet/bathroom | Findings deposit counter | Workers must offload findings periodically. Counter has limited slots. |
| Room upgrade (fee increase) | Chamber level (worker capacity) | Lvl 1 = 1 worker, Lvl 2 = 2, Lvl 3 = 3. More workers = more output + more needs. |
| Elevator | Tunnel/passage system | Rope pulleys, mine carts between levels |
| Hotel levels | Expedition sites | Each is a unique location with unique mechanics |
| Inventory (towels) | Supplies (water, food) | Workers arrive with tools. Player delivers consumables only. |
| Cleaner NPC | NPC supply runner | Automates supply delivery to workers |
| Loader NPC | Artifact transporter | Auto-carries relics to base camp |

### Supply System
Workers arrive with their own excavation tools (pickaxe, brushes). The player only delivers **consumable supplies** when workers develop needs mid-excavation. Player carries up to 3 items.

**Need types (shown as icons above worker heads):**
- **Water/Drink:** Most frequent need. Worker pauses until delivered.
- **Food:** Medium frequency. Worker pauses until delivered.
- **Torches:** Required in dark deep chambers. Introduced gradually as player digs deeper into a site. Workers in deep chambers periodically need torch replacement.

**If a need is not met:** The worker simply pauses — no penalty, no frustration mechanic. This keeps the game casual-friendly. The cost is lost excavation time only.

**NPC Supply Runner (upgrade unlock):**
- Early game: player does all deliveries manually
- Mid game: unlock one NPC runner who handles one storeroom automatically
- Late game: multiple runners, bigger carrying capacity

---

## 3. Progression Systems

### Site Structure (Three-Act)
- **Act 1 — Entrance:** Linear chambers, tutorial/warmup for the site. Introduces site-specific mechanic.
- **Act 2 — Branching:** Central chamber with 2-3 sealed doors. Player chooses which wing first. Others stay sealed until chosen wing completes. All wings unlock eventually (order choice, not exclusion). Max 1-2 branching points per site.
- **Act 3 — Deep Chamber:** Final chamber after all wings complete. Site's legendary artifact. Dramatic reveal. Completing this unlocks next expedition site.

### Branching System
- At milestone points, player finds a chamber with 2-3 sealed doors
- Player picks which wing to excavate first; others stay sealed
- Once chosen wing is fully excavated, next door opens
- All wings unlock eventually — no permanent lockout
- Optional v2 enhancement: symbols/hints on doors suggesting contents
- **Implementation:** Reuse `RoomHiddenState`. Add `BranchId` + `BranchDependency` to room config. `EntityModule` checks branch completion before unhiding next group.

### Expedition Sites

| Site | Theme | Color Palette | Unique Mechanic |
|---|---|---|---|
| 1 | Egyptian Pyramid | Warm golds, sandy beige, terracotta | Tutorial site. Standard excavation. |
| 2 | Jungle Temple | Lush greens, mossy teal, weathered grey | Overgrown vines. Workers need machetes. |
| 3 | Underwater Ruins | Deep blues, turquoise, coral pink | Diving. Air tanks replace torches. |
| 4 | Volcanic Cavern | Dark basalt, magma orange, ember red | Heat hazard. Cooling supplies. Time-limited chambers. |
| 5 | Frozen Tomb | Icy blues, whites, silver | Ice barriers. Heat sources. Slippery movement. |

---

## 4. Artifact System

### Core Concept
Chamber layouts are hand-crafted and fixed. Artifacts found inside are randomized via loot rolls. Each completed excavation triggers a rarity roll. This provides discovery dopamine without procedural level generation.

### Rarity Tiers

| Rarity | Drop Rate | Examples | Value Multiplier |
|---|---|---|---|
| Common | 70% | Clay shards, broken pottery, stone fragments | 1x |
| Rare | 20% | Bronze statuettes, engraved tablets, jewelry | 3x |
| Epic | 8% | Golden idols, gemstone amulets, ornate masks | 10x |
| Legendary | 2% | Cursed artifacts, mythical relics, pharaoh treasures | 50x |

Each site has its own themed artifact pool (Egyptian scarabs vs. Jungle jade figurines, etc.).

### Discovery Reveal Moment
When a chamber is fully excavated for the first time (stage 4 — Fully Revealed):
1. Sealed wall crumbles with particle effect (dust, rubble)
2. Golden light spills out from behind the wall
3. Artifact revealed with rarity-appropriate visual flourish
4. Brief popup: artifact name, rarity, value

This is the emotional core of the game. Must feel rewarding every time, never routine.

### Collection Album
Every artifact the player collects slots into a **Collection Album** — a museum-style catalog organized by sets.

**How it works:**
- Each expedition site has themed artifact sets (e.g., "Pharaoh's Burial Chamber" set: golden scarab, canopic jar, ceremonial mask, ankh amulet)
- Completing a full set unlocks a **permanent bonus** (faster excavation, better rarity odds, cosmetic reward, new base camp decoration)
- Every artifact drop matters — even common ones — because the player is always working toward completing a set
- **Duplicates** auto-convert to gold currency (common) or can be "sold to a museum" for premium currency (rare+)

**Why this works for retention:** The album gives long-term goals beyond just unlocking chambers. Players keep coming back to chase missing pieces. The "almost complete" set feeling is a powerful motivator.

**Monetization tie-in:** When a player is one or two pieces away from completing a set, they can purchase a "Mystery Expedition Crate" that guarantees an artifact from a specific collection. This is the primary IAP driver — not random loot boxes, but targeted help for a specific goal the player already cares about. Framed as "funding a special expedition" to find the missing piece.

---

## 5. Characters

### Two Playable Characters (cosmetic choice)
Both share same rig, animations, gameplay. Swap visual prefab only.

**Character 1 — The Explorer (Male):**
- Archetype: Rugged gentleman adventurer (Indiana Jones inspired)
- Wide-brimmed fedora, brown leather jacket, khaki pants
- Satchel bag, whip on belt (visual), compass
- Color signature: Brown/khaki

**Character 2 — The Adventurer (Female):**
- Archetype: Fearless athletic explorer (original design, NOT Lara Croft)
- Short wavy auburn hair, olive green expedition shirt, cargo pants
- Aviator goggles on forehead (signature accessory), utility belt with tools
- Color signature: Teal/olive
- NO tank top, NO braid, NO weapons — distance from Lara Croft IP

### Workers (NPCs)
Smaller than player, khaki uniforms, safari hats. Differentiated by hat band color or tool type. Lifecycle: arrive at Base Camp → assigned to chamber → excavate → request supplies → uncover artifact → leave.

---

## 6. Art Direction

### Visual Style
- **Style:** Cartoony and colorful 3D, Supercell-quality
- **Mood:** "Cozy adventure." Ancient and mysterious environments, warm inviting colors, friendly proportions
- **Geometry:** Chunky stylized. Simple blocky shapes, richness from textures and lighting
- **Camera:** Isometric top-down, portrait orientation

### Lighting (Secret Weapon)
- Torches cast warm orange light pools with softer shadows between
- Volumetric dust particles in torch beams
- Golden glow seeping through cracks in sealed doors
- Warm rim lighting on characters
- Overall: firelight in ancient cave — cozy, warm, golden, exciting shadows

### UI Style
Stone/parchment textures. Golden artifact currency icon. Sandstone progress bars. Carved-stone buttons. Explorer's field journal feel.

---

## 7. Monetization

### Advertising
- **Rewarded Video:** Double artifact value, speed up excavation, bonus supplies, reveal branching hints. Always optional, player-initiated.
- **Interstitial:** Between major milestones. Frequency-capped.
- **Banner:** Minimal. Removable via IAP.

### In-App Purchases
- **Remove Ads:** One-time purchase
- **Premium Currency:** Speed up timers, cosmetic character skins
- **Starter Pack:** Discounted bundle (currency + ad removal + exclusive cosmetic)
- **Character Skins:** Cosmetic outfit variations, no gameplay advantage
- **Mystery Expedition Crate:** Guarantees an artifact from a specific collection set. Primary IAP driver — targets players who are close to completing a set. Framed as "funding a special expedition" rather than a random loot box.
- **Relic Exchange:** Trade duplicate artifacts (using premium currency) for a specific missing piece. Safety valve for bad RNG.

---

## 8. Development Roadmap (Mechanics First)

### Phase 1 — Core Re-theme (config only)
Update GameConfig timing values. Test existing MPH loop as archaeology with placeholder assets.

### Phase 2 — Revised Worker Flow
Workers arrive with tools, register at desk, go to chamber. Remove the need for player to equip workers. Adjust worker lifecycle states. Implement need icons (thirst/hunger) that appear mid-excavation. Player delivers consumable supplies from storeroom.

### Phase 3 — Findings Deposit Counter
Replace toilet/bathroom mechanic with deposit counter. Workers carry findings to counter periodically. Player collects from counter. Implement limited sorting slots and upgrade path.

### Phase 4 — Multi-Worker Chambers & Room Levels
Chamber levels 1-3 controlling worker capacity. Visual staggering of workers inside chambers. Supply needs scale with worker count.

### Phase 5 — Progressive Chamber Reveal
Four visual stages (sealed → partially cleared → mostly excavated → fully revealed). Permanent progress. Tied to cumulative excavation work.

### Phase 6 — Artifact Loot System & Collection Album
New ArtifactModule with rarity tables. Micro-reveal at deposit counter. Album UI with sets, completion tracking, and permanent bonuses. Duplicate handling (auto-convert to currency).

### Phase 7 — Discovery Reveal Sequence
DOTween sequence when chamber reaches fully revealed state. Particles + camera + UI popup.

### Phase 8 — Branching System
BranchId/BranchDependency in RoomConfig. Branch completion checks. Hidden-to-purchasable transitions.

### Phase 9 — NPC Supply Runner
Unlockable NPC that automates supply delivery. Upgrade path: number of runners, carrying capacity.

### Phase 10 — Second Character
Second PlayerConfig entry. Duplicate model with different material for now.

### Phase 11 — Site 2 (Jungle Temple)
New scene, different layout, harder configs, ElevatorController transition.

### Alpha Target
One site (Pyramid), full revised core loop (worker flow, deposit counter, multi-worker rooms, progressive reveal, album), branching working, one character. Enough for peer testing.
