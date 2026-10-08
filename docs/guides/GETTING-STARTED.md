# Installation and settings

Use the matching Ringworld bridge development build. Install Parallax Continued with its complete dependency/asset set, then install this extension into GameData/RingworldParallax. Do not combine the extension with published Ringworld 1.1.7 until a release containing the new host interface exists.

Open Ringworld's Extensions tab and expand Ringworld Parallax. Installed supported bridges start enabled; your saved disable choice persists. Distance is in metres. Density, the total visible-object budget and the generation budget limit GPU/CPU work. Quality presets populate these values. Start with Slow or lower on a laptop. Rotten Potato disables Parallax populations through zero density.

Tree trunks have optional simple nearby collision proxies. Grass, flowers and underwater plants have no colliders. Rendering does not alter atmosphere, terrain contact, buoyancy or stock parts.

If assets/shaders cannot load, the status message identifies the failure and native forest rendering remains. Missing a shared leaf material makes that tree family unavailable rather than drawing bare trunks. Distant forests use the base mod's aggregate canopy; map/Tracking views do not instantiate the entire ring.

Current limitation: this integrates scattered objects, not Parallax terrain tessellation/displacement. Native ground micro-cover and a few native rocks remain alongside added profiles while broader ownership/handover work is validated. Other Parallax versions, higher desktop profiles and combined Deferred paths need testing before release.

## Parallax-only preview

This development branch defaults **Native scatter fallback** to off while the Parallax extension is enabled. Native trees, grass, rocks, dust/pollen and forest canopy meshes are not generated/rendered; rural buildings, landmarks and megastructures remain. This isolates the bridge result. Enable the fallback toggle in Extensions for comparisons, or disable Parallax to restore native scenery. Asset files are retained for that reversible fallback; none are bundled into the bridge.
