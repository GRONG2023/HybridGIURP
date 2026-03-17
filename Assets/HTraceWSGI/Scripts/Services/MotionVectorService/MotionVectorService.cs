using System.Collections.Generic;
using UnityEngine;

namespace HTraceWSGI.Scripts.Services.MotionVectorService
{
	public class MotionVectorRuntimeData
	{
		public MotionVectorRuntimeData(List<Renderer> renderers)
		{
			Renderers             = renderers;
			MaterialPropertyBlock = new MaterialPropertyBlock();
			PreviousModalMatrices = new Dictionary<Renderer, Matrix4x4>(renderers.Count);
			for (int index = 0; index < renderers.Count; index++)
			{
				PreviousModalMatrices.Add(renderers[index], Matrix4x4.zero);
			}
		}
		public          bool                            WasMovedThisFrame;
		public readonly MaterialPropertyBlock           MaterialPropertyBlock;
		public          Dictionary<Renderer, Matrix4x4> PreviousModalMatrices;
		public readonly List<Renderer>                  Renderers;
	}
	
	public class MotionVectorService : IService
	{
		private static readonly int s_ForceNoMotion         = Shader.PropertyToID("_ForceNoMotion");
		private static readonly int s_HasLastPositionData   = Shader.PropertyToID("_HasLastPositionData");
		private static readonly int s_MotionVectorDepthBias = Shader.PropertyToID("_MotionVectorDepthBias");
		private static readonly int s_PreviousM             = Shader.PropertyToID("_PreviousM");

		private static MotionVectorService s_Instance;
		
		public static MotionVectorService Instance
		{
			get
			{
				if (s_Instance == null)
					s_Instance = new MotionVectorService();
				return s_Instance;
			}
		}
		public        Dictionary<GameObject, MotionVectorRuntimeData> GetObjects => _runtimeDatas;
		
		private readonly Dictionary<GameObject, MotionVectorRuntimeData> _runtimeDatas = new Dictionary<GameObject, MotionVectorRuntimeData>();

		public void AddObject(GameObject gameObject, Renderer renderer)
		{
			if (gameObject == null || renderer == null)
				return;

			if (_runtimeDatas.ContainsKey(gameObject) == false)
			{
				_runtimeDatas.Add(gameObject, new MotionVectorRuntimeData(new List<Renderer>(){renderer}));
			}
		}

		public void AddObject(GameObject gameObject, List<Renderer> renderers)
		{
			if (gameObject == null || renderers == null || renderers.Count == 0)
				return;

			if (_runtimeDatas.ContainsKey(gameObject) == false)
			{
				_runtimeDatas.Add(gameObject, new MotionVectorRuntimeData(renderers));
			}
		}

		public void RemoveObject(GameObject gameObject)
		{
			if (_runtimeDatas.ContainsKey(gameObject) == true)
			{
				_runtimeDatas.Remove(gameObject);
			}
		}
		
		public void Update()
		{
			foreach (var gObject in _runtimeDatas)
			{
				foreach (var renderer in gObject.Value.Renderers)
				{
					var  skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
					bool isSkinned           = skinnedMeshRenderer != null;
					
					Matrix4x4 currentMatrix  = renderer.localToWorldMatrix;
					Matrix4x4 previousMatrix = gObject.Value.PreviousModalMatrices[renderer];
					bool      hasMoved       = !MatricesAreEqual(currentMatrix, previousMatrix);
					gObject.Value.WasMovedThisFrame = hasMoved;
					
					gObject.Value.MaterialPropertyBlock.SetFloat(s_ForceNoMotion, renderer.motionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion ? 1.0f : 0.0f);
					gObject.Value.MaterialPropertyBlock.SetInt(s_HasLastPositionData, isSkinned ? 1 : 0);
					gObject.Value.MaterialPropertyBlock.SetFloat(s_MotionVectorDepthBias, -0.001f);
					gObject.Value.MaterialPropertyBlock.SetMatrix(s_PreviousM, previousMatrix); // skinnedmesh haven't yet updated rootbone matrices, why?
					renderer.SetPropertyBlock(gObject.Value.MaterialPropertyBlock);
					
					gObject.Value.PreviousModalMatrices[renderer] = currentMatrix;
				}
			}
		}

		public void Cleanup()
		{
			
		}

		private static bool MatricesAreEqual(Matrix4x4 a, Matrix4x4 b, float tolerance = 0.0001f)
		{
			for (int i = 0; i < 16; i++)
			{
				if (Mathf.Abs(a[i] - b[i]) > tolerance)
					return false;
			}
			return true;
		}
	}
}
