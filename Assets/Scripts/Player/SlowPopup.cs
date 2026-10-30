using TMPro;
using UnityEngine;

public class SlowPopup : MonoBehaviour
{
    public TMP_Text label;
    public Vector3 offset = new Vector3(0f, 1.05f, 0f);
    public float fadeDuration = 0.45f;
    public float bounceHeight = 0.28f;
    public float bounceTime = 0.5f;

    Transform target;
    float holdTimer;
    float fadeTimer;
    float bounceTimer;
    bool showing;

    void Awake()
    {
        if (label == null)
            label = GetComponent<TMP_Text>();
        if (label != null)
        {
            // The shared font material tints every letter dark red. Use a copy so only this popup is white.
            Material mat = label.fontMaterial;
            mat.SetColor("_FaceColor", Color.white);
            label.fontMaterial = mat;
        }
        SetAlpha(0f);
    }

    void LateUpdate()
    {
        float bounce = 0f;
        if (showing)
        {
            bounceTimer += Time.deltaTime;
            float t = Mathf.Clamp01(bounceTimer / bounceTime);
            // One up-and-down hop that settles as the text appears
            bounce = Mathf.Sin(t * Mathf.PI * 2f) * bounceHeight * (1f - t);

            if (holdTimer > 0f)
            {
                holdTimer -= Time.deltaTime;
                SetAlpha(1f);
            }
            else
            {
                fadeTimer += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(fadeTimer / fadeDuration);
                SetAlpha(alpha);
                if (alpha <= 0f)
                    showing = false;
            }
        }

        if (target == null)
        {
            player.CatController cat = FindFirstObjectByType<player.CatController>();
            if (cat != null)
                target = cat.transform;
        }
        if (target != null)
            transform.position = target.position + offset + Vector3.up * bounce;
    }

    public void Show(float seconds)
    {
        if (label == null)
            return;
        label.text = $"Slow down +{seconds:0.#}s";
        label.ForceMeshUpdate();
        holdTimer = seconds;
        fadeTimer = 0f;
        bounceTimer = 0f;
        showing = true;
        SetAlpha(1f);
    }

    void SetAlpha(float alpha)
    {
        if (label == null)
            return;
        label.color = new Color(1f, 1f, 1f, alpha);
    }
}
