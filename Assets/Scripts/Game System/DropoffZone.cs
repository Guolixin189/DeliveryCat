using UnityEngine;

public class DropoffZone : MonoBehaviour
{
    public int houseIndex;         
    public GameObject indicator;   
    public GameObject[] puffs;   
    public float bobSpeed = 2f;     
    public float bobHeight = 0.15f;  
    public float puffInterval = 0.35f; 

    private Vector3 basePos;
    private float timer;

    void Start()
    {
        if (indicator != null)
        {
            basePos = indicator.transform.position;
            indicator.SetActive(false);
        }
    }

    void Update()
    {
        if (indicator == null || !indicator.activeSelf)
            return;
        indicator.transform.position =
            basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;


        if (puffs == null || puffs.Length == 0)
            return;
        timer += Time.deltaTime;
        int n = Mathf.FloorToInt(timer / puffInterval) % (puffs.Length + 1);
        for (int i = 0; i < puffs.Length; i++)
            if (puffs[i] != null)
                puffs[i].SetActive(i < n);
    }

    public void SetIndicator(bool on)
    {
        if (indicator == null)
            return;
        indicator.SetActive(on);
        if (on)
        {
            timer = 0f;
            if (puffs != null)
                foreach (var p in puffs)
                    if (p != null)
                        p.SetActive(false);
        }
    }
}
