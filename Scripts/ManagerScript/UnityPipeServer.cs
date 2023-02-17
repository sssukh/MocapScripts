using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Body
{
    Head,
    Neck,
    RShoulder,
    RElbow,
    RWrist,
    LShoulder,
    LElbow,
    LWrist,
    RHip,
    RKnee,
    RAnkle,
    LHip,
    LKnee,
    LAnkle,
    End
}

public enum Music
{
    SoHappy,
    HeroesTonight,
}


public class UnityPipeServer : MonoBehaviour
{
    [SerializeField]
    private GameObject sceneChangeManagerObj = null;

    // 유저를 트래킹한 값을 적용할 모델 오브젝트
    private GameObject[] UserBody = new GameObject[14];
    // 파이프가 연결이 3초 이상 끊겨있을 경우 화면에 나타낼 신호 및 시간 측정
    private GameObject connectionLost;
    private float connectionLostTime = 0f;

    // 게임 씬에 필요한 전역 변수들
    public Fileios fileios = null;
    private Scoreeffect scoreEffect = null;
    public GameObject gameTimerObj = null;
    public GameObject fileiosObject = null;
    public GameObject effectObject = null;
    public GameObject audioObject = null;
    private AudioManager audioManager = null;
    private GameTimer gameTimer = null;
    private SceneChangeManager sceneChangeManager = null;
    private GameObject danceManagerObj = null;
    private DanceManager danceManager = null;

    // 결과 씬에 필요한 변수들
    private GameObject scoreCounterObj = null;
    private ScoreCounter scoreCounter = null;

    // frame counter
    private int frameCounter;
    private float totalScore;

    // 유저를 트래킹해서 받아온 포인트 값들을 저장할 배열
    // private int[] publicBuffer;
    private float[] publicBuffer = new float[36];


    // 유저에게서 받아온 각도들을 저장할 버퍼
    private float[] userAnglesBuffer = new float[9];
    // 파일에 저장되있던 각도들을 저장할 버퍼
    private float[] fileAngleBuffer = new float[9];
  
    // 파이프
    private NamedPipeServerStream pipeServer;
    private Thread serverReadThread;

    // 현재 씬 위치
    private Scene curScene;


    // float 유저 input 리스트 조절하기
    public int xScale;
    public int yScale;

    public int xOffset;
    public int yOffset;

    // 점수를 평균 낼 프레임의 수
    [SerializeField]
    private int frame_amounts = 120;

    private int scoreResult;

    private bool isServerConnected = false;

    public int musicIdx;

    public bool b_IsGameEnd = false;

    public bool isComing = false;
    private string currentPath;
    private bool alreadyChecked = false;

    // 각도를 구할 관절 그리고 관절과 이어진 양 끝점을 인자로 넣어서 각도를 구한다.
    private float GetAngle(Body _first,Body _second, Body _middle)
    {
        // Debug.Log("check3");

        if (publicBuffer[(int)_first * 2] < 0 || publicBuffer[(int)_second * 2] < 0 ||
            publicBuffer[(int)_middle * 2] < 0 )
        {
            return -1f;
        }
        Vector2 first = new Vector2(publicBuffer[(int)_first * 2], publicBuffer[(int)_first * 2 + 1]);
        Vector2 second = new Vector2(publicBuffer[(int)_second * 2], publicBuffer[(int)_second * 2 + 1]);

        Vector2 middle = new Vector2(publicBuffer[(int)_middle * 2], publicBuffer[(int)_middle * 2 + 1]);

        first -= middle;
        second -= middle;
        // 양수의 각도값을 구해준다.(0<= angle <=180)
        return Vector2.Angle(first, second);
       
    }

