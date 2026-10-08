using System;using System.IO;using System.Linq;using System.Text.RegularExpressions;using Ringworld.Parallax;using Ringworld.Core;
int checks=0;void Check(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
foreach(double radius in new[]{1e6,153e8,150e9}){
 double c=2*Math.PI*radius;long n=Placement.AlongCells(c);double size=c/n;
 Check(size<=256&&size>0,"Invalid canonical size");
 Check(Placement.WrapCell(-1,n)==n-1,"Wrap seam");Check(Placement.WrapCell(n,n)==0,"Wrap seam end");
 for(long x=-1000;x<1000;x++){
  var u=Placement.Unit(x,17,1970,43);Check(u>0&&u<1,"Random range");
  Check(u==Placement.Unit(x,17,1970,43),"Unstable placement");
  Check(Placement.WrapCell(x,n)==Placement.WrapCell(x+n,n),"Periodic key");
 }
}
var ecology=new ClimateBlend{Forest=1};
foreach(Biome b in Enum.GetValues(typeof(Biome))){var t=new TerrainSample{Biome=b,Height=100,WaterHeight=double.NegativeInfinity};var w=Placement.Weight("forest",t,ecology,1);Check(w>=0&&w<=1,"Bad biome density");if(b==Biome.Road||b==Biome.Ruins||b==Biome.Rimwall||b==Biome.Scrith)Check(w==0,"Infrastructure vegetation");}
var wet=new TerrainSample{Biome=Biome.Lake,Height=10,WaterHeight=20};Check(Placement.Weight("forest",wet,ecology,1)==0,"Tree under water");Check(Placement.Weight("submerged",wet,ecology,1)==1,"Seabed mask");Check(Placement.Weight("forest",new TerrainSample{WaterHeight=double.NegativeInfinity},ecology,-1)==0,"Clearing mask");
// Validate the shipped configuration, not just individual implementation modes.
var cfg=File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../GameData/RingworldParallax/Scatters.cfg")));
var profiles=Regex.Matches(cfg,@"RINGWORLD_PARALLAX_SCATTER\s*\{([^}]*)\}").Cast<Match>().Select(m=>m.Groups[1].Value).ToArray();
string Field(string p,string key)=>Regex.Match(p,@"(?m)^\s*"+key+@"\s*=\s*([^\r\n]+)").Groups[1].Value.Trim();
Check(profiles.Length==16,"Shipped biome profile count changed; audit the coverage table");
Check(profiles.Select(p=>Field(p,"id")).Distinct().Count()==profiles.Length,"Duplicate shipped IDs");
foreach(Biome b in Enum.GetValues(typeof(Biome))){
 var water=b==Biome.Ocean||b==Biome.Lake||b==Biome.River||b==Biome.Wetland;
 var t=new TerrainSample{Biome=b,Height=100,WaterHeight=water?101:double.NegativeInfinity,Shore=water?1:0};
 var e=new ClimateBlend{Meadow=b==Biome.Grassland?1:0,Forest=b==Biome.Forest?1:0,Desert=b==Biome.Desert?1:0,Cold=b==Biome.Snow?1:0,Highland=b==Biome.Mountain?1:0};
 double total=profiles.Where(p=>Field(p,"biomes").Split(',').Contains(b.ToString())).Sum(p=>Placement.Weight(Field(p,"mode"),t,e,1));
 bool bare=b==Biome.Road||b==Biome.Rimwall||b==Biome.Scrith;
 Check(bare?total==0:total>0,"No correct shipped coverage for "+b);
}
Check(Placement.Weight("shore",new TerrainSample{Biome=Biome.Wetland,Height=0,WaterHeight=1},new ClimateBlend(),-1)>0,"Shallow wetland reeds rejected");
Check(Placement.Weight("shore",new TerrainSample{Biome=Biome.Ocean,Height=0,WaterHeight=10},new ClimateBlend(),1)==0,"Deep water reeds");
Check(Placement.Weight("seabed",new TerrainSample{Biome=Biome.Ocean,Height=-1000,WaterHeight=0},new ClimateBlend(),-1)>0,"Deep ocean floor missing");
Check(Placement.Weight("snow",new TerrainSample{Biome=Biome.Snow,WaterHeight=double.NegativeInfinity},new ClimateBlend(),-1)>0,"Older-generator snow classification ignored");
Check(Placement.Weight("rubble",new TerrainSample{Biome=Biome.Ruins,WaterHeight=double.NegativeInfinity},new ClimateBlend(),-1)>0,"Ruin rubble excluded");
Check(Placement.Weight("forest",new TerrainSample{Biome=Biome.Ruins,WaterHeight=double.NegativeInfinity},ecology,1)==0,"Ruin buildings overgrown by ordinary forest");
Console.WriteLine($"PASS {checks} canonical placement/biome checks");
