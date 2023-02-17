using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Playables;





// 게임플레이하는 동안 파일을 읽어오는 기능
// deltatime에 맞춰서 읽어서 sync를 맞추도록 한다.
public class Fileios : MonoBehaviour
{
    // E:\Unity\Projects\My project\Assets\Contents\SoHappy
    public string fileName = "test.bin";
    //private string prePath = "Assets/Contents";
    //private string happy = "/SoHappy/SoHappy.bin";
    //private string heroes = "/HeroesTonight/HeroesTonight.bin";
    // Assets/Contents/SoHappy/Raven & Kreyn - So Happy[NCS Release].mp3
    // Assets/Contents/SoHappy/SoHappy.jpg
    // Assets/Contents/HeroesTonight/Janji - Heroes Tonight (feat. Johnning) [NCS Release].mp3
    // Assets/Contents/HeroesTonight/HeroesTonightCover.jpg

    FileStream fs;
    BinaryWriter binaryWriter;
    BinaryReader binaryReader;
    StreamReader streamReader;
    StreamWriter streamWriter;

    bool EOF = false;

    // Start is called before the first frame update
    void Start()
    {
        // fs = new FileStream(fileName, FileMode.OpenOrCreate,FileAccess.ReadWrite);
        
    }
    public void GetName()
    {
        Debug.LogError(fs.Name);
    }
    // Update is called once per frame
    
    // 전달받은 array 작성
    public void bWrite(float _deltaTime, ref float[] _array)
    {
        // deltatime 작성
        binaryWriter.Write(_deltaTime);
        Debug.Log("time : " + _deltaTime + "just time : " + fs.Seek(0,SeekOrigin.Current));
        byte[] lByteBuffer = new byte[_array.Length * 4];
        // 인자로 전달받은 float 배열 byte배열로 변환
        Buffer.BlockCopy(_array, 0, lByteBuffer, 0, lByteBuffer.Length);
        binaryWriter.Write(lByteBuffer, 0, lByteBuffer.Length);
        Debug.Log("after data : " + fs.Seek(0, SeekOrigin.Current));
        
    }

    // array의 길이만큼 읽어온다.
    public void bRead(int _arrayLength, float _deltaTime, ref float[] floatResult, float _timeDiff)
    {
        float var1;
        byte[] byteBuffer = new byte[_arrayLength*4];
        float[] floatBuffer = new float[byteBuffer.Length / 4];
        // 시간 오차값
        //_timeDiff = 100;
        while (true)
        {
            try
            {
                // time 값 읽어오기
                var1 = binaryReader.ReadSingle();
                Debug.Log(var1 + " 배열 읽기 전 : " + fs.Seek(0, SeekOrigin.Current));

                // diff가 양수이면 유저의 현재 시간이 더 나중이다. 음수이면 파일의 시간이 더 나중이다.
                float diff = (_deltaTime - var1);
                if(diff>0.2f)
                {
                    // 파일이 너무 예전 시간을 읽고 있기 때문에 다음으로 당긴다.
                    Debug.Log("다음 시간 읽기 : " + fs.Seek(36, SeekOrigin.Current));
                }
                else if(diff<-0.2f)
                {
                    // 파일이 너무 나중 시간을 읽고있기 때문에 이전으로 간다.
                    Debug.Log("이전 시간 읽기 : " + fs.Seek(-44, SeekOrigin.Current));
                }
                else
                {
                    // 정상적인 offset

                    // 리턴으로 buffer에 읽어온 byte의 개수를 리턴한다.
                    int readbytelength = binaryReader.Read(byteBuffer, 0, byteBuffer.Length);
                    Debug.Log("읽은 개수 : " + readbytelength);
                    // float값으로 변환
                    Buffer.BlockCopy(byteBuffer, 0, floatBuffer, 0, byteBuffer.Length);
                    floatResult = floatBuffer;
                    Debug.Log("배열 읽은 후 : " + fs.Seek(0, SeekOrigin.Current));
                    return;
                }

                // 유저 프레임과 시간차가 이전 프레임보다 현재 프레임이 더 작을 때 
                //if (_timeDiff > Mathf.Abs(_deltaTime-var1))
                //{
                //    Debug.Log("Time diff : " + Mathf.Abs(_deltaTime - var1));
                //    // timeDiff 재설정
                //    _timeDiff = Mathf.Abs(_deltaTime - var1);


                //    // 리턴으로 buffer에 읽어온 byte의 개수를 리턴한다.
                //    int readbytelength = binaryReader.Read(byteBuffer, 0, byteBuffer.Length) ;
                //    Debug.Log("읽은 개수 : " + readbytelength);
                //    /*
                //    for (int i = 0; i < byteBuffer.Length; ++i)
                //    {
                //        // Debug.Log(byteBuffer[i]);
                //    }
                //    */
                //    // float값으로 변환
                //    Buffer.BlockCopy(byteBuffer, 0, floatBuffer, 0, byteBuffer.Length);
                //    floatResult = floatBuffer;
                //    /*
                //    for(int idx=0;idx<floatBuffer.Length;idx++)
                //    {
                //        Debug.Log(floatBuffer[idx]);
                //    }
                //    */
                //    Debug.Log("배열 읽은 후 : " + fs.Seek(0, SeekOrigin.Current));
                //}
                //// 현재 프레임이 시간차가 더 커질 때
                //// 이전 프레임에서 읽어온 값을 전달
                //else
                //{
                //    // 현재 프레임 시간값 읽어올 수 있도록 이전으로 돌리기
                //    // 시간 4byte, 데이터 9개 * 4byte = 36byte
                //    //fs.Seek(-10, SeekOrigin.Current);
                //    Debug.LogWarning("현재 : " + fs.Seek(-44, SeekOrigin.Current));
                //    // 읽어온 값 넣어주기.
                //    floatResult = floatBuffer;
                    
                //    return;
                //}


            }
            catch (EndOfStreamException e)
            {
                EOF = true;
                Debug.LogError(e.Message);
                break;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                break;
            }
        }
    }

    

    public bool compareScoreBig(string _filename ,int _score)
    {
        streamReader = new StreamReader(_filename + ".txt");
        string old = streamReader.ReadLine();
        Debug.LogWarning(old);
        streamReader.Close();
        if (Convert.ToInt32(old) < _score)
        {
            streamWriter = new StreamWriter(_filename + ".txt", append: false);
            streamWriter.WriteLine(_score.ToString());
            streamWriter.Close();
            return true;
        }
        else
            return false;
    }

    public void setFileName(string _filename)
    {
        fileName = _filename;
        fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

    }
   
    public void createWriter()
    {
        binaryWriter = new BinaryWriter(fs);
    }
    public void createReader()
    {
        binaryReader = new BinaryReader(fs);
        binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);
    }
    public void deleteWriter()
    {
        binaryWriter.Close();
    }
    public void deleteReader()
    {
        binaryReader.Close();
    }
    public void deleteFilestream()
    {
        fs.Close();
    }
    public bool getEOF()
    {
        return EOF;
    }

}
