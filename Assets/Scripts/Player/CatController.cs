using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public class CatController : MonoBehaviour
    {
        public float moveSpeed = 3f; 
        [Header("Slow down on pedestrian hit")]
        public float slowFactor = 0.4f;     // speed multiplier while slowed
        public float slowDuration = 2f;     // slow duration in seconds
        public SlowPopup slowPopup;
        private float slowTimer = 0f;       // remaining slow time
        [Header("Trap effects")]
        public float mudFactor = 0.5f;          // speed multiplier in mud
        public float mudLingerDuration = 1f;    // muddy slow after leaving mud
        public float boostFactor = 1.5f;        // speed multiplier on boost pad
        public float boostDuration = 2f;        // boost duration in seconds
        private float mudTimer = 0f;
        private float mudLingerTimer = 0f;
        private float boostTimer = 0f;
        private float trapSlowTimer = 0f;       // generic trap slow (sprinkler jet)
        private float trapSlowFactor = 1f;
        private float slipperyTimer = 0f;       // puddle: steering wobble
        private float stunTimer = 0f;           // hard control lock
        private float knockbackTimer = 0f;      // knockback slide
        private Vector2 knockbackDir = Vector2.zero;
        private float knockbackSpeed = 0f;
        private Rigidbody2D rb;
        private Animator animator;
        private Vector2 movement;

        public bool hasPackage = false;
        private bool canPickUp = false;
        private bool canDropOff = false;
        private GameManager gameManager;
        private DeliveryManager deliveryManager;
        private int currentDropOffIndex = -1;
        private Company company = Company.Freelancer;
        private bool channeling = false;
        private float channelTimer = 0f;
        private int channelHouseIndex = -1;
        public AudioSource packageSound;
        public AudioClip collisionSound;
        private AudioSource collisionAudioSource;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            

            gameManager = FindFirstObjectByType<GameManager>();
            deliveryManager = FindFirstObjectByType<DeliveryManager>();
            company = GameSettings.SelectedCompany;

            collisionAudioSource = GetComponent<AudioSource>();
            if (slowPopup == null)
                slowPopup = FindFirstObjectByType<SlowPopup>();
        }

        void Update()
        {
            if (slowTimer > 0f)
                slowTimer -= Time.deltaTime;
            if (mudTimer > 0f)
                mudTimer -= Time.deltaTime;
            if (mudTimer <= 0f && mudLingerTimer > 0f)
                mudLingerTimer -= Time.deltaTime;
            if (boostTimer > 0f)
                boostTimer -= Time.deltaTime;
            if (trapSlowTimer > 0f)
                trapSlowTimer -= Time.deltaTime;
            if (slipperyTimer > 0f)
                slipperyTimer -= Time.deltaTime;
            if (stunTimer > 0f)
                stunTimer -= Time.deltaTime;
            if (knockbackTimer > 0f)
                knockbackTimer -= Time.deltaTime;

            if (channeling)
            {
                if (!canDropOff || !hasPackage || currentDropOffIndex != channelHouseIndex
                    || (gameManager != null && gameManager.isGameOver))
                    channeling = false;
                else
                {
                    channelTimer += Time.deltaTime;
                    if (channelTimer >= CompanyRules.SignatureTime(company))
                    {
                        channeling = false;
                        DeliverNow();
                    }
                }
            }

            movement = Vector2.zero;

            
            if (gameManager != null && gameManager.isGameOver)
        {
            movement = Vector2.zero;
            animator.SetFloat("Speed", 0);
            return;
        }

            

            // 1. WASD
            if (Keyboard.current.wKey.isPressed) movement.y += 1;
            if (Keyboard.current.sKey.isPressed) movement.y -= 1;
            if (Keyboard.current.aKey.isPressed) movement.x -= 1;
            if (Keyboard.current.dKey.isPressed) movement.x += 1;

            // Stun locks input; the knockback slide itself is applied in FixedUpdate
            if (stunTimer > 0f)
                movement = Vector2.zero;

            // 2. speed passed onto Animator
            animator.SetFloat("Speed", movement.sqrMagnitude);

            // 3. update Animator only during moving
            if (movement.sqrMagnitude > 0.01f)
            {
                animator.SetFloat("MoveX", movement.x);
                animator.SetFloat("MoveY", movement.y);
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // state 1
                if (canPickUp && !hasPackage)
                {
                    hasPackage = true;
                    animator.SetBool("hasPackage", true);
                    packageSound.Play();
                    Debug.Log("Package Picked Up");
                    if (deliveryManager != null)
                        deliveryManager.OnPackagePickedUp();
                }
                // state 2
                else if (canDropOff && hasPackage)
                {
                    if (company == Company.UPS && deliveryManager != null
                        && deliveryManager.IsTargetHouse(currentDropOffIndex))
                    {
                        channeling = true;
                        channelTimer = 0f;
                        channelHouseIndex = currentDropOffIndex;
                        Debug.Log("Signature channel started");
                    }
                    else
                        DeliverNow();
                }
            }
        }

        void FixedUpdate()
        {
            // Knockback slide overrides input
            if (knockbackTimer > 0f)
            {
                rb.MovePosition(rb.position + knockbackDir * knockbackSpeed * Time.fixedDeltaTime);
                return;
            }
            Vector2 dir = movement.normalized;
            // Puddle: steering wobbles side to side
            if (slipperyTimer > 0f && dir.sqrMagnitude > 0.01f)
            {
                float wobble = Mathf.Sin(Time.time * 9f) * 0.5f;
                float cos = Mathf.Cos(wobble);
                float sin = Mathf.Sin(wobble);
                dir = new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos);
            }
            float speed = moveSpeed;
            if (slowTimer > 0f)
                speed *= slowFactor;
            if (mudTimer > 0f || mudLingerTimer > 0f)
                speed *= mudFactor;
            if (trapSlowTimer > 0f)
                speed *= trapSlowFactor;
            if (boostTimer > 0f)
                speed *= boostFactor;
            rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
        }

        // Called by NPCs on pedestrian hit: slow for slowDuration seconds; re-hitting while slowed refreshes the timer
        public void ApplySlow()
        {
            slowTimer = slowDuration;
            if (slowPopup != null)
                slowPopup.Show(slowDuration);
        }

        // Called by mud traps while the player stays inside; re-entering cancels lingering
        public void ApplyMud()
        {
            mudTimer = 0.2f;
            mudLingerTimer = 0f;
        }

        // Called by mud traps when the player leaves: keeps the slow for a short while
        public void ApplyMudLinger()
        {
            mudLingerTimer = mudLingerDuration;
        }

        public void ApplyBoost()
        {
            boostTimer = boostDuration;
        }

        // Generic trap slow (sprinkler jet); refreshed while the player stays in the jet
        public void ApplyTrapSlow(float factor)
        {
            trapSlowFactor = factor;
            trapSlowTimer = 0.3f;
        }

        // Puddle: steering wobbles while slippery; refreshed while inside
        public void ApplySlippery(float duration)
        {
            slipperyTimer = duration;
        }

        public void ApplyStun(float duration)
        {
            stunTimer = Mathf.Max(stunTimer, duration);
        }

        // Knockback slide: pushes the player along dir for a short time, locking input
        public void ApplyKnockback(Vector2 dir, float distance, float duration = 0.25f)
        {
            if (dir.sqrMagnitude < 0.001f)
                return;
            knockbackDir = dir.normalized;
            knockbackSpeed = distance / duration;
            knockbackTimer = duration;
            stunTimer = Mathf.Max(stunTimer, duration);
        }

        // UPS signature channel state (used by the progress UI)
        public bool IsSignatureChanneling => channeling;
        public float SignatureProgress => channeling
            ? Mathf.Clamp01(channelTimer / CompanyRules.SignatureTime(company)) : 0f;

        private void DeliverNow()
        {
            hasPackage = false;
            animator.SetBool("hasPackage", false);
            if (deliveryManager != null)
                deliveryManager.OnPackageDelivered(currentDropOffIndex);
            packageSound.Play();
            Debug.Log("Package Delivered");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("PickUpZone"))
            {
                canPickUp = true;
                Debug.Log("Enter Pick Up Zone");
            }
            else if (other.CompareTag("DropOffZone"))
            {
                canDropOff = true;
                DropoffZone dz = other.GetComponent<DropoffZone>();
                if (dz != null)
                    currentDropOffIndex = dz.houseIndex;
                Debug.Log("Enter Drop Off Zone");
                if (company == Company.USPS && hasPackage && deliveryManager != null
                    && deliveryManager.IsTargetHouse(currentDropOffIndex))
                    DeliverNow();
            }
        }
        private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collisionSound != null && collisionAudioSource != null)
    {
        collisionAudioSource.PlayOneShot(collisionSound);
    }
    }

        // trigger exit
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("PickUpZone"))
            {
                canPickUp = false;
                Debug.Log("Left Pick Up Zone");
            }
            else if (other.CompareTag("DropOffZone"))
            {
                canDropOff = false;
                Debug.Log("Left Drop Off Zone");
            }
        }
    }
}
