using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleOffset : MonoBehaviour
{
    public Transform rifle;

    public Vector3[] idleTrans;
    public Vector3[] walkTrans;
    public Vector3[] runTrans;
    public Vector3[] aimTrans;

    public void IdleOffset()
    {
        rifle.transform.localPosition = idleTrans[0];
        rifle.transform.localRotation = Quaternion.Euler(idleTrans[1]);
    }
    public void WalkOffset()
    {
        rifle.transform.localPosition = walkTrans[0];
        rifle.transform.localRotation = Quaternion.Euler(walkTrans[1]);
    }
    public void RunOffset()
    {
        rifle.transform.localPosition = runTrans[0];
        rifle.transform.localRotation = Quaternion.Euler(runTrans[1]);
    }
    public void AimOffset()
    {
        rifle.transform.localPosition = aimTrans[0];
        rifle.transform.localRotation = Quaternion.Euler(aimTrans[1]);
    }
}
