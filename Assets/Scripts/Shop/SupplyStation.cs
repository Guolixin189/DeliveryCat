using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// A supply station built on an existing market stall in the scene.
// A pixel red cross floats and bobs above the stall so the player can
// spot it. Walk close and press Space to open the shop. The shop
// pauses the game while it is open.
public class SupplyStation : MonoBehaviour
{
    [Header("Prompt")]
    public float triggerRadius = 2.2f;   // how close the player must be
    // The stall is solid; the player stands in front of it (below),
    // so the trigger zone is offset down from the stall centre.
    public Vector2 triggerOffset = new Vector2(0f, -2.5f);
    // The cross floats in the stall, shifted a little to the right
    // of the stall centre (the stall is about 4 units tall, centred
    // on this object).
    public float crossHeight = 0.5f;
    public float crossOffsetX = 1.5f;    // how far right of centre
    public float crossBob = 0.25f;       // bob amplitude
    public float crossBobSpeed = 2f;

    SpriteRenderer crossRenderer;
    TextMeshPro promptText;
    Vector3 crossBasePos;
    bool playerNear;
    bool shopOpenHere;

    void Start()
    {
        // Make sure there is a trigger collider even if the scene
        // wiring missed it.
        CircleCollider2D trigger = GetComponent<CircleCollider2D>();
        if (trigger == null)
            trigger = gameObject.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = triggerRadius;
        trigger.offset = triggerOffset;

        BuildCross();
        BuildPrompt();
    }

    void Update()
    {
        if (crossRenderer != null)
        {
            float bob = Mathf.Sin(Time.time * crossBobSpeed) * crossBob;
            crossRenderer.transform.position = crossBasePos + Vector3.up * bob;
        }
        if (promptText != null)
            promptText.gameObject.SetActive(playerNear && !ShopUI.IsOpen);
        // Open the shop on Space while the player stands close.
        // The game is paused while the shop is open, and CatController
        // ignores Space then, so this cannot double-fire a delivery.
        if (playerNear && !ShopUI.IsOpen && Keyboard.current != null
            && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ShopUI.OpenShop();
            shopOpenHere = true;
        }
        if (shopOpenHere && !ShopUI.IsOpen)
            shopOpenHere = false;
    }

    void BuildCross()
    {
        // Prefer the SupplyCross child placed in the scene (it shows
        // in the Hierarchy and can be dragged in the editor). Fall
        // back to creating one at runtime if the scene has none.
        Transform child = transform.Find("SupplyCross");
        GameObject go;
        if (child != null)
        {
            go = child.gameObject;
            crossRenderer = go.GetComponent<SpriteRenderer>();
            if (crossRenderer == null)
                crossRenderer = go.AddComponent<SpriteRenderer>();
        }
        else
        {
            go = new GameObject("SupplyCross");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(crossOffsetX, crossHeight, 0f);
            crossRenderer = go.AddComponent<SpriteRenderer>();
        }
        // The scene child already has the supply_cross sprite; only
        // generate one at runtime when it is missing.
        if (crossRenderer.sprite == null)
            crossRenderer.sprite = CreateRedCrossSprite();
        crossRenderer.sortingOrder = 10;
        crossBasePos = go.transform.position;
    }

    void BuildPrompt()
    {
        GameObject go = new GameObject("ShopPrompt");
        go.transform.SetParent(transform, false);
        // The prompt sits just in front of the stall, near the player,
        // so it does not cover the cross in the middle of the stall.
        go.transform.localPosition = new Vector3(0f, -2.2f, 0f);
        promptText = go.AddComponent<TextMeshPro>();
        promptText.text = "Press SPACE";
        promptText.fontSize = 1f;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        promptText.outlineWidth = 0.2f;
        promptText.outlineColor = Color.black;
        promptText.sortingOrder = 10;
        TMP_FontAsset font = FindFont();
        if (font != null)
            promptText.font = font;
        go.SetActive(false);
    }

    TMP_FontAsset FindFont()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.finalScoreText != null && gm.finalScoreText.font != null)
            return gm.finalScoreText.font;
        return null;
    }

    // Pixel red cross with a white outline, drawn at runtime so no
    // new texture asset (and no hand-written .meta) is needed.
    public static Sprite CreateRedCrossSprite()
    {
        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0f, 0f, 0f, 0f);
        Color red = new Color(0.9f, 0.12f, 0.15f, 1f);
        Color white = Color.white;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inV = x >= 13 && x <= 18 && y >= 4 && y <= 27;
                bool inH = x >= 4 && x <= 27 && y >= 13 && y <= 18;
                bool inCross = inV || inH;
                // 1px white outline around the cross.
                bool nearCross = false;
                for (int dy = -1; dy <= 1 && !nearCross; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        bool nv = nx >= 13 && nx <= 18 && ny >= 4 && ny <= 27;
                        bool nh = nx >= 4 && nx <= 27 && ny >= 13 && ny <= 18;
                        if (nv || nh) { nearCross = true; break; }
                    }
                }
                tex.SetPixel(x, y, inCross ? red : (nearCross ? white : clear));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 32f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<player.CatController>() != null)
            playerNear = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<player.CatController>() != null)
            playerNear = false;
    }
}
