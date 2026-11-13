using UnityEngine;
using UnityEngine.UI;
using TMPro;

// How To Play hub: the How To Play page shows three buttons, each
// opening its own panel:
//   1. BASIC CONTROLS - the original rules (goal, move, pick up /
//      deliver, avoid NPCs) with the keycap images.
//   2. GAME MECHANICS - companies, combo, tip orders, perfect
//      delivery, stars.
//   3. TRAPS - a short explanation of every trap.
// The controls content is moved (reparented) from the How To Play
// panel into its own panel at runtime, so the scene text and keycap
// images stay exactly as the team made them. Builds all new UI at
// runtime. Attach to the MainMenu GameObject in the Main Menu scene
// (it is always active, unlike the How To Play panel itself).
public class HowToPlayExtra : MonoBehaviour
{
    [Header("Style")]
    public Color panelColor = new Color(0.798f, 0.953f, 0.652f, 1f);
    public Color buttonColor = new Color(0.98f, 0.93f, 0.78f, 1f);
    public Color titleColor = new Color(0.02f, 0.03f, 0.02f, 1f);
    public Color bodyColor = new Color(0.1f, 0.12f, 0.08f, 1f);
    public int titleFontSize = 60;
    public int bodyFontSize = 34;
    public int buttonFontSize = 36;
    public int hubButtonFontSize = 40;

    Canvas canvas;
    GameObject howToPlayPanel;
    GameObject controlsPanel;
    GameObject mechanicsPanel;
    GameObject trapsPanel;

    // The basic-controls content that lives on the How To Play panel
    // in the scene; moved into the controls panel at runtime.
    static readonly string[] controlsChildren =
        { "Goal", "Move", "Action", "Warning", "WASDKeys", "SpacebarKey" };

    void Start()
    {
        canvas = FindScreenCanvas();
        howToPlayPanel = GameObject.Find("HowtoPlayPanel");
        if (canvas == null || howToPlayPanel == null)
        {
            Debug.LogWarning("[HowToPlayExtra] Canvas or HowtoPlayPanel not found.");
            return;
        }
        try
        {
            controlsPanel = BuildControlsPanel();
            mechanicsPanel = BuildInfoPanel("MechanicsPanel", "GAME MECHANICS", MechanicsText());
            trapsPanel = BuildInfoPanel("TrapsPanel", "TRAPS", TrapsText());
            BuildHubButtons();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[HowToPlayExtra] failed to build: " + e.Message);
        }
    }

    // The hub: three big buttons stacked in the middle of the
    // How To Play page. The page keeps its title and Back button.
    void BuildHubButtons()
    {
        MakePanelButton(howToPlayPanel.transform, "ControlsButton", "BASIC CONTROLS",
            new Vector2(0f, 150f), new Vector2(480f, 80f), hubButtonFontSize,
            () => ShowPanel(controlsPanel));
        MakePanelButton(howToPlayPanel.transform, "MechanicsButton", "GAME MECHANICS",
            new Vector2(0f, 0f), new Vector2(480f, 80f), hubButtonFontSize,
            () => ShowPanel(mechanicsPanel));
        MakePanelButton(howToPlayPanel.transform, "TrapsButton", "TRAPS",
            new Vector2(0f, -150f), new Vector2(480f, 80f), hubButtonFontSize,
            () => ShowPanel(trapsPanel));
    }

