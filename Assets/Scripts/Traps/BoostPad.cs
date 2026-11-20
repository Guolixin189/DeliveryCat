using UnityEngine;

// Boost pad: stepping on it gives a short speed boost. The four arrows
// (up, right, down, left) light up in sequence, clockwise, so the pad
// reads as boosting in every direction instead of just one.
public class BoostPad : MonoBehaviour
{
    public SpriteRenderer[] arrows; // clockwise: up, right, down, left
    public float cycleTime = 0.6f;  // one full flow cycle in seconds

    private static readonly Color bright = new Color(0.96f, 0.84f, 0.48f, 1f);
    private static readonly Color dim = new Color(0.60f, 0.60f, 0.56f, 1f);

    void Update()
    {
        if (arrows == null || arrows.Length == 0)
            return;
        float phase = (Time.time % cycleTime) / cycleTime;
        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i] == null)
                continue;
            float local = (phase * arrows.Length - i + arrows.Length) % arrows.Length;
            float k = Mathf.Clamp01(1f - local);
            arrows[i].color = Color.Lerp(dim, bright, k);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat != null)
            cat.ApplyBoost();
    }
}