    // 다음과 같은 순서로 인자로 받은 배열에 각도 값을 채운다.
    private void SetJointAngles(ref float[] _angles)
    {
        // Debug.Log("check2");

        // 목            머리 - 목 - 오른쪽 어깨
        userAnglesBuffer[0] = GetAngle(Body.Head, Body.RShoulder, Body.Neck);


        // 왼팔꿈치         왼손 - 왼팔꿈치 - 왼어깨      
        userAnglesBuffer[1] = GetAngle(Body.LWrist, Body.LShoulder, Body.LElbow);


        // 왼어깨          목 - 왼어깨 - 왼팔꿈치
        userAnglesBuffer[2] = GetAngle(Body.Neck, Body.LElbow, Body.LShoulder);


        // 오른팔꿈치        오른손 - 오른팔꿈치 - 오른어깨
        userAnglesBuffer[3] = GetAngle(Body.RWrist, Body.RShoulder, Body.RElbow);


        // 오른어깨            목 - 오른어깨 - 오른팔꿈치
        userAnglesBuffer[4] = GetAngle(Body.Neck, Body.RElbow, Body.RShoulder);

        // 왼무릎              왼골반 - 왼무릎 - 왼발
        userAnglesBuffer[5] = GetAngle(Body.LHip, Body.LAnkle, Body.LKnee);

        // 왼골반              오른골반 - 왼골반 - 왼무릎
        userAnglesBuffer[6] = GetAngle(Body.RHip, Body.LKnee, Body.LHip);

        // 오른무릎             오른골반 - 오른무릎 - 오른발
        userAnglesBuffer[7] = GetAngle(Body.RHip, Body.RAnkle, Body.RKnee);

        // 오른골반             왼골반 - 오른골반 - 오른무릎
        userAnglesBuffer[8] = GetAngle(Body.LHip, Body.RKnee, Body.RHip);
    }
    private void OnApplicationQuit()
    {
        pipeServer.Close();
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        // public buffer 초기화
        for(int i=0;i<36;++i)
        {
            publicBuffer[i] = -1;
        }
        // 파이프 초기화
        pipeServer = new NamedPipeServerStream("CSServer", PipeDirection.In);
        Debug.Log("Opend pipe");
        // 파이프 연결끊김 표시할 오브젝트 
        connectionLost = GameObject.Find("connectionLostSymbol");
        connectionLost.SetActive(false);
        // 유저를 트래킹할 모델
        for (Body i = Body.Head; i < Body.End; ++i)
        {
            UserBody[(int)i] = GameObject.Find(i.ToString());
        }

        serverReadThread = new Thread(ServerThread_Read);
        serverReadThread.Start();

        frameCounter = 0;


        curScene = SceneManager.GetActiveScene();

        sceneChangeManager = sceneChangeManagerObj.GetComponent<SceneChangeManager>();

    }


    public void EnterScene(string _sceneName, string _prePath = "", string _content = "", float _score = 0f, bool _isHigher = false)
    {
        curScene = SceneManager.GetActiveScene();

        if (_sceneName==SceneName.Game1.ToString())
        {
            gameTimerObj = GameObject.Find("Timer");
            
            fileiosObject = GameObject.Find("Fileios");
            
            effectObject = GameObject.Find("Effect");

            audioObject = GameObject.Find("Audio");

            danceManagerObj = GameObject.Find("Male 1");

            gameTimer = gameTimerObj.GetComponent<GameTimer>();
            scoreEffect = effectObject.GetComponent<Scoreeffect>();
            fileios = fileiosObject.GetComponent<Fileios>();
            fileios.setFileName(_prePath + _content + ".bin");
            currentPath = _prePath + _content;
            audioManager = audioObject.GetComponent<AudioManager>();
            danceManager = danceManagerObj.GetComponent<DanceManager>();
            alreadyChecked = false;
            if (_content[1] == 'S')
            {
                audioManager.SetMusic(0);
                musicIdx = (int)Music.SoHappy;
                danceManager.SetAnimTrigger(0);
            }
            else
            { 
                audioManager.SetMusic(1);
                musicIdx = (int)Music.HeroesTonight;
                danceManager.SetAnimTrigger(1);
            }

            fileios.createWriter();
            fileios.createReader();
            // 파일 확장자 없이 되있음. 확장자도 추가할 것.
            Time.timeScale = 0f;

            gameTimer.PreparingTime();

            scoreResult = 0;

        }
        else if(_sceneName == SceneName.ScoreResult.ToString())
        {
            scoreCounterObj = GameObject.Find("ResultBackground");
            scoreCounter = scoreCounterObj.GetComponent<ScoreCounter>();
            scoreCounter.getHighBool(_isHigher);
            scoreCounter.StartCount(_score);
        }
    }
    
