using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    private bool fadeOut, fadeIn;
    public float fadeSpeed;

    // Update is called once per frame
    void Update()
    {
        if (fadeOut)
        {
            Color objectColor = this.GetComponent<Renderer>().material.color;
            float fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            this.GetComponent<Renderer>().material.color = objectColor;
            if (objectColor.a <= 0)
                fadeOut = false;
        }
        if (fadeIn)
        {
            Color objectColor = this.GetComponent<Renderer>().material.color;
            float fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            this.GetComponent<Renderer>().material.color = objectColor;
            if (objectColor.a >= 1)
                fadeIn = false;
        }
    }

    public void FadeOutObject()
    {
        if (fadeIn)
            return;
        fadeOut = true;
    }

    public void FadeInObject()
    {
        if (fadeOut)
            return;
        fadeIn = true;
    }
}
