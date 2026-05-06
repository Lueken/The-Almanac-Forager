# The Almanac: Forager

The first content mod in [**The Almanac**](https://github.com/Lueken/The-Almanac-VS) — period-faithful vanilla enhancements for [Vintage Story](https://www.vintagestory.at/).

Forager ships a **substrate-gated knapping shim**, a **trait-tag system over vanilla flora**, and (from v0.2.0) **richer codex metadata for ~135 vanilla flora species** that surfaces through [The Almanac: Codex](https://github.com/Lueken/The-Almanac-Codex).

> **Status:** v0.2.1 in development. Substrate gate, tag system, and Codex metadata pass are all live.

---

## What Forager does

### Substrate-gated knapping
Knapping recipes can declare `attributes.almanac.requiresHardSurface = true`, and Forager hides them in the Knapping UI unless the player is sitting on a hard rock substrate. Hard rock = any of the 13 vanilla rock types: andesite, basalt, bauxite, chert, claystone, conglomerate, granite, limestone, peridotite, phyllite, sandstone, shale, slate. Chalk excluded — too soft.

The `almanac-hardrock` tag is applied to all 13 via JSON patch, forward-compatible with any rock-mod that adopts the tag.

A `almanacforager.gates` ProtoBuf network channel broadcasts the gated-recipe-name list to each client on join, because `LayeredVoxelRecipe` drops `Attributes` during server-to-client sync. Apothecary and Alchemist inherit this shim transparently — author recipes, gating is automatic.

### Trait-tag system
JSON patches apply `almanac-*` trait tags to vanilla flora codes via `tagsByType` selectors. Tags currently in use:

`almanac-aromatic`, `almanac-medicinal`, `almanac-decorative`, `almanac-toxic`, `almanac-culinary`, `almanac-psychoactive`, `almanac-fibrous`, `almanac-fruity`, `almanac-sweet`, `almanac-acidic`, `almanac-starchy`, `almanac-leafy`, `almanac-seedy`

The patches cover herbs, mushrooms (~47 species), flowers, fruiting bushes, fruits, vegetables, grains, legumes, spices, aquatics, reeds, and bamboo. Wildcard selectors (`mushroom-reishi-*`, `fruitingbush-*-blackberry-*`) ensure all orientation/state variants of the same species pick up the same tags.

### Codex metadata (v0.2.0+)
Forager registers every tagged collectible with The Almanac: Codex and supplies metadata for each:

- **Latin binomial** for the actual species the VS asset represents
- **Classification slug** that the Codex resolves to a label (Bracket fungus, Bramble, Tropical fruit, ...)
- **Habitat** — brief observational phrase ("Old-growth forest; dead hardwood")
- **Description** — 1–2 sentences in the Almanac's period-faithful voice

Metadata is data-driven, in `assets/almanacforager/config/codex-entries.json`, and resolves through a three-tier lookup (exact match → progressive prefix shortening → wildcard pattern matching) so every variant of every species shares one metadata record.

The `knap` process is registered with Codex with flavor + hint text (`"stone on stone"`, `"on a hard rock surface"`).

---

## Requirements

- **Vintage Story 1.22.0+**
- **[The Almanac: Codex](https://github.com/Lueken/The-Almanac-Codex)** — hard dependency from v0.2.0 onward. The codex metadata pass is delivered through Codex's discovery system; without Codex, the trait-tag patches still apply but no rich metadata surfaces.
- **[vsimgui](https://mods.vintagestory.at/vsimgui)** — required transitively (Codex declares it; Forager doesn't directly).
- **Recommended companion (not required):** [Biodiversity](https://mods.vintagestory.at/biodiversity) — Forager applies trait tags to its plants if loaded.

---

## For modders writing knapping recipes that need hard substrate

Add to your knapping recipe JSON:

```json
{
  "ingredient": { ... },
  "pattern": [[ ... ]],
  "name": "...",
  "output": { ... },
  "attributes": { "almanac": { "requiresHardSurface": true } }
}
```

That's it — Forager's handler is global, your recipe gets the gating for free. No code dependency on Forager required at compile time; just declare it as a load-order dependency in your `modinfo.json`.

---

## Build

```powershell
$env:VINTAGE_STORY = "$env:APPDATA\Vintagestory"
dotnet build
```

Output: `bin/Debug/Mods/AlmanacForager.dll`. Deploy as a folder mod by copying `modinfo.json`, `AlmanacForager.dll`, and the `assets/` folder to `%APPDATA%\VintagestoryData\Mods\almanacforager\`.

---

## License

MIT. See [LICENSE](LICENSE).

---

## Credits

Inspired by techniques from *Six Seasons* (Joshua McFadden), *The New Wildcrafted Cuisine* (Pascal Baudar), and *The Noma Guide to Fermentation* (René Redzepi & David Zilber). All in-game text is original; book titles credited as inspiration only.
