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
        private float slowTimer = 0f;       // remaining slow time
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
        }

        void Update()
        {
            if (slowTimer > 0f)
                slowTimer -= Time.deltaTime;

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
            float speed = slowTimer > 0f ? moveSpeed * slowFactor : moveSpeed;
            rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
        }

        // Called by NPCs on pedestrian hit: slow for slowDuration seconds; re-hitting while slowed refreshes the timer
        public void ApplySlow()
        {
            slowTimer = slowDuration;
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