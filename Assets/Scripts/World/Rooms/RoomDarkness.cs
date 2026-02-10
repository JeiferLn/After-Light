using System.Collections;
using UnityEngine;

public class RoomDarkness : MonoBehaviour
{
    // ------------- VISUALS -------------
    SpriteRenderer sr;

    // ------------- VARIABLES -------------
    public bool isRevealed;

    // ------------- INITIALIZATION -------------
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // ------------- PUBLIC METHODS -------------
    public void RevealRoom()
    {
        StartCoroutine(FadeOut());
        isRevealed = true;
    }

    IEnumerator FadeOut()
    {
        float t = 0.1f;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            sr.color = new Color(0, 0, 0, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
