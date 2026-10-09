using System;using System.Collections;using System.IO;using UnityEngine;using NivenRingworld;using NivenRingworld.Extensions;
namespace Ringworld.Parallax {
 // Runs only when explicitly requested by the base development smoke harness.
 public static class BridgeSmoke {
  static void ValidateNativeDefaults(){
   if(ExtensionProviders.Parallax==null)throw new Exception("Default scatter test requires an installed bridge");
   var s=Settings.Load();s.Apply(new ConfigNode("OPTIONS"));
   if(!s.ParallaxExtension||s.ParallaxNativeFallback||s.NativeSurfaceScatters)throw new Exception("Enabled bridge did not suppress native scatters by default");
   s.ParallaxNativeFallback=true;var copy=Settings.Load();copy.Apply(s.Save());
   if(!copy.ParallaxNativeFallback||!copy.NativeSurfaceScatters)throw new Exception("Explicit native fallback did not persist");
   s.ParallaxNativeFallback=false;s.ParallaxExtension=false;
   if(!s.NativeSurfaceScatters)throw new Exception("Disabling bridge did not restore native scatters");
   s.ParallaxExtension=true;var provider=ExtensionProviders.Parallax;
   var field=typeof(ExtensionProviders).GetField("parallax",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
   try{field.SetValue(null,null);if(!s.NativeSurfaceScatters)throw new Exception("Uninstalled bridge suppressed native scatters");}
   finally{field.SetValue(null,provider);}
   Debug.Log("[RingworldSmoke] PASS Parallax native-scatter defaults, explicit saved fallback, disabled and absent bridge scopes");
  }
  public static IEnumerator RunPhoto(RingworldFlight f){
   ValidateNativeDefaults();
   var provider=ExtensionProviders.Parallax as ParallaxProvider;if(provider==null)throw new Exception("Photo bridge provider missing");
   var options=f.Settings.Save();RingQualityPresets.Apply(options,6);options.SetValue("parallaxExtension",true,true);options.SetValue("parallaxNativeFallback",false,true);
   options.SetValue("parallaxRange",300,true);options.SetValue("parallaxDensity","0.15",true);options.SetValue("parallaxMaxObjects",4000,true);f.ApplyOptions(options,true);
   var start=f.Settings.Geometry.Coordinates(f.Position(FlightGlobals.ActiveVessel));double along=0,across=0;bool found=false;
   for(int i=0;i<256;i++){
    double a=start.Along+20000+i*2048,b=start.Across+12000+i*337;var t=f.Settings.Terrain.Sample(a,b);
    if(t.Biome==Ringworld.Core.Biome.Grassland||t.Biome==Ringworld.Core.Biome.Forest){along=a;across=b;found=true;break;}
   }
   if(!found)throw new Exception("Photo vegetation fixture unavailable");
   f.arrivalHeight=12;typeof(RingworldFlight).GetMethod("VisitCoordinates",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(f,new object[]{along,across});while(!f.Ready)yield return null;
   // Exercise actual photo-setting application/restoration for every preset
   // without rendering high-budget scenes on the development laptop.
   for(int i=0;i<RingQualityPresets.Names.Length;i++){
    var expected=f.Settings.Save();RingQualityPresets.Apply(expected,i);f.PhotoTerrain(true,i);
    if(f.Settings.ParallaxRange!=double.Parse(expected.GetValue("parallaxRange"),System.Globalization.CultureInfo.InvariantCulture)||f.Settings.ParallaxMaxObjects!=int.Parse(expected.GetValue("parallaxMaxObjects"))||!f.Settings.ParallaxExtension)throw new Exception("Photo preset scatter budgets/switch mismatch: "+i);
    f.PhotoTerrain(false);
    if(f.Settings.ParallaxRange!=300||f.Settings.ParallaxDensity!=.15||f.Settings.ParallaxMaxObjects!=4000)throw new Exception("Photo preset did not restore custom live scatter budgets");
   }
   Debug.Log("[RingworldSmoke] PASS Parallax photo budgets/restoration across all 11 presets (higher presets not rendered)");
   var camera=FlightCamera.fetch.mainCamera;var g=f.Settings.Geometry;var at=f.Position(FlightGlobals.ActiveVessel);
   camera.transform.position=FlightGlobals.ActiveVessel.transform.position+ConvertVector.Unity(g.Up(at)*6-g.AlongDirection(at)*30);
   camera.transform.rotation=Quaternion.LookRotation(ConvertVector.Unity(g.AlongDirection(at)-g.Up(at)*.06),ConvertVector.Unity(g.Up(at)));
   double ut=Planetarium.GetUniversalTime();var position=f.Position(FlightGlobals.ActiveVessel);var layer=provider.Current;int captures=layer.PhotoCaptureDraws;
   var scaled=ScaledCamera.Instance;var originalReference=scaled==null?null:scaled.tgtRef;int alignedCameras=0;float maxAngle=0,maxProjectionError=0;
   Camera.CameraCallback inspect=c=>{
    if(!f.visuals.CapturingPhoto||c==camera)return;
    if(c.name!="GalaxyCamera"&&c.name!="Camera 01"&&(scaled==null||c!=scaled.cam))return;
    alignedCameras++;maxAngle=Math.Max(maxAngle,Quaternion.Angle(c.transform.rotation,camera.transform.rotation));
    for(int row=0;row<2;row++)for(int col=0;col<4;col++)maxProjectionError=Math.Max(maxProjectionError,Math.Abs(c.projectionMatrix[row,col]-camera.projectionMatrix[row,col]));
    if(scaled!=null&&c==scaled.cam){
     var point=camera.transform.position+camera.transform.forward*1000+camera.transform.right*100+camera.transform.up*200;
     var near=camera.WorldToViewportPoint(point);var far=c.WorldToViewportPoint((Vector3)ScaledSpace.LocalToScaledSpace(point));
     maxProjectionError=Math.Max(maxProjectionError,new Vector2(near.x-far.x,near.y-far.y).magnitude);
    }
   };Camera.onPreCull+=inspect;
   try{
    if(!f.visuals.BeginPhoto(1,6,512))throw new Exception("Parallax photo entry failed: "+f.visuals.Status);
    if(f.PhotoTerrainReady||f.Settings.ParallaxRange!=600||f.Settings.ParallaxDensity!=.35||f.Settings.ParallaxMaxObjects!=12000)throw new Exception("Selected Slow photo budgets/readiness not applied");
    float deadline=Time.realtimeSinceStartup+240;
    while(!f.visuals.PhotoFinished&&Time.realtimeSinceStartup<deadline)yield return null;
    if(!f.visuals.PhotoFinished||!File.Exists(f.visuals.LastPhoto)||!layer.PhotoReady||layer.PhotoCaptureDraws<=captures)throw new Exception("Photo captured without ready Parallax draw: "+provider.Message+" / "+f.visuals.Status);
    var counts=layer.Counts();if(counts[0]+counts[1]+counts[2]==0)throw new Exception("Parallax photo camera drew no visible objects");
    if(Math.Abs(Planetarium.GetUniversalTime()-ut)>.021||(f.Position(FlightGlobals.ActiveVessel)-position).Length>.05)throw new Exception("Parallax photo advanced flight");
    var chart=g.Coordinates(ConvertVector.Core((Vector3d)camera.transform.position-f.Center));
    if(!layer.ObserverMatches(chart))throw new Exception("Photo scatters did not use the photo camera chart");
    if(alignedCameras<2||maxAngle>.01||maxProjectionError>.002)throw new Exception("Photo world/sky camera mismatch: cameras="+alignedCameras+" angle="+maxAngle+" projection="+maxProjectionError);
    Debug.Log("[RingworldSmoke] PASS actual photo world/sky camera alignment; cameras="+alignedCameras+" angle="+maxAngle+" viewport error="+maxProjectionError);
    Debug.Log("[RingworldSmoke] PASS Parallax ready population in actual 512-pixel photo capture; visible LODs="+string.Join(",",counts)+" file="+f.visuals.LastPhoto);
   }finally{Camera.onPreCull-=inspect;f.visuals.EndPhoto();}
   yield return null;
   if(scaled!=null&&scaled.tgtRef!=originalReference)throw new Exception("Photo leaked scaled-camera reference");
   if(f.Settings.ParallaxRange!=300||f.Settings.ParallaxDensity!=.15||f.Settings.ParallaxMaxObjects!=4000||Time.timeScale!=1)throw new Exception("Photo resume did not restore live Parallax settings/time");
   if(!f.visuals.BeginPhoto(1,6,512))throw new Exception("Parallax photo cancellation entry failed");f.visuals.EndPhoto();yield return null;
   if(f.Settings.ParallaxRange!=300||f.Settings.ParallaxDensity!=.15||Time.timeScale!=1)throw new Exception("Photo cancellation did not restore Parallax");
   var disabled=f.Settings.Save();disabled.SetValue("parallaxExtension",false,true);f.ApplyOptions(disabled,false);
   int disabledCaptures=provider.Current.PhotoCaptureDraws;
   try{
    if(!f.visuals.BeginPhoto(1,6,512))throw new Exception("Disabled Parallax photo entry failed");
    float deadline=Time.realtimeSinceStartup+240;while(!f.visuals.PhotoFinished&&Time.realtimeSinceStartup<deadline)yield return null;
    if(!f.visuals.PhotoFinished||f.Settings.ParallaxExtension||!provider.Current.PhotoReady||provider.Current.PhotoCaptureDraws!=disabledCaptures)throw new Exception("Disabled bridge enabled or blocked photo capture");
   }finally{f.visuals.EndPhoto();}
   Debug.Log("[RingworldSmoke] PASS Parallax photo resume/cancel restoration and disabled-extension capture bypass");
  }
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
   ValidateNativeDefaults();
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
