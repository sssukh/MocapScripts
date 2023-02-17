using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private Camera myCam;
    // Start is called before the first frame update
    public void MaskOn()
    {
        myCam.cullingMask = ~(1 << LayerMask.NameToLayer("Default"));
    }
    public void MaskOff()
    {
        myCam.cullingMask = -1;

    }
}
