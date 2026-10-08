using System;using System.Collections;using System.IO;using UnityEngine;using NivenRingworld;using NivenRingworld.Extensions;
namespace Ringworld.Parallax {
 // Runs only when explicitly requested by the base development smoke harness.
 public static class BridgeSmoke {
  public static bool GrassTintReady(){var layer=(ExtensionProviders.Parallax as ParallaxProvider)?.Current;return layer!=null&&layer.CandidatesFor("grass")>0&&layer.HasTerrainTint("grass");}
  public static void ValidateTint(){
   var layer=(ExtensionProviders.Parallax as ParallaxProvider)?.Current;if(layer==null||!GrassTintReady())throw new Exception("Grass does not carry terrain tint");
   var lod=layer.CountsFor("grass");if(lod[0]+lod[1]+lod[2]==0)throw new Exception("No visible terrain-coloured grass GPU output");
   Debug.Log("[RingworldSmoke] GRASS terrainTint=True candidates="+layer.CandidatesFor("grass")+" visibleLODs="+string.Join(",",lod));
   foreach(var p in layer.Profiles)foreach(var part in p.Assets.Parts)foreach(var mat in part.Materials){
    int i=mat.shader.FindPropertyIndex("_RefractionTexture");if(i>=0&&mat.shader.GetPropertyTextureDimension(i)==UnityEngine.Rendering.TextureDimension.Cube&&mat.GetTexture("_RefractionTexture")!=null&&!(mat.GetTexture("_RefractionTexture") is Cubemap))throw new Exception("Wrong refraction texture dimension: "+p.Id);
   }
   Debug.Log("[RingworldSmoke] PASS terrain-coloured grass buffers and material texture dimensions");
  }
  public static IEnumerator RunDesert(RingworldFlight f){
   var options=f.Settings.Save();options.SetValue("seed",1403775798,true);f.ApplyOptions(options,true);
   var rng=new System.Random(371325661);double along=0,across=0;bool found=false;
   for(int i=0;i<20000;i++){
    double a=rng.NextDouble()*f.Settings.Geometry.P.Circumference,b=(rng.NextDouble()-.5)*f.Settings.Geometry.P.Width*.9;
    var t=f.Settings.Terrain.Sample(a,b);
    if(t.Biome==Ringworld.Core.Biome.Desert&&!t.Wet){along=a;across=b;found=true;break;}
   }
   if(!found)throw new Exception("No desert fixture found");
   Debug.Log("[RingworldSmoke] DESERT fixture along="+along+" across="+across+" stockDensity="+StockGraphics.Scatter+" bridgeDensity="+f.Settings.ParallaxDensity);
   f.arrivalHeight=12;typeof(RingworldFlight).GetMethod("VisitCoordinates",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(f,new object[]{along,across});
   while(!f.Ready)yield return null;
   // Replay the caller identified in the live dump, not only the math helper.
   var sun=FlightGlobals.GetBodyByName("Sun");var kerbin=FlightGlobals.GetBodyByName("Kerbin");
   var distantOrbit=new Orbit(0,0,1e18,0,0,0,Planetarium.GetUniversalTime(),sun);
   var transfer=new TransferDataSimple(null,null){SourceBody=f.Star,TargetBody=kerbin};
   var plannerClock=System.Diagnostics.Stopwatch.StartNew();
   double alignment=TransferMath.AlignmentTime(transfer,distantOrbit,kerbin.orbit,Planetarium.GetUniversalTime());
   if(plannerClock.Elapsed.TotalSeconds>1||double.IsNaN(alignment)||double.IsInfinity(alignment)||alignment<0||double.IsNaN(transfer.phaseAngleTarget))throw new Exception("Stock transfer planner large-distance regression failed");
   Debug.Log("[RingworldSmoke] PASS stock TransferMath.AlignmentTime interstellar-distance fixture milliseconds="+plannerClock.Elapsed.TotalMilliseconds);
   var provider=ExtensionProviders.Parallax as ParallaxProvider;if(provider==null)throw new Exception("Bridge provider not discovered");
   float deadline=Time.realtimeSinceStartup+100;
   while((provider.Current==null||provider.Current.Resident<100)&&Time.realtimeSinceStartup<deadline)yield return null;
   yield return new WaitForSecondsRealtime(20);
   var layer=provider.Current;if(layer==null)throw new Exception("Desert provider missing");
   var camera=FlightCamera.fetch.mainCamera;var pos=f.Position(FlightGlobals.ActiveVessel);var up=f.Settings.Geometry.Up(pos);var heading=f.Settings.Geometry.AlongDirection(pos);
   camera.transform.position=FlightGlobals.ActiveVessel.transform.position+ConvertVector.Unity(up*6-heading*30);
   camera.transform.rotation=Quaternion.LookRotation(ConvertVector.Unity(heading-up*.06),ConvertVector.Unity(up));camera.Render();
   int generated=0,visible=0;
   foreach(var id in new[]{"dry-grass","rocks","desert-cacti"}){
    int count=layer.CandidatesFor(id);var lod=layer.CountsFor(id);generated+=count;visible+=lod[0]+lod[1]+lod[2];
    Debug.Log("[RingworldSmoke] DESERT "+id+" generated="+count+" visibleLODs="+string.Join(",",lod));
   }
   if(generated==0||visible==0)throw new Exception("Desert scatters generated/drawn zero objects");
   var rt=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);var old=camera.targetTexture;var active=RenderTexture.active;var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
   try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(KSPUtil.ApplicationRootPath,"../Ringworld Parallax/artifacts/validation/desert-ground.png"),pixels.EncodeToPNG());}
   finally{camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.Destroy(pixels);rt.Release();UnityEngine.Object.Destroy(rt);}
   Debug.Log("[RingworldSmoke] PASS real desert placement and installed-material GPU output");
  }
  public static void ValidateBiomes(RingworldFlight f){
   var provider=ExtensionProviders.Parallax as ParallaxProvider;var layer=provider?.Current;
   if(layer==null)throw new Exception("No active bridge for biome asset validation");
   int count=0;foreach(var profile in layer.Profiles){
    if(profile.Assets==null)throw new Exception("Biome asset not loaded: "+profile.Id+" / "+profile.Source);
    foreach(var part in profile.Assets.Parts)for(int i=0;i<3;i++)if(part.Meshes[i]==null||part.Materials[i]==null||!part.Materials[i].shader.isSupported)throw new Exception("Missing biome object LOD/material: "+profile.Id);
    count++;
   }
   foreach(Ringworld.Core.Biome biome in Enum.GetValues(typeof(Ringworld.Core.Biome))){
    bool wet=biome==Ringworld.Core.Biome.Ocean||biome==Ringworld.Core.Biome.Lake||biome==Ringworld.Core.Biome.River||biome==Ringworld.Core.Biome.Wetland;
    var t=new Ringworld.Core.TerrainSample{Biome=biome,Height=100,WaterHeight=wet?101:double.NegativeInfinity,Shore=wet?1:0};
    var e=new Ringworld.Core.ClimateBlend{Forest=biome==Ringworld.Core.Biome.Forest?1:0,Meadow=biome==Ringworld.Core.Biome.Grassland?1:0,Desert=biome==Ringworld.Core.Biome.Desert?1:0,Cold=biome==Ringworld.Core.Biome.Snow?1:0,Highland=biome==Ringworld.Core.Biome.Mountain?1:0};
    float weight=0;foreach(var p in layer.Profiles)weight+=p.Weight(t,e,1);
    bool bare=biome==Ringworld.Core.Biome.Road||biome==Ringworld.Core.Biome.Rimwall||biome==Ringworld.Core.Biome.Scrith;
    if(bare?weight!=0:weight<=0)throw new Exception("Biome mask coverage failed: "+biome);
    Debug.Log("[RingworldSmoke] BIOME "+biome+" coverage="+weight+" bare="+bare);
   }
   Debug.Log("[RingworldSmoke] PASS 13 biome policies / "+count+" installed asset profiles / all three object LODs loaded");
  }
  public static IEnumerator Run(RingworldFlight f){
   var provider=ExtensionProviders.Parallax as ParallaxProvider;if(provider==null)throw new Exception("Bridge provider not discovered");
   var options=f.Settings.Save();RingQualityPresets.Apply(options,6);options.SetValue("atmosphereBackend",0,true);f.ApplyOptions(options,true);
   var start=f.Settings.Terrain.Landmarks[0];double along=start.Along+20000,across=start.Across+12000;
   for(int i=0;i<128;i++){
    double a=start.Along+20000+i*2048,b=start.Across+12000+i*337;
    var t=f.Settings.Terrain.Sample(a,b);
    if(t.Biome==Ringworld.Core.Biome.Grassland||t.Biome==Ringworld.Core.Biome.Forest){along=a;across=b;break;}
   }
   Debug.Log("[RingworldSmoke] PARALLAX fixture biome="+f.Settings.Terrain.Sample(along,across).Biome+" nativeScatter="+StockGraphics.Scatter);
   f.arrivalHeight=12;typeof(RingworldFlight).GetMethod("VisitCoordinates",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(f,new object[]{along,across});while(!f.Ready)yield return null;
   float deadline=Time.realtimeSinceStartup+100;
   while((provider.Current==null||provider.Current.Resident<200)&&Time.realtimeSinceStartup<deadline)yield return null;
   var layer=provider.Current;if(layer==null||layer.Resident<200)throw new Exception("No real Parallax population: "+provider.Message);
   float treesDeadline=Time.realtimeSinceStartup+180;
   while(!provider.OwnsTrees(along,across,0)&&Time.realtimeSinceStartup<treesDeadline)yield return null;
   Debug.Log("[RingworldSmoke] PARALLAX native tree handover="+provider.OwnsTrees(along,across,0)+" resident="+layer.Resident);
   if(!provider.OwnsTrees(along,across,0))throw new Exception("Native forest never handed over to ready Parallax patches");
   yield return new WaitForSecondsRealtime(4);
   if(!f.Settings.ParallaxNativeFallback){
    if(UnityEngine.Object.FindObjectsOfType<ForestCanopy>().Length!=0)throw new Exception("Native forest generated during Parallax-only preview");
    var ground=GameObject.Find("Ringworld close ground cover");if(ground!=null&&ground.activeInHierarchy)throw new Exception("Native ground cover remained visible");
    if(GameObject.Find("Ring biome LOD canopy")!=null)throw new Exception("Native distant canopy generated during preview");
    Debug.Log("[RingworldSmoke] PARALLAX preview: native trees/ground cover/canopy absent; structural scenery retained");
   }
   var camera=FlightCamera.fetch.mainCamera;camera.Render();var counts=layer.Counts();
   Debug.Log("[RingworldSmoke] PARALLAX resident="+layer.Resident+" patches="+layer.PatchCount+" visible="+string.Join(",",counts)+" status="+provider.Message);
   if(counts[0]+counts[1]+counts[2]==0)throw new Exception("Installed Parallax material pipeline drew no candidates");
   string folder=Path.Combine(KSPUtil.ApplicationRootPath,"../Ringworld Parallax/artifacts/validation");Directory.CreateDirectory(folder);
   var target=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32);var oldTarget=camera.targetTexture;var old=RenderTexture.active;var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
   try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,"bridge-ground.png"),pixels.EncodeToPNG());}
   finally{camera.targetTexture=oldTarget;RenderTexture.active=old;UnityEngine.Object.Destroy(pixels);target.Release();UnityEngine.Object.Destroy(target);}
   // Map isolation must return ownership to the native aggregate representation.
   MapView.EnterMapView();yield return new WaitForSecondsRealtime(2);if(provider.OwnsTrees(0,0,0))throw new Exception("Map-mode scatter ownership retained");MapView.ExitMapView();
   yield return new WaitForSecondsRealtime(2);
   var disabled=f.Settings.Save();disabled.SetValue("parallaxExtension",false,true);f.ApplyOptions(disabled,false);yield return null;
   if(provider.OwnsTrees(0,0,0))throw new Exception("Disabled bridge kept tree ownership");
   Debug.Log("[RingworldSmoke] PARALLAX installed assets, GPU LOD output, map isolation and disable fallback passed");
  }
 }
}
