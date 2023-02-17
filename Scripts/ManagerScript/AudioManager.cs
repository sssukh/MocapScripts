using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public struct Sounds
    {
        public string name;
        public AudioClip audio;
    }


   
    public Sounds [] Audios;
    public Sounds[] EffectSound;

    [SerializeField]
    private AudioSource audioSource = null;

    // Start is called before the first frame update
    private void Awake()
    {
        DontDestroyOnLoad(this);

    }
    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }

    public void MusicPlay()
    {
        audioSource.Play();
    }
    public void MusicStop()
    {
        audioSource.Pause();
    }
    public void SetMusic(int idx)
    {
        audioSource.clip = Audios[idx].audio;
    }
    public void printState()
    {
        Debug.Log(audioSource.clip.loadState);
    }
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}
