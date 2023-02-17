using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    [SerializeField]
    private GameObject textObj = null;
    private TextMeshProUGUI myText = null;

    [SerializeField]
    private Animator CountAnimator = null;

    [SerializeField]
    private Animator ResultSceneBegin = null;

    [SerializeField]
    private GameObject highScore = null;

    string start = "start";
    // Start is called before the first frame update
    void Start()
    {
        myText = textObj.GetComponent<TextMeshProUGUI>();   
    }

    // Update is called once per frame
    public void StartCount(float _score)
    {
        StartCoroutine(Count(_score,0));
    }

    IEnumerator Count(float target,float current)
    {
        ResultSceneBegin.SetTrigger(start);
        yield return new WaitForSeconds(1.0f);
        CountAnimator.SetTrigger(start);
        // 애니메이션 끝나기 까지 기다리는 시간.
        yield return new WaitForSeconds(0.5f);
        float duration = 0.5f;
        float offset = (target - current) / duration;

        while(current<target)
        {
            current += offset * Time.deltaTime;
            myText.text = ((int)current).ToString();
            yield return new WaitForSeconds(0.001f);
        }
        current = target;
        myText.text = ((int)current).ToString();
    }

    public void getHighBool(bool _isHigh)
    {
        if(_isHigh)
        {
            highScore.GetComponent<HighscoreEffect>().makeVisible();
        }
        else
        {
            highScore.GetComponent<HighscoreEffect>().makeTransparent();
        }
    }
}
