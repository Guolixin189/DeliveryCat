using UnityEngine;
using UnityEngine.UI;

// Level select screen: shows the best stars earned on each level, as a
// row of three small stars to the right of the Tutorial and Official
// buttons. Best stars are saved by GameManager in PlayerPrefs when a
// level ends. Builds all UI at runtime, so no scene setup is needed
// except assigning the two star sprites. Attach to the Canvas object.
public class BestStarsDisplay : MonoBehaviour
{
    [Header("Star Sprites")]
    public Sprite filledStarSprite;
    public Sprite emptyStarSprite;

    [Header("Layout")]
    public float starSize = 36f;
    public float starSpacing = 6f;
    public float gapFromButton = 16f;

    Sprite whiteSprite;

    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        whiteSprite = MakeWhiteSprite();
        ShowBestStars("TutorialButton", "level_1");
        ShowBestStars("OfficialButton", "level_2");
    }

    void ShowBestStars(string buttonName, string sceneName)
    {
        GameObject buttonObj = GameObject.Find(buttonName);
        if (buttonObj == null)
            return;
        RectTransform buttonRt = buttonObj.GetComponent<RectTransform>();
        if (buttonRt == null)
            return;

        // Parent the stars to the button itself, so they sit behind any
        // menu panel (How To Play, Settings, Company) that covers the
        // buttons, instead of floating on top of it.
        int best = GameManager.GetBestStars(sceneName);
        float rowWidth = 3f * starSize + 2f * starSpacing;
        float rowCenterX = buttonRt.sizeDelta.x * 0.5f
            + gapFromButton + rowWidth * 0.5f;

        for (int i = 0; i < 3; i++)
        {
            RectTransform star = MakeRT("BestStar_" + sceneName + "_" + (i + 1), buttonRt);
            star.anchorMin = star.anchorMax = new Vector2(0.5f, 0.5f);
            star.pivot = new Vector2(0.5f, 0.5f);
            float x = rowCenterX + (i - 1) * (starSize + starSpacing);
            star.anchoredPosition = new Vector2(x, 0f);
            star.sizeDelta = new Vector2(starSize, starSize);
            Image img = MakeImage(star, Color.white);
            Sprite filled = filledStarSprite;
            Sprite empty = emptyStarSprite;
            img.sprite = (i < best)
                ? (filled != null ? filled : whiteSprite)
                : (empty != null ? empty : whiteSprite);
        }
    }

    RectTransform MakeRT(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    Image MakeImage(RectTransform rt, Color color)
    {
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = whiteSprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    Sprite MakeWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
}
