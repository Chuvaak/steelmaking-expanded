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
| Electricity | **AC / DC, volts (V)** | Two coupled networks (Stage V): AC transmission backbone + DC consumer drops; see §5.4. |

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

This roadmap **adds exactly one new ore — wolframite (tungsten)** — which has no vanilla equivalent
and is genuinely required by HSS (~18 % W). **Every other** alloying element is already in vanilla
world-gen, so progression is otherwise gated by *finding and processing* existing ores:

| Alloy element | Ore | New? | Needed for | Gating intent |
|---|---|---|---|---|
| **Manganese** | rhodochrosite | vanilla | Hadfield steel | The ore-hunt gate before HP machinery (Stage III/IV). |
| **Chromium** | chromite | vanilla | HSS | Deep/rare — Stage V tool steel. |
| **Titanium** | ilmenite | vanilla | HSS | Hot-hardness contributor for HSS (Stage V). |
| **Tungsten** | **wolframite** | **new** | HSS | The one added ore — rare & deep; the Stage V endgame gate. |

Vanilla raw inputs reused: iron ore (limonite/hematite/magnetite), copper, cassiterite (tin),
sphalerite (zinc), coal → **coke**, **limestone** (flux), clay, fireclay/refractory tiers, plus the
alloy ores above.
**Vanadium is dropped** — no vanilla ore and not worth a second new one; HSS is specced as a
**tungsten + chromium + titanium** hot-hard tool steel (real HSS is W–Cr–V; titanium stands in for
the dropped vanadium) *(decided)*.

All ore is **crushed before the ore mixer** (existing pipeline). A powered **steam crusher** is
an *optional* QoL alternative to manual crushing (higher throughput) — low priority, not required
by any recipe.

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
| **Ingot iron** | III | Bessemer **over-blow** (decarburised, slag-free) | ~0 % C | Soft, slag-free; **not wrought iron** — re-melt to recycle |
| **Hadfield steel** | III/IV | Bessemer + manganese (ladle alloying) | Mn austenitic, work-hardening | **HP machinery only** (boilers, engine cylinders); **not** tools/weapons |
| **Copper (blister/converter)** | IIIa | Reverberatory → converter | impure | Rods, impure wire, bronze base |
| **Pure copper** | V | Electrolytic refining | high purity | Efficient dynamo wire |
| **Bronze types** | IIIa | Ladle: copper + tin / zinc / bismuth / gold+silver | — | Tin bronze, brass, **bismuth bronze**, **black bronze** — bearings, fittings, cocks, deco |
| **HSS** | V | Arc furnace + tungsten + chromium + titanium | hot-hard | **Best tools; needs no tempering** |
| **Waste alloy** | any | Off-spec ladle mix (wrong ratio / boiled-off zinc) | n/a | Crush → re-melt in blast/reverb furnace; recovers base Fe or Cu |

**Historical/timeline notes:** Hadfield steel is really ~1882 (intentionally late — it is a
deliberate ore-hunting gate). Cowper stoves are regenerative hot-blast stoves and belong only
with the hot-blast furnace (Stage III), never cold blast. Crucible steel (Huntsman, 1740s) and
Bessemer (1856), reverberatory copper + Pierce-Smith converting, electrolytic refining (1860s–70s),
electric arc furnace + HSS (Taylor-White, ~1900) are all correctly ordered.

**Material-gated power tiers (key design lever).** The two steam-power tiers are separated **by
construction material**, not merely by recipe. **LP machinery** (Stage II boilers/engines) is built
from **cast iron**; **HP machinery** (Stage IV Lancashire boilers, Cornish/Corliss cylinders) can
**only** be built from **hadfield steel**. Hadfield serves **two** design purposes: (1) it
**introduces the alloying mechanic** — it is the first metal that *must* be made by mixing a base
metal (mild steel) with an alloying element (manganese) in the **ladle** (§7), teaching the
ladle-alloying system that gates HSS and the bronzes later; and (2) it **gates HP machinery by
material** — a hard, ore-hunt-locked alloy that walls off high-pressure power (Stage IV Lancashire
boilers, Cornish/Corliss cylinders) until the player has done the Stage III steel work and found
manganese, exactly mirroring how cast iron gates the LP tier. It is deliberately **not** a
tool/weapon material.

### 4.1 Proposed baseline unit economy (tunable)

| Item | Mass (u) | Item | Mass (u) |
|---|---|---|---|
| Ingot (vanilla) | 100 | Sand "pig" | 100 |
| Small billet | 800 | **Large billet** | **2400** |
| Heavy plate | 400 | Plate (vanilla) | 200 |
| Sheet metal | 100 | Strip | 50 |
| Heavy bar | 300 | Rod (vanilla) | 100 |
| Thin rod | 50 | Wire rod | 25 |
| Large pipe | 300 | Rolled pipe | 150 |
| Small tube | 75 | Nail (stamped) | ~3 (batched) |

