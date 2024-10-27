using System.Collections;
using UnityEngine;

public static class CameraViewportObjectChecker
{
    public static bool CheckObjSeenOnCamera(Transform checkingObj)
    {
        bool isOnScreen = false;

        Plane[] playerCamFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        var bounds = checkingObj.GetComponent<Collider>().bounds;


        // 카메라에 표시조차 되지 않는다면 return false
        if (!GeometryUtility.TestPlanesAABB(playerCamFrustum, bounds)) return false;

        // 카메라에 표시가 된다면 제일 먼저 충돌하는 Obj가 checkingObj인지 체크
        if (Physics.Linecast(Camera.main.transform.position, checkingObj.position, out RaycastHit hitInfo))
        {
            if (hitInfo.transform == checkingObj) isOnScreen = true;
        }


        return isOnScreen;

    }
}