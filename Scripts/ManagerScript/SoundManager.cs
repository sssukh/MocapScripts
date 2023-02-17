using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public GameObject soundOrigin;
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = soundOrigin.GetComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.mute = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        audioSource.Play();
    }
}
