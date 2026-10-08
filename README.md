# Ringworld Parallax

Biome-aware Parallax Continued scatter assets on Niven's Ringworld's cylindrical terrain.

This is an **unreleased development bridge**, paired with the `parallax-continued-bridge` branch of the base mod. It is not compatible with the published base 1.1.7 binary: it requires the new scatter-provider ABI and Terrain API v2.

The bridge uses **separately installed Parallax Continued models, textures and materials**. Ringworld supplies deterministic placement, cylindrical height/up, streaming, bounded GPU culling, three object LODs and nearby tree-trunk proxies. Shared trunk/leaf scatters use the same transforms. The GPU evaluator is original Ringworld code; Parallax source and asset bundles are not included.

The development preview shows Parallax scatter objects by default. Native vegetation can be restored with the fallback setting or by disabling the bridge. Base terrain supplies distant/map views. This bridge does not replace Parallax's planetary PQS system or add displaced Ringworld terrain. See [coverage for all 13 biome labels](docs/BIOMES.md) for aquatic vegetation, cold/alpine cover, rocks, desert plants and the intentionally clear infrastructure surfaces.

## Installation

Build/install the matching base branch, then this extension into `GameData/RingworldParallax`. Install Parallax Continued and its dependencies separately. See [installation and settings](docs/guides/GETTING-STARTED.md), [release/dependency notes](RELEASE-NOTES.md), and [configuration and implementation](docs/developers/BRIDGE.md).

Installed supported extensions are enabled by default. Open **Ringworld ? Extensions ? Ringworld Parallax** for density, distance, object/generation budgets and nearby contacts. Quality presets adjust those budgets; disabling the extension restores native vegetation. Rotten Potato uses zero Parallax density.

## Development

From this directory run `build-visuals.ps1` with Unity 2019.4, then build the base mod and run `build.ps1 -Install`. Pure placement tests run automatically. Test dependencies under `artifacts/dependencies` are local research/test downloads and are excluded from repository/release content. No package or remote repository has been published.

[Credits](CREDITS.md) ? [licensing](THIRD-PARTY-NOTICES.md) ? [documentation](docs/README.md)

## Parallax-only preview

This development branch defaults **Native scatter fallback** to off while the Parallax extension is enabled. Native trees, grass, rocks, dust/pollen and forest canopy meshes are not generated/rendered; rural buildings, landmarks and megastructures remain. This isolates the bridge result. Enable the fallback toggle in Extensions for comparisons, or disable Parallax to restore native scenery. Asset files are retained for that reversible fallback; none are bundled into the bridge.
