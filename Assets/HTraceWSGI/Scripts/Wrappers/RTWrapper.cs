//pipelinedefine
#define H_HDRP

using HTraceWSGI.Scripts.Globals;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace HTraceWSGI.Scripts.Wrappers
{
	public class RTWrapper
	{
		private RenderTextureDescriptor _dscr;
		
		public RTHandle      rt;
		
		
		
		public void HTextureAlloc(string name, Vector2 scaleFactor, GraphicsFormat graphicsFormat, int volumeDepthOrSlices = -1, int depthBufferBits = 0,
			TextureDimension textureDimension = TextureDimension.Unknown,
			bool useMipMap = false, bool autoGenerateMips = false, bool enableRandomWrite = true, bool useDynamicScale = true) //useDynamicScale default = true for Upscalers switch between Hardware and Software
		{
            volumeDepthOrSlices = volumeDepthOrSlices == -1 ? TextureXR.slices : volumeDepthOrSlices;
			textureDimension = textureDimension == TextureDimension.Unknown ? TextureDimension.Tex2D : textureDimension;
			//textureDimension = textureDimension == TextureDimension.Unknown ? TextureXR.dimension : textureDimension;

            rt = RTHandles.Alloc(scaleFactor, volumeDepthOrSlices, dimension: textureDimension, colorFormat: graphicsFormat, name: name,
				enableRandomWrite: enableRandomWrite, useMipMap: useMipMap, useDynamicScale: useDynamicScale, autoGenerateMips: autoGenerateMips,
				depthBufferBits: (DepthBits)depthBufferBits);
		}
		
		public void HTextureAlloc(string name, ScaleFunc scaleFunc, GraphicsFormat graphicsFormat, int volumeDepthOrSlices = -1, int depthBufferBits = 0,
			TextureDimension textureDimension = TextureDimension.Unknown,
			bool useMipMap = false, bool autoGenerateMips = false, bool enableRandomWrite = true, bool useDynamicScale = true) //useDynamicScale default = true for Upscalers switch between Hardware and Software
		{
			volumeDepthOrSlices = volumeDepthOrSlices == -1 ? TextureXR.slices : volumeDepthOrSlices;
			//textureDimension    = textureDimension == TextureDimension.Unknown ? TextureXR.dimension : textureDimension;
			textureDimension    = textureDimension == TextureDimension.Unknown ? TextureDimension.Tex2D : textureDimension;
			
			rt = RTHandles.Alloc(scaleFunc, volumeDepthOrSlices, dimension: textureDimension, colorFormat: graphicsFormat, name: name,
				enableRandomWrite: enableRandomWrite, useMipMap: useMipMap, useDynamicScale: useDynamicScale, autoGenerateMips: autoGenerateMips,
				depthBufferBits: (DepthBits)depthBufferBits);
		}
		
		public void HTextureAlloc(string name, int width, int height, GraphicsFormat graphicsFormat, int volumeDepthOrSlices = -1, int depthBufferBits = 0,
			TextureDimension textureDimension = TextureDimension.Unknown,
			bool useMipMap = false, bool autoGenerateMips = false, bool enableRandomWrite = true, bool useDynamicScale = true) //useDynamicScale default = true for Upscalers switch between Hardware and Software
		{
			volumeDepthOrSlices = volumeDepthOrSlices == -1 ? TextureXR.slices : volumeDepthOrSlices;
			//textureDimension    = textureDimension == TextureDimension.Unknown ? TextureXR.dimension : textureDimension;
			textureDimension    = textureDimension == TextureDimension.Unknown ? TextureDimension.Tex2D : textureDimension;
			
			rt = RTHandles.Alloc(width, height, volumeDepthOrSlices, dimension: textureDimension, colorFormat: graphicsFormat, name: name, 
				enableRandomWrite: enableRandomWrite, useMipMap: useMipMap, useDynamicScale: useDynamicScale, autoGenerateMips: autoGenerateMips,
				depthBufferBits: (DepthBits)depthBufferBits);
		}
		
		public void HRelease()
		{
			RTHandles.Release(rt);
		}
		
	}
}
