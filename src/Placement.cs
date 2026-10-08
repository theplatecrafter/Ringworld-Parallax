using System;
using Ringworld.Core;
namespace Ringworld.Parallax {
 // Canonical periodic cells, independent of Unity IDs, render tessellation and frame epochs.
 public static class Placement {
  public const double PatchSize=256;
  public static long AlongCells(double circumference){return Math.Max(1,(long)Math.Ceiling(circumference/PatchSize));}
  public static long WrapCell(long cell,long count){return (cell%count+count)%count;}
  public static uint Hash(long x,long y,int seed,int salt){unchecked{ulong h=(ulong)x*0x9e3779b185ebca87UL^(ulong)y*0xc2b2ae3d27d4eb4fUL^(uint)seed^(uint)salt;h^=h>>33;h*=0xff51afd7ed558ccdUL;h^=h>>33;h*=0xc4ceb9fe1a85ec53UL;return (uint)(h^(h>>32));}}
  public static double Unit(long x,long y,int seed,int salt){return (Hash(x,y,seed,salt)+.5)/4294967296.0;}
  public static double Weight(string mode,TerrainSample t,ClimateBlend e,double forestMargin){
   if(t.Biome==Biome.Road||t.Biome==Biome.Rimwall||t.Biome==Biome.Scrith)return 0;
   if(t.Biome==Biome.Ruins)return mode=="rubble"&&!t.Wet?1:0;
   switch(mode){
    case "forest":return !t.Wet&&forestMargin>=0?Math.Min(1,e.TreeCover*1.2):0;
    case "meadow":return t.Wet?0:Math.Min(1,Math.Max(t.Biome==Biome.Grassland?.5:0,e.Meadow+e.Forest*.25));
    case "undergrowth":return !t.Wet&&forestMargin>=0?e.Forest:0;
    case "desert":return t.Wet?0:Math.Max(t.Biome==Biome.Desert?.65:0,e.Desert);
    case "snow":return t.Wet?0:Math.Max(t.Biome==Biome.Snow?.65:0,e.Cold);
    case "conifer":return !t.Wet?Math.Min(1,.12+.45*e.Cold+.25*e.Highland):0;
    case "talus":return !t.Wet?Math.Max(t.Biome==Biome.Mountain?.7:0,e.Highland):0;
    case "shore":return (!t.Wet||t.WaterHeight-t.Height<=2)?Math.Max(t.Shore,t.Biome==Biome.Wetland?.8:0):0;
    case "submerged":return t.Wet&&t.WaterHeight-t.Height<100?1:0;
    case "seabed":return t.Wet?1:0;
    case "rubble":return 0;
    default:return t.Wet?0:1;
   }
  }
 }
}
