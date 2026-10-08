# Ringworld Parallax release notes

## Unreleased development 0.1.0

- Terrain-coloured scatters now receive Ringworld's terrain tint instead of white; this corrects the white Grassland blades while preserving uncoloured profiles' own materials. Texture loading follows shader-declared dimensions, including cubemap refraction properties. Requires the paired base lighting development build; existing external dependencies and ABI requirements are unchanged. Upstream shaders/assets remain unmodified and separately installed.
- Slow-preset runtime colour validation passed at the reported Grassland coordinates: 439 grass candidates, visible object LOD counts 1/34/98, green-blade close-up capture, and valid shader texture dimensions. The paired base retained consistent lighting over 120 moving camera views. No upstream dependency or asset changes are required.
- All 13 existing biome labels now have explicit scatter policies: 16 profiles include alpine/cold conifers, cacti, mountain boulders, ice, shallow aquatic vegetation, deep seabed stones and ruin rubble. Roads, rim walls and scrith intentionally remain clear. Biome-specific masks fix wetland reeds and older-generator desert/snow classification. Water-depth limits and wider default floor-elevation limits support deep and rescaled terrain. Collider checks keep ruin rubble out of buildings and vessels.
- Configuration/placement suite: 18,050 checks passed. Biome policy and asset-binding checks are separate from all-biome visual/performance certification. Dependencies remain the same separately installed stock scatter pack and Parallax Continued stack; no new texture pack or bundled asset is introduced.
- Installed-assets validation at Slow loaded all 16 profiles and all three mesh/material LODs, and passed the 13-biome policy check (`PostCrash-biomes.log`). Visual and performance testing across every biome remains pending.
- October 8 actual desert fixture passed at Slow, with KSP scatter density 20.2962% and extension density 35%: 126 dry-grass candidates, 10 rocks and 9 cacti; all three families produced nonzero visible GPU LOD counts. The capture was at night, so this is placement/render-output validation rather than daylight appearance certification. Small objects remain sparse at these combined densities.

- Original cylindrical placement and GPU evaluator driving installed Parallax scatter models/materials.
- Configurable ring/biome profiles for trees, grass, undergrowth, flowers, dry cover, shore vegetation, submerged plants and rocks; infrastructure exclusions remain authoritative.
- Stable periodic placement cells independent of terrain LOD and origin shifts; three object LOD buffers, deterministic transition/fade selection and bounded visibility buffers.
- Shared trunk/leaf parts, replacement-readiness checks, native tree fallback, local contacts and map isolation.
- Extension controls in the base mod and quality-preset budgets.

### Required dependencies

- **NivenRingworld: matching unpublished bridge branch**, Terrain API v2 and `IScatterProvider` ABI. Published 1.1.7 alone is insufficient. Assign a minimum base release only after that version is chosen and validated.
- **ParallaxContinued**: integration targets public **1.0.4**; other binary/asset layouts are not certified.
- **ParallaxContinued-Scatter-Textures**: default profiles reference stock asset pack **1.0.0**. Own/community asset profiles can reference other installed scatter definitions.
- **KSPTextureLoader**: texture loading API supplied by Parallax/Kopernicus's dependency stack.
- KSP 1.12.5 Windows x64 / Direct3D 11; compute shaders and instancing required. Proton/DX11 and native Linux are not runtime-certified by this development test.
- Base-mod Harmony2 requirement remains >=2.2.1.0. Parallax's current CKAN metadata additionally requires ModuleManager, Kopernicus, KSPBurst, Terrain texture and Planet texture packages. Install the complete upstream dependency set; a manual scatter-only installation is not a full planetary Parallax installation.

### Optional integrations and conflicts

Ringworld Clouds, Ringworld Scattering and Cyla remain separate optional base extensions. Deferred can affect installed Parallax material lighting; its combined rendering path still needs dedicated validation. Older Parallax packages conflict with Continued according to its metadata. The bridge introduces no replacement config provider and does not bundle any dependencies or modify NetKAN metadata.

### Installation and saves

Install into `GameData/RingworldParallax` alongside the matching base. Visual settings are saved per ring. Changing density/preset does not change terrain, science or vessel persistence. Tree contact proxies are temporary nearby physics objects, not persisted parts. Publication and ZIP packaging remain pending.

### Validation

Base/core: 110,581 checks passed. Bridge: 18,029 deterministic placement/biome checks passed. Base and bridge DLLs built without warnings; original D3D11 compute bundle compiled. Installed-Parallax flight/preview regression passed on this laptop at Slow: nine families loaded, all three LOD outputs nonempty, native preview layers absent, map/disable isolation passed. See docs/developers/BRIDGE.md for the log and limits. No complete compatibility or FPS claim is made yet.

## Parallax-only preview

This development branch defaults **Native scatter fallback** to off while the Parallax extension is enabled. Native trees, grass, rocks, dust/pollen and forest canopy meshes are not generated/rendered; rural buildings, landmarks and megastructures remain. This isolates the bridge result. Enable the fallback toggle in Extensions for comparisons, or disable Parallax to restore native scenery. Asset files are retained for that reversible fallback; none are bundled into the bridge.

## Development lighting follow-up

The paired base development build corrects overlapping starlight and scopes Deferred diffuse ambient lighting for ring cameras. The bridge continues to use separately installed Parallax materials; no upstream assets or dependency binaries were changed. The base update also limits retained near terrain during continuous movement and guards stock conic/orbit queries during transfers and debris initialization. Dependencies and publication status remain unchanged. See the base lighting/landing investigation for validation and limits.
