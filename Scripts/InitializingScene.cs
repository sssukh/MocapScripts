using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializingScene : MonoBehaviour
{
    [SerializeField]
    private GameObject pipeServerObj = null;

    [SerializeField]
    private GameObject sceneChangeObj = null;

    private UnityPipeServer pipeServer = null;

    private SceneChangeManager sceneChangeManager = null;
    // Start is called before the first frame update
    void Start()
    {
        pipeServer = pipeServerObj.GetComponent<UnityPipeServer>();
        sceneChangeManager = sceneChangeObj.GetComponent<SceneChangeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(pipeServer.IsConnected())
        {
            sceneChangeManager.ChangeScene(SceneName.MainMenu);
        }

    }
}
