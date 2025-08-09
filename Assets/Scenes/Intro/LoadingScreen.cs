using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fadeImage;

    [Header("Settings")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float minimumLoadTime = 2f; // Minimum time to show loading screen

    private void Start()
    {
        // Ensure fade image starts fully transparent
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        // Start loading process
        StartCoroutine(LoadMenuScene());
    }

    private IEnumerator LoadMenuScene()
    {
        float startTime = Time.time;

        // Begin loading the menu scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(menuSceneName);
        asyncLoad.allowSceneActivation = false; // Prevent automatic scene activation

        // Update loading progress
        while (!asyncLoad.isDone)
        {
            // Check if loading is complete (progress reaches 0.9)
            if (asyncLoad.progress >= 0.9f)
            {
                // Ensure minimum loading time has passed
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < minimumLoadTime)
                {
                    yield return new WaitForSeconds(minimumLoadTime - elapsedTime);
                }

                // Start fade out and activate scene
                yield return StartCoroutine(FadeOutAndLoadScene(asyncLoad));
                break;
            }

            yield return null; // Wait for next frame
        }
    }

    private IEnumerator FadeOutAndLoadScene(AsyncOperation asyncLoad)
    {
        // Fade out to black
        if (fadeImage != null)
        {
            float fadeTimer = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

            while (fadeTimer < fadeSpeed)
            {
                fadeTimer += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, fadeTimer / fadeSpeed);

                Color currentColor = fadeImage.color;
                currentColor.a = alpha;
                fadeImage.color = currentColor;

                yield return null;
            }

            // Ensure fade is complete
            fadeImage.color = targetColor;
        }

        // Small delay for effect
        yield return new WaitForSeconds(0.1f);

        // Activate the loaded scene
        asyncLoad.allowSceneActivation = true;
    }
}

// Optional: Loading screen settings component
[System.Serializable]
public class LoadingScreenSettings
{
    [Header("Fade Settings")]
    public Color fadeColor = Color.black;
    public float fadeSpeed = 1f;

    [Header("Timing")]
    public float minimumLoadTime = 2f;
}