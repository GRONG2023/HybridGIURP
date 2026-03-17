using System;
using System.Reflection;
using HTraceWSGI.Scripts.Globals;
using HTraceWSGI.Scripts.Wrappers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_2021 || UNITY_2022
using UnityEngine.Experimental.Rendering;
#endif

namespace HTraceWSGI.Scripts.Extensions
{
	public static class HExtensions
	{
		public static string ERROR_OUT_RANGE_VALUE = "Your \"{0}\" value is out of range: {1}";

		public static void DebugPrint(DebugType type, string msg)
		{
			msg = "HTrace log: " + msg;
			
			switch (type)
			{
				case DebugType.Log:
					Debug.Log(msg);
					break;
				case DebugType.Warning:
					Debug.LogWarning(msg);
					break;
				case DebugType.Error:
					Debug.LogError(msg);
					break;
			}
		}
		
		public static ComputeShader LoadComputeShader(string shaderName)
		{
			var computeShader = (ComputeShader)Resources.Load($"HTraceWSGI/Computes/{shaderName}");
			if (computeShader == null)
			{
				Debug.LogError($"{shaderName} is missing in HTraceWSGI/Resources/Computes folder");
				return null;
			}

			return computeShader;
		}
		
		public static RayTracingShader LoadRayTracingShader(string shaderName)
		{
			var rtShader = (RayTracingShader)UnityEngine.Resources.Load($"HTraceWSGI/Computes/{shaderName}");
			if (rtShader == null)
			{
				Debug.LogError($"{shaderName} is missing in HTraceWSGI/Resources/Computes folder");
				return null;
			}

			return rtShader;
		}
		
		public static bool ContainsOnOfElement(this string str, string[] elements)
		{
			foreach (var element in elements)
			{
				if (str.Contains(element))
					return true;
			}
			return false;
		}

		public static T Next<T>(this T src) where T : struct
		{
			if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

			T[] array = (T[])Enum.GetValues(src.GetType());
			int nextIndex = Array.IndexOf<T>(array, src) + 1;
			return (array.Length == nextIndex) ? array[0] : array[nextIndex];
		}

		public static T Prev<T>(this T src) where T : struct
		{
			if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

			T[] array = (T[])Enum.GetValues(src.GetType());
			int prevIndex = Array.IndexOf<T>(array, src) - 1;
			return (-1 == prevIndex) ? array[array.Length - 1] : array[prevIndex];
		}

		public static int ParseToProbeSize(this RayCountMode rayCountMode)
		{
			switch (rayCountMode)
			{
				case RayCountMode.Performance:
					return 8;
				case RayCountMode.Quality:
					return 6;
				case RayCountMode.Cinematic:
					return 4;
			}

			return 6;
		}
		
		public static int ParseToInt(this SpatialRadius spatialRadius)
		{
			switch (spatialRadius)
			{
				case SpatialRadius.None:
					return 1;
				case SpatialRadius.Medium:
					return 9;
				case SpatialRadius.Wide:
					return 25;
			}

			return 1;
		}

		//custom Attributes
#if UNITY_EDITOR
		
		/// <summary>
		/// Read Only attribute.
		/// Attribute is use only to mark ReadOnly properties.
		/// </summary>
		public class ReadOnlyAttribute : PropertyAttribute
		{
		}

		/// <summary>
		/// This class contain custom drawer for ReadOnly attribute.
		/// </summary>
		[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
		public class ReadOnlyDrawer : PropertyDrawer
		{
			/// <summary>
			/// Unity method for drawing GUI in Editor
			/// </summary>
			/// <param name="position">Position.</param>
			/// <param name="property">Property.</param>
			/// <param name="label">Label.</param>
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				// Saving previous GUI enabled value
				var previousGUIState = GUI.enabled;
				// Disabling edit for property
				GUI.enabled = false;
				// Drawing Property
				EditorGUI.PropertyField(position, property, label);
				// Setting old GUI enabled value
				GUI.enabled = previousGUIState;
			}
		}
#endif
		
		/// <summary>
		///   <para>Attribute used to make a float or int variable in a script be restricted to a specific range.</para>
		/// </summary>
		[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
		public class HRangeAttribute : Attribute
		{
			public readonly bool isFloat;

			public readonly float minFloat;
			public readonly float maxFloat;
			public readonly int minInt;
			public readonly int maxInt;

			/// <summary>
			///   <para>Attribute used to make a float or int variable in a script be restricted to a specific range.</para>
			/// </summary>
			/// <param name="minFloat">The minimum allowed value.</param>
			/// <param name="maxFloat">The maximum allowed value.</param>
			public HRangeAttribute(float minFloat, float maxFloat)
			{
				this.minFloat = minFloat;
				this.maxFloat = maxFloat;
				isFloat = true;
			}

			/// <summary>
			///   <para>Attribute used to make a float or int variable in a script be restricted to a specific range.</para>
			/// </summary>
			/// <param name="minFloat">The minimum allowed value.</param>
			/// <param name="maxFloat">The maximum allowed value.</param>
			public HRangeAttribute(int minInt, int maxInt)
			{
				this.minInt = minInt;
				this.maxInt = maxInt;
				isFloat = false;
			}
		}

		public struct HRangeAttributeElement
		{
			public bool isFloat;
			public float minFloat;
			public float maxFloat;
			public int minInt;
			public int maxInt;
		}

		public static float Clamp(float value, Type type, string nameOfField)
		{
			HRangeAttribute rangeAttribute = null;
      
			var filed = type.GetField(nameOfField);
			if (filed != null)
			{
				rangeAttribute = filed.GetCustomAttribute<HRangeAttribute>();
			}
			var property = type.GetProperty(nameOfField);
			if (property != null)
			{
				rangeAttribute = property.GetCustomAttribute<HRangeAttribute>();
			}

			return Mathf.Clamp(value, rangeAttribute.minFloat, rangeAttribute.maxFloat);
		}

		public static int Clamp(int value, Type type, string nameOfField)
		{
			HRangeAttribute rangeAttribute = null;
      
			var filed = type.GetField(nameOfField);
			if (filed != null)
			{
				rangeAttribute = filed.GetCustomAttribute<HRangeAttribute>();
			}
			var property = type.GetProperty(nameOfField);
			if (property != null)
			{
				rangeAttribute = property.GetCustomAttribute<HRangeAttribute>();
			}

			return Mathf.Clamp(value, rangeAttribute.minInt, rangeAttribute.maxInt);
		}
		
		public static void HRelease(this ComputeBuffer computeBuffer)
		{
			if (computeBuffer != null)
				computeBuffer.Release();
		}

		public static void HRelease(this CommandBuffer commandBuffer)
		{
			if (commandBuffer != null)
			{
				commandBuffer.Clear();
				commandBuffer.Release();
			}
		}

		public static void HRelease(this GraphicsBuffer graphicsBuffer)
		{
			if (graphicsBuffer != null)
			{
				graphicsBuffer.Release();
			}
		}

		public static void HRelease(this HDynamicBuffer hDynamicBuffer)
		{
			if (hDynamicBuffer != null)
			{
				hDynamicBuffer.Release();
			}
		}

		public static void HRelease(this RayTracingAccelerationStructure rayTracingAccelerationStructure)
		{
			if (rayTracingAccelerationStructure != null)
			{
				rayTracingAccelerationStructure.Release();
			}
		}
	}
}
