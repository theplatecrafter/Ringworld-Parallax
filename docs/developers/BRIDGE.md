# Bridge configuration and implementation

Status: working development bridge; one installed-assets flight/preview regression passed. Not release-certified.

## Configuration

Add RINGWORLD_PARALLAX_SCATTER nodes anywhere under GameData; ModuleManager may patch them. Each ID must be unique within the selected ring profile set. Use `ringId = *` for all rings, or a saved ring ID. `sourceScatter` is an exact key from Parallax's loaded definitions, including its body prefix (for example Kerbin-OakTree). Choose a parent scatter; shared child meshes/materials are automatically attached to the same placement.

Fields: id, ringId, sourceScatter, family (Trees/Ground/Rocks), mode, biomes (comma-separated exact Ringworld labels or *), spacing in metres, chance, range in metres, minimumUpDot, scaleMultiplier, minElevation, maxElevation, minWaterDepth and maxWaterDepth. Modes: forest, meadow, undergrowth, desert, snow, conifer, talus, shore, submerged, seabed, rubble and dry. Altitude ranges are Ringworld inner-floor elevations, not spherical host altitude. Water limits use metres above the seabed (dry = zero). Forest defaults use the ecological forest/clearing mask and existing conifer selection; alpine conifers use a separate cold/highland mask and height cutoff. Roads, walls and exposed scrith remain clear. Ruins accept rubble only, with building/vessel collider clearance. See [all-biome coverage](../BIOMES.md).

Reference models/textures must be installed separately. Custom assets can be registered through a separately installed Parallax scatter definition and referenced here; they do not need to replace a stock planet's definition.

## Data and rendering

Photo capture uses the same scatter evaluator/materials and selected temporary preset budgets. The optional host `IPhotoScatterLayer.PhotoReady` handshake gates frozen scene capture until generation finishes; absent/disabled/unavailable providers do not block it. During photo preparation, the host supplies the photo camera's ring chart instead of the vessel chart. Ready objects bypass their live introduction fade for the captured image. Contact-proxy generation is suspended while time is frozen. The existing settings snapshot restores custom budgets on Resume/Cancel. This handshake requires the base >=1.1.8 and bridge >=1.0.0; it adds no external dependency.

The host capture stack aligns local/far, scaled-space and galaxy views to the actual photo camera. KSP's native scaled camera follows the parent flight-camera rig; directly positioning its child for a test/custom view otherwise leaves different orientations in one image. The scaled-ring observer reference and camera transforms/projections are scoped to capture and restored afterwards.

October 9 verification: `template_instance/ParallaxPhoto-3.log` passed actual photo application/restoration of scatter budgets for all eleven presets, camera-centred readiness, a real 512-pixel Slow capture with visible LOD counts 3/24/41, frozen UT/position, Resume/Cancel and disabled-extension capture without extra bridge draws. A capture-time camera probe measured zero rotation difference and shared-point viewport error 5.96e-8 between local and background views; the native scaled-camera reference restored. The enabled saved image was inspected and contains grass and trees. High-preset scenes were not rendered on this laptop.

Run `tools/VerifyPhoto.ps1` from the bridge directory with KSP closed and the external asset/dependency stack installed. It builds both matching DLLs, runs an isolated save and restores the normal base DLL and backed-up KSP settings in `finally`. `ParallaxPhoto-1.log` is an intermediate fixture failure: the disabled check expected a lifetime capture counter to reset to zero rather than remain unchanged. That assertion was corrected before complete passes in runs 2 and 3. No dependency was added or bundled and no release ZIP was created.

Canonical patches cover approximately 256 m along and exactly 256 m across. The along count covers the circumference exactly, so the key wraps without a partial duplicate seam. Candidate hashing uses ring seed, profile salt and canonical sample indices. Rendered mesh subdivision and frame epochs do not seed placement.

Generation yields between rows within the saved millisecond budget. Near positions project to the base's streamed triangle height; normal filtering uses cylindrical surface up. Patch-local floats are computed after double-precision anchor subtraction. Live matrices apply frame rotation/origin changes. Counts, residency and generation time are bounded.

The evaluator is authored in this repository. It produces the instance-matrix/tint interface expected by installed Parallax materials, using safe orientation bases, sphere/frustum culling, deterministic LOD transitions, range fades and capacity-clamped draw arguments. It does not modify Parallax/PQS registries, shader globals or stock body biomes. Each shared trunk/leaf mesh uses the same instance buffers.

