# Ringworld Parallax release notes

## 1.0.1 — artifacts prepared 15 October 2026

### Local metadata review after packaging — 15 October 2026

`NetKAN/NetKAN/RingworldParallax.netkan` now explicitly declares the bridge's direct stock scatter asset requirement (ParallaxContinued-Scatter-Textures >=1.0.0) and KSPTextureLoader >=1.0.36 texture-loading API. These were already required in the manual-pack dependency record; relying on Kopernicus's transitive >=1.0.26 loader bound allowed an older, unvalidated API. This is a metadata correction, not an added payload or base ABI change. Keep NivenRingworld >=1.1.8 and ParallaxContinued >=1.0.3. Source compiles against released base 1.1.8 and 1.1.9 SDKs. Exact upstream 1.0.3 rendering is not newly certified; the installed-stack tests use 1.0.4.

The already prepared 1.0.1 ZIP is not rebuilt by this metadata task. Owner submission remains necessary. See the base metadata compatibility audit for evidence and runtime limits.



[Player-facing release notes](docs/releases/v1.0.1.md). Local preparation only; the owner handles publication.

Requires NivenRingworld >=1.1.8. Authoritative metadata uses ParallaxContinued >=1.0.3; runtime/asset validation targets 1.0.4, stock scatter/terrain/planet assets 1.0.0, KSPTextureLoader 1.0.36 and KSPBurst/KSPBurst-Lite 1.7.4.11 with upstream Harmony/ModuleManager/Kopernicus requirements. Other upstream layouts/exact minimum runtime combinations are not certified. No upstream redistribution. Clouds, Scattering and Cyla optional; older Parallax conflicts with Continued through upstream metadata. Install into GameData/RingworldParallax.

No base minimum or runtime requirement change; the metadata follow-up above makes existing direct asset/loader requirements explicit. No DLL/type rename, save-format change, terrain-generator change or reset. Existing ring identities, world parameters, resident poses, progress and explicit settings are retained. Stable assembly identities are preserved while file/product versions advance. Standalone archives contain only their own mod and documentation. The Parallax NetKAN correction above is prepared locally for owner submission.

Release validation (15 October 2026): fresh `StartupCompatibility-release-1.1.10-retained.log` passed retained formats 3/4/pre-1.1.8 format 5, repeated migration/options/generator/pose/progress preservation, legacy resident restore/resave and real Flight Map -> Tracking Station -> Space Center transitions with KSPCF. Original retained files were hash-protected. Historical scenario data and a synthetic legacy resident are complementary coverage; this does not certify every old craft/mod binary combination. Current renderer source also passed `FramePerformance-overcast-followup-high.log`, `ParallaxPhoto-performance-4.log` and `Issue7Performance-rendering-final-9.log` before the version-only preparation. Core/placement suites: 143,203/18,050. No serialization or preset-budget changes. Normal DLLs omit test harnesses; archive checks verify versions, CRCs, hashes, paths and dependency exclusions.

### startup settings documentation (15 October 2026)

- Document Parallax Continued's KSP Terrain Detail **High** requirement separately from Ringworld quality presets, plus the related Kopernicus terrain-detail-change warning. The bridge does not force global graphics settings or suppress upstream checks.
- The shared development instance had Low selected while Kopernicus remembered High. A read-only main-menu observer reproduced Parallax's actual error and checked missing-texture records. See the base [startup investigation](../Ring%20World%20KSP/docs/developers/STARTUP-WARNINGS.md) for corrected-setting validation and save considerations.

Validation: High-setting startup observed 150 menu frames, no active popups and zero Kopernicus missing textures. Normal base binaries were restored. No dependency, asset redistribution, persisted Ringworld preference or bridge ABI change. The development instance global PQS preset changed from Low to High with a backup; the plugin does not enforce that change for players. Required minimums remain base >=1.1.8 and the separately installed Parallax Continued/assets stack.

### scatter culling and allocation (15 October 2026)

