using UnityEngine;
using player;

public class SignatureBar : MonoBehaviour
{
    private Transform bar;
    private RectTransform fill;
    private float fullWidth = 0f;
    private CatController cat;

    void Awake()
    {
        cat = FindFirstObjectByType<CatController>();
        bar = transform.Find("Bar");
        var f = transform.Find("Bar/Fill");
        if (f != null)
        {
            fill = f as RectTransform;
            fullWidth = fill.sizeDelta.x;
        }
        if (bar != null)
            bar.gameObject.SetActive(false);
    }

    void Update()
    {
        bool show = cat != null && cat.IsSignatureChanneling;
        if (bar != null && bar.gameObject.activeSelf != show)
            bar.gameObject.SetActive(show);
        if (show && fill != null)
            fill.sizeDelta = new Vector2(Mathf.Max(1f, fullWidth * cat.SignatureProgress), fill.sizeDelta.y);
    }
}
