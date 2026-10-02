using UnityEngine;

public class PickupZone : MonoBehaviour
{
    public GameObject indicator;  
    public float bobSpeed = 2f;  
    public float bobHeight = 0.15f;

    private Vector3 basePos;

    void Start()
    {
        if (indicator != null)
        {
            basePos = indicator.transform.position;
            indicator.SetActive(true);
        }
    }

    void Update()
    {
        if (indicator == null || !indicator.activeSelf)
            return;

        indicator.transform.position =
            basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    }

    public void SetIndicator(bool on)
    {
        if (indicator != null)
            indicator.SetActive(on);
    }
}