    public void ExitScene(string _sceneName)
    {
        if (_sceneName == SceneName.Game1.ToString())
        {
            
            gameTimer = null;
            gameTimerObj = null;
            fileiosObject = null;
            effectObject = null;
            fileios = null;
            scoreEffect = null;
        }
    }

    // 서버 쓰레드에서 파이프 연결
    private void ServerThread_Read()
    {
        try
        {
            Debug.Log("Waiting for connection...");
            pipeServer.BeginWaitForConnection(new AsyncCallback(this.PipeConnected), null);
        }
        catch(IOException e)
        {
            Debug.LogError("IOException : " + e.Message);
        }
        catch(Exception e)
        {
            Debug.LogError("Error : " + e.Message);
        }
    }
    protected void PipeConnected(IAsyncResult ar)
    {
        try
        {
            this.pipeServer.EndWaitForConnection(ar);
            Debug.Log("Client connected!");
            isServerConnected = true;

            while(pipeServer.IsConnected)
            {
                AwaitRead(33);
            }
        }
        catch(Exception e)
        {
            Debug.LogError("Error : " +  e.Message);
        }
    }


    // 제대로 추적되지 않는 포인트 출력제외, 각 씬에 맞는 출력부분 처리하기
    private void ControllRenderPoints(int _offsetX, int _offsetY,string _curSceneName)
    {
        // 현재 씬이 어떤 씬인지 추적
        bool isTesting;
        
        if (_curSceneName == "InitializingScene")
            isTesting = true;
        else
            isTesting = false;
        

        if (isTesting)
        {
            for (int i = 0; i < (int)Body.End; ++i)
            {
                // NaN 일 시 해당 포인트 출력하지 않기
                if (publicBuffer[2 * i] <0 || publicBuffer[2 * i + 1] <0)
                {
                    UserBody[i].SetActive(false);
                    publicBuffer[2 * i] = 0;
                    publicBuffer[2 * i + 1] = 0;
                }
                else
                {
                    // test 씬이면 전부 출력

                    UserBody[i].SetActive(true);
                   
                }
               
            }
        }
        else
        {
            for (int i = 0; i < (int)Body.End; ++i)
            {
                if ((i==(int)Body.RWrist||i==(int)Body.LWrist) && publicBuffer[2 * i] >0 && publicBuffer[2 * i + 1] >0)
                {
                    UserBody[i].SetActive(true);
                }
                else
                {
                    UserBody[i].SetActive(false);
                    
                }
               
            }
        }
    }
    private async void AwaitRead(int timeout)
    {
        byte[] buffer = new byte[144];
        float[] fBuffer = new float[36];

        try
        {
            bool isValueComing = false;
            var num = await pipeServer.ReadAsync(buffer, 0, 144);

            //Debug.Log("got " + num + " bytes");

            Buffer.BlockCopy(buffer, 0, fBuffer, 0, buffer.Length);

            for (int idx = 0; idx < 36; ++idx)
            {
                //Debug.LogWarning(idx + " : " + (float)fBuffer[idx]);
                if (fBuffer[idx] > 0)
                    isValueComing = true;
                // NaN일 시 건드리지 않기
                if (fBuffer[idx] == Single.NaN)
                {
                    continue;
                }
                else if (idx % 2 == 0)
                {
                    fBuffer[idx] *= xScale;
                }
                else
                {
                    fBuffer[idx] *= yScale;
                }
            }
            if (isValueComing)
            {
                isComing = isValueComing;
                publicBuffer = fBuffer;
            }
            
            Debug.Log("Read....");
        }
        catch(Exception e)
        {
            Debug.LogError("pipe read error : " + e.Message);
            return;
        }
    }
    
