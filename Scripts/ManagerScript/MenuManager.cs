using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Animator animator = null;
    [SerializeField]
    private GameObject GameTimer = null;
    [SerializeField]
    private GameObject PauseMenu = null;
    [SerializeField]
    private GameObject MainCam = null;

    private bool isPaused = false;
  
    
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                PauseGame();
            else
                PauseClose();
        }
    }
    public void Close()
    {
        StartCoroutine(CloseAfterDelay());
    }

    public void PauseGame()
    {
        isPaused = true;
        StartCoroutine(PauseAfterDelay());
        MainCam.GetComponent<CameraManager>().MaskOn();
        
        GameTimer.GetComponent<GameTimer>().PauseTime();
       
        //PauseMenu.SetActive(true);
       
    }
    public void PauseClose()
    {
        
        StartCoroutine(CloseAfterDelay());
        GameTimer.GetComponent<GameTimer>().PreparingTime();
        MainCam.GetComponent<CameraManager>().MaskOff();
        isPaused = false;
    }
    private IEnumerator CloseAfterDelay()
    {
        animator.SetTrigger("Close");
        yield return new WaitForSeconds(0.5f);
        //gameObject.SetActive(false);
        animator.ResetTrigger("Close");
    }
    private IEnumerator PauseAfterDelay()
    {
        animator.SetTrigger("Pause");
        yield return new WaitForSeconds(0.5f);
        animator.ResetTrigger("Pause");
    }
}
