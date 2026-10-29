using UnityEngine;

// Boost pad: stepping on it gives a short speed boost. The three chevrons
// light up in sequence (flowing forward) while the pad is on screen.
public class BoostPad : MonoBehaviour
{
    public SpriteRenderer[] chevrons; // left to right
    public float cycleTime = 0.6f;    // one full flow cycle in seconds

    private static readonly Color bright = new Color(0.96f, 0.84f, 0.48f, 1f);
    private static readonly Color dim = new Color(0.60f, 0.60f, 0.56f, 1f);

    void Update()
    {
        if (chevrons == null || chevrons.Length == 0)
            return;
        float phase = (Time.time % cycleTime) / cycleTime;
        for (int i = 0; i < chevrons.Length; i++)
        {
            if (chevrons[i] == null)
                continue;
            float local = (phase * chevrons.Length - i + chevrons.Length) % chevrons.Length;
            float k = Mathf.Clamp01(1f - local);
            chevrons[i].color = Color.Lerp(dim, bright, k);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat != null)
            cat.ApplyBoost();
    }
}
