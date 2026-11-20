using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

// Supply station shop. Built at runtime (same Start-with-retry
// pattern as TipHUD/StarProgressHUD). Attach to the GameManager in
// each level that has supply stations. Opening pauses the game;
// closing resumes it. Prices are public so they are easy to tune.
public class ShopUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Prices (coins)")]
    public int speedPrice = 10;
    public int scorePrice = 15;
    public int shieldPrice = 15;

    [Header("Style")]
    public Vector2 panelSize = new Vector2(640f, 520f);

    static ShopUI instance;

    Canvas canvas;
    GameObject panel;
    TMP_Text moneyText;
    TMP_Text[] priceTexts = new TMP_Text[3];
    TMP_Text[] boughtTexts = new TMP_Text[3];
    Button[] buyButtons = new Button[3];
    bool uiBuilt;
    int speedBought;
    int scoreBought;

    public static void OpenShop()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<ShopUI>();
            if (instance == null)
            {
                Debug.LogWarning("[Shop] no ShopUI in the scene.");
                return;
            }
        }
        instance.Show();
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        canvas = FindScreenCanvas();
        TryBuild();
    }

    void Update()
    {
        if (!uiBuilt)
        {
            if (canvas == null)
                canvas = FindScreenCanvas();
            TryBuild();
            return;
        }
        if (panel == null)
            return;
        if (IsOpen)
        {
            RefreshMoney();
            RefreshButtons();
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }
    }

    void Show()
    {
        if (panel == null)
            return;
        IsOpen = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
        RefreshMoney();
        RefreshButtons();
    }

    public void Close()
    {
        IsOpen = false;
        if (panel != null)
            panel.SetActive(false);
        Time.timeScale = 1f;
    }

    void RefreshMoney()
    {
        DeliveryManager dm = DeliveryManager.Instance;
        if (moneyText != null)
            moneyText.text = "Coins: " + (dm != null ? dm.Money : 0);
    }

    void RefreshButtons()
    {
        DeliveryManager dm = DeliveryManager.Instance;
        int money = dm != null ? dm.Money : 0;
        int[] prices = { speedPrice, scorePrice, shieldPrice };
        player.CatController cat = FindFirstObjectByType<player.CatController>();
        for (int i = 0; i < 3; i++)
        {
            if (buyButtons[i] == null)
                continue;
            bool affordable = money >= prices[i];
            // Already holding a shield: no need to buy another one.
            if (i == 2 && cat != null && cat.hasShield)
                affordable = false;
            // Speed and score stack, so they stay buyable.
            buyButtons[i].interactable = affordable;
            if (priceTexts[i] != null)
                priceTexts[i].color = affordable ? Color.white : new Color(1f, 0.4f, 0.4f, 1f);
        }
        // Show how many double-score deliveries are left.
        if (boughtTexts[1] != null && dm != null)
            boughtTexts[1].text = dm.doubleScoreDeliveriesLeft > 0
                ? "x2 left: " + dm.doubleScoreDeliveriesLeft
                : "";
    }

    void BuySpeed()
    {
        DeliveryManager dm = DeliveryManager.Instance;
        player.CatController cat = FindFirstObjectByType<player.CatController>();
        if (dm == null || cat == null)
            return;
        if (!dm.SpendMoney(speedPrice))
            return;
        cat.speedMultiplier *= 1.1f;
        speedBought++;
        RefreshMoney();
        RefreshButtons();
    }

    void BuyScore()
    {
        DeliveryManager dm = DeliveryManager.Instance;
        if (dm == null)
            return;
        if (!dm.SpendMoney(scorePrice))
            return;
        dm.GrantDoubleScore(2);
        scoreBought++;
        RefreshMoney();
        RefreshButtons();
    }

    void BuyShield()
    {
        DeliveryManager dm = DeliveryManager.Instance;
        player.CatController cat = FindFirstObjectByType<player.CatController>();
        if (dm == null || cat == null)
            return;
        if (cat.hasShield)
            return; // already holding one; spend it first
        if (!dm.SpendMoney(shieldPrice))
            return;
        cat.hasShield = true;
        RefreshMoney();
        RefreshButtons();
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

    void TryBuild()
    {
        if (uiBuilt || canvas == null)
            return;
        try
        {
            BuildUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[ShopUI] failed to build: " + e.Message);
            if (panel != null) Destroy(panel);
            panel = null;
        }
        uiBuilt = true;
    }

    void BuildUI()
    {
        TMP_FontAsset font = FindFont();
        // Dim backdrop + centred panel.
        panel = new GameObject("ShopPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        Image backdrop = panel.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.45f);

        GameObject card = new GameObject("Card");
        card.transform.SetParent(panel.transform, false);
        RectTransform cardRt = card.AddComponent<RectTransform>();
        cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = panelSize;
        Image cardImg = card.AddComponent<Image>();
        cardImg.color = new Color(0.98f, 0.95f, 0.88f, 1f);

        // Title.
        TMP_Text title = MakeText(card, "Title", "SUPPLY STATION", 40, FontStyles.Bold,
            new Color(0.25f, 0.2f, 0.15f, 1f), TextAlignmentOptions.Center, font);
        RectTransform titleRt = title.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -24f);
        titleRt.sizeDelta = new Vector2(panelSize.x - 40f, 56f);

        // Money line.
        moneyText = MakeText(card, "Money", "Coins: 0", 28, FontStyles.Bold,
            new Color(0.6f, 0.45f, 0.1f, 1f), TextAlignmentOptions.Center, font);
        RectTransform moneyRt = moneyText.GetComponent<RectTransform>();
        moneyRt.anchorMin = new Vector2(0.5f, 1f);
        moneyRt.anchorMax = new Vector2(0.5f, 1f);
        moneyRt.pivot = new Vector2(0.5f, 1f);
        moneyRt.anchoredPosition = new Vector2(0f, -84f);
        moneyRt.sizeDelta = new Vector2(panelSize.x - 40f, 40f);

        // Three upgrade rows.
        string[] names = { "SPEED +10%", "DOUBLE SCORE", "SHIELD" };
        string[] descs = { "Move 10% faster", "Double points on next 2 deliveries", "Block one hit" };
        int[] prices = { speedPrice, scorePrice, shieldPrice };
        System.Action[] buys = { BuySpeed, BuyScore, BuyShield };
        float rowTop = -140f;
        float rowHeight = 92f;
        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            GameObject row = new GameObject("Row" + idx);
            row.transform.SetParent(card.transform, false);
            RectTransform rowRt = row.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(1f, 1f);
            rowRt.pivot = new Vector2(0.5f, 1f);
            rowRt.anchoredPosition = new Vector2(0f, rowTop - idx * rowHeight);
            rowRt.sizeDelta = new Vector2(0f, rowHeight - 12f);

            TMP_Text nameText = MakeText(row, "Name", names[idx], 26, FontStyles.Bold,
                new Color(0.2f, 0.2f, 0.2f, 1f), TextAlignmentOptions.Left, font);
            RectTransform nameRt = nameText.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 0.5f);
            nameRt.anchorMax = new Vector2(0f, 0.5f);
            nameRt.pivot = new Vector2(0f, 0.5f);
            nameRt.anchoredPosition = new Vector2(28f, 14f);
            nameRt.sizeDelta = new Vector2(220f, 36f);

            TMP_Text descText = MakeText(row, "Desc", descs[idx], 20, FontStyles.Normal,
                new Color(0.4f, 0.4f, 0.4f, 1f), TextAlignmentOptions.Left, font);
            RectTransform descRt = descText.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0.5f);
            descRt.anchorMax = new Vector2(0f, 0.5f);
            descRt.pivot = new Vector2(0f, 0.5f);
            descRt.anchoredPosition = new Vector2(28f, -18f);
            descRt.sizeDelta = new Vector2(280f, 28f);

            // Price label.
            priceTexts[idx] = MakeText(row, "Price", prices[idx] + " coins", 24, FontStyles.Bold,
                Color.white, TextAlignmentOptions.Center, font);
            RectTransform priceRt = priceTexts[idx].GetComponent<RectTransform>();
            priceRt.anchorMin = new Vector2(1f, 0.5f);
            priceRt.anchorMax = new Vector2(1f, 0.5f);
            priceRt.pivot = new Vector2(1f, 0.5f);
            priceRt.anchoredPosition = new Vector2(-160f, 0f);
            priceRt.sizeDelta = new Vector2(130f, 36f);

            // Buy button.
            GameObject btnGo = new GameObject("BuyButton");
            btnGo.transform.SetParent(row.transform, false);
            RectTransform btnRt = btnGo.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1f, 0.5f);
            btnRt.anchorMax = new Vector2(1f, 0.5f);
            btnRt.pivot = new Vector2(1f, 0.5f);
            btnRt.anchoredPosition = new Vector2(-24f, 0f);
            btnRt.sizeDelta = new Vector2(110f, 52f);
            Image btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.35f, 0.7f, 0.35f, 1f);
            Button btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => buys[idx]());
            buyButtons[idx] = btn;
            TMP_Text btnText = MakeText(btnGo, "Text", "BUY", 24, FontStyles.Bold,
                Color.white, TextAlignmentOptions.Center, font);
            RectTransform btnTextRt = btnText.GetComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.offsetMin = Vector2.zero;
            btnTextRt.offsetMax = Vector2.zero;

            boughtTexts[idx] = MakeText(row, "Bought", "", 20, FontStyles.Normal,
                new Color(0.4f, 0.4f, 0.4f, 1f), TextAlignmentOptions.Left, font);
        }

        // Close button.
        GameObject closeGo = new GameObject("CloseButton");
        closeGo.transform.SetParent(card.transform, false);
        RectTransform closeRt = closeGo.AddComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.5f, 0f);
        closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.pivot = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0f, 24f);
        closeRt.sizeDelta = new Vector2(200f, 56f);
        Image closeImg = closeGo.AddComponent<Image>();
        closeImg.color = new Color(0.85f, 0.85f, 0.8f, 1f);
        Button closeBtn = closeGo.AddComponent<Button>();
        closeBtn.onClick.AddListener(Close);
        TMP_Text closeText = MakeText(closeGo, "Text", "CLOSE", 26, FontStyles.Bold,
            new Color(0.25f, 0.2f, 0.15f, 1f), TextAlignmentOptions.Center, font);
        RectTransform closeTextRt = closeText.GetComponent<RectTransform>();
        closeTextRt.anchorMin = Vector2.zero;
        closeTextRt.anchorMax = Vector2.one;
        closeTextRt.offsetMin = Vector2.zero;
        closeTextRt.offsetMax = Vector2.zero;

        panel.SetActive(false);
    }

    TMP_Text MakeText(GameObject parent, string name, string text, int size,
        FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        TMP_Text t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = align;
        t.enableWordWrapping = false;
        t.raycastTarget = false;
        if (font != null)
            t.font = font;
        return t;
    }

    TMP_FontAsset FindFont()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.finalScoreText != null && gm.finalScoreText.font != null)
            return gm.finalScoreText.font;
        foreach (TextMeshProUGUI t in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (t != null && t.font != null)
                return t.font;
        }
        return null;
    }
}
