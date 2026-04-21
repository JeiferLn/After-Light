using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DataTestingHUD : MonoBehaviour
{
    private TextMeshProUGUI fpsText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
        // Application.targetFrameRate = 100;
    }

    // Update is called once per frame
    void Update()
    {
        fpsText.text = "FPS: " + (int)(1f / Time.deltaTime);
    }
}
