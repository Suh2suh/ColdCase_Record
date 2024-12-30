using System.Collections.Generic;
using UnityEngine;

public class BlenderShapeController : MonoBehaviour
{
	[Header("Attatch Mesh Renderer with has Blender Shape")]
	[SerializeField] private List<SkinnedMeshRenderer> blenderShapeRenderers;
	private Dictionary<string, SkinnedMeshRenderer> SMRendererPerBlenderShape;
	private Dictionary<string, Mesh> MeshPerBlenderShape;


	private void Awake()
	{
		SMRendererPerBlenderShape = new();
		MeshPerBlenderShape = new();

		InitializeBlenderShapeData();
	}


	public void SetBlenderShapeValue(string shapeKey, float value)
	{
		var skinnedRenderer = SMRendererPerBlenderShape[shapeKey];
		skinnedRenderer.SetBlendShapeWeight(GetBlenderShapeIndex(shapeKey), value);
	}

	public float GetBlenderShapeValue(string shapeKey)
	{
		var skinnedRenderer = SMRendererPerBlenderShape[shapeKey];
		return skinnedRenderer.GetBlendShapeWeight(GetBlenderShapeIndex(shapeKey));
	}

	private int GetBlenderShapeIndex(string shapeKey)
	{
		var mesh = MeshPerBlenderShape[shapeKey];
		int blenderShapeIndex = mesh.GetBlendShapeIndex(shapeKey);

		return blenderShapeIndex;
	}
	

	private void InitializeBlenderShapeData()
	{
		foreach (var blenderShapeRenderer in blenderShapeRenderers)
		{
			Mesh mesh = blenderShapeRenderer.sharedMesh;

			for (int shapeIndex = 0; shapeIndex < mesh.blendShapeCount; shapeIndex++)
			{
				string blenderShapeName = mesh.GetBlendShapeName(shapeIndex);

				SMRendererPerBlenderShape[blenderShapeName] = blenderShapeRenderer;
				MeshPerBlenderShape[blenderShapeName] = mesh;
			}
		}

	}


}
