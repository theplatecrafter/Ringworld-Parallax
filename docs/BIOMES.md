# Biome scatter coverage

Ringworld Parallax uses the ring's own biome labels, terrain elevation, cylindrical surface normal and water depth. It does not substitute the host star's biome map. Default profiles reference separately installed Parallax Continued stock scatter assets.

| Ringworld biome | Default appearance |
| --- | --- |
| Ocean | Seabed stones at all depths; seagrass in shallow water and kelp at 3-80 m depth. |
| Lake | Shallow vegetation, kelp where deep enough, seabed stones and dry-bank reeds. |
| River | Reeds at shallow margins, submerged vegetation and bed stones. |
| Wetland | Reeds in up to 2 m of water, shallow submerged cover, dry patches of grass and bed stones. |
| Grassland | Grass, flowers, ferns, dry cover, stones and ecologically selected tree stands. |
| Forest | Oak/pine stands, ferns, grass and stones; clearing masks remain respected. |
| Desert | Dry grass, sparse small cacti and stones. |
| Mountain | Talus/boulders, stones and sparse conifers below 3,500 m elevation. Steeper rock-placement slopes are permitted. |
| Snow | Ice/gravel, stones and sparse low-elevation conifers. |
| Ruins | Small rubble, with a collider-clearance check around buildings and vessels. Ordinary forest/grass placement remains excluded. |
| Scrith | Intentionally bare exposed structural material. |
| Rimwall | Intentionally clear wall surface and access infrastructure. |
| Road | Intentionally clear traffic surface. |

These are **scatter profiles**, not Parallax's planetary terrain/displacement shader. Base Ringworld terrain materials remain in use. Buildings, landmarks and megastructures remain owned by the base mod.

Scatters whose Parallax definition requests terrain colouring use the ring's local ground colour, including grass. Other objects retain their own texture colours; white flower petals are expected. Shadow resolution follows KSP graphics settings, with a local shadow range supplied by the paired base development build.

All object families use their installed near/middle/far meshes and materials. Settings under Extensions control the shared density, range and budget; installing more biome profiles does not raise those budgets. Very low settings may show sparse objects. Broad views still use the base terrain representation.

KSP's **Terrain Scatter Density** also multiplies the extension density. For example, a 20% KSP density with the Slow extension's 35% density gives about 7% of the profile's candidate density. Dry grass is limited to 180 m, cacti to 600 m, and rocks to the extension's scatter range (up to the profile's 1,200 m). These small objects can be difficult to see from an aircraft or a distant camera. Check the actual biome label and the Extensions status to distinguish sparse desert cover from missing assets.

## Customization

Patch `RINGWORLD_PARALLAX_SCATTER` nodes with ModuleManager or add uniquely named profiles anywhere under GameData. `biomes` is a comma-separated list of the exact names above, or `*`. `ringId = *` targets all rings. The complete example set is in `GameData/RingworldParallax/Scatters.cfg`.

Use `minElevation` / `maxElevation` for inner-floor elevation in metres and `minWaterDepth` / `maxWaterDepth` for water depth in metres. Dry land has depth zero. Alpine height limits and aquatic depth limits are editable per profile. Modes include forest, meadow, undergrowth, desert, snow, conifer, talus, shore, submerged, seabed, rubble and dry. Rubble is restricted to ruins; roads, rim walls and scrith are protected from ordinary scattering.

Asset names, models and textures must exist in a separately installed Parallax definition. No dependency assets are bundled. Shipped policies are covered by configuration tests; runtime asset/LOD binding validation is recorded in the developer guide. These checks do not certify screenshots or performance in every biome on every GPU.
