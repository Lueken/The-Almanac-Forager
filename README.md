# The Almanac: Forager

The first sub-mod in [**The Almanac**](https://github.com/Lueken/The-Almanac-VS) — period-faithful vanilla enhancements for [Vintage Story](https://www.vintagestory.at/).

Forager ships the **substrate-gated knapping shim**, the **trait-tag system over vanilla flora**, and **6 preparation/preservation blocks**. It is the foundation that future Almanac mods (Apothecary, Alchemist, ...) extend.

> **Status:** v0.1.0 in development. Day 1 smoke test passing — substrate gate working end-to-end. Tag-patching of vanilla flora and preparation blocks are next.

## What it does today (v0.1.0 in progress)

- **Substrate-gated knapping** — recipes can declare `attributes.almanac.requiresHardSurface = true`, and Forager hides them in the Knapping UI unless the surface sits on a hard rock substrate (any of 13 vanilla rock types: andesite, basalt, bauxite, chert, claystone, conglomerate, granite, limestone, peridotite, phyllite, sandstone, shale, slate). Chalk excluded — too soft.
- **`almanac-hardrock` tag** — applied to all 13 hard rock blocks via JSON patch. Forward-compatible with any rock-mod that adopts the tag.
- **Network-channel index** — because `LayeredVoxelRecipe` drops `Attributes` during server-to-client sync, Forager broadcasts the gated-recipe-name list to each client on join over a `almanacforager.gates` ProtoBuf channel. Apothecary and Alchemist will inherit this shim — they just author recipes; gating is automatic.

## Requirements

- Vintage Story 1.22.0 or later
- No required mods. **Recommended companion:** [Biodiversity](https://mods.vintagestory.at/biodiversity) — Forager applies trait tags to its plants if loaded.

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

## Build

```powershell
$env:VINTAGE_STORY = "$env:APPDATA\Vintagestory"
dotnet build
```

Output goes to `bin/Debug/Mods/AlmanacForager.dll`. To deploy as a folder mod, copy `modinfo.json`, `AlmanacForager.dll`, and the `assets/` folder into `%APPDATA%\VintagestoryData\Mods\almanacforager\`.

## License

- **Code:** MIT
- **Assets:** CC-BY-NC-SA 4.0

## Credits

Inspired by techniques from *Six Seasons* (Joshua McFadden), *The New Wildcrafted Cuisine* (Pascal Baudar), and *The Noma Guide to Fermentation* (René Redzepi & David Zilber). All in-game text is original; book titles credited as inspiration only.