    // The controls panel reuses the scene's own rules content: move
    // the Goal / Move / Action / Warning texts and the keycap images
    // from the How To Play panel into this panel. Both panels are the
    // same size and centred, so the layout is unchanged.
    GameObject BuildControlsPanel()
    {
        GameObject panel = NewPanel("ControlsPanel");
        TMP_Text titleText = MakeText(panel.transform, "Title", "BASIC CONTROLS",
            titleFontSize, titleColor, new Vector2(0f, 353f), new Vector2(1000f, 100f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        MakePanelButton(panel.transform, "BackButton", "BACK",
            new Vector2(0f, -341f), new Vector2(260f, 70f), buttonFontSize,
            () => ShowHowToPlay());
        foreach (string name in controlsChildren)
        {
            Transform child = howToPlayPanel.transform.Find(name);
            if (child != null)
                child.SetParent(panel.transform, false);
            else
                Debug.LogWarning("[HowToPlayExtra] controls child not found: " + name);
        }
        panel.SetActive(false);
        return panel;
    }

    void ShowPanel(GameObject panel)
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
        if (panel != null)
            panel.SetActive(true);
    }

    void ShowHowToPlay()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        if (mechanicsPanel != null)
            mechanicsPanel.SetActive(false);
        if (trapsPanel != null)
            trapsPanel.SetActive(false);
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    GameObject NewPanel(string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(1200f, 1000f);
        Image bg = panel.AddComponent<Image>();
        bg.sprite = MakeWhiteSprite();
        bg.color = panelColor;
        bg.raycastTarget = true;
        return panel;
    }

    GameObject BuildInfoPanel(string name, string title, string body)
    {
        GameObject panel = NewPanel(name);
        TMP_Text titleText = MakeText(panel.transform, "Title", title,
            titleFontSize, titleColor, new Vector2(0f, 353f), new Vector2(1000f, 100f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;

        TMP_Text bodyText = MakeText(panel.transform, "Body", body,
            bodyFontSize, bodyColor, new Vector2(0f, -30f), new Vector2(1020f, 620f));
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.enableWordWrapping = true;

        MakePanelButton(panel.transform, "BackButton", "BACK",
            new Vector2(0f, -341f), new Vector2(260f, 70f), buttonFontSize,
            () => ShowHowToPlay());

        panel.SetActive(false);
        return panel;
    }

    void MakePanelButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, int fontSize, System.Action onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.sprite = MakeWhiteSprite();
        img.color = buttonColor;
        img.raycastTarget = true;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick());
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;

        TMP_Text text = MakeText(go.transform, "Text (TMP)", label,
            fontSize, titleColor, Vector2.zero, size);
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
    }

    TMP_Text MakeText(Transform parent, string name, string content,
        int fontSize, Color color, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        TMP_Text t = go.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        TMP_FontAsset font = FindFont(t);
        if (font != null)
            t.font = font;
        return t;
    }

    TMP_FontAsset FindFont(TMP_Text exclude)
    {
        foreach (TextMeshProUGUI t in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (t != null && t != exclude && t.font != null)
                return t.font;
        }
        return null;
    }

    Canvas FindScreenCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
        {
            if (c != null && c.isRootCanvas && c.renderMode != RenderMode.WorldSpace)
                return c;
        }
        return null;
    }

    Sprite MakeWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }

    string MechanicsText()
    {
        return
            "COMPANIES (official level)\n" +
            "UPS: Hold SPACE at the right door for 3s to sign. +200. Wrong house -100.\n" +
            "USPS: Drop off at the target house, it delivers by itself. Wrong house -100.\n" +
            "FedEx: 15s per order. +200 on time, +100 late, -100 wrong.\n" +
            "Freelancer: Classic. +100 right, -100 wrong.\n" +
            "\n" +
            "COMBO\n" +
            "Deliver correctly in a row to multiply points.\n" +
            "x1.5 at 2 in a row, x2 at 3 or more.\n" +
            "\n" +
            "TIP ORDER\n" +
            "A gold coin marks a tip order. Deliver within 20s for +50.\n" +
            "\n" +
            "PERFECT DELIVERY\n" +
            "No pedestrian or trap hit in one order for +50.\n" +
            "\n" +
            "STARS\n" +
            "1 star at 1 delivery, 2 stars at 3, 3 stars at 5.";
    }

    string TrapsText()
    {
        return
            "Watch out for traps on the road!\n" +
            "\n" +
            "PUDDLE: Slippery! Your steering wobbles.\n" +
            "MUD: Slows you down.\n" +
            "BANANA PEEL: Knocks you back and stuns you for 1s.\n" +
            "CAR DOOR: Swings open and knocks you back.\n" +
            "SPRINKLER: A rotating jet of water slows you down.\n" +
            "BARRIER GATE: Blocks the road when the light is red.\n" +
            "BOX PILE: Pushes you around.\n" +
            "\n" +
            "BOOST PAD: Speeds you up! This one helps you.";
    }
}
