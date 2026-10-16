# Ringworld Parallax

Biome-aware Parallax Continued scatter assets on Niven's Ringworld's cylindrical terrain.

Version **1.0.1**, prepared locally for release. Requires **Niven Ringworld >= 1.1.8**, the scatter-provider ABI and Terrain API v2. Base 1.1.7 and earlier are incompatible.

The bridge uses **separately installed Parallax Continued models, textures and materials**. Ringworld supplies deterministic placement, cylindrical height/up, streaming, bounded GPU culling, three object LODs and nearby tree-trunk proxies. Shared trunk/leaf scatters use the same transforms. The GPU evaluator is original Ringworld code; Parallax source and asset bundles are not included.

The bridge shows Parallax scatter objects by default. Native vegetation can be restored with the fallback setting or by disabling the bridge. Base terrain supplies distant/map views. This bridge does not replace Parallax's planetary PQS system or add displaced Ringworld terrain. See [coverage for all 13 biome labels](docs/BIOMES.md) for aquatic vegetation, cold/alpine cover, rocks, desert plants and the intentionally clear infrastructure surfaces.

## Installation

Install Niven Ringworld 1.1.8 or newer, then this extension into `GameData/RingworldParallax`. Install Parallax Continued and its dependencies separately. Set KSP's main-menu **Terrain Detail to High** as Parallax requires; Ringworld's own preset can remain Slow or lower. See [installation and settings](docs/guides/GETTING-STARTED.md), [release/dependency notes](RELEASE-NOTES.md), and [configuration and implementation](docs/developers/BRIDGE.md).

Installed supported extensions are enabled by default. Open **Ringworld ? Extensions ? Ringworld Parallax** for density, distance, object/generation budgets and nearby contacts. Quality presets adjust those budgets; disabling the extension restores native vegetation. Rotten Potato uses zero Parallax density.

Photo mode uses the selected photo quality preset for enabled Parallax scatters, prepares detail around the framed camera and waits for it before saving the image. Resume or Cancel restores your custom gameplay budgets. See [photo instructions](docs/guides/GETTING-STARTED.md#photo-mode).

## Development

From this directory run `build-visuals.ps1` with Unity 2019.4, then build the base mod and run `build.ps1 -Install`. Pure placement tests run automatically. Test dependencies under `artifacts/dependencies` are local research/test downloads and are excluded from repository/release content. The standalone release ZIP contains only this extension and documentation; dependencies are separate downloads. The owner handles publication.

[Credits](CREDITS.md) ? [licensing](THIRD-PARTY-NOTICES.md) ? [documentation](docs/README.md)

## Parallax-only preview

Version 1.0.0 defaults **Native scatter fallback** to off while the Parallax extension is enabled. Native trees, grass, rocks, dust/pollen and forest canopy meshes are not generated/rendered; rural buildings, landmarks and megastructures remain. This isolates the bridge result. Enable the fallback toggle in Extensions for comparisons, or disable Parallax to restore native scenery. Asset files are retained for that reversible fallback; none are bundled into the bridge.
