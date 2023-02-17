using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingActivator : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject unitypipeObj;

    private UnityPipeServer UnityPipe;

    [SerializeField]
    private bool activeObject = false;
    void Start()
    {
        UnityPipe = unitypipeObj.GetComponent<UnityPipeServer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(UnityPipe.getIsValueComing())
        {
            gameObject.SetActive(activeObject);
        }
    }
}
