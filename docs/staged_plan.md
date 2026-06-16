# Industrialization Roadmap — Full Design Specification

Companion design document to [`staged_plan.png`](staged_plan.png). This is the **complete,
self-contained spec** for the staged expansion of the Expanded mods (IMEX / PPEX / SMEX) into a
coherent 19th-century ferrous-metallurgy and steam-power progression. Every process is given as
an explicit **input → output**, every machine has a reference card, and all interaction/timing
rules are pinned down.

> **How to read this.** Numbers (volumes, rates, unit values, pressures) are the **proposed
> baseline economy** and are **config-tunable** via the exlib config system unless stated
> otherwise. Where a value is already fixed by the live codebase it is marked *(live)*. Genuine
> unknowns are collected in [Open balance questions](#open-balance-questions) — everything else
> is decided.

---

## 1. Goals & design pillars

- **Cheap kickstart.** Iron-tier machinery (water + cast iron) bootstraps everything. Steel is
  never required to *start* the system; it is the reward for progressing.
- **Smooth, gapless progression.** Each stage's output material is the next stage's construction
  requirement, so the tech tree is linear with no hard circular dependencies.
- **Immersive + historically grounded.** Real processes, real machines, real materials, with
  small deliberate gameplay liberties called out where taken.
- **Industrialization pays off in bulk vanilla goods**, not just in more machines (§12).
- **No GUI windows** unless genuinely unavoidable; all interaction is in-world and verb-based,
  all state readable from block-info (§10).

---

## 2. Conventions, units & global rules

| Quantity | Unit | Notes |
|---|---|---|
| Metal mass | **units (u)** | Vanilla convention: 100 u = 1 ingot. |
| Fluid/gas volume | **litres (L)** *(live)* | Pipe segment holds 10 L *(live)*. Both ppex & smex use litres, never m³. |
| Mechanical power | **MP** *(live)* | Vanilla MP network; constant-power generator model (§5.3). |
| Steam/water flow | **L/s** | Per-tick flow smoothed by EMA for the throughput readout *(live)*. |
| Temperature | **°C** | One network-wide pipe temperature *(live)*; molten canals are per-cell. |
| Pressure | **atm** | 1 atm = ambient. LP steam ≤ ~3 atm; HP steam ~8–12 atm (tunable). |
| Electricity | **DC, watts** | New network (Stage V); see §5.4. |

**Global rules**

- A pipe network is a **single medium** at a time (gas *or* water, never both) with unified
  Volume/Temperature/Pressure/MediumType *(live)*. Air, steam and exhaust are gas media; water
  and condensate are liquid media.
- Molten metal lives in **per-cell molten canals** (each block owns its metal; flows cell to
  cell) *(live)*. The **ladle** (§7) is the only block that merges canal networks and mixes metals.
- Stock/billet items carry their remaining mass in a **unit-count stack attribute** *(decided)*,
  so any cut/divide step is exact arithmetic.
- "Steam machinery only": **billets and profiled stock cannot be worked on a vanilla anvil** —
  only on the rolling mill / steam hammer.

---

## 3. World-gen & raw inputs

This roadmap **adds three new ores** (none exist in vanilla); world-gen for them ships with the
mod that needs them:

| Ore | Added by | Needed for | Rarity intent |
|---|---|---|---|
| **Manganese** | SMEX (Stage III) | Hadfield steel | Uncommon — the ore-hunting gate before HP machinery. |
| **Tungsten** | SMEX/PPEX (Stage V) | HSS | Rare, deep. |
| **Chromium** | SMEX/PPEX (Stage V) | HSS | Rare, deep. |

Vanilla raw inputs reused: iron ore (limonite/hematite/magnetite), copper, cassiterite (tin),
sphalerite (zinc), coal → **coke**, **limestone** (flux), clay, fireclay/refractory tiers.
**Vanadium is dropped** — no in-game ore, HSS uses tungsten + chromium only *(decided)*.

All ore is **crushed before the ore mixer** (existing pipeline). A powered **steam crusher** is
an *optional* QoL alternative to manual crushing (higher throughput / small yield bonus) — low
priority, not required by any recipe.

---

## 4. Material catalogue

| Material | Stage | Produced from (process) | Carbon/character | Primary use |
|---|---|---|---|---|
| **Pig iron** | I | Blast furnace (molten) → sand "pigs" | very high C, brittle | Feedstock for cupola & puddling; not used directly |
| **Cast iron** | I | Cupola remelts pig → molds | high C, castable, brittle | Castable components; **substitutes vanilla iron in build recipes** |
| **Wrought iron** | I | Puddling pig → shingling | low C, tough, fibrous | **= vanilla iron**; forgeable bar/plate |
| **Blister steel** | Ia | Cementation of wrought bars | carburised surface | Crucible-steel feedstock only |
| **Crucible steel** | Ia | Crucible furnace melts blister | homogeneous high C | Tools/weapons (**+durability +damage**) |
| **Mild steel** | III | Bessemer blows pig | low C | **= vanilla steel**; general structural |
| **Hadfield steel** | III/IV | Bessemer + manganese (ladle alloying) | Mn austenitic, work-hardening | **HP machinery only** (boilers, engine cylinders); **not** tools/weapons |
| **Copper (blister/converter)** | IIIa | Reverberatory → converter | impure | Rods, impure wire, bronze base |
| **Pure copper** | V | Electrolytic refining | high purity | Efficient dynamo wire |
| **Bronze types** | IIIa | Crucible + tin/zinc (ladle) | — | Bearings, fittings, cocks |
| **HSS** | V | Arc furnace + tungsten + chromium | hot-hard | **Best tools; needs no tempering** |

**Historical/timeline notes:** Hadfield steel is really ~1882 (intentionally late — it is a
deliberate ore-hunting gate). Cowper stoves are regenerative hot-blast stoves and belong only
with the hot-blast furnace (Stage III), never cold blast. Crucible steel (Huntsman, 1740s) and
Bessemer (1856), reverberatory copper + Pierce-Smith converting, electrolytic refining (1860s–70s),
electric arc furnace + HSS (Taylor-White, ~1900) are all correctly ordered.

### 4.1 Proposed baseline unit economy (tunable)

| Item | Mass (u) | Item | Mass (u) |
|---|---|---|---|
| Ingot (vanilla) | 100 | Sand "pig" | 100 |
| Small billet | 400 | **Large billet** | **800** |
| Heavy plate | 400 | Plate (vanilla) | 200 |
| Sheet metal | 100 | Strip | 50 |
| Heavy bar | 300 | Rod (vanilla) | 100 |
| Thin rod | 50 | Wire rod | 25 |
| Large pipe | 300 | Rolled pipe | 150 |
| Small tube | 75 | Nail (stamped) | ~3 (batched) |

Yield rule of thumb: smelting/converting carries a **slag/loss factor** (cold blast worst, hot
blast best). Baseline: cold blast ~0.8 yield, hot blast ~0.95; puddling ~0.85 (slag); Bessemer
~0.9. All tunable.

---

## 5. Networks

Four independent transport networks. All reuse the existing block-network graph infrastructure.

### 5.1 Molten-canal network *(live)*
Per-cell metal, flows cell→cell, end caps recomputed on tesselation. **Ladle** merges canals and
mixes. Feeds molds and converter/furnace runners. Carries: molten pig iron, cast iron, mild/
hadfield steel, copper, bronze, HSS.

### 5.2 Pipe network — gas *or* water *(live)*
Single medium per network, unified state pool. Used for: **water** (intake→boiler→reservoir),
**steam** (boiler→engine→condensate return), **compressed air** (blower→furnace tuyere ports),
**exhaust** (Stage IIa passthrough heating). Pressure & one network-wide temperature.
Connectors are ports reading the adjacent cell; valves sever/flow; pressure valves overflow.

### 5.3 Mechanical-power (MP) network *(live)*
Constant-power generator model: `speed = power_budget / total_load`; a machine stalls past ~2×
its rated load and stops past that *(live behavior)*. Sources: waterwheel (Stage I), engine
flywheel sub-machines (Stage II+). Loads: helve hammers, rolling mill, steam hammer lift, boring
machine, wire extruder, ore crusher.

### 5.4 Electrical network — DC *(new, Stage V)*
Wires carry DC watts. Model it as a pool like the pipe network: **source** = dynamo (watts =
f(MP in, wire purity)), **sinks** = light bulbs, battery (storage), arc furnace, electrolysis
cells. Impure copper wire → high resistance → large efficiency penalty; pure copper wire removes it.

---

## 6. Stages — detailed process flows

Each process below is **input → output** with the machine, rate, and setup. Footprints and build
costs are in the machine cards (§8).

### Stage I — Mass iron production (IMEX, early 19th c.)
*Goal: rapid iron parts + large cast components, cheap to set up, no steel required. Powered by
waterwheel MP only.*

| Process | Machine | Input → Output |
|---|---|---|
| Mix charge | Ore mixer | crushed iron ore + coke + limestone flux (high-coke ratio) → **high-coke blast mix** |
| Smelt | **Cold-blast furnace** | blast mix + cold pressurized air (MP blower) + refractory-brick build → **molten pig iron** (continuous, *short runs / low yield* because cold blast can't sustain ~1550 °C) |
| Cast pigs | Sand 1×2 mold | molten pig iron (200 u) → **2 sand "pigs"** (solid pig iron) |
| Remelt → cast iron | **Cupola furnace** | pigs + coke + cold air (MP blower) → **molten cast iron** → ceramic molds → **cast-iron plates / rods / industrial components** |
| Refine → wrought | **Puddling furnace** | pig iron + heat + **rabbling-bar** stirring (manual, §10) → **wrought-iron balls** |
| Shingle | Helve hammer (MP) | wrought-iron ball → **wrought-iron ingot / plate** (= vanilla iron; ~0.85 yield, slag loss) |

Setup: waterwheel → wooden-gear transmission → MP blowers feeding furnace + cupola tuyeres.
Cold-blast furnace is **iron-age tier** (tier-1/2 refractory bricks) — deliberately limited so
the *full* mass-production rig waits for hot blast (Stage III).

### Stage Ia — Crucible steel (IMEX add-on)
*Removes the vanilla "hammer blister steel into steel" mechanic; routes it through crucibles.*

| Process | Machine | Input → Output |
|---|---|---|
| Carburise | Cementation furnace *(vanilla)* | wrought-iron bars + charcoal → **blister steel** |
| Fire crucible | Beehive kiln *(vanilla)* | hand-claymolded crucible → **fired clay crucible** |
| Melt | **Crucible furnace** | blister-steel ingot(s) + coke + fired crucible → **molten crucible steel** |
| Cast | Special cast-iron mold | molten crucible steel → **cast crucible-steel ingot** |
| Work | Helve hammer (MP) | cast crucible-steel ingot → **workable crucible-steel ingot / plate** → tools/weapons (+durability +damage) |

Crucible steel is **not** vanilla steel; it is a separate high-carbon tool alloy.

### Stage II — Low steam power (PPEX, early-mid 19th c.)
*Goal: free the rig from rivers/seasons; the bridge that unlocks hot blast. All machines built
from **cast-iron** industrial components — no steel needed.*

| Process | Machine | Input → Output |
|---|---|---|
| Raise steam | **Cornish boiler** | water (pipe) + fuel (coal/coke/wood) → **LP steam** + heat; shared tank, ~3-min heat-up state machine *(live)* |
| Steam → power | **Watt engine** | LP steam (fixed draw, e.g. 20 L/s) → engine power; **condensate** (water) returned/spilled if unpiped *(live behavior)* |
| Pump water | Fluid pump (sub-machine) | engine power + intake → **water into pipe net @ 16.67 L/s** *(live)* |
| Blow air | Air blower (sub-machine) | engine power → **compressed air @ 48 L/s** to furnace/cupola port *(live)* |
| Transmit power | Flywheel (sub-machine) | engine power → **MP network** (drives hammers/mills) |

Sub-machine outputs scale off **absolute** engine power *(live)*; `steam-engine-efficiency` (0.7)
sets pump/blower output pressure *(live)*. Setup: boiler → engine → one sub-machine per engine
(pump / blower / flywheel), connected by pipes (steam in, water/air out) and MP for the flywheel.

### Stage IIa — Advanced heating (PPEX add-on — separate opt-in mod)
*Intentionally punishing: build heating before first winter. Payoff: heat a greenhouse all winter.*

| Path | Chain | Input → Output |
|---|---|---|
| Exhaust | heat source → passthrough wall → chimney | fuel burning in firepit/coalpile/**brick stove** → **exhaust** routed through **passthrough blocks** (heat adjacent rooms) → vented at **chimney** |
| Steam | boiler → radiator(s) | **heated steam** (pipe) → **small/large cast radiator** → emits room heat, → **hot water** (condensate) returned; radiators **chainable** across rooms |

The steam path is more efficient but requires a boiler. Greenhouse kept above the cold threshold →
crops continue through winter. **No config softening — installing the mod is the opt-in.**

### Stage III — Mass steel production (SMEX, mid 19th c.)
*Goal: industrial steel. Needs Stage-II steam (air + power). Adds the billet pipeline.*

| Process | Machine | Input → Output |
|---|---|---|
| Preheat blast | **Cowper stoves** (regenerative) | cold air (blower) + furnace **hot exhaust** (alternating cycle) → **hot blast air**; spent cold exhaust → **smoke stack** |
| Smelt (hot) | **Hot-blast furnace** | low-coke blast mix + **hot blast** → **molten pig iron** at higher rate / lower coke *(vs cold blast)* |
| Convert | **Bessemer converter** | molten pig iron + pressurized air (blown through) → **molten mild steel** (carbon burned off; exothermic, timed blow) |
| Alloy | Crucible-furnace runners + **manganese** | molten mild steel + manganese → **molten hadfield steel** |
| Merge/mix/pour | **Ladle** (multiblock) | converges molten canals; holds/mixes mild &/or hadfield → pours **large-billet molds** (800 u) or **industrial-component molds** |
| Form | **Rolling mill(s)** | billet → **profiled stock** (does not cut; §9) |
| Cut/stamp | **Steam hammer** + die | profiled stock → **discrete vanilla items**; or billet → forged heavy components (§9) |

New rolled/forged components unlocked here: heavy plate, heavy cap, rolled pipe (plus everything
in §11). Only steam machinery can work the 600–800 u billets.

### Stage IIIa — Mass copper production (SMEX add-on)
| Process | Machine | Input → Output |
|---|---|---|
| Smelt matte | **Reverberatory furnace** | copper ore + coal/coke → **copper matte** (+ slag) |
| Convert | Bessemer / **Pierce-Smith converter** | copper matte + pressurized air + MP → **molten copper** |
| Alloy (optional) | Crucible runners + tin/zinc | molten copper + tin/zinc (ladle) → **molten bronze** |
| Cast | Ceramic molds (via ladle) | molten copper → **copper rods** |
| Draw wire | **Wire extruder** (MP) | copper rod → **copper wire** |

Supports **continuous casting** for late-tech throughput.

### Stage IV — High steam power (PPEX, mid-late 19th c.)
*Goal: power whole production lines + the large blast furnace. Requires hadfield steel & rolled pipe.*

| Process | Machine | Input → Output |
|---|---|---|
| Raise HP steam | **Lancashire boiler** | water + fuel → **HP steam** (~8–12 atm); build needs **hadfield plating + rolled pipe** |
| HP power | **Cornish engine** | HP steam → MP; **throttleable** up/down; more MP per litre than Watt; cylinder needs **hadfield steel** |
| Big pump/blast | **Large Cornish pumping engine** (~3w × 6t) | HP steam → **heavy fluid pump + air blower**; sized to feed a multi-boiler bank + **6 tuyeres** |
| Line power | **Tandem Corliss horizontal engine** | HP+LP steam (**tandem-compound, cylinders inline**) → **heavy flywheel** → large MP / dynamo drive |

> **Large blast furnace** (internal 5×5×8, **6 tuyeres**): depends on Stage-IV power. The large
> pumping engine must supply **both** the blast air for all 6 tuyeres **and** the feedwater for the
> boilers driving it. Balance the large engine's pump + blower output against 6-tuyere demand.

### Stage V — Electric power (PPEX + SMEX add-on, late 19th c.)
*Goal: high-tech expansion — electricity, light, and HSS. Build last, independent.*

| Process | Machine | Input → Output |
|---|---|---|
| Generate | **Dynamo** (copper wire) | Tandem-Corliss MP → **DC watts**; **impure** copper wire → huge efficiency penalty |
| Light/store | Light bulb / **acid-or-dry-cell battery** | DC → light / stored DC |
| Refine copper | **Electrolysis cells** | impure copper plate (anode) + DC → **pure copper plate** (cathode) |
| Pure wire | Wire extruder (MP) | pure copper plate → **pure copper wire** → upgrades dynamo & wires (removes penalty) |
| Alloy steel | **Arc furnace** | steel scrap/ingots + **tungsten + chromium** + DC → **molten HSS** → ingot molds → **HSS ingots** |

HSS is the top tool alloy and **needs no tempering** (stays hard at heat).

---

## 7. Ladle (central alloying multiblock)

The ladle is a **stationary multiblock**, not a hand tool. It:

- **Converges multiple molten-canal networks** into one vessel (e.g. a Bessemer mild-steel canal
  + a crucible-furnace hadfield runner).
- **Mixes metals** by held proportion (mild + hadfield → hadfield blend; copper + tin/zinc → a
  bronze type), producing the alloy as a new molten medium.
- **Pours** the result into billet molds, component molds, or onward canals/runners.

It is the only block allowed to merge/mix canals; everywhere else metal stays per-cell.

---

## 8. Machine reference cards

Footprint = bounding cells. Power column: what drives it. Build = key industrial components
(full component list in §11). "Exists" = already present in current ppex/smex; "Planned" = new.

| Machine | Stage | Footprint | Power | Build (key components) | Inputs → Output | Status |
|---|---|---|---|---|---|---|
| Cold-blast furnace | I | multiblock | MP blower air | refractory brick, cast pipe, grate | blast mix + cold air → molten pig | Exists |
| Ore mixer | I | 1 block | — | cast hopper | ore + coke + flux → blast mix | Exists |
| Cupola furnace | I | tall | MP blower air | refractory, grate, cast pipe | pigs + coke + air → molten cast iron | Planned |
| Puddling furnace | I | 1–2 block | manual (rabbling) | refractory, furnace door | pig + heat + stir → wrought balls | Planned |
| Helve hammer | I | 1 block | MP | cast frame, forged head | ball/ingot → ingot/plate | Exists |
| Cementation furnace | Ia | multiblock | fuel | refractory | wrought bars + charcoal → blister | Vanilla |
| Crucible furnace | Ia/III | multiblock | fuel/air | refractory, crucible | blister/Mn + coke → molten steel | Planned |
| Cornish boiler | II | mega-block | fuel | boiler plate (cast→steel), cap, injector | water + fuel → LP steam | Exists |
| Watt engine | II | mega-block + sub-machine | LP steam | bedplate, cylinder sleeve (bored), conrod, flywheel, brass bearings | steam → MP / pump / blower | Exists |
| Fluid pump | II | sub-machine | engine | cast pump body, valve body | engine → water @16.67 L/s | Exists |
| Air blower | II | sub-machine | engine | cast fan, ducting | engine → air @48 L/s | Exists |
| Radiator (sm/lg) | IIa | 1–2 block | steam | cast radiator section | steam → room heat + hot water | Planned |
| Passthrough wall / chimney | IIa | block set | — | brick / sheet | exhaust → room heat → vent | Planned |
| Cowper stove | III | mega-block | air + exhaust | refractory, checker brick | cold air + exhaust → hot blast | Exists |
| Hot-blast furnace | III | multiblock | hot blast | refractory, heavy plate | low-coke mix + hot blast → molten pig | Exists |
| Bessemer converter | III | multiblock | pressurized air + MP tilt | heavy plate, heavy cap, refractory lining | molten pig + air → molten mild steel | Exists |
| Ladle | III/IIIa | multiblock | MP/manual tilt | heavy plate, refractory | converge canals + mix → pour | Planned |
| Rolling mill | III | 1–2 block | MP | bedplate, **rollers** (tooling), heavy gears | billet → profiled stock | Planned |
| Steam hammer | III | tall | steam (LP forge/shear, HP stamp) | bedplate, heavy cap, **dies** (tooling) | stock/billet → items/components | Planned |
| Boring machine / lathe | III | 1–2 block | MP | bedplate, cutting head | cast cylinder sleeve → finished cylinder | Planned |
| Reverberatory furnace | IIIa | multiblock | fuel | refractory | copper ore + coke → copper matte | Planned |
| Pierce-Smith converter | IIIa | multiblock | air + MP | refractory, heavy plate | matte + air → molten copper | Planned |
| Wire extruder | IIIa/V | 1 block | MP | bedplate, draw die | rod/plate → wire | Planned |
| Lancashire boiler | IV | mega-block | fuel | **hadfield** plating, rolled pipe, cap, injector | water + fuel → HP steam | Exists |
| Cornish engine | IV | mega-block + sub-machine | HP steam | **hadfield** cylinder, bedplate, conrod, flywheel, bearings | HP steam → MP (throttleable) | Exists |
| Large Cornish pumping engine | IV | ~3w×6t | HP steam | hadfield cylinder, heavy beam, heavy flywheel | HP steam → heavy pump + blower | Planned |
| Tandem Corliss engine | IV | large | HP+LP steam | hadfield cylinders ×2, heavy flywheel | steam → large MP / dynamo | Planned |
| Dynamo | V | 1–2 block | MP | copper wire coil, bedplate | MP + wire → DC watts | Planned |
| Electrolysis cell | V | block array | DC | brass/copper, acid bath | impure plate + DC → pure plate | Planned |
| Arc furnace | V | multiblock | DC | refractory, electrodes, heavy plate | scrap + W + Cr + DC → molten HSS | Planned |
| Battery (acid/dry cell) | V | 1 block | — (store) | brass, plates | DC ↔ stored DC | Planned |
| Steam ore crusher | opt | 1–2 block | MP | bedplate, hadfield jaws | ore → crushed ore (bulk) | Optional |

---

## 9. The billet pipeline (forming, cutting, stamping)

**Billets** (baseline 800 u large / 400 u small) are bulk stock workable **only** on steam
machinery. Stock carries its remaining mass in a unit-count attribute. The rolling mill **forms**
(does not divide); the **steam hammer shears** (divides). Flow:

```
Billet (e.g. 800u)
   │  rolling mill(s) — install rollers, set gap; reduces & profiles, does NOT cut
   ▼
Profiled stock  ──►  plate-stock[800] / bar-stock[800] / pipe-stock[800]   (unit-count attribute)
   │  steam hammer + shear-die — divides by item mass
   ▼
Discrete items   plate-stock[800] ÷ 200u = 4 plates;  bar-stock ÷ 100u = 8 rods;  pipe-stock ÷ 150u ≈ 5 pipes
```

### 9.1 Rolling mill — rollers, gaps, trains

- **Rollers = replaceable tooling** setting the **profile** (cross-section). **Gap setting** sets
  the **thickness** step. Profile + gap pick the product, so **one roller type = one product
  family** — a 3-mill train deliberately cannot make every rolled item.
- **Gap steps (up to 5; each roller exposes only its real-product gaps):**

| Gap | Flat rollers — *4 steps (5–2)* | Grooved rollers — *4 steps (5–2)* | Pipe rollers — *3 steps (5–3)* |
|---|---|---|---|
| 5 | heavy plate (HP boiler/engine) | heavy bar | large pipe |
| 4 | plate (vanilla) | rod (vanilla) | rolled pipe (HP steam) |
| 3 | sheet metal | thin rod | small tube |
| 2 | strip (feeds stamping) | wire rod (→ extruder / rivet blanks) | — |
| 1 | — | — | — |

  Blank rows are **not** selectable for that roller; **no filler items are invented** — the
  setting count is simply restricted per roller (flat 5–2, grooved 5–2, pipe 5–3).

- **Reduction:** each pass/stand reduces **one gap step**; a billet can't reach final thickness in
  one pass.
  - **Budget (one mill):** roll → step gap down → re-feed → repeat. Tedious but one machine.
  - **Train (tandem mill):** several mills in a line, each pre-set to the next smaller gap; stock
    is handed down the line and exits finished in one motion. More blocks/components up front,
    big convenience payoff. Stock must enter at the largest gap; each mill configured separately.

- **Hand-off timing:** **on animation end.** A mill runs its roll animation to completion, passes
  the stock to the next mill's input, and on the **next tick the next mill's animation starts** —
  the piece visibly clears one stand before the next picks it up. Adjacent BEs, no moving entities.

- **In-transit rendering:** the travelling stock is a **mesh rendered/animated inside each mill in
  turn** (not a dropped entity). Live deformation is impractical, so **each stand renders the mesh
  already at its own output thickness** and animates it sliding through the rollers — the piece
  **steps down** stand to stand, reading as an almost-seamless continuously-thinning piece across a
  packed train.

- **Jam:** if stock meets a stand set **wider-or-equal** to the previous (wrong order / too thin),
  the **whole train violently stops** — harsh sound + smoke burst (ExSounds / ExParticles) — the
  **rollers must be removed** to clear it, and the **jammed stock drops to the ground** as an item.
  A misbuilt train is a visible, recoverable failure, never a silent no-op.

### 9.2 Steam hammer — dies & LP/HP capability

Installable dies select the operation: **open die** (free forging large components), **shear-die**
(cut profiled stock), **stamp dies** (nail / rivet header / bracket / washer / plate-blank).

- **LP steam (Stage III):** open-die forging works, **and shearing is the explicit LP exception** —
  cutting needs a single decisive blow, not sustained energy, so the hammer can divide billet stock
  into plates/rods/pipes from day one of mass steel.
- **HP steam (Stage IV):** unlocks **stamp/blanking** — bulk small goods (nails, brackets, rivets,
  washers) from strip/bar. Sustained high blow energy is what enables die-blanking. Same machine,
  grown capability.

- **Rivets:** bar-stock → shear into blanks → **header die** forms the head (or one cold-header die
  blanks + heads in a cycle).
- **Stamp** is the volume manufacturer of ordinary vanilla parts (cheaper/faster than the anvil).

---

## 10. Operation & interaction grammar

**No GUI windows unless unavoidable.** All state is read from **block-info / hover**.

| Action | Gesture |
|---|---|
| Install roller/die **or** load billet/stock | **Sneak + RMB** with the item in hand (context by held item) |
| Remove installed tool | **Sneak + RMB** empty-handed |
| Setting step **up** (larger gap) | **RMB** *(engine-style, mirrors the throttle)* |
| Setting step **down** (smaller gap) | **Sprint + RMB** |
| Operate a **manual** station | **RMB (repeated)** — each press = one stroke/pass; player drives rhythm, steam/MP provides force (no pressure → no stroke) |

- A **rolling mill is powered/automatic**, so on it RMB/Sprint+RMB are repurposed to the gap cycle
  and Sneak+RMB installs rollers / loads stock — no manual per-pass click.
- **Reconfiguration lock:** a machine **cannot be reconfigured while working**; in a **train**, no
  machine can be reconfigured while **any** train member is working. Prevents mid-pass gap changes
  from corrupting in-flight stock.

**Two operating tiers:**

- **Manual heat stations** (puddling, cupola tap, crucible pour, sand casting): RMB-hold with a
  **tool** + animation + timing/repetition, in the style of the existing hand-crank pump (2 L/s
  *(live)*). Hands-on; this is where operating tools live.
- **Powered machines** (rolling mill, steam hammer, boring machine, wire extruder): install tool,
  load material, then run while steam/MP is available; a **train** advances stock automatically.
  Implemented as MP-load recipe loops on the constant-power generator model (§5.3).

---

## 11. Industrial components catalogue

Organized by **fabrication method** (each maps to a station and gates a believable machine subset).
Target: a machine needs **2–4 component types**, each **1–2 steps** from a base material. Recipes
below are input → output.

**Cast** (cupola → ceramic/sand molds; cast iron) — compression/static:
- **Cylinder sleeve** — cast iron → (boring machine) → **finished cylinder** (engine bore)
- **Engine bedplate** — cast iron → bedplate casting (universal "anchor" for large machines)
- **Flywheel casting** — cast iron → flywheel (rotating mass)
- **Cast pipe segment** — cast iron → LP water main
- **Furnace grate / door casting** — cast iron → furnace parts
- **Cast radiator section** — cast iron → radiator (Stage IIa)
- **Valve body casting** — cast iron → valve body

**Forged** (puddling balls → helve hammer; wrought iron) — tension:
- **Connecting rod & crank** — wrought bar → forged conrod (piston ↔ flywheel)
- **Forged shaft / axle** — wrought bar → shaft (transmission)
- **Tie rod / stay bar** — wrought bar → stay (boiler stays, tension)
- **Rivets & staybolts** — bar-stock → shear blanks → header die → rivets (see §9.2)

**Rolled** (rolling mill; mild/hadfield) — plate & tube:
- **Boiler plate ("heavy plate")** — billet → flat rollers gap 5 → heavy plate
- **Rolled / seamless pipe** — billet → pipe rollers gap 4 → rolled pipe (HP steam)
- **Sheet metal** — billet → flat rollers gap 3 → sheet (cladding/duct/hopper)

**Stamped / drop-forged** (steam hammer + die; mild/hadfield) — mass parts:
- **Heavy cap / end cap** — billet → open die (HP) → heavy cap (boiler heads, cylinder covers)
- **Stamped bracket / fitting** — strip/bar → stamp die (HP) → brackets in bulk

**Copper / brass** (Stage IIIa) — fittings & bearings:
- **Brass bearing / bushing** — bronze → cast/turned bearing (every rotating shaft)
- **Brass cock / pressure gauge / Giffard injector** — bronze/brass → boiler accessory
- **Copper boiler tube** — copper → heat-exchange tube

**Illustrative bills of materials:**
- *Steam engine* = bedplate + bored cylinder sleeve + connecting rod + flywheel + brass bearings
- *LP boiler* = boiler plate + rivets + stays + heavy cap + injector
- *HP (Lancashire) boiler* = **hadfield** plate + rolled pipe + rivets + stays + heavy cap + injector

---

## 12. Payoffs — what components do outside building machines

Industrialization must reward more than "more machines":

1. **Cheap bulk vanilla parts.** The stamp mass-produces **nails, strips, plates, brackets** far
   cheaper/faster than the anvil → lowers the cost of vanilla base-building (supports, reinforced
   blocks, doors, chutes) and feeds other mods. **The main reward of going industrial.**
2. **An iron-age decorative/structural block set.** Cast iron → railings, lamp posts, fences,
   grates, manhole covers, stairs, riveted plating; wrought-iron gates/fences; sheet-metal roofing/
   cladding; **functional cast radiators** (IIa); pipes as visible plumbing; stoves; street lamps
   (→ electric); signage.
3. **Tools, armor, trade.** Crucible/HSS for better tools; plates for armor; finished components as
   high-value trade goods.

---

## 13. Tools

**Operating tools** (durability/consumable, manual stations):
- **Rabbling bar / puddler's rod** — stir the puddling furnace, ball up the wrought iron
- **Crucible tongs** — carry fired crucibles between kiln and crucible furnace

*(No slag skimmer — slag pours off a separate tap. No hand ladle — the ladle is a stationary
multiblock, §7.)*

**Tooling / dies** (installed in machines to select output): flat / grooved / pipe **rollers** (+
gap), **steam-hammer dies** (open / shear / nail / rivet-header / bracket / washer / plate-blank).

---

## 14. Mod & dependency map

| Mod | Contains | Hard deps |
|---|---|---|
| **exlib (ExpandedLib)** | shared framework: config, networks, particles/sounds, orientation, sub-commands | game |
| **IMEX (Ironmaking Expanded)** | Stage I iron line | exlib |
| **IMEX – Crucible add-on** | Stage Ia | IMEX |
| **PPEX (Pipes & Power Expanded)** | Stage II + IV steam/power, pipe & MP networks | exlib |
| **PPEX – Heating add-on** | Stage IIa (separate, opt-in, punishing) | PPEX |
| **SMEX (Steelmaking Expanded)** | Stage III steel + billet pipeline | exlib, PPEX (needs steam) |
| **SMEX – Copper add-on** | Stage IIIa | SMEX |
| **PPEX + SMEX – Electric add-on** | Stage V dynamo/electrolysis/arc/HSS | PPEX(IV) + SMEX(III) |

Tech-tree order (materials gate construction): cast iron → Stage II engines; steam → Stage III hot
blast/Bessemer; hadfield + rolled pipe → Stage IV HP boilers/engines; Stage IV power + pure copper
→ Stage V. No hard circular dependencies.

---

## 15. Suggested build order

Spine first, branches independent:

1. **Stage I** — iron line, cast/forged components, ceramic molds.
2. **Stage II** — Watt engines, blower/pump/flywheel, component-gated builds.
3. **Stage III** — hot blast + Bessemer + **billet pipeline** (rolling mill(s), steam hammer with
   shear/stamp dies, ladle).
4. **Branches, any order:** Ia crucible · IIa heating · IIIa copper.
5. **Stage IV** — HP steam, large engine + large blast furnace.
6. **Stage V** — electrical network, dynamo, electrolysis, arc furnace, HSS. Last, isolated.

---

## 16. Open balance questions

Design is decided; these are **numbers/feel to tune in playtest**, not open mechanics:

- Exact billet sizes (400/800 u?) and item-mass divisors (plate 200 u, rod 100 u, …).
- Yield factors per process (cold ~0.8 / hot ~0.95 / puddling ~0.85 / Bessemer ~0.9).
- LP vs HP pressure bands and the MP/litre efficiency gap between Watt and Cornish engines.
- Large-engine output vs 6-tuyere + feedwater demand for the 5×5×8 furnace.
- Dynamo watts as a function of MP-in and wire purity; the impure-wire penalty magnitude.
- New-ore rarity/depth for manganese, tungsten, chromium.
- Tick-by-tick hand-off pacing in a long rolling train (purely feel).
- Stamp batch sizes (stock-in → small-items-out per cycle) and HP-steam speed multiplier.
```
