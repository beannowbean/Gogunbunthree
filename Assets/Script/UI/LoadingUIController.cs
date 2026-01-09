using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Image loadingImage;
    [SerializeField] private float textChangeInterval = 0.5f;
    [SerializeField] private float rotationSpeed = 200f;
    private string[] loadingTexts = new string[] { "Loading.", "Loading..", "Loading..." };
    private int currentTextIndex = 0;

    private void Start()
    {
        StartCoroutine(UpdateLoadingText());
    }

    private void Update()
    {
        if (loadingImage != null)
        {
            loadingImage.transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            if (loadingText != null)
            {
                loadingText.text = loadingTexts[currentTextIndex];
                currentTextIndex = (currentTextIndex + 1) % loadingTexts.Length;
            }
            yield return new WaitForSeconds(textChangeInterval);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}