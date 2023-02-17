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

    // ������ Ʈ��ŷ�� ���� ������ �� ������Ʈ
    private GameObject[] UserBody = new GameObject[14];
    // �������� ������ 3�� �̻� �������� ��� ȭ�鿡 ��Ÿ�� ��ȣ �� �ð� ����
    private GameObject connectionLost;
    private float connectionLostTime = 0f;

    // ���� ���� �ʿ��� ���� ������
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

    // ��� ���� �ʿ��� ������
    private GameObject scoreCounterObj = null;
    private ScoreCounter scoreCounter = null;

    // frame counter
    private int frameCounter;
    private float totalScore;

    // ������ Ʈ��ŷ�ؼ� �޾ƿ� ����Ʈ ������ ������ �迭
    // private int[] publicBuffer;
    private float[] publicBuffer = new float[36];


    // �������Լ� �޾ƿ� �������� ������ ����
    private float[] userAnglesBuffer = new float[9];
    // ���Ͽ� ������ִ� �������� ������ ����
    private float[] fileAngleBuffer = new float[9];
  
    // ������
    private NamedPipeServerStream pipeServer;
    private Thread serverReadThread;

    // ���� �� ��ġ
    private Scene curScene;


    // float ���� input ����Ʈ �����ϱ�
    public int xScale;
    public int yScale;

    public int xOffset;
    public int yOffset;

    // ������ ��� �� �������� ��
    [SerializeField]
    private int frame_amounts = 120;

    private int scoreResult;

    private bool isServerConnected = false;

    public int musicIdx;

    public bool b_IsGameEnd = false;

    public bool isComing = false;
    private string currentPath;
    private bool alreadyChecked = false;

    // ������ ���� ���� �׸��� ������ �̾��� �� ������ ���ڷ� �־ ������ ���Ѵ�.
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
        // ����� �������� �����ش�.(0<= angle <=180)
        return Vector2.Angle(first, second);
       
    }

    // ������ ���� ������ ���ڷ� ���� �迭�� ���� ���� ä���.
    private void SetJointAngles(ref float[] _angles)
    {
        // Debug.Log("check2");

        // ��            �Ӹ� - �� - ������ ���
        userAnglesBuffer[0] = GetAngle(Body.Head, Body.RShoulder, Body.Neck);


        // ���Ȳ�ġ         �޼� - ���Ȳ�ġ - �޾��      
        userAnglesBuffer[1] = GetAngle(Body.LWrist, Body.LShoulder, Body.LElbow);


        // �޾��          �� - �޾�� - ���Ȳ�ġ
        userAnglesBuffer[2] = GetAngle(Body.Neck, Body.LElbow, Body.LShoulder);


        // �����Ȳ�ġ        ������ - �����Ȳ�ġ - �������
        userAnglesBuffer[3] = GetAngle(Body.RWrist, Body.RShoulder, Body.RElbow);


        // �������            �� - ������� - �����Ȳ�ġ
        userAnglesBuffer[4] = GetAngle(Body.Neck, Body.RElbow, Body.RShoulder);

        // �޹���              �ް�� - �޹��� - �޹�
        userAnglesBuffer[5] = GetAngle(Body.LHip, Body.LAnkle, Body.LKnee);

        // �ް��              ������� - �ް�� - �޹���
        userAnglesBuffer[6] = GetAngle(Body.RHip, Body.LKnee, Body.LHip);

        // ��������             ������� - �������� - ������
        userAnglesBuffer[7] = GetAngle(Body.RHip, Body.RAnkle, Body.RKnee);

        // �������             �ް�� - ������� - ��������
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
        // public buffer �ʱ�ȭ
        for(int i=0;i<36;++i)
        {
            publicBuffer[i] = -1;
        }
        // ������ �ʱ�ȭ
        pipeServer = new NamedPipeServerStream("CSServer", PipeDirection.In);
        Debug.Log("Opend pipe");
        // ������ ������� ǥ���� ������Ʈ 
        connectionLost = GameObject.Find("connectionLostSymbol");
        connectionLost.SetActive(false);
        // ������ Ʈ��ŷ�� ��
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
            // ���� Ȯ���� ���� ������. Ȯ���ڵ� �߰��� ��.
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

    // ���� �����忡�� ������ ����
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


    // ����� �������� �ʴ� ����Ʈ �������, �� ���� �´� ��ºκ� ó���ϱ�
    private void ControllRenderPoints(int _offsetX, int _offsetY,string _curSceneName)
    {
        // ���� ���� � ������ ����
        bool isTesting;
        
        if (_curSceneName == "InitializingScene")
            isTesting = true;
        else
            isTesting = false;
        

        if (isTesting)
        {
            for (int i = 0; i < (int)Body.End; ++i)
            {
                // NaN �� �� �ش� ����Ʈ ������� �ʱ�
                if (publicBuffer[2 * i] <0 || publicBuffer[2 * i + 1] <0)
                {
                    UserBody[i].SetActive(false);
                    publicBuffer[2 * i] = 0;
                    publicBuffer[2 * i + 1] = 0;
                }
                else
                {
                    // test ���̸� ���� ���

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
                // NaN�� �� �ǵ帮�� �ʱ�
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

                // ���� ������ �۵�
                InGameScene(curScene.name);
                

                for (int i = (int)Body.Head; i < (int)Body.End; ++i)
                {
                    UserBody[i].GetComponent<RectTransform>().anchoredPosition = new Vector3((publicBuffer[2 * i]*-1) -xOffset, (publicBuffer[(2 * i) + 1] * -1 )- yOffset, 0);
                }

           
        }

        else
        {
            // 3���̻� ���� ���� �� ȭ�鿡 ǥ��
            connectionLostTime += Time.deltaTime;
            if (connectionLostTime >= 1.0f)
            {
                connectionLost.SetActive(true);
            }

        }
    }

    // ���� ��
    float checkScore(ref float[] _userArray, ref float[] _fileArray)
    {
        float result = 0;
        int count = 0;
        for(int i=0;i<_userArray.Length;++i)
        {
            float userData = _userArray[i];
            float fileData = _fileArray[i];

            Debug.Log("User : " + _userArray[i] + " , File : " + _fileArray[i]);

            // NaN�� ��� �� ���� �ʱ�
            if (userData < 0 || fileData < 0)
                continue;
            // �������� result�� ���ϱ�
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
            // 30�����ӵ��� ���� ��ճ���
            float average = totalScore / frame_amounts;
            totalScore = 0;
            int num;
            // ����Ʈ ȭ�鿡 ��Ÿ����
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
    // �����Ϳ��� �׽�Ʈ�� ���� ��ư�� ������ ������ ���� ��ư
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
