using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialEffectManager", menuName = "ScriptableObjects/Managers/MaterialEffectManager", order = 1)]
public class MaterialEffectManager : ScriptableObject
{

    [SerializeField] Material DissolveMaterial;
    [SerializeField] Material PhaseMaterial;



	public IEnumerator ApplyMaterialEffect(Transform targetObject, 
                                                        ShaderGraphEffectType effectType, ShaderGraphEffectDirection shaderGraphEffectDirection = ShaderGraphEffectDirection.None,
                                                         float effectDuration = 3.0f)
    {
        // Preparation for obtain effect
        #region Obtain Effect Preparation (Shader Graph)
        var obtObjRenderer = targetObject.GetComponentInChildren<Renderer>();

        Vector2 effectSplitValue = new();
        if (effectType == ShaderGraphEffectType.Dissolve)
        {
            obtObjRenderer.material = GetDissolvedMat(obtObjRenderer);
            effectSplitValue = GetDissolveSplit();
        }
        else
        {
            obtObjRenderer.material = GetPhasedMat(obtObjRenderer);
            effectSplitValue = GetPhaseSplit(targetObject.transform, shaderGraphEffectDirection);
        }
        float minSplit = effectSplitValue[0], maxSplit = effectSplitValue[1];

        #endregion


        float time = 0;

        while (time < effectDuration)
        {
            time += Time.deltaTime;

            var splitValue = Mathf.Lerp(minSplit, maxSplit, time / effectDuration);
            obtObjRenderer.material.SetFloat("_Split_Value", splitValue);

            yield return null;
        }
        obtObjRenderer.material.SetFloat("_Split_Value", maxSplit);
    }


    // 이거 에러 고치기
    // static 변수 -> static material을 지속 전달하는 걸로 바꾸기
    Material GetDissolvedMat(Renderer objRenderer)
    {
        var convertedMat = new Material(DissolveMaterial);

        var originalBaseMap = objRenderer.material.GetTexture("_MainTex");
        var originalNormalMap = objRenderer.material.GetTexture("_NormalMap");

        if(originalBaseMap && originalBaseMap)
		{
            convertedMat.SetTexture("_MainTex", originalBaseMap);
            convertedMat.SetTexture("_NormalMap", originalNormalMap);
        }
        else
		{
            var originalColor = objRenderer.material.GetColor("_BaseColor");
            convertedMat.SetColor("_EmissionColor", originalColor);
        }
        

        return convertedMat;
    }
    Material GetPhasedMat(Renderer objRenderer)
    {
        var convertedMat = new Material(PhaseMaterial);

        var originalBaseMap = objRenderer.material.GetTexture("_MainTex");
        var originalNormalMap = objRenderer.material.GetTexture("_NormalMap");

        convertedMat.SetTexture("_MainTex", originalBaseMap);
        convertedMat.SetTexture("_NormalMap", originalNormalMap);

        return convertedMat;
    }


    Vector2 GetDissolveSplit()
	{
        return new Vector2(1.1f, 0f);
    }
    Vector2 GetPhaseSplit(Transform targetObj, ShaderGraphEffectDirection effectDirection)
	{
        Vector2 splitValue = new();

        switch (effectDirection)
        {
            case ShaderGraphEffectDirection.x:
                splitValue[0] = targetObj.position.x;

                break;
            case ShaderGraphEffectDirection.y:
                splitValue[0] = targetObj.position.y;

                break;
            case ShaderGraphEffectDirection.z:
                splitValue[0] = targetObj.position.z;

                break;
        }

        splitValue[1] = -splitValue[0];


        return splitValue;
    }

}

public enum ShaderGraphEffectType
{ None, Dissolve, Phase }

public enum ShaderGraphEffectDirection
{ None, x, y, z }