**Conversions are mass-conserving — no hidden yield loss** *(decided)*. Every smelt/convert/refine
step preserves the input mass (1 u in → 1 u out of the new material), so the player can compute
exactly how many items a run will yield and never silently loses metal to a slag factor. Slag,
smoke and fume are **cosmetic only**. Process *tiers* still differ — cold blast is slower and burns
more coke than hot blast — but the difference is **throughput and fuel cost, never material yield**.
(Per-process yield factors were considered and dropped: they make the player unable to predict a
run's output and cause frustrating waste.)

### 4.2 Alloy compositions (target ratios)

Every alloy carries a **composition** (mass fractions). **Carbon is set by *process*** — the blast
furnace carburises, the Bessemer blows carbon out, cementation adds it back — while the metallic
alloying elements (Mn, W, Cr, Ti, Sn, Zn) are added **by held proportion in the ladle** (§7).
Targets below are tunable; iron-family carbon is shown explicitly:

| Material | Fe / Cu base | Carbon | Other (target) | Character |
|---|---|---|---|---|
| **Pig iron** | ~96 % Fe | **~4.0 % C** | Si/Mn/P/S (slag flavour) | very high C, brittle |
| **Cast iron** | ~97 % Fe | **~3.0 % C** | — | castable, brittle |
| **Wrought iron** | ~99.9 % Fe | **<0.1 % C** | fibrous slag stringers | tough, forgeable (= vanilla iron); **puddling-only** |
| **Blister steel** | ~99 % Fe | **~1.0 % C** | (uneven, surface-carburised) | crucible feedstock only |
| **Crucible steel** | ~98.8 % Fe | **~1.2 % C** | — | homogeneous high-C tool steel |
| **Mild steel** | ~99.8 % Fe | **~0.2 % C** | — | = vanilla steel |
| **Ingot iron** | ~100 % Fe | **~0 % C** | — (slag-free) | Bessemer over-blow; soft, **not** wrought iron — re-melt |
| **Hadfield steel** | ~86.3 % Fe | **~1.2 % C** | **~12.5 % Mn** | austenitic, work-hardening |
| **HSS** | ~76.2 % Fe | **~0.8 % C** | **~18 % W, ~4 % Cr, ~1 % Ti** | hot-hard; Ti replaces vanilla HSS's vanadium |
| **Tin bronze** | ~88 % Cu | — | **~12 % Sn** | bearings, fittings, cocks |
| **Brass** | ~70 % Cu | — | **~30 % Zn** | accessories, gauges; **needs a coke cover or the Zn boils off** (§7) |
| **Bismuth bronze** | ~60 % Cu | — | **~25 % Zn + ~15 % Bi** *(vanilla range)* | vanilla tool/deco alloy |
| **Black bronze** | ~84 % Cu | — | **~8 % Au + ~8 % Ag** *(vanilla range)* | vanilla tool/deco alloy |
| **Converter copper** | ~98 % Cu | — | impurities | impure rod/wire stock |
| **Pure copper** | ~99.95 % Cu | — | — | electrolytic; low-resistance wire |
| **Waste alloy** | base Fe **or** Cu | — | off-spec / slag-rich | crush → recycle into blast/reverb furnace |

> Bismuth-bronze and black-bronze fractions follow the **vanilla alloy ranges** — verify against the
> installed game's `metalalloy` definitions before pinning exact numbers.

**Ladle mixing in practice** (mass-conserving, §4.1) — to alloy, pour the base metal and add each
element in its listed fraction. Worked examples for an 800 u small billet:

- **Hadfield (800 u):** ~700 u mild steel + ~100 u manganese (12.5 %).
- **HSS (800 u):** ~616 u carbon-steel base + ~144 u tungsten + ~32 u chromium + ~8 u titanium.
- **Tin bronze (800 u):** ~704 u copper + ~96 u tin.

The composition above is each alloy's **valid window**.

**Off-spec → waste alloy (recyclable).** If the held proportions fall outside the window (wrong
ratio, missing element, boiled-off zinc, etc.) the ladle does **not** silently snap to the nearest
alloy — it yields a **waste alloy**. The waste **retains the full base-metal mass (Fe or Cu)** but is
otherwise useless; **crush it** and throw the crushed waste back into the **blast / reverberatory
furnace** to remelt and recover the base metal. So a botched mix costs time and the alloying
additions, **never the underlying iron or copper** (mass-conserving, §4.1).

**Carbon is adjustable both ways.** A converter blow *removes* carbon (§Stage III); the player
*adds* it back by **hand-dropping powdered coke into the molten metal in the ladle** — see §7 for
recarburising and the brass coke-cover rule.

**Wrought iron is puddling-only.** Its toughness comes from slag *fibres* worked into a pasty ball
and elongated by shingling — a fully molten route (Bessemer) can't reproduce that, so an over-blown
melt yields **ingot iron** (slag-free, ~0 % C), **never** wrought iron. Ingot iron is a botched heat,
not a shortcut: its best use is to be **re-melted** (recycle loop above).

---

## 5. Networks

Four transport-network families (the electrical one splits into AC + DC). All reuse the existing
block-network graph infrastructure.

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

**Engine power ratings (kW, shown in block-info).** Every engine displays its **rated raw power in
kW** on hover. For scale, one **helve hammer** draws **~1 kW** at nominal speed (≈ 0.125 MP-load
units → display rule **1 load unit ≈ 8 kW**):

| Engine | Rated power | Nominal load |
|---|---|---|
| Watt engine (LP) | **~4 kW** | **4 helve hammers** at rated speed |
| Cornish engine (HP) | **~4 kW** nominal, throttleable higher | 4 helve hammers; more MP per litre than Watt |
| Tandem Corliss (HP+LP) | **~36 kW** | **36 helve hammers without slowing** (~9× a Watt engine) — the line/dynamo prime mover |

These are the constant-power *budgets*; load past ~2× the rating still stalls the engine.

### 5.4 Electrical networks — AC transmission + DC consumption *(new, Stage V)*
Electricity is **two coupled sub-networks**, deliberately split so players transmit on one and
consume on the other:

- **AC backbone (transmission).** The **dynamo** (driven by an engine flywheel) produces
  **alternating current at a fixed voltage**, *not* raw watts. **Transformers** step that voltage
  **up** for transmission and **down** for delivery. The AC network **ignores wire resistance**, so
  it carries power over long distances cheaply — but **most consumer blocks cannot use AC directly**.
- **DC drops (consumption).** A **rectifier** block converts **AC → DC** at the point of use. The
  **DC network sums total resistance**: *every* wire and block on it adds resistance, voltage sags
  by `I × R_total`, and once it falls below a consumer's rated voltage that consumer underpowers or
  stops. This **caps how large a single DC network can grow** — the limiter on electrical sprawl.
- **Wire purity drives resistance.** Impure (converter) copper wire has **high resistance** → small
  DC networks, big losses; **pure** (electrolytic) copper wire has **low resistance** → larger DC
  reach. Purity barely matters on the resistance-free AC backbone — its payoff is entirely on DC drops.
- **Dynamo conversion efficiency (purity again).** The dynamo is a **Corliss-only flywheel-variant
  sub-machine** (§Stage V): you place the large flywheel, then **upgrade it in place to a dynamo**,
  which **removes its MP connection and adds an AC output**. It converts the **Corliss's mechanical
  kW → electrical kW** at an efficiency set by its **coil-wire purity**: impure-copper coil ~**20 %**,
  pure-copper coil ~**80 %** (≈4× — the same purity lever, now applied at generation). Both the
  Corliss's raw kW and the dynamo's output kW are shown in block-info.

**Design intent:** run **AC for long-distance transfer** (transformers + resistance-free backbone),
then drop to **DC for the end consumers** that need it (electrolysis cells, arc furnace, batteries),
while resistance keeps each DC cluster local and bounded. Simple resistive loads (light bulbs) can
hang off AC directly.

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
| Smelt | **Cold-blast furnace** | blast mix + cold pressurized air (MP blower) + refractory-brick build → **molten pig iron** (continuous, *low throughput / high coke cost* because cold blast can't sustain ~1550 °C) |
| Cast pigs | Sand 1×2 mold | molten pig iron (200 u) → **2 sand "pigs"** (solid pig iron) |
| Remelt → cast iron | **Cupola furnace** | pigs + coke + cold air (MP blower) → **molten cast iron** → ceramic molds → **cast-iron plates / rods / industrial components** |
| Refine → wrought | **Puddling furnace** | pig iron + heat + **rabbling-bar** stirring (manual, §10) → **wrought-iron balls** |
| Shingle | Helve hammer (MP) | wrought-iron ball → **wrought-iron ingot / plate** (= vanilla iron; mass-conserving) |

Setup: waterwheel → wooden-gear transmission → MP blowers feeding furnace + cupola tuyeres.
Cold-blast furnace is **iron-age tier** (tier-1/2 refractory bricks) — deliberately limited so
the *full* mass-production rig waits for hot blast (Stage III).

**Dense blast mix (changed).** The ore-mixer recipe (iron ore + coke + flux) now yields **1 blast
mix**, not 16 — each unit is **16× denser**, so a full **blast-mix pile holds ~900 u** of iron
charge (vs ~60 u before). The furnace **consumes one whole pile per melt**, and the melt interval is
raised from 10 s to **~30 s** to suit the larger charge. Pig output is mass-conserving (§4.1):

```
pig_iron (u/s) = pile_charge (u) ÷ melt_interval (s)
```

- **Cold-blast furnace (Stage I):** 900 u ÷ 30 s = **~30 u/s** (≈5× the old 6 u/s).

**Cold-blast furnace physical design (Stage I build).** It has **no exhaust outlets** — it does not
recycle its gases. The **open top of the stack is the chimney**: heat and fume vent straight up. The
charge is loaded through a **pair of hoppers on the top, set slightly off-centre** toward the
**slag-tap side** (one block over). It still draws cold blast through its **tuyeres** (24 L/s each,
see the tuyere/exhaust balance in Stage III). **A player who falls into a working furnace dies almost
instantly** — the lit interior is a lethal hazard.

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
| Convert | **Bessemer converter** | molten pig iron + pressurized air (blown through) → **molten mild steel**; the blow **continuously burns off carbon + slag** (over-blow → ingot iron — see Carbon & slag control) |
| Alloy | **Tilting crucible furnace** + **Ladle** | tilting furnace smelts manganese (rhodochrosite) → pours molten Mn into a canal → **ladle** mixes mild steel + ~12.5 % Mn → **molten hadfield steel** |
| Merge/mix/pour | **Ladle** (multiblock) | converges molten canals; holds/mixes mild &/or hadfield → pours **large-billet molds** (2400 u), **small-billet molds** (800 u) or **industrial-component molds** |
| Form | **Rolling mill(s)** | billet → **profiled stock** (does not cut; §9) |
| Cut/stamp | **Steam hammer** + die | profiled stock → **discrete vanilla items**; or billet → forged heavy components (§9) |

New rolled/forged components unlocked here: heavy plate, heavy cap, rolled pipe (plus everything
in §11). Only steam machinery can work the 800–2400 u billets.

**Industrial-scale throughput (rebalanced).** Live values today are **60 u per 10 s** of pig
(= 6 u/s) and a Bessemer holding **1200 u** converted over **~5 min** — both too small for the new
2400 u billets. They are scaled up using the production formulas:

```
pig_iron (u/s)   = pile_charge (u)      ÷ melt_interval (s)     # blast furnaces
mild_steel (u/s) = converter_charge (u) ÷ blow_time (s)         # Bessemer
```

| Machine | Proposed baseline (tunable) | Note |
|---|---|---|
| Cold-blast furnace (I) | 900 u ÷ 30 s = **~30 u/s** | dense-pile charge, 30 s melt; ≈5× old. |
| Hot-blast furnace (III) | 900 u ÷ 20 s = **~45 u/s** | hotter → shorter melt + lower coke ratio. |
| Bessemer converter (III) | **≥ 4800 u** charge ÷ ~300 s blow = **~16 u/s** | holds **≥ 2 large billets** (up from 1200 u); one blow = **two** large billets of mild steel. |
| Ladle (III) | holds **≥ 4800 u** | buffers two large-billet pours. |

Rule of thumb: converter and ladle should each hold **at least two large billets (4800 u)** so the
steel line never stalls on undersized vessels. One hot-blast furnace (~45 u/s) fills a 4800 u
Bessemer in ~107 s — comfortably inside one ~5-min blow — so a single furnace can feed one converter
and stockpile the surplus in molten canals. The Stage-IV large blast furnace scales up again (Stage IV).

**Carbon & slag control (simulated).** Conversion is a *continuous* refinement, not a fixed timer,
and carbon runs **both directions**:

- The **Bessemer blow steadily burns off carbon and slag** — the longer the air blows, the lower
  both fall. Tap at the right moment for **mild steel (~0.2 % C)**; **over-blow** and carbon drives
  toward zero → **ingot iron** (soft, slag-free, ~0 % C). Block-info shows live **C %** and slag
  level so the player can time the tap. **Ingot iron is a botched heat, not a shortcut to wrought
  iron** (which is puddling-only, §4.2) — its best use is to be **re-melted** (recycle loop, §4.2).
- **Recarburising in the ladle:** carbon is raised back up by **hand-dropping powdered coke** (crushed
  coke) into the molten metal. Each **1 powdered coke adds a fixed *mass* of carbon**, so its effect
  on **% C is relative to the volume of iron present** (more iron → more coke per point). This is how
  you hit crucible / Hadfield / HSS carbon targets after a clean blow.
- **Brass needs a carbon cover.** When alloying **zinc** (brass), the melt **must be kept under a
  coke cover** (hand-added powdered coke) or the **zinc boils off** and the alloy drifts copper-rich
  → off-spec → waste. Modelled as zinc loss-per-second whenever an uncovered hot zinc melt sits in
  the ladle.

**Tuyere & exhaust balance (rebalanced).** Air in now matches gas out:

- **Each tuyere consumes 24 L/s** (was 12). A standard 2-tuyere furnace (cold-blast *or* hot-blast)
  draws **48 L/s** — exactly one standard air blower (§Stage II).
- The **Stage III hot-blast furnace** (the currently existing furnace) has **2 exhaust outlets**,
  each emitting **24 L/s** (48 L/s total = its blast intake). That exhaust feeds the **cowper stoves**
  (preheat) and vents via **smoke stack**.
- The **cold-blast furnace (Stage I) has no exhaust outlets** — it vents out its open top, so it
  neither feeds cowpers nor needs a smoke stack.
- The **Stage IV large furnace** has **6 tuyeres (144 L/s in)** and **4 exhaust outlets at 36 L/s
  each (144 L/s out)** — balanced. It needs **3 smoke stacks** to vent, and the **cowper throughput
  cap is removed** so **2 cowpers** can preheat its full 144 L/s blast (see Stage IV).

### Stage IIIa — Mass copper production (SMEX add-on)
| Process | Machine | Input → Output |
|---|---|---|
| Smelt matte | **Reverberatory furnace** | copper ore + coal/coke → **copper matte** (+ slag) |
| Convert | Bessemer / **Pierce-Smith converter** | copper matte + pressurized air + MP → **molten copper** |
| Alloy (optional) | **Tilting crucible furnace** + **Ladle** | tilting furnace smelts tin / zinc / bismuth → canal → ladle mixes with molten copper → **tin bronze / brass / bismuth bronze** (zinc alloys need a **coke cover** or the Zn boils off, §Stage III) |
| Cast | Ceramic molds (via ladle) | molten copper → **copper rods** |
| Draw wire | **Wire extruder** (MP) | copper rod → **copper wire** |

Supports **continuous casting** for late-tech throughput.

### Stage IV — High steam power (PPEX, mid-late 19th c.)
*Goal: power whole production lines + the large blast furnace. Requires hadfield steel & rolled pipe.*

| Process | Machine | Input → Output |
|---|---|---|
| Raise HP steam | **Lancashire boiler** | water + fuel → **HP steam** (~8–12 atm); build needs **hadfield plating + rolled pipe** |
| HP power | **Cornish engine** | HP steam → MP; **throttleable** up/down; more MP per litre than Watt; cylinder needs **hadfield steel** |
| Big pump/blast | **Large Cornish pumping engine** (~3w × 6t) | HP steam → **heavy air blower @ ~160 L/s + heavy fluid pump @ ~36 L/s**; each tuyere draws **24 L/s**, so 6 tuyeres = 144 L/s and the blower runs ~16 L/s over that to *build* pressure; the pump (~2.16× the standard 16.67 L/s pump) feeds the boiler bank |
| Line power | **Tandem Corliss horizontal engine** | HP+LP steam (**tandem-compound, cylinders inline**) → **heavy flywheel** → large MP / dynamo drive |

> **Large blast furnace** (internal 5×5×8, **6 tuyeres**, **4 exhaust outlets**): depends on
> Stage-IV power. The large pumping engine must supply **both** the blast air for all 6 tuyeres
> (6 × 24 = **144 L/s**, so the blower targets **~160 L/s** to hold pressure) **and** the feedwater
> for the boilers driving it. Its 4 exhaust outlets emit **36 L/s each (144 L/s out = blast in)**,
> needing **3 smoke stacks**; with the **cowper throughput cap removed**, **2 cowper stoves** preheat
> the full 144 L/s blast. **Not a solo build** — it is sized for **server communities** to feed, fuel
> and operate *collectively* (multiple players charging, tapping and tending it), not for one player
> to run alone. Scale its pig-iron output up from the Stage-III furnaces (≈3× a hot-blast furnace for
> 3× the tuyeres → proposed **~135 u/s**, tunable) so 6 tuyeres of blast give community-scale output.

### Stage V — Electric power (PPEX + SMEX add-on, late 19th c.)
*Goal: high-tech expansion — electricity, light, and HSS. Build last, independent.*

| Process | Machine | Input → Output |
|---|---|---|
| Generate | **Dynamo** (Corliss flywheel upgrade) | **upgrade the Corliss's large flywheel in place** → removes MP, adds AC output; **Corliss power (~36 kW) × coil efficiency** → **AC at a fixed voltage** (impure ~20 % → ~7.2 kW / pure ~80 % → ~28.8 kW) |
| Transform | **Transformer** | AC in → AC at stepped voltage (up for transmission, down for delivery) |
| Rectify | **Rectifier** | AC → **DC** at the point of consumption (resistance-budgeted from here on) |
| Light | **Light bulb** | AC (or DC) → light; low draw — one dynamo lights many |
| Store | **Acid / dry-cell battery** | DC → stored DC ↔ DC out |
| Refine copper | **Electrolysis cell** | impure copper plate (anode) + **DC** → **pure copper plate** (cathode) |
| Pure wire | Wire extruder (MP) | pure copper plate → **pure copper wire** → lowers resistance, extends DC reach, upgrades dynamos |
| Alloy steel | **Arc furnace** | steel scrap/ingots + **tungsten + chromium + titanium** + **DC** → **molten HSS** → ingot molds → **HSS ingots** |

HSS is the top tool alloy and **needs no tempering** (stays hard at heat).

**Dynamo sizing (the electrical economy).** The dynamo is **not a free-standing machine**: it is a
**large-flywheel sub-machine variant that only the Tandem-Corliss engine can drive**. You place the
flywheel first, then **upgrade it in place to a dynamo** — that **removes its MP connection and adds
an electrical (AC) output**. So **every dynamo runs on a full Corliss (~36 kW)**, and its output is
`36 kW × coil efficiency`:

- **Impure-copper coil (~20 %): ~7.2 kW.** Kickstart only — enough for **one electrolysis cell**
  (≈7.2 kW each). It never scales: feeding anything bigger off impure dynamos means a *wall* of
  Corliss engines, so its whole job is to refine the first batch of pure copper.
- **Pure-copper coil (~80 %): ~28.8 kW.** Powers **4 electrolysis cells**, **or** a *lot* of light
  bulbs (low-draw resistive loads) — electricity's "civic" payoff, lighting whole bases and streets.
- **The arc furnace needs *two pure-copper* dynamos (~57.6 kW ≈ 8 cells) = two Corliss engines.**
  It is the hungriest consumer by design, and the pointed nudge to refine pure copper first (one
  pure dynamo only covers *half* an arc furnace).

(Consumer draws are what's pinned; the ~7.2 kW/cell figure just falls out of "one impure Corliss
dynamo = one cell.")

> **Realism note.** Pinning the arc furnace at **2 pure dynamos (≈8 cells)** keeps it the clear
> heavyweight without forcing a whole dedicated power plant. A real electric-arc furnace still draws
> *far* more than a refining cell (megawatts vs. a low-voltage/high-current cell) — raise it further
> if you want it even hungrier.

---

## 7. Ladle (central alloying multiblock)

The ladle is a **stationary multiblock**, not a hand tool. It:

- **Converges multiple molten-canal networks** into one vessel (e.g. a Bessemer mild-steel canal
  plus alloying-metal runners poured in by **tilting crucible furnaces**, §8).
- **Mixes metals** by held proportion into the alloys of §4.2: **Hadfield** (steel + ~12.5 % Mn),
  **HSS** (steel + W + Cr + Ti), **tin bronze**, **brass**, **bismuth bronze** and **black bronze**.
- **Recarburises by hand:** dropping **powdered coke** into the melt raises its carbon (each unit a
  fixed mass of C, relative to the iron volume) — the way to hit crucible/Hadfield/HSS carbon after
  a clean Bessemer blow.
- **Holds a carbon cover for zinc alloys:** brass/bismuth-bronze melts must be kept under a coke
  cover or the **zinc boils off** (§Stage III, Carbon & slag control).
- **Pours** the result into billet molds, component molds, or onward canals/runners.

**Off-spec mixes don't snap to the nearest alloy** — they become a **waste alloy** that keeps the
base Fe/Cu mass; crush it and re-feed the blast/reverberatory furnace to recover the metal (§4.2).

It is the only block allowed to merge/mix canals; everywhere else metal stays per-cell.

---

## 8. Machine reference cards

Footprint = bounding cells. Power column: what drives it. Build = key industrial components
(full component list in §11). "Exists" = already present in current ppex/smex; "Planned" = new.

| Machine | Stage | Footprint | Power | Build (key components) | Inputs → Output | Status |
|---|---|---|---|---|---|---|
| Cold-blast furnace | I | multiblock, open-top stack | MP blower air (2 tuyeres @ 24 L/s) | refractory brick, cast pipe, grate | dense blast mix + cold air → molten pig (~30 u/s); **no exhaust outlets** (open top = chimney), off-centre hopper pair by slag tap, **falling in = death** | Exists |
| Ore mixer | I | 1 block | — | cast hopper | ore + coke + flux → blast mix | Exists |
| Cupola furnace | I | tall | MP blower air | refractory, grate, cast pipe | pigs + coke + air → molten cast iron | Planned |
| Puddling furnace | I | 1–2 block | manual (rabbling) | refractory, furnace door | pig + heat + stir → wrought balls | Planned |
| Helve hammer | I | 1 block | MP | cast frame, forged head | ball/ingot → ingot/plate | Exists |
| Cementation furnace | Ia | multiblock | fuel | refractory | wrought bars + charcoal → blister | Vanilla |
| Crucible furnace | Ia | multiblock | fuel/air | refractory, fired crucible | blister steel + coke → molten crucible steel | Planned |
| Tilting crucible furnace | III | multiblock + tilt | fuel/air | refractory, crucible, tilt mechanism | alloying ore/metal (Mn, W, Cr, Ti, Sn, Zn…) → molten metal **tilt-poured straight into a molten canal** | Planned |
| Cornish boiler | II | mega-block | fuel | boiler plate (cast→steel), cap, injector | water + fuel → LP steam | Exists |
| Watt engine | II | mega-block + sub-machine | LP steam | bedplate, cylinder sleeve (bored), conrod, flywheel, brass bearings | steam → MP / pump / blower (**~4 kW = 4 helve hammers**) | Exists |
| Fluid pump | II | sub-machine | engine | cast pump body, valve body | engine → water @16.67 L/s | Exists |
| Air blower | II | sub-machine | engine | cast fan, ducting | engine → air @48 L/s | Exists |
| Radiator (sm/lg) | IIa | 1–2 block | steam | cast radiator section | steam → room heat + hot water | Planned |
| Passthrough wall / chimney | IIa | block set | — | brick / sheet | exhaust → room heat → vent | Planned |
| Cowper stove | III | mega-block | air + exhaust | refractory, checker brick | cold air + exhaust → hot blast | Exists |
| Hot-blast furnace | III | multiblock (existing furnace) | hot blast (2 tuyeres @ 24 L/s) | refractory, heavy plate | low-coke mix + hot blast → molten pig (~45 u/s); **2 exhaust outlets @ 24 L/s** → cowpers + smoke stack | Exists |
| Bessemer converter | III | multiblock | pressurized air + MP tilt | heavy plate, heavy cap, refractory lining | molten pig + air → molten mild steel | Exists |
| Ladle | III/IIIa | multiblock | MP/manual tilt | heavy plate, refractory | converge canals + mix → pour | Planned |
| Rolling mill | III | 1–2 block | MP | bedplate, **rollers** (tooling), heavy gears | billet → profiled stock | Planned |
| Steam hammer | III | tall | steam (LP forge/shear, HP stamp) | bedplate, heavy cap, **dies** (tooling) | stock/billet → items/components | Planned |
| Boring machine / lathe | III | 1–2 block | MP | bedplate, cutting head | cast cylinder sleeve → finished cylinder | Planned |
| Reverberatory furnace | IIIa | multiblock | fuel | refractory | copper ore + coke → copper matte | Planned |
| Pierce-Smith converter | IIIa | multiblock | air + MP | refractory, heavy plate | matte + air → molten copper | Planned |
| Wire extruder | IIIa/V | 1 block | MP | bedplate, draw die | rod/plate → wire | Planned |
| Lancashire boiler | IV | mega-block | fuel | **hadfield** plating, rolled pipe, cap, injector | water + fuel → HP steam | Exists |
| Cornish engine | IV | mega-block + sub-machine | HP steam | **hadfield** cylinder, bedplate, conrod, flywheel, bearings | HP steam → MP (**~4 kW nominal, throttleable higher**) | Exists |
| Large Cornish pumping engine | IV | ~3w×6t | HP steam | hadfield cylinder, heavy beam, heavy flywheel | HP steam → heavy blower @ ~160 L/s + pump @ ~36 L/s (feeds 6 tuyeres) | Planned |
| Tandem Corliss engine | IV | large | HP+LP steam | hadfield cylinders ×2, heavy flywheel | steam → large MP / dynamo (**~36 kW = 36 helve hammers**) | Planned |
| Dynamo | V | sub-machine (Corliss flywheel variant) | Corliss engine | copper wire coil (replaces flywheel) | upgrade flywheel in place: removes MP, adds AC @ fixed V (Corliss kW × ~20 % impure / ~80 % pure) | Planned |
| Transformer | V | 1 block | — | iron core, copper wire coil | AC → AC at stepped V | Planned |
| Rectifier | V | 1 block | — | copper plate, brass | AC → DC | Planned |
| Light bulb | V | 1 block | AC/DC | glass, copper wire | current → light | Planned |
| Electrolysis cell | V | block array | DC (~7.2 kW each) | brass/copper, acid bath | impure plate + DC → pure plate | Planned |
| Arc furnace | V | multiblock | DC (**2 pure dynamos ≈ 57.6 kW**) | refractory, electrodes, heavy plate | scrap + W + Cr + Ti + DC → molten HSS | Planned |
| Battery (acid/dry cell) | V | 1 block | — (store) | brass, plates | DC ↔ stored DC | Planned |
| Steam ore crusher | opt | 1–2 block | MP | bedplate, hadfield jaws | ore → crushed ore (bulk) | Optional |

---

## 9. The billet pipeline (forming, cutting, stamping)

**Billets** (baseline 2400 u large / 800 u small) are bulk stock workable **only** on steam
machinery. Billets exist in **every forgeable/rollable metal** (wrought iron, mild steel, hadfield) —
they are the *universal* solid feedstock, and **all fabricated components start from a billet, never
from a vanilla ingot or plate** (§11). Cast iron is never billeted: it is poured molten into molds.
Stock carries its remaining mass in a unit-count attribute. The rolling mill **forms** (does not
divide); the **steam hammer shears** (divides). Flow:

```
Billet (large 2400u / small 800u)
   │  rolling mill(s) — install rollers, set gap; reduces & profiles, does NOT cut
   ▼
Profiled stock  ──►  plate-stock[2400] / bar-stock[2400] / pipe-stock[2400]   (unit-count attribute)
   │  steam hammer + shear-die — divides by item mass
   ▼
Discrete items   plate-stock[2400] ÷ 200u = 12 plates;  bar-stock ÷ 100u = 24 rods;  pipe-stock ÷ 150u = 16 pipes
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

- **Mill speed:** each mill processes a piece in **~3 s** (one pass = one gap-step reduction). A
  budget single-mill reduction therefore takes `3 s × number of gap steps`; a primed tandem train
  overlaps passes so finished stock exits roughly every **3 s**.

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
- **Batch size = die cavity count.** A stamp die yields **one item per impression modelled in its
  shape**: a single-plate die stamps **1 plate** per blow; a multi-cavity die stamps **N items** per
  blow (e.g. a many-holed rivet/nail die punches that many blanks at once). The die mesh *is* the
  batch spec — read the count straight off its cavities, no separate config value.

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

**Every fabricated component starts from a billet — never a vanilla ingot or plate.** Forged parts
start from a **wrought-iron billet** (helve hammer); rolled/stamped parts from a **mild-steel or
hadfield billet** (rolling mill / steam hammer). **Cast** parts are the sole exception: they are
poured straight from **molten** cast iron into component molds (no solid billet involved).

**Cast** (cupola → ceramic/sand molds; cast iron) — compression/static:
- **Cylinder sleeve** — cast iron → (boring machine) → **finished cylinder** (engine bore)
- **Engine bedplate** — cast iron → bedplate casting (universal "anchor" for large machines)
- **Flywheel casting** — cast iron → flywheel (rotating mass)
- **Cast pipe segment** — cast iron → LP water main
- **Furnace grate / door casting** — cast iron → furnace parts
- **Cast radiator section** — cast iron → radiator (Stage IIa)
- **Valve body casting** — cast iron → valve body

**Forged** (puddling balls → helve hammer; wrought iron) — tension:
- **Connecting rod & crank** — wrought-iron billet → forged conrod (piston ↔ flywheel)
- **Forged shaft / axle** — wrought-iron billet → shaft (transmission)
- **Tie rod / stay bar** — wrought-iron billet → stay (boiler stays, tension)
- **Rivets & staybolts** — bar-stock (from billet) → shear blanks → header die → rivets (see §9.2)

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

- Item-mass divisors (plate 200 u, rod 100 u, …). Billet sizes are now **decided**: 800 u small /
  2400 u large.
- Production rates via the formulas (§Stage III): dense-pile charge (~900 u), melt intervals
  (cold ~30 s → ~30 u/s, hot ~20 s → ~45 u/s, large ~135 u/s), Bessemer charge ≥ 4800 u / ~300 s
  blow → ~16 u/s. *(Yield/slag-loss factors dropped — conversions are mass-conserving, §4.1.)*
- LP vs HP pressure bands and the MP/litre efficiency gap between Watt and Cornish engines.
- Engine kW ratings (Watt/Cornish ~4 kW = 4 helve hammers; Corliss ~36 kW = 36) and the load-unit→kW
  display scale (~8 kW/unit) — purely display/feel.
- Large-engine output (~160 L/s blower, ~36 L/s pump) vs 6-tuyere demand (6 × 24 = 144 L/s); exhaust
  balance (4 outlets × 36 = 144 L/s, 3 smoke stacks, 2 cowpers); cowper throughput-cap removal.
- Dynamo **AC voltage & current rating** (Corliss-only flywheel upgrade: ~36 kW × coil efficiency
  ~20 % impure → ~7.2 kW / ~80 % pure → ~28.8 kW); the **DC resistance budget** (resistance per
  wire/block, sag threshold).
- Arc-furnace draw pinned at **2 pure-copper dynamos (~57.6 kW, two Corliss engines)** — raise
  further for more realism/pacing (§6 V).
- Rarity/depth tuning for the alloy ores (rhodochrosite / chromite / ilmenite + the new wolframite).
- Bessemer over-blow curve: carbon-burn and slag-drop per second, and the mild-steel tap window
  before it runs to ingot iron.
- Recarburising: carbon mass added per **powdered coke**; the coke-cover consumption rate and the
  **zinc boil-off rate** for an uncovered brass/bismuth-bronze melt.
- Waste-alloy crush yield (target: recover ~100 % of the base Fe/Cu) and exact bismuth-/black-bronze
  fractions vs the vanilla `metalalloy` ranges.
- Tick-by-tick hand-off pacing in a long rolling train (purely feel; base mill speed ~3 s/pass).
- HP-steam stamp speed multiplier. *(Stamp batch size is now **decided** = die cavity count, §9.2.)*
```
