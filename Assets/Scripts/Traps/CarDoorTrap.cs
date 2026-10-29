using UnityEngine;

// Parked car: when the player walks past the door side, the door swings
// open and knocks the player back a short distance.
public class CarDoorTrap : MonoBehaviour
{
    public Transform door;               // door sprite object
    public float knockbackDistance = 1.2f;
    public float cooldown = 3f;          // seconds before it can trigger again
    public float openTime = 0.2f;        // door swing-out time
    public float holdTime = 0.8f;        // door stays open
    public float closeTime = 0.3f;       // door swing-back time

    private Vector3 doorClosedPos;
    private Quaternion doorClosedRot;
    private float cooldownTimer = 0f;
    private float animTimer = -1f;       // < 0 means idle

    void Start()
    {
        if (door != null)
        {
            doorClosedPos = door.localPosition;
            doorClosedRot = door.localRotation;
        }
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
        if (animTimer < 0f || door == null)
            return;
        animTimer += Time.deltaTime;
        float t = animTimer;
        if (t < openTime)
        {
            float k = t / openTime;
            door.localPosition = doorClosedPos + new Vector3(0.15f * k, -0.14f * k, 0f);
            door.localRotation = doorClosedRot * Quaternion.Euler(0f, 0f, -18f * k);
        }
        else if (t < openTime + holdTime)
        {
            door.localPosition = doorClosedPos + new Vector3(0.15f, -0.14f, 0f);
            door.localRotation = doorClosedRot * Quaternion.Euler(0f, 0f, -18f);
        }
        else if (t < openTime + holdTime + closeTime)
        {
            float k = 1f - (t - openTime - holdTime) / closeTime;
            door.localPosition = doorClosedPos + new Vector3(0.15f * k, -0.14f * k, 0f);
            door.localRotation = doorClosedRot * Quaternion.Euler(0f, 0f, -18f * k);
        }
        else
        {
            door.localPosition = doorClosedPos;
            door.localRotation = doorClosedRot;
            animTimer = -1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (cooldownTimer > 0f)
            return;
        player.CatController cat = other.GetComponent<player.CatController>();
        if (cat == null)
            return;
        Vector2 dir = (Vector2)cat.transform.position - (Vector2)transform.position;
        cat.ApplyKnockback(dir, knockbackDistance);
        animTimer = 0f;
        cooldownTimer = cooldown;
    }
}
