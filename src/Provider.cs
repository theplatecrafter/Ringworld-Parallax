using System;using System.Globalization;using UnityEngine;using NivenRingworld;using NivenRingworld.Extensions;
namespace NivenRingworld.Extensions {
 internal sealed class ParallaxProvider : IScatterProvider {
  internal Ringworld.Parallax.Layer Current;internal string Message="Waiting for flight and installed Parallax Continued.";
  public string Status{get{return Message;}}
  public IScatterLayer Create(Settings settings){return Current=new Ringworld.Parallax.Layer(settings,this);}
  public bool OwnsTrees(double along,double across,double size){return Current!=null&&Current.OwnsTrees(along,across,size);}
  string range,density,maximum,budget;Settings bound;
  public void DrawSettings(RingworldFlight flight){
   var s=flight.Settings;if(bound!=s){bound=s;range=s.ParallaxRange.ToString(CultureInfo.InvariantCulture);density=s.ParallaxDensity.ToString(CultureInfo.InvariantCulture);maximum=s.ParallaxMaxObjects.ToString();budget=s.ParallaxBudget.ToString(CultureInfo.InvariantCulture);}
   bool enabled=GUILayout.Toggle(s.ParallaxExtension,"Enabled (installed Parallax assets)");if(enabled!=s.ParallaxExtension){var n=s.Save();n.SetValue("parallaxExtension",enabled,true);flight.ApplyOptions(n,false);bound=null;return;}
   GUILayout.Label(Message);GUILayout.Label("Stable biome placement and three object LODs. Native fallback is optional; buildings and terrain remain.");
   bool native=GUILayout.Toggle(s.ParallaxNativeFallback,"Native scatter fallback (trees, grass, rocks and canopy)");
   if(native!=s.ParallaxNativeFallback){var n=s.Save();n.SetValue("parallaxNativeFallback",native,true);flight.ApplyOptions(n,false);bound=null;return;}
   GUILayout.Label("Off shows only Parallax scatters. Buildings, landmarks and terrain remain. Missing Parallax assets leave bare terrain in this preview mode.");
   GUI.enabled=s.ParallaxExtension;
   range=Field("Scatter distance (metres, 50â€“20000)",range);density=Field("Density (0â€“2)",density);maximum=Field("Visible object budget (256â€“500000)",maximum);budget=Field("Generation budget (milliseconds, .25â€“8)",budget);
   bool contacts=GUILayout.Toggle(s.ParallaxColliders,"Nearby tree-trunk contacts");if(contacts!=s.ParallaxColliders){var n=s.Save();n.SetValue("parallaxColliders",contacts,true);flight.ApplyOptions(n,false);bound=null;return;}
   if(GUILayout.Button("Apply Parallax settings")){
    double r,d,m,b;if(!Number(range,50,20000,out r)||!Number(density,0,2,out d)||!Number(maximum,256,500000,out m)||!Number(budget,.25,8,out b)){Message="Enter finite values within the displayed ranges.";}
    else{var n=s.Save();n.SetValue("parallaxRange",r,true);n.SetValue("parallaxDensity",d,true);n.SetValue("parallaxMaxObjects",(int)m,true);n.SetValue("parallaxBudget",b,true);flight.ApplyOptions(n,false);bound=null;}
   }GUI.enabled=true;
  }
  static string Field(string label,string text){GUILayout.Label(label);return GUILayout.TextField(text,20);}
  static bool Number(string text,double min,double max,out double value){return double.TryParse(text,NumberStyles.Float,CultureInfo.InvariantCulture,out value)&&!double.IsNaN(value)&&!double.IsInfinity(value)&&value>=min&&value<=max;}
 }
}
