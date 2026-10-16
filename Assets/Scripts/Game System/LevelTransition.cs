using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    private bool isTransitioning = false;

    public void LoadLevel2()
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeToLevel2());
        }
    }

    IEnumerator FadeToLevel2()
    {
        isTransitioning = true;

        fadePanel.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = 1f;

        SceneManager.LoadScene("level_2");
    }
}