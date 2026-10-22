using UnityEngine;
using TMPro;
using player;

public class SignatureLabel : MonoBehaviour
{
    private TMP_Text label;
    private CatController cat;

    void Awake()
    {
        label = GetComponentInChildren<TMP_Text>(true);
        cat = FindFirstObjectByType<CatController>();
        if (label != null)
            label.gameObject.SetActive(false);
    }

    void Update()
    {
        bool show = cat != null && cat.IsSignatureChanneling;
        if (label != null && label.gameObject.activeSelf != show)
            label.gameObject.SetActive(show);
        if (show && label != null)
            label.text = "Signing... " + Mathf.RoundToInt(cat.SignatureProgress * 100f) + "%";
    }
}
