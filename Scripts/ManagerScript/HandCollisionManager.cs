using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandCollisionManager : MonoBehaviour
{
    [SerializeField]
    private GameObject unityPipeServerObj;

    private UnityPipeServer UnityPipeServer;

    private RectTransform thisRectTransform;

    [SerializeField]
    private GameObject sceneChangeObj = null;

    private SceneChangeManager sceneChangeManager = null;

    private Image thisImage;

    private float HandSize = 50f;
    private float time = 0f;
    [SerializeField]
    private const float maxTime = 3f;

    [SerializeField]
    private bool makeViewable = false;

    // Start is called before the first frame update
    void Start()
    {
        thisRectTransform = GetComponent<RectTransform>();
        thisImage = GetComponent<Image>();
        unityPipeServerObj = GameObject.Find("UserInput");
        UnityPipeServer = unityPipeServerObj.GetComponent<UnityPipeServer>();
        sceneChangeObj = GameObject.Find("SceneChangeManager");
        sceneChangeManager = sceneChangeObj.GetComponent<SceneChangeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (makeViewable&&UnityPipeServer.getIsValueComing())
            gameObject.GetComponentInChildren<Image>().color = new Color(255,255,255,255);
        Vector2 RHandPos = UnityPipeServer.getRHandPos();
        Vector2 LHandPos = UnityPipeServer.getLHandPos();
        Vector2 thisPos = thisRectTransform.anchoredPosition;
        Vector2 thisSize = thisRectTransform.sizeDelta;
        if(Mathf.Abs(RHandPos.x - thisPos.x)<HandSize + thisSize.x/2 && Mathf.Abs(RHandPos.y - thisPos.y)<HandSize + thisSize.y/2
            && Mathf.Abs(LHandPos.x - thisPos.x) < HandSize  + thisSize.x/2 && Mathf.Abs(LHandPos.y - thisPos.y) < HandSize  + thisSize.y/2)
        {
            // 충돌 발생
            time += Time.deltaTime;
            if (time >= maxTime)
            {
                //EventOccur();
                StartCoroutine(EventCoroutine());
                time = 0;
            }
            thisImage.fillAmount = time / maxTime;
        }
        else
        {
            time = 0f;
            thisImage.fillAmount = 0f;
        }
    }

    private void EventOccur()
    {
        // 이 스크립트를 장착한 오브젝트의 이름에 따라 작동하는 기능이 다르다.
        switch(gameObject.name)
        {
            case "StartGame":
                // 시작
                sceneChangeManager.ChangeScene(SceneName.MainMenu);
                break;
            case "BackBtnBg":
                sceneChangeManager.ChangeScene(SceneName.MainMenu);
                break;
            default:
                gameObject.GetComponentInChildren<Button>().onClick.Invoke();
                break;



        }
    }
    private IEnumerator EventCoroutine()
    {
        switch (gameObject.name)
        {
            case "StartGame":
                // 시작
                sceneChangeManager.ChangeScene(SceneName.MainMenu);
                break;
            case "BackBtnBg":
                sceneChangeManager.ChangeScene(SceneName.MainMenu);
                break;
            case "ResumeBtnBg":
                if(Time.timeScale==0)
                    gameObject.GetComponentInChildren<Button>().onClick.Invoke();
                break;
            default:
                gameObject.GetComponentInChildren<Button>().onClick.Invoke();
                break;
            
        }
        Debug.LogError(gameObject.name);
        yield return new WaitForSecondsRealtime(3f);
    }
}
