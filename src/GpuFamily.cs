using System;using System.Collections.Generic;using UnityEngine;
namespace Ringworld.Parallax {
 [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
 internal struct Candidate {internal Vector4 PositionYaw,UpSeed,Normal,ScaleRadius,Tint;}
 internal sealed class GpuFamily : IDisposable {
  internal readonly Profile Profile;internal readonly ComputeBuffer[] Output=new ComputeBuffer[3];
  internal readonly List<ComputeBuffer[]> Arguments=new List<ComputeBuffer[]>();
  readonly ComputeBuffer counts;readonly ComputeShader shader;readonly int cull,finish,capacity;
  internal int[] LastCounts(){var data=new uint[3];counts.GetData(data);return new[]{(int)Math.Min(data[0],capacity),(int)Math.Min(data[1],capacity),(int)Math.Min(data[2],capacity)};}
  internal GpuFamily(Profile profile,ComputeShader shader,int capacity){
   Profile=profile;this.shader=shader;this.capacity=capacity;cull=shader.FindKernel("Cull");finish=shader.FindKernel("Finish");
   try{
    counts=new ComputeBuffer(3,sizeof(uint));for(int i=0;i<3;i++)Output[i]=new ComputeBuffer(capacity,64);
    foreach(var part in profile.Assets.Parts){var args=new ComputeBuffer[3];Arguments.Add(args);for(int i=0;i<3;i++){
     args[i]=new ComputeBuffer(5,4,ComputeBufferType.IndirectArguments);
     args[i].SetData(new uint[]{part.Meshes[i].GetIndexCount(0),0,part.Meshes[i].GetIndexStart(0),part.Meshes[i].GetBaseVertex(0),0});
    }}
   }catch{Dispose();throw;}
  }
  internal void Begin(Camera camera){
   counts.SetData(new uint[3]);shader.SetInt("_Capacity",capacity);shader.SetVector("_Camera",camera.transform.position);
   var planes=GeometryUtility.CalculateFrustumPlanes(camera);var values=new Vector4[6];for(int i=0;i<6;i++)values[i]=new Vector4(planes[i].normal.x,planes[i].normal.y,planes[i].normal.z,planes[i].distance);shader.SetVectorArray("_Planes",values);
   shader.SetBuffer(cull,"_Counts",counts);shader.SetBuffer(cull,"_Lod0",Output[0]);shader.SetBuffer(cull,"_Lod1",Output[1]);shader.SetBuffer(cull,"_Lod2",Output[2]);
   shader.SetInt("_Align",Profile.Assets.Align?1:0);
  }
  internal void Evaluate(ComputeBuffer input,int count,Matrix4x4 transform,float range,float patchFade){
   if(count==0||patchFade<=0)return;
   shader.SetBuffer(cull,"_Candidates",input);shader.SetInt("_Count",count);shader.SetMatrix("_LocalToWorld",transform);shader.SetFloat("_Range",range);
   float first=Math.Min(Profile.Assets.Lod1,range*.25f),second=Math.Min(Profile.Assets.Lod2,range*.65f);
   shader.SetFloat("_Lod1Distance",first);shader.SetFloat("_Lod2Distance",second);shader.SetFloat("_Fade",Math.Max(2,first*.15f));shader.SetFloat("_PatchFade",patchFade);
   shader.Dispatch(cull,(count+63)/64,1,1);
  }
  internal void Draw(Camera camera,Bounds bounds,bool shadows){
   shader.SetBuffer(finish,"_Counts",counts);shader.SetInt("_Capacity",capacity);
   for(int p=0;p<Arguments.Count;p++){
    var args=Arguments[p];shader.SetBuffer(finish,"_Args0",args[0]);shader.SetBuffer(finish,"_Args1",args[1]);shader.SetBuffer(finish,"_Args2",args[2]);shader.Dispatch(finish,1,1,1);
    Profile.Assets.Parts[p].Draw(Output,args,bounds,camera,shadows);
   }
  }
  public void Dispose(){counts?.Dispose();foreach(var b in Output)b?.Dispose();foreach(var args in Arguments)foreach(var b in args)b?.Dispose();Arguments.Clear();}
 }
}