- Extract camera frustum planes once and reuse plane/counter arrays. Group patches by scatter family instead of scanning every patch for each family.
- Reject entire patches only when conservative bounds prove that the existing GPU frustum/range tests cannot retain any candidate. Skip empty-family compute/indirect draws without GPU readbacks in live flight. Preserve object population, biome masks, all three LODs, fade rules and photo-camera routing.
- Plan incoming cells as value records with cached sort distances; construct patch objects/contact dictionaries only for admitted cells. Keep the previous ordering and population limits. Reuse vessel-position storage and remove per-candidate LINQ closures in tree contact checks; collision ranges remain unchanged and independent of camera culling.

Required base minimum remains 1.1.8 with its existing photo handshake. Parallax Continued >=1.0.3 and the existing separately installed upstream scatter assets/transitive requirements are unchanged. Nothing from upstream is redistributed. Normal builds omit profiling/test hooks; no NetKAN edits, save migration or publication is performed. Runtime comparison checks cover GPU object LOD counts with and without the new patch rejection; photo validation is recorded with the performance evidence.

Validation: four viewing directions retained identical GPU object LOD counts with patch rejection enabled/disabled. `ParallaxPhoto-performance-4.log` passed a real 512-pixel capture (visible LODs 3/24/41), ready population, all eleven preset-budget round trips, world/sky alignment and Resume/Cancel/disabled-extension behaviour. Higher presets were not rendered. Placement/configuration checks: 18,050. See the base [performance report](../Ring%20World%20KSP/docs/developers/FRAME-PERFORMANCE-20261015.md) for timings and limits.

## 1.0.0 - artifacts prepared October 9, 2026

- Photo mode now waits for the selected preset's scatter population and generates it around the photo camera. Ready objects render fully visible in the scene capture, including new populations that would still be fading into live flight. Resume/Cancel restores custom live distance, density, capacity and generation budgets. Disabled extensions stay disabled; Rotten Potato retains zero density. Temporary photo budgets do not generate physical tree proxies while flight is frozen.
- Requires the base 1.1.8+ with the optional `IPhotoScatterLayer` handshake. External Parallax Continued/stock scatter asset/KSPTextureLoader/Kopernicus/ModuleManager/KSPBurst dependencies and version targets below remain unchanged; nothing is bundled. No NetKAN edit, package or publication is part of this fix.
- October 9 photo validation (`ParallaxPhoto-3.log`): all eleven presets applied/restored Parallax budgets without rendering high-budget scenes; a real 512-pixel Slow photo used the ready camera-centred population and produced visible object LOD counts 3/24/41. The saved image contains grass and trees. Time/position stayed frozen, custom-budget Resume/Cancel checks passed, and disabled-extension capture completed without extra bridge draws. The base world/sky capture alignment check also passed with zero angular difference and viewport error 5.96e-8. Core checks: 110,644; placement/configuration checks: 18,050; builds passed without warnings/errors. Normal base/bridge DLLs are installed and original instance settings restored. High-quality/high-resolution desktop photo performance and broader camera-mod coexistence remain unverified.

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

### Metadata for the release owner

New identifier: RingworldParallax. Required edges: NivenRingworld min_version 1.1.8; ParallaxContinued version 1.0.4 (the binary/asset layout actually tested); ParallaxContinued-Scatter-Textures version 1.0.0; KSPTextureLoader min_version 1.0.36. Upstream Parallax dependencies supply terrain/planet packs 1.0.0, KSPBurst 1.7.4.11 and KSPBurst-Lite 1.7.4.11 in the tested stack. Do not declare a replacement-config provider or conflicts with the base Ringworld mod. Base should suggest the bridge, not require it. No .netkan/.ckan distribution copies were created and authoritative NetKAN files were not edited.

Only original bridge code/evaluator/configs are included. Parallax Continued and its assets are All Rights Reserved and require separate authorized downloads or explicit author permission for redistribution; local test downloads must not enter standalone release archives.

