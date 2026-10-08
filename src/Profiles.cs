using System;using System.Collections.Generic;using System.Globalization;using System.Linq;
using Ringworld.Core;using UnityEngine;
namespace Ringworld.Parallax {
 internal sealed class Profile {
  internal string Id,Source,Mode,Ring;internal bool Tree;internal HashSet<string> Biomes;
  internal float Spacing,Chance,Range,MinElevation,MaxElevation,MinSlope,Size;internal int Salt;
  internal float MinDepth,MaxDepth;
  internal Resources Assets;
  internal static List<Profile> Load(string ring){
   var result=new List<Profile>();var ids=new HashSet<string>();
   foreach(var n in GameDatabase.Instance.GetConfigNodes("RINGWORLD_PARALLAX_SCATTER")){
    string id=n.GetValue("id"),target=n.GetValue("ringId")??"*";if(target!="*"&&target!=ring)continue;
    if(string.IsNullOrEmpty(id)||!ids.Add(id)){Debug.LogError("[RingworldParallax] Missing/duplicate scatter ID: "+id);continue;}
    var p=new Profile{Id=id,Source=n.GetValue("sourceScatter"),Mode=n.GetValue("mode")??"dry",Ring=target,Tree=n.GetValue("family")=="Trees",
     Spacing=Number(n,"spacing",16,1,256),Chance=Number(n,"chance",1,0,2),Range=Number(n,"range",1000,25,20000),MinElevation=Number(n,"minElevation",-1000000,-1000000,1000000),MaxElevation=Number(n,"maxElevation",1000000,-1000000,1000000),
     MinDepth=Number(n,"minWaterDepth",0,0,1000000),MaxDepth=Number(n,"maxWaterDepth",1000000,0,1000000),
     MinSlope=Number(n,"minimumUpDot",.65f,0,1),Size=Number(n,"scaleMultiplier",1,.01f,100)};
    string biomes=n.GetValue("biomes")??"*";p.Biomes=new HashSet<string>(biomes.Split(',').Select(x=>x.Trim()),StringComparer.OrdinalIgnoreCase);
    p.Salt=StableStringHash(id);if(string.IsNullOrEmpty(p.Source)){Debug.LogError("[RingworldParallax] Missing sourceScatter for "+id);continue;}result.Add(p);
   }return result;
  }
  internal float Weight(TerrainSample t,ClimateBlend e,double margin){
   double depth=t.Wet?t.WaterHeight-t.Height:0;if(depth<MinDepth||depth>MaxDepth)return 0;
   return Biomes.Contains("*")||Biomes.Contains(t.Biome.ToString())?(float)Placement.Weight(Mode,t,e,margin):0;
  }
  static float Number(ConfigNode n,string key,float fallback,float min,float max){float v;return float.TryParse(n.GetValue(key),NumberStyles.Float,CultureInfo.InvariantCulture,out v)&&!float.IsNaN(v)&&!float.IsInfinity(v)?Mathf.Clamp(v,min,max):fallback;}
  static int StableStringHash(string s){unchecked{int h=17;foreach(char c in s)h=h*31+c;return h;}}
 }
}
