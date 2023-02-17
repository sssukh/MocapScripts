using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum SceneName
{
    MainMenu,
    Test,
    Game1,
    ScoreResult,
}

public class SceneChangeManager : MonoBehaviour
{
    private GameObject pipeServer = null;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        pipeServer = GameObject.Find("UserInput");
    }
    public void ChangeScene(SceneName p_sceneName, string _prePath = "", string _content = "", float _score = 0, bool _isHigher = false)
    {
        StartCoroutine(LoadAsyncScene(p_sceneName, _prePath, _content,_score,_isHigher));
        /*
        // 각 씬에서 나가면서 안쓰는 것들 제거
        pipeServer.GetComponent<UnityPipeServer>().ExitScene(p_sceneName.ToString());
        // 씬 전환
        SceneManager.LoadScene(p_sceneName.ToString());

        // 각 씬에 진입하면서 필요한 것들 세팅
        pipeServer.GetComponent<UnityPipeServer>().EnterScene(p_sceneName.ToString(),_prePath,_content);
        */
    }

    public IEnumerator LoadAsyncScene(SceneName p_sceneName, string _prePath, string _content, float _score,bool _isHigher)
    {
        // 각 씬에서 나가면서 안쓰는 것들 제거
        pipeServer.GetComponent<UnityPipeServer>().ExitScene(p_sceneName.ToString());
        // 씬 전환
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(p_sceneName.ToString());
        while(!asyncLoad.isDone)
        {
            yield return null;
        }
        // 각 씬에 진입하면서 필요한 것들 세팅
        pipeServer.GetComponent<UnityPipeServer>().EnterScene(p_sceneName.ToString(), _prePath, _content,_score,_isHigher);
    }
    public void SetGameEnd()
    {
        pipeServer.GetComponent<UnityPipeServer>().b_IsGameEnd = true;
    }
}
