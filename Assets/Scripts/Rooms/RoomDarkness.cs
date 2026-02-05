using System.Collections;
using UnityEngine;

public class RoomDarkness : MonoBehaviour
{
    // ------------- VISUALS -------------
    SpriteRenderer sr;

    // ------------- INITIALIZATION -------------
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // ------------- PUBLIC METHODS -------------
    public void RevealRoom()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime;
            sr.color = new Color(0, 0, 0, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
