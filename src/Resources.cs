using System;using System.Collections;using System.Collections.Generic;using System.Linq;using System.Reflection;
using UnityEngine;using KSPTextureLoader;
namespace Ringworld.Parallax {
 internal static class Native {
  internal static object Read(object o,string key){if(o==null)return null;var t=o as Type??o.GetType();var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;var f=t.GetField(key,flags);if(f!=null)return f.GetValue(o is Type?null:o);var p=t.GetProperty(key,flags);return p==null?null:p.GetValue(o is Type?null:o,null);}
  internal static Type Type(string name){foreach(var a in AppDomain.CurrentDomain.GetAssemblies()){var t=a.GetType(name,false);if(t!=null)return t;}return null;}
  internal static IEnumerable<object> Values(object o){var d=o as IDictionary;if(d!=null){foreach(var v in d.Values)yield return v;}else if(o is IEnumerable)foreach(var v in (IEnumerable)o)yield return v;}
  internal static float F(object o,string key,float fallback){var v=Read(o,key);return v==null?fallback:Convert.ToSingle(v);}
  internal static Vector3 V(object o,string key,Vector3 fallback){var v=Read(o,key);return v is Vector3?(Vector3)v:fallback;}
  internal static string S(object o,string key){return Read(o,key) as string;}
  internal static List<object> Scatters(){var result=new List<object>();foreach(var b in Values(Read(Type("Parallax.ConfigLoader"),"parallaxScatterBodies")))result.AddRange(Values(Read(b,"scatters")));return result;}
 }
 internal sealed class AssetPart : IDisposable {
  internal readonly Mesh[] Meshes=new Mesh[3];internal readonly Material[] Materials=new Material[3];
  internal void Draw(ComputeBuffer[] outputs,ComputeBuffer[] args,Bounds bounds,Camera camera,bool shadows){for(int i=0;i<3;i++){Materials[i].SetBuffer("_InstanceData",outputs[i]);Graphics.DrawMeshInstancedIndirect(Meshes[i],0,Materials[i],bounds,args[i],0,null,shadows?UnityEngine.Rendering.ShadowCastingMode.On:UnityEngine.Rendering.ShadowCastingMode.Off,true,15,camera);}}
  public void Dispose(){foreach(var m in Materials)if(m!=null)UnityEngine.Object.Destroy(m);}
 }
 internal sealed class Resources : IDisposable {
  readonly Dictionary<string,TextureHandle> textures=new Dictionary<string,TextureHandle>();
  internal readonly List<AssetPart> Parts=new List<AssetPart>();
  internal Vector3 MinScale,MaxScale;internal float Radius,Height,Lod1,Lod2;internal bool Align,ColoredByTerrain;
  internal Resources(object source,List<object> all){
   var d=Native.Read(source,"distributionParams");MinScale=Native.V(d,"minScale",Vector3.one);MaxScale=Native.V(d,"maxScale",Vector3.one);Align=Native.F(d,"alignToTerrainNormal",0)!=0;ColoredByTerrain=Native.F(d,"coloredByTerrain",0)!=0;
   float range=Native.F(d,"range",1000);Lod1=Math.Max(10,Native.F(Native.Read(d,"lod1"),"range",.15f)*range);Lod2=Math.Max(Lod1+10,Native.F(Native.Read(d,"lod2"),"range",.6f)*range);
   try{
    Parts.Add(Part(source));
    foreach(var child in all)if(ReferenceEquals(Native.Read(child,"parent"),source))Parts.Add(Part(child));
    foreach(var part in Parts){var b=part.Meshes[0].bounds;Radius=Math.Max(Radius,b.extents.magnitude);Height=Math.Max(Height,b.max.y-b.min.y);}
   }catch{Dispose();throw;}
  }
  AssetPart Part(object source){
   var result=new AssetPart();var distribution=Native.Read(source,"distributionParams");
   try{for(int i=0;i<3;i++){
    var lod=i==0?null:Native.Read(distribution,i==1?"lod1":"lod2");
    string path=i==0?Native.S(source,"modelPath"):Native.S(lod,"modelPathOverride");
    var prefab=GameDatabase.Instance.GetModel(path);if(prefab==null)throw new Exception("Missing Parallax model: "+path);
    var filter=prefab.GetComponent<MeshFilter>();if(filter==null||filter.sharedMesh==null)throw new Exception("Unsupported Parallax model mesh: "+path);
    // GameDatabase owns meshes. Never destroy or modify them.
    result.Meshes[i]=filter.sharedMesh;
    result.Materials[i]=Material(i==0?Native.Read(source,"materialParams"):Native.Read(lod,"materialOverride"));
   }return result;}catch{result.Dispose();throw;}
  }
  Material Material(object parameters){
   string shaderName=Native.S(parameters,"shader");var bank=Native.Read(Native.Type("Parallax.AssetBundleLoader"),"parallaxScatterShaders") as IDictionary;
   var shader=bank==null?null:bank[shaderName] as Shader;if(shader==null||!shader.isSupported)throw new Exception("Missing/unsupported installed Parallax shader: "+shaderName);
   var m=new Material(shader){enableInstancing=true};
   try{
    foreach(string k in Native.Values(Native.Read(parameters,"shaderKeywords")))m.EnableKeyword(k);
    var properties=Native.Read(parameters,"shaderProperties");
    Set(properties,"shaderFloats",(k,v)=>m.SetFloat(k,Convert.ToSingle(v)));
    Set(properties,"shaderInts",(k,v)=>m.SetInt(k,Convert.ToInt32(v)));
    Set(properties,"shaderVectors",(k,v)=>m.SetVector(k,(Vector3)v));
    Set(properties,"shaderColors",(k,v)=>m.SetColor(k,(Color)v));
    Set(properties,"shaderTextures",(k,v)=>{
     string path=v as string,key=path+"|"+k;TextureHandle handle;
     if(!textures.TryGetValue(key,out handle)){
      var options=new TextureLoadOptions{Linear=k!="_MainTex"&&k!="_ColorMap",Unreadable=true,Hint=TextureLoadHint.Synchronous};
      // Read the shader property dimension rather than guessing from its name.
      int index=shader.FindPropertyIndex(k);bool cube=index>=0&&shader.GetPropertyTextureDimension(index)==UnityEngine.Rendering.TextureDimension.Cube;
      handle=cube?(TextureHandle)TextureLoader.LoadTexture<Cubemap>(path,options):TextureLoader.LoadTexture<Texture2D>(path,options);textures.Add(key,handle);
     }
     var texture=handle.GetTexture();if(texture==null)throw new Exception("Missing Parallax texture: "+path);m.SetTexture(k,texture);
    });
    // Derive up from each instance, not from the host star's spherical centre.
    m.EnableKeyword("TWO_SIDED");m.SetFloat("_PlanetOpacity",1);
    bool cutout=m.IsKeywordEnabled("ALPHA_CUTOFF");m.renderQueue=cutout?2450:2000;m.SetOverrideTag("RenderType",cutout?"TransparentCutout":"Opaque");
    m.SetInt("_SrcMode",(int)UnityEngine.Rendering.BlendMode.One);m.SetInt("_DstMode",(int)UnityEngine.Rendering.BlendMode.Zero);
    return m;
   }catch{UnityEngine.Object.Destroy(m);throw;}
  }
  static void Set(object properties,string name,Action<string,object> setter){var d=Native.Read(properties,name) as IDictionary;if(d!=null)foreach(DictionaryEntry v in d)setter((string)v.Key,v.Value);}
  public void Dispose(){foreach(var p in Parts)p.Dispose();Parts.Clear();foreach(var t in textures.Values)t.Dispose();textures.Clear();}
 }
}