### Required dependencies

- **NivenRingworld >= 1.1.8**, Terrain API v2 and `IScatterProvider` ABI. Base 1.1.7 and earlier are incompatible.
- **ParallaxContinued**: integration targets public **1.0.4**; other binary/asset layouts are not certified.
- **ParallaxContinued-Scatter-Textures**: default profiles reference stock asset pack **1.0.0**. Own/community asset profiles can reference other installed scatter definitions.
- **KSPTextureLoader**: texture loading API supplied by Parallax/Kopernicus's dependency stack.
- KSP 1.12.5 Windows x64 / Direct3D 11; compute shaders and instancing required. Proton/DX11 and native Linux are not runtime-certified by this development test.
- Base-mod Harmony2 requirement remains >=2.2.1.0. Parallax's current CKAN metadata additionally requires ModuleManager, Kopernicus, KSPBurst, Terrain texture and Planet texture packages. Install the complete upstream dependency set; a manual scatter-only installation is not a full planetary Parallax installation.

### Optional integrations and conflicts

Ringworld Clouds, Ringworld Scattering and Cyla remain separate optional base extensions. Deferred can affect installed Parallax material lighting; its combined rendering path still needs dedicated validation. Older Parallax packages conflict with Continued according to its metadata. The bridge introduces no replacement config provider and does not bundle any dependencies or modify NetKAN metadata.

### Installation and saves

Extract from the KSP root into `GameData/RingworldParallax` alongside base >=1.1.8. Visual settings are saved per ring. Changing density/preset does not change terrain, science or vessel persistence. Tree contact proxies are temporary nearby physics objects, not persisted parts. The standalone 1.0.0 ZIP is prepared locally for the owner to upload. Publication has not been performed.

### Validation

Frozen photo preparation uses a bounded work budget of at least 4 ms per frame at the 15 FPS photo cap, without changing saved live budgets, population limits or selected render quality. The first release recheck reached terrain readiness but timed out with nine scatter patches queued; the corrected rerun passed.

Prior validation: base/core and bridge deterministic placement/biome checks passed. Base and bridge DLLs built without warnings; original D3D11 compute bundle compiled. Installed-Parallax flight/preview regression passed on this laptop at Slow: nine families loaded, all three LOD outputs nonempty, native preview layers absent, map/disable isolation passed. See docs/developers/BRIDGE.md for the log and limits. No complete compatibility or FPS claim is made yet.

## Parallax-only preview

Version 1.0.0 defaults **Native scatter fallback** to off while the Parallax extension is enabled. Native trees, grass, rocks, dust/pollen and forest canopy meshes are not generated/rendered; rural buildings, landmarks and megastructures remain. This isolates the bridge result. Enable the fallback toggle in Extensions for comparisons, or disable Parallax to restore native scenery. Asset files are retained for that reversible fallback; none are bundled into the bridge.

## Development lighting follow-up

The paired base development build corrects overlapping starlight and scopes Deferred diffuse ambient lighting for ring cameras. The bridge continues to use separately installed Parallax materials; no upstream assets or dependency binaries were changed. The base update also limits retained near terrain during continuous movement and guards stock conic/orbit queries during transfers and debris initialization. Dependencies and publication status remain unchanged. See the base lighting/landing investigation for validation and limits.

Release validation: ParallaxPhoto-release-1.0.0-2.log passed native default suppression, explicit fallback persistence, disabled/absent-provider scopes, all eleven photo-preset budget/restoration checks, real 512-pixel Slow capture (visible object LOD counts 3/24/41), two secondary-camera alignment checks (zero rotation error, viewport error 5.96e-8), frozen flight, Resume/Cancel restoration and disabled-provider capture. All 18,050 placement/biome and 110,644 base/core checks passed. Normal release builds exclude BridgeSmoke and the base smoke harness. No high-preset scene or desktop performance certification is implied.
