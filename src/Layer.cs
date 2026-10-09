using System;using System.Collections;using System.Collections.Generic;using System.Linq;using Stopwatch=System.Diagnostics.Stopwatch;
using UnityEngine;using Ringworld.Core;using NivenRingworld;using NivenRingworld.Extensions;
namespace Ringworld.Parallax {
 internal sealed class Patch : IDisposable {
  internal long X,Y;internal Profile Profile;internal DVec Anchor;internal double Phase,Along,Across,Width;
  internal bool Ready,Retired;internal ComputeBuffer Input;internal Candidate[] Candidates;internal float Born;
  internal IEnumerator Work;internal GameObject ColliderRoot;internal readonly Dictionary<int,Collider> Colliders=new Dictionary<int,Collider>();
  public void Dispose(){Retired=true;(Work as IDisposable)?.Dispose();Work=null;Input?.Dispose();Input=null;if(ColliderRoot!=null)UnityEngine.Object.Destroy(ColliderRoot);Colliders.Clear();}
 }
 internal sealed class Layer : IScatterLayer, IPhotoScatterLayer {
  readonly Settings settings;readonly List<Profile> profiles;readonly Dictionary<string,Patch> patches=new Dictionary<string,Patch>();
  readonly Dictionary<Profile,GpuFamily> families=new Dictionary<Profile,GpuFamily>();readonly Queue<Patch> pending=new Queue<Patch>();
  readonly ParallaxProvider owner;readonly List<Profile> treeProfiles;ComputeShader shader;AssetBundle bundle;
  bool initialized,disposed,faulted;long lastX=long.MinValue,lastY;double lastRange=-1,lastDensity=-1;int lastLimit;float nextRetry,nextColliders;
  DVec observer;RingPoint point;int resident;internal int PatchCount{get{return patches.Count;}}internal int Resident{get{return resident;}}
  readonly Collider[] clearance=new Collider[16];
  internal IEnumerable<Profile> Profiles{get{return profiles;}}
  bool PhotoActive{get{return RingworldFlight.Instance?.visuals?.PhotoActive==true;}}
  public bool PhotoReady{get{return !Enabled||pending.Count==0;}}
  internal int PhotoCaptureDraws{get;private set;}
  internal bool ObserverMatches(RingPoint chart){return Math.Abs(settings.Geometry.AlongDistance(point.Along,chart.Along))<.1&&Math.Abs(point.Across-chart.Across)<.1;}
  internal Layer(Settings s,ParallaxProvider provider){settings=s;owner=provider;profiles=Profile.Load(s.RingId);treeProfiles=profiles.Where(p=>p.Tree).ToList();Camera.onPreCull+=Render;}
  internal bool Enabled{get{return !disposed&&!faulted&&initialized&&settings.ParallaxExtension&&settings.ParallaxDensity>0&&StockGraphics.Scatter>0&&RingworldFlight.Instance!=null&&RingworldFlight.Instance.Active&&!MapView.MapIsEnabled;}}
  bool Initialize(){
   var all=Native.Scatters();if(all.Count==0){owner.Message="Waiting for installed Parallax Continued scatter configs/shaders.";return false;}
   try{
    bundle=AssetBundle.LoadFromFile(System.IO.Path.Combine(KSPUtil.ApplicationRootPath,"GameData/RingworldParallax/Assets/ringworldparallax"));
    var template=bundle==null?null:bundle.LoadAsset<ComputeShader>("Assets/Shaders/RingScatter.compute");if(template==null||!SystemInfo.supportsComputeShaders||!SystemInfo.supportsInstancing)throw new Exception("Bridge compute bundle or GPU capability unavailable.");
    shader=UnityEngine.Object.Instantiate(template);
    foreach(var p in profiles){
     var source=all.FirstOrDefault(x=>Native.S(x,"scatterName")==p.Source);if(source==null){owner.Message="Missing scatter "+p.Source+"; enable native fallback if needed.";Debug.LogWarning("[RingworldParallax] "+owner.Message);continue;}
     try{p.Assets=new Resources(source,all);families[p]=new GpuFamily(p,shader,Math.Max(1,settings.ParallaxMaxObjects/Math.Max(1,profiles.Count*3)));}
     catch(Exception e){p.Assets?.Dispose();p.Assets=null;Debug.LogError("[RingworldParallax] "+p.Id+" unavailable: "+e.Message);}
    }
    initialized=families.Count>0;lastLimit=settings.ParallaxMaxObjects;owner.Message=initialized?"Active: "+families.Count+" biome scatter families, installed Parallax materials.":"No usable Parallax assets; enable native fallback if needed.";
    Debug.Log("[RingworldParallax] "+owner.Message);return initialized;
   }catch(Exception e){Fail(e);return false;}
  }
  void Fail(Exception e){faulted=true;owner.Message="Bridge unavailable; enable native fallback: "+e.Message;Debug.LogError("[RingworldParallax] "+e);ClearPatches();}
  public void Update(DVec cameraObserver){
   if(disposed||faulted)return;observer=cameraObserver;point=settings.Geometry.Coordinates(observer);
   if(!settings.ParallaxExtension||settings.ParallaxDensity<=0||StockGraphics.Scatter<=0){ClearPatches();return;}
   if(!initialized){if(Time.unscaledTime<nextRetry)return;nextRetry=Time.unscaledTime+2;if(!Initialize())return;}
   try{
    if(lastDensity!=settings.ParallaxDensity||lastLimit!=settings.ParallaxMaxObjects){ClearPatches();lastDensity=settings.ParallaxDensity;}
    if(lastLimit!=settings.ParallaxMaxObjects){foreach(var f in families.Values)f.Dispose();families.Clear();foreach(var p in profiles)if(p.Assets!=null)families[p]=new GpuFamily(p,shader,Math.Max(1,settings.ParallaxMaxObjects/Math.Max(1,profiles.Count*3)));lastLimit=settings.ParallaxMaxObjects;}
    long count=Placement.AlongCells(settings.Geometry.P.Circumference);double width=settings.Geometry.P.Circumference/count;
    long x=(long)Math.Floor(point.Along/width),y=(long)Math.Floor(point.Across/Placement.PatchSize);
    if(x!=lastX||y!=lastY||lastRange!=settings.ParallaxRange){Plan(x,y,count,width);lastX=x;lastY=y;lastRange=settings.ParallaxRange;}
    // Frozen photographs do not share their frame budget with flight physics.
    // Keep preparation progressing at the 15 FPS photo limit without changing
    // the selected live budget, population limit or rendering quality.
    double workBudget=PhotoActive?Math.Max(4,settings.ParallaxBudget):settings.ParallaxBudget;
    var clock=Stopwatch.StartNew();int iterations=0;
    while(pending.Count>0&&clock.Elapsed.TotalMilliseconds<workBudget&&iterations++<128){
     var p=pending.Peek();if(p.Retired){pending.Dequeue();continue;}
     if(!p.Work.MoveNext()){pending.Dequeue();(p.Work as IDisposable)?.Dispose();p.Work=null;}
    }
    // Flight is frozen during photography. Do not create physical tree
    // proxies for temporary photo budgets; live contacts resume afterwards.
    if(!PhotoActive&&Time.unscaledTime>=nextColliders){nextColliders=Time.unscaledTime+.3f;Contacts();}
    owner.Message="Parallax: "+families.Count+" families / "+patches.Count+" patches / "+resident+" candidates / "+pending.Count+" pending";
   }catch(Exception e){Fail(e);}
  }
  static string Key(Profile profile,long x,long y){return profile.Id+":"+x+":"+y;}
  void Plan(long x,long y,long count,double width){
   var wanted=new HashSet<string>();var incoming=new List<Patch>();
   foreach(var profile in families.Keys){
    double range=Range(profile);int n=(int)Math.Ceiling((range+Placement.PatchSize)/Placement.PatchSize);
    for(int dy=-n;dy<=n;dy++)for(int dx=-n;dx<=n;dx++){
     long px=Placement.WrapCell(x+dx,count),py=y+dy;double a=px*width,b=py*Placement.PatchSize;
     if(b>=settings.Geometry.P.Width/2||b+Placement.PatchSize<=-settings.Geometry.P.Width/2)continue;
     double da=settings.Geometry.AlongDistance(a+width/2,point.Along),db=b+Placement.PatchSize/2-point.Across;
     if(da*da+db*db>(range+400)*(range+400))continue;
     string key=Key(profile,px,py);wanted.Add(key);if(patches.ContainsKey(key))continue;
     incoming.Add(new Patch{X=px,Y=py,Profile=profile,Along=a,Across=b,Width=width});
    }
   }
   // Retire distant cells only; neighbouring ready patches remain live while new cells build.
   foreach(var key in patches.Keys.ToArray())if(!wanted.Contains(key)){var p=patches[key];resident-=p.Candidates==null?0:p.Candidates.Length;p.Dispose();patches.Remove(key);}
   incoming.Sort((a,b)=>Distance(a).CompareTo(Distance(b)));
   int maxPatches=Math.Min(4096,Math.Max(128,settings.ParallaxMaxObjects/64));
   foreach(var p in incoming){if(patches.Count>=maxPatches)break;patches.Add(Key(p.Profile,p.X,p.Y),p);p.Work=Build(p);pending.Enqueue(p);}
  }
  double Distance(Patch p){double a=settings.Geometry.AlongDistance(p.Along+p.Width*.5,point.Along),b=p.Across+128-point.Across;return a*a+b*b;}
  double Range(Profile p){return p.Tree?settings.ParallaxRange:Math.Min(p.Range,settings.ParallaxRange);}
  IEnumerator Build(Patch p){
   p.Phase=settings.Geometry.OrientationRadians;p.Anchor=settings.Geometry.Position(p.Along,p.Across,0);
   // Freeze the chart during yielded generation. Frame motion only updates the live matrix.
   var g=new RingGeometry(settings.Geometry.P){Basis=settings.Geometry.Basis,OrientationRadians=p.Phase};
   var candidates=new List<Candidate>();int nx=Math.Max(1,(int)Math.Ceiling(p.Width/p.Profile.Spacing)),ny=Math.Max(1,(int)Math.Ceiling(Placement.PatchSize/p.Profile.Spacing));
   double sx=p.Width/nx,sy=Placement.PatchSize/ny;var terrain=settings.Terrain;var f=RingworldFlight.Instance;
   for(int y=0;y<ny;y++){
    for(int x=0;x<nx;x++){
     if(x>0&&x%8==0)yield return null;
     long hx=p.X*nx+x,hy=p.Y*ny+y;int salt=p.Profile.Salt;
     double a=p.Along+(x+.1+.8*Placement.Unit(hx,hy,g.P.Seed,salt))*sx,b=p.Across+(y+.1+.8*Placement.Unit(hx,hy,g.P.Seed,salt+1))*sy;
     if(Math.Abs(b)>g.P.Width/2-2)continue;
     var sample=terrain.Sample(a,b);if(sample.Height<p.Profile.MinElevation||sample.Height>p.Profile.MaxElevation)continue;
     var ecology=Ecology.Sample(terrain,a,b);double margin=BiomePresentation.ForestMargin(terrain,a,b);
     float probability=p.Profile.Weight(sample,ecology,margin)*p.Profile.Chance*(float)settings.ParallaxDensity*StockGraphics.Scatter;
     if(Placement.Unit(hx,hy,g.P.Seed,salt+2)>probability)continue;
     if(p.Profile.Tree&&p.Profile.Mode=="forest"&&(p.Profile.Source.EndsWith("OakTree")||p.Profile.Source.EndsWith("PineTree"))){bool conifer=terrain.Noise(a,b,18000,601)>.5;if(p.Profile.Source.EndsWith("PineTree")!=conifer)continue;}
     double height=f==null?sample.Height:f.ScatterFloor(a,b);
     var at=g.Position(a,b,height);var up=g.Up(at);
     var along=(g.Position(a+4,b,f==null?terrain.Sample(a+4,b).Height:f.ScatterFloor(a+4,b))-g.Position(a-4,b,f==null?terrain.Sample(a-4,b).Height:f.ScatterFloor(a-4,b))).Unit;
     var across=(g.Position(a,b+4,f==null?terrain.Sample(a,b+4).Height:f.ScatterFloor(a,b+4))-g.Position(a,b-4,f==null?terrain.Sample(a,b-4).Height:f.ScatterFloor(a,b-4))).Unit;
     var normal=DVec.Cross(across,along).Unit;if(DVec.Dot(normal,up)<0)normal=-normal;
     if(DVec.Dot(normal,up)<p.Profile.MinSlope)continue;
     float pick=(float)Placement.Unit(hx,hy,g.P.Seed,salt+3);Vector3 scale=Vector3.Lerp(p.Profile.Assets.MinScale,p.Profile.Assets.MaxScale,pick)*p.Profile.Size;
     if(p.Profile.Mode=="rubble"){
      float radius=Math.Max(.15f,p.Profile.Assets.Radius*Math.Max(scale.x,Math.Max(scale.y,scale.z)));
      var world=(Vector3)(settings.Center+ConvertVector.Ksp(settings.Geometry.Position(a,b,height)))+ConvertVector.Unity(up)*(radius+.25f);
      // Ruin rubble must not overlap buildings, landing craft or the floor.
      if(Physics.OverlapSphereNonAlloc(world,radius,clearance,(1<<15)|1,QueryTriggerInteraction.Ignore)>0)continue;
     }
     var position=ConvertVector.Unity(at-p.Anchor);var u=ConvertVector.Unity(up);var n=ConvertVector.Unity(normal);
     Color tint=p.Profile.Assets.ColoredByTerrain?TerrainTint.Color(sample):Color.white;
     candidates.Add(new Candidate{PositionYaw=new Vector4(position.x,position.y,position.z,pick*Mathf.PI*2),UpSeed=new Vector4(u.x,u.y,u.z,(float)Placement.Unit(hx,hy,g.P.Seed,salt+4)),Normal=new Vector4(n.x,n.y,n.z,0),
      ScaleRadius=new Vector4(scale.x,scale.y,scale.z,p.Profile.Assets.Radius*Math.Max(scale.x,Math.Max(scale.y,scale.z))),Tint=tint});
     if(resident+candidates.Count>=settings.ParallaxMaxObjects*4)break;
    }
    yield return null;if(resident+candidates.Count>=settings.ParallaxMaxObjects*4)break;
   }
   if(p.Retired)yield break;
   p.Candidates=candidates.ToArray();if(candidates.Count>0){p.Input=new ComputeBuffer(candidates.Count,80);p.Input.SetData(p.Candidates);}
   resident+=candidates.Count;p.Ready=true;p.Born=Time.unscaledTime;
  }
  internal Matrix4x4 Matrix(Patch p){double delta=settings.Geometry.OrientationRadians-p.Phase;return Matrix4x4.TRS((Vector3)(settings.Center+ConvertVector.Ksp(settings.Geometry.RotateAroundAxis(p.Anchor,delta))),settings.AxisRotation(delta),Vector3.one);}
  internal bool TreePatchOwned(Patch patch){return OwnsTrees(patch.Along,patch.Across,patch.Width);}
  public bool OwnsTrees(double a,double b,double size){
   if(!Enabled||treeProfiles.Count==0||treeProfiles.Any(p=>p.Assets==null))return false;
   var camera=FlightCamera.fetch?.mainCamera;
   if(camera!=null&&Vector3.Distance(camera.transform.position,(Vector3)(settings.Center+ConvertVector.Ksp(observer)))>settings.ParallaxRange)return false;
   long count=Placement.AlongCells(settings.Geometry.P.Circumference);double width=settings.Geometry.P.Circumference/count;
   if(size==0){a=Math.Floor(a/width)*width;b=Math.Floor(b/256)*256;size=width;}
   double da=Math.Abs(settings.Geometry.AlongDistance(a+size*.5,point.Along))+size*.71,db=Math.Abs(b+size*.5-point.Across)+size*.71;
   if(Math.Sqrt(da*da+db*db)>settings.ParallaxRange*.85)return false;
   long x0=(long)Math.Floor(a/width),x1=(long)Math.Floor((a+Math.Max(0,size-.001))/width),y0=(long)Math.Floor(b/256),y1=(long)Math.Floor((b+Math.Max(0,size-.001))/256);
   foreach(var profile in treeProfiles)for(long y=y0;y<=y1;y++)for(long x=x0;x<=x1;x++){
    Patch patch;if(!patches.TryGetValue(Key(profile,Placement.WrapCell(x,count),y),out patch)||!patch.Ready)return false;
   }return true;
  }
  void Render(Camera camera){
   if(!Enabled||camera==null||camera==PlanetariumCamera.Camera||!HighLogic.LoadedSceneIsFlight)return;
   var flight=RingworldFlight.Instance;if(flight==null||flight.Settings!=settings)return;
   // Only exterior flight/photo cameras; skip reflection, IVA and UI cameras.
   if(camera!=FlightCamera.fetch?.mainCamera&&camera.name.IndexOf("Ringworld photo",StringComparison.OrdinalIgnoreCase)<0)return;
   try{
    var worldObserver=settings.Center+ConvertVector.Ksp(observer);var bounds=new Bounds((Vector3)worldObserver,Vector3.one*(float)(settings.ParallaxRange*2+1024));
    foreach(var family in families.Values){family.Begin(camera);
     foreach(var p in patches.Values)if(p.Profile==family.Profile&&p.Ready&&p.Input!=null&&(!p.Profile.Tree||!settings.ParallaxNativeFallback||TreePatchOwned(p)))family.Evaluate(p.Input,p.Candidates.Length,Matrix(p),(float)Range(p.Profile),PhotoActive||p.Profile.Tree&&settings.ParallaxNativeFallback?1:Mathf.Clamp01((Time.unscaledTime-p.Born)/.75f));
     family.Draw(camera,bounds,settings.ForestQuality>0&&QualitySettings.shadows!=ShadowQuality.Disable);
    }
    if(flight.visuals!=null&&flight.visuals.CapturingPhoto)PhotoCaptureDraws++;
   }catch(Exception e){Fail(e);}
  }
  void Contacts(){
   var f=RingworldFlight.Instance;if(f==null)return;var nearby=FlightGlobals.VesselsLoaded.Where(v=>v!=null&&f.Owns(v)).Select(v=>(Vector3)v.transform.position).ToArray();int budget=0;
   foreach(var p in patches.Values){
    if(!p.Ready||!p.Profile.Tree)continue;
    bool enabled=Enabled&&settings.ParallaxColliders&&TreePatchOwned(p);
    if(p.ColliderRoot!=null)p.ColliderRoot.SetActive(enabled);if(!enabled)continue;
    if(Distance(p)>600*600&&p.Colliders.Count==0)continue;
    if(p.ColliderRoot==null){p.ColliderRoot=new GameObject("Parallax ring tree contacts");p.ColliderRoot.layer=15;}
    var m=Matrix(p);p.ColliderRoot.transform.position=m.GetColumn(3);p.ColliderRoot.transform.rotation=settings.AxisRotation(settings.Geometry.OrientationRadians-p.Phase);
    for(int i=0;i<p.Candidates.Length;i++){
     var c=p.Candidates[i];var position=m.MultiplyPoint3x4(c.PositionYaw);bool inRange=nearby.Any(v=>(v-position).sqrMagnitude<80*80&&(v-position).sqrMagnitude>18*18);
     Collider contact;if(p.Colliders.TryGetValue(i,out contact)){contact.enabled=inRange;continue;}
     if(!inRange||budget++>=128)continue;
     var go=new GameObject("Ringworld tree trunk");go.layer=15;go.transform.SetParent(p.ColliderRoot.transform,false);go.transform.localPosition=c.PositionYaw;go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,c.UpSeed);
     var capsule=go.AddComponent<CapsuleCollider>();capsule.height=p.Profile.Assets.Height*c.ScaleRadius.y*.65f;capsule.radius=Math.Min(.7f,p.Profile.Assets.Radius*c.ScaleRadius.x*.07f);capsule.center=Vector3.up*capsule.height*.5f;p.Colliders.Add(i,capsule);
    }
   }
  }
  public void Reposition(){if(disposed)return;foreach(var p in patches.Values)if(p.ColliderRoot!=null){var m=Matrix(p);p.ColliderRoot.transform.position=m.GetColumn(3);p.ColliderRoot.transform.rotation=settings.AxisRotation(settings.Geometry.OrientationRadians-p.Phase);}}
  void ClearPatches(){foreach(var p in patches.Values)p.Dispose();patches.Clear();pending.Clear();resident=0;lastX=long.MinValue;}
  public void Dispose(){if(disposed)return;disposed=true;Camera.onPreCull-=Render;ClearPatches();foreach(var f in families.Values)f.Dispose();families.Clear();foreach(var p in profiles)p.Assets?.Dispose();if(shader!=null)UnityEngine.Object.Destroy(shader);if(bundle!=null)bundle.Unload(false);if(owner.Current==this)owner.Current=null;}
  internal int[] Counts(){var total=new int[3];foreach(var f in families.Values){var count=f.LastCounts();for(int i=0;i<3;i++)total[i]+=count[i];}return total;}
  internal int CandidatesFor(string id){return patches.Values.Where(p=>p.Profile.Id==id&&p.Ready&&p.Candidates!=null).Sum(p=>p.Candidates.Length);}
  internal int[] CountsFor(string id){var f=families.Values.FirstOrDefault(x=>x.Profile.Id==id);return f==null?new int[3]:f.LastCounts();}
  internal bool HasTerrainTint(string id){return patches.Values.Where(p=>p.Profile.Id==id&&p.Ready&&p.Candidates!=null).SelectMany(p=>p.Candidates).Any(c=>c.Tint.x<.99f||c.Tint.y<.99f||c.Tint.z<.99f);}
 }
}
