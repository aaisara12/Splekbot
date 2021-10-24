using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimArrow : MonoBehaviour
{
    SectorAim sectorAim;
    CharacterStateMachine stateMachine;
    [SerializeField] SpriteRenderer arrowSpriteRenderer;
    [SerializeField] Transform arrowScalePoint;         // The point from which the arrow should grow
    Color invisible;

    void Awake()
    {
        sectorAim = GetComponentInParent<SectorAim>();
        stateMachine = GetComponentInParent<CharacterStateMachine>();

        if(sectorAim == null)
        {
            Debug.LogError(gameObject.name + " does not have a parent with a SectorAim component.");
            return;
        }
        if(stateMachine == null)
        {
            Debug.LogError(gameObject.name + " does not have a parent with a CharacterStateMachine component.");
            return;
        }

        sectorAim.OnChangeDirection += MatchDirection;

        if(arrowSpriteRenderer == null)
        {
            Debug.LogError(gameObject.name + " does not have an assigned SpriteRenderer component.");
            return;
        }

        invisible = new Color(1, 1, 1, 0);
        arrowSpriteRenderer.color = invisible;

        stateMachine.OnStartCharge += HandleStartCharge;
        stateMachine.OnEndCharge += HandleEndCharge;
        stateMachine.OnNewPercentCharge += HandleNewPercentCharge;
    }

    void MatchDirection(Vector2 direction)
    {
        float angle = Vector2.SignedAngle(direction, Vector2.up);
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    bool isInTransition;        // Is another fade currently on-going?
    void HandleStartCharge() => StartCoroutine(FadeIn());
    void HandleEndCharge() => StartCoroutine(FadeOut());

    IEnumerator FadeIn()
    {
        while(isInTransition)
            yield return new WaitForSeconds(0.1f);      // Wait a little bit before trying to fade again 

        isInTransition = true;

        

        float currentAlpha = 0;
        while(arrowSpriteRenderer.color.a < 0.8)
        {
            Color c = arrowSpriteRenderer.color;
            currentAlpha = Mathf.Lerp(currentAlpha, 1, 0.2f);
            arrowSpriteRenderer.color = new Color(c.r, c.b, c.g, currentAlpha);
            yield return new WaitForSeconds(0.01f);
        }

        isInTransition = false;
    }

    IEnumerator FadeOut()
    {
        while(isInTransition)
            yield return new WaitForSeconds(0.1f);      // Wait a little bit before trying to fade again 

        isInTransition = true;

        

        float currentAlpha = 0.8f;
        while(arrowSpriteRenderer.color.a > 0.01)
        {
            Color c = arrowSpriteRenderer.color;
            currentAlpha = Mathf.Lerp(currentAlpha, 0, 0.2f);
            arrowSpriteRenderer.color = new Color(c.r, c.b, c.g, currentAlpha);
            yield return new WaitForSeconds(0.01f);
        }

        isInTransition = false;
    }



     void HandleNewPercentCharge(float newPercent)
     {
         newPercent /= 100;
         Color c = arrowSpriteRenderer.color;
         arrowSpriteRenderer.color = Color.Lerp(new Color(1, 1, 1, c.a), new Color(1, 0, 0, c.a), newPercent);
         arrowScalePoint.localScale = new Vector3(1, 1, newPercent * 4);
     }   
}