    void Update()
    {
        //Debug.Log("updating....");
        if (pipeServer.IsConnected)
        {
            connectionLostTime = 0f;
            connectionLost.SetActive(false);
  
                ControllRenderPoints(xOffset, yOffset,curScene.name);

                // 게임 씬에서 작동
                InGameScene(curScene.name);
                

                for (int i = (int)Body.Head; i < (int)Body.End; ++i)
                {
                    UserBody[i].GetComponent<RectTransform>().anchoredPosition = new Vector3((publicBuffer[2 * i]*-1) -xOffset, (publicBuffer[(2 * i) + 1] * -1 )- yOffset, 0);
                }

           
        }

        else
        {
            // 3초이상 연결 끊길 시 화면에 표시
            connectionLostTime += Time.deltaTime;
            if (connectionLostTime >= 1.0f)
            {
                connectionLost.SetActive(true);
            }

        }
    }

    // 각도 비교
    float checkScore(ref float[] _userArray, ref float[] _fileArray)
    {
        float result = 0;
        int count = 0;
        for(int i=0;i<_userArray.Length;++i)
        {
            float userData = _userArray[i];
            float fileData = _fileArray[i];

            Debug.Log("User : " + _userArray[i] + " , File : " + _fileArray[i]);

            // NaN일 경우 비교 하지 않기
            if (userData < 0 || fileData < 0)
                continue;
            // 오차값들 result에 더하기
            result += Math.Abs(userData - fileData) / 180f;
            count++;
            Debug.Log("result : " + result);
        }
        return 100 - (result/count)*100;
    }


    void InGameScene(string _curSceneName)
    {
       
        if (_curSceneName != "Game1"||gameTimer.getTimer()<=0)
        {
            return;
        }
        if(gameTimer.IsMusicOver(musicIdx,0f)&&fileios.getEOF())
        {
            danceManager.StopDance();
        }
        if (gameTimer.IsMusicOver(musicIdx, 1f)&&!audioManager.IsPlaying()&&!alreadyChecked)
        {
            alreadyChecked = true;
            bool isHigher = fileios.compareScoreBig(currentPath,scoreResult);
            sceneChangeManager.ChangeScene(SceneName.ScoreResult, "", "", scoreResult,isHigher);
        }


        SetJointAngles(ref userAnglesBuffer);
        for(int i=0;i<userAnglesBuffer.Length;++i)
        {
            Debug.Log("Angle" + i + " : " + (float)userAnglesBuffer[i]);
        }


        // write
        //fileios.bWrite(gameTimer.GetComponent<GameTimer>().getTimer(), ref userAnglesBuffer);

        // read
        fileios.bRead(fileAngleBuffer.Length, gameTimer.GetComponent<GameTimer>().getTimer(), ref fileAngleBuffer, 100f);

        float score = checkScore(ref userAnglesBuffer, ref fileAngleBuffer);

        Debug.Log("Score : " + score);
        totalScore += score;
        if (frameCounter >= frame_amounts)
        {
            frameCounter = 0;
            // 30프레임동안 점수 평균내기
            float average = totalScore / frame_amounts;
            totalScore = 0;
            int num;
            // 이펙트 화면에 나타내기
            if (average > 90)
            {
                // great
                num = 3;
                scoreResult += 30;
            }
            else if (average > 80)
            {
                // good
                num = 2;
                scoreResult += 20;
            }
            else if (average > 60)
            {
                // not bad
                num = 1;
                scoreResult += 10;
            }
            else
            {
                // bad
                num = 0;
            }
            scoreEffect.JudgementEffect(num);
        }
        else
        {
            frameCounter++;
        }


    }

    public bool IsConnected()
    {
        return isServerConnected;
    }
    ~UnityPipeServer()
    {
        pipeServer.Close();
    }
    // 에디터에서 테스트시 쓰는 버튼에 적용할 파이프 종료 버튼
    public void PipeCloseBtn()
    {
        pipeServer.Close();
    }

    public Vector2 getRHandPos()
    {
        return UserBody[(int)Body.RWrist].GetComponent<RectTransform>().anchoredPosition;
    }
    public Vector2 getLHandPos()
    {
        return UserBody[(int)Body.LWrist].GetComponent<RectTransform>().anchoredPosition;
    }
    public bool getIsValueComing()
    {
        return isComing;
    }
}
