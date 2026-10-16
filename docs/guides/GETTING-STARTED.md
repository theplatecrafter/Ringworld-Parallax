# Installation and settings

Use Niven Ringworld 1.1.8 or newer. Install Parallax Continued with its complete dependency/asset set, then install this extension into GameData/RingworldParallax. Base 1.1.7 and earlier do not contain the required host interface.

Open Ringworld's Extensions tab and expand Ringworld Parallax. Installed supported bridges start enabled; your saved disable choice persists. Distance is in metres. Density, the total visible-object budget and the generation budget limit GPU/CPU work. Quality presets populate these values. Start with Slow or lower on a laptop. Rotten Potato disables Parallax populations through zero density.

Tree trunks have optional simple nearby collision proxies. Grass, flowers and underwater plants have no colliders. Rendering does not alter atmosphere, terrain contact, buoyancy or stock parts.

## KSP graphics settings and startup warnings

Parallax Continued requires **Settings > Graphics > Terrain Detail: High** in the KSP main menu. This is KSP's planetary terrain setting, separate from Ringworld's quality presets: you can keep Ringworld on Slow or lower. The bridge does not change either setting automatically. Keep reflection resolution at 128 to avoid Parallax's separate high-reflection-cost warning.

If Parallax reports that terrain detail is not High, correct that setting and restart KSP. Kopernicus can also warn that terrain detail changed when its remembered setting differs from KSP's. Changing planetary terrain detail can change stock/planet-pack surface elevations; back up saves with vessels landed on those bodies before changing it. Ringworld's cylindrical terrain and saved ring geography do not use this PQS setting.

A Kopernicus **missing textures**, **planetary system failed to load**, or **unsupported KSP version** warning is a different problem. Preserve the complete warning and KSP.log; do not suppress it or reset the save. The [startup warning investigation](../../../Ring%20World%20KSP/docs/developers/STARTUP-WARNINGS.md) records the development instance's actual results.

## Photo mode

Frame the shot in flight, choose **Photo quality** in the Ringworld panel and enter photo mode. Installed, enabled Parallax scatters use that temporary preset's distance, density and object/generation budgets. Detail is centred on the photo camera and capture waits for generation to finish. New objects are fully visible in the saved photo instead of being caught mid-fade. Frozen preparation uses a small additional generation budget while physics is paused; your gameplay budget is unchanged. Higher presets can take longer to prepare; each scatter profile's distance limit and stock scatter density still apply.

Resume or Cancel restores your gameplay values, including custom budgets. Photo mode does not enable a disabled extension. Rotten Potato sets Parallax density to zero. Temporary photo settings do not create extra tree-trunk collision proxies while flight is frozen.

If assets/shaders cannot load, the status message identifies the failure. With the default native-fallback setting off, the surface remains bare of native vegetation; enable fallback or disable the bridge to restore it. Missing a shared leaf material makes that tree family unavailable rather than drawing bare trunks. Map/Tracking views do not instantiate the entire ring. Native aggregate canopy is suppressed while the bridge is enabled with fallback off.

Current limitation: this integrates scattered objects, not Parallax terrain tessellation/displacement. Native ground cover and rocks are suppressed by default alongside trees and canopy. Buildings remain. Other Parallax versions, higher desktop profiles and combined Deferred paths need testing before release.

## Parallax-only preview

Version 1.0.0 defaults **Native scatter fallback** to off while the Parallax extension is enabled. Native trees, grass, rocks, dust/pollen and forest canopy meshes are not generated/rendered; rural buildings, landmarks and megastructures remain. This isolates the bridge result. Enable the fallback toggle in Extensions for comparisons, or disable Parallax to restore native scenery. Asset files are retained for that reversible fallback; none are bundled into the bridge.