Scatter definitions with `distributionParams.coloredByTerrain` use the base terrain tint in that instance buffer. Other definitions use the neutral white multiplier. Shader texture dimensions govern 2D/cubemap loading. The base's shared ring-camera lighting/shadow context applies to these installed materials; there is no per-part brightness patch. See the base [surface-lighting investigation](../../../Ring World KSP/docs/developers/SURFACE-LIGHTING.md).

The base Terrain API v2 publishes ready/retiring notifications, near top-mesh colours and arbitrary loaded-ring-coordinate environmental/biome samples. Existing snapshot callers remain supported. The extension currently uses the first-party provider ABI and Ringworld.Core; API-version requirements must be documented when released.

Native canopy patch visuals remain until tree patches are ready. Native far canopies/biome colours remain in terrain LOD/map views. Temporary contact proxies follow the same frame and are destroyed with their generation. Failed assets and disabled settings return ownership to the native representation.

## Validation record

- October 8 colour/shadow follow-up: `artifacts/validation/surface-lighting-smoke.txt` confirms actual `coloredByTerrain` grass buffers, 439 candidates and visible LOD counts 1/34/98. The base diagnostic `terrain-tinted-grass.png` visibly shows green blades. Cube properties passed dimension checks without the old assignment errors. The paired base shared-light checks passed 30 paused renders and 120 moving views at Slow. Low-resolution shadow aliasing and other hardware/presets remain limitations.
- October 8: `artifacts/validation/desert-smoke.txt` uses a real Desert sample at along 53,704,982,472.2849 m / across 54,298,840.1298056 m, seed 1403775798. At Slow and the user's 20.2962% stock scatter density, generated counts were dry grass 126, rocks 10 and cacti 9. Visible object LOD counts were 1/2/7, 0/0/4 and 1/2/0 respectively. `desert-ground.png` is a nighttime capture; it establishes no daylight colour/visibility certification. No density increase or new dependency was needed for these families to appear.
- October 7 biome expansion: 18,050 placement/configuration checks passed. `artifacts/validation/biomes-smoke.txt` records all 16 profiles loading against the installed upstream assets, supported materials and meshes in all three object LODs, and the 13-biome coverage policies. This checks actual asset binding and policy sampling, not screenshots/performance at every natural biome.
- Base/core 110,581 checks passed.
- Bridge 18,029 checks passed for periodic canonical keys, deterministic samples, climate masks, biome exclusions, submerged placement and clearings.
- Base/plugin and bridge builds passed without warnings; original compute bundle compiled on Unity 2019.4.18f1 for D3D11.
- `RingworldParallaxSmoke-20261007-182054.log`: all nine profile families loaded against Parallax Continued 1.0.4 / stock scatter assets 1.0.0. Forest fixture produced 371 resident candidates at capture, visible object LOD counts 7/40/38; nearest native-tree handover readiness passed. Most distant pending patches had not completed at capture.
- Parallax-only preview asserted no native ForestCanopy components, no active native ground cover and no native distant canopy objects. Screenshot `artifacts/validation/bridge-ground.png` reviewed: Parallax pines/ferns/ground cover render, buildings/colossi remain, oversized native crown triangles are absent.
- Map isolation and disabling the bridge passed. Base/native rendering restores when the bridge is disabled. This is a focused scene regression, not all-biome or hardware certification.
- The first manual dependency test also exposed an upstream BiomeLoader exception when eligible biome counts went below zero. Installing the full upstream asset/dependency set did not eliminate every upstream/PQS warning; the bridge does not consume that biome map. Planetary coexistence still needs investigation.

Pending release gates: visible-scene image review, motion/LOD/native-fallback handover, all biome fixtures including underwater, terrain revisions, moving/tilted/reverse-spin rings, collision contact alignment, absence/version mismatch tests, full planetary Parallax coexistence, camera/map/photo transitions, leak and performance measurements. Higher presets must be tested on suitable desktop hardware; this laptop is restricted to Slow or lower.

## Parallax-only preview

Version 1.0.0 defaults **Native scatter fallback** to off while the Parallax extension is enabled. Native trees, grass, rocks, dust/pollen and forest canopy meshes are not generated/rendered; rural buildings, landmarks and megastructures remain. This isolates the bridge result. Enable the fallback toggle in Extensions for comparisons, or disable Parallax to restore native scenery. Asset files are retained for that reversible fallback; none are bundled into the bridge.
