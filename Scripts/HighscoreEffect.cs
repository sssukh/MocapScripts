using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreEffect : MonoBehaviour
{
    // Start is called before the first frame update
    Image thisimage;

    private void Awake()
    {
        thisimage = GetComponent<Image>();
    }

    public void makeTransparent()
    {
        Color color = thisimage.color;
        color.a = 0;
        thisimage.color = color;
    }
    public void makeVisible()
    {
        Color color = thisimage.color;
        color.a = 1;
        thisimage.color = color;
    }
}
