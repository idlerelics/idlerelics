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
1. Workers arrive at Base Camp and get assigned to a dig chamber
2. Workers excavate rubble but deplete supplies (water, tools, torches)
3. Player delivers supplies from the supply station to workers in chambers
4. Workers uncover artifacts (randomized rarity roll)
5. Player collects artifacts and carries them to Base Camp for valuation
6. Currency earned is used to unlock new sealed chambers
7. Repeat, going deeper into the dig site

### Mechanic Mapping from My Perfect Hotel

| MPH Mechanic | Lost Chambers Equivalent | Notes |
|---|---|---|
| Reception desk | Base Camp | Workers register and get assigned |
| Hotel rooms | Tomb chambers | Excavated one by one, artifact loot rolls |
| Guests | Expedition workers | Arrive, dig, need supplies. NO tourists. |
| Room cleaning | Excavation/supply delivery | Workers need water, tools, torches |
| Cash piles | Artifact discoveries | Randomized rarity replaces flat cash |
| Elevator | Tunnel/passage system | Rope pulleys, mine carts between levels |
| Hotel levels | Expedition sites | Each is a unique location with unique mechanics |
| Inventory (towels) | Supplies (canteens, pickaxes, torches) | Three types vs one = more depth |
| Cleaner NPC | Excavation crew | Auto-clears debris |
| Loader NPC | Artifact transporter | Auto-carries relics to base camp |

### Supply System
Player carries up to 3 items. Three supply types:
- **Water Canteens:** Most frequent need. Workers dehydrate and stop working without water.
- **Tools (Pickaxes/Brushes):** Medium frequency. Broken tools slow excavation to near zero.
- **Torches:** Required in dark deep chambers. Workers refuse to enter without light. Introduced gradually as player goes deeper.

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
When a chamber is fully excavated for the first time:
1. Sealed wall crumbles with particle effect (dust, rubble)
2. Golden light spills out from behind the wall
3. Artifact revealed with rarity-appropriate visual flourish
4. Brief popup: artifact name, rarity, value

This is the emotional core of the game. Must feel rewarding every time, never routine.

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

---

## 8. Development Roadmap (Mechanics First)

### Phase 1 — Core Re-theme (config only)
Update GameConfig timing values. Test existing MPH loop as archaeology with placeholder assets.

### Phase 2 — Multiple Supply Types
Extend InventoryType enum. Add RequiredSupplyType to items. Type-checking in delivery routing. **Hardest phase.**

### Phase 3 — Artifact Loot System
New ArtifactModule with rarity tables. Hook into room state transitions. Replace flat StayFee with loot rolls.

### Phase 4 — Discovery Reveal
DOTween sequence on first chamber unlock. Particles + camera + UI popup.

### Phase 5 — Branching System
BranchId/BranchDependency in RoomConfig. Branch completion checks. Hidden-to-purchasable transitions.

### Phase 6 — Second Character
Second PlayerConfig entry. Duplicate model with different material for now.

### Phase 7 — Site 2 (Jungle Temple)
New scene, different layout, harder configs, ElevatorController transition.

### Alpha Target
Two sites (Pyramid + Jungle Temple), full core loop, branching working, both characters, discovery reveal. Enough for peer testing.
