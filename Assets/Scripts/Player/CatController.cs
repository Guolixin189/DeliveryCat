using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public class CatController : MonoBehaviour
    {
        public float moveSpeed = 3f;
        private Rigidbody2D rb;
        private Animator animator;
        private Vector2 movement;

        public bool hasPackage = false;
        private bool canPickUp = false;
        private bool canDropOff = false;
        private GameManager gameManager;
        private DeliveryManager deliveryManager;
        private int currentDropOffIndex = -1;
        public AudioSource packageSound;

        //collision with NPC
        public float jitterAmount = 0.1f;
        public float penaltyDuration = 1.5f;
        public bool canMove = true;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            gameManager = FindFirstObjectByType<GameManager>();
            deliveryManager = FindFirstObjectByType<DeliveryManager>();
        }

        void Update()
        {
            movement = Vector2.zero;

            if (gameManager != null && gameManager.isGameOver)
            {
                movement = Vector2.zero;
                animator.SetFloat("Speed", 0);
                return;
            }

            // 1. WASD
            if (canMove)
            {
                if (Keyboard.current.wKey.isPressed)
                    movement.y += 1;
                if (Keyboard.current.sKey.isPressed)
                    movement.y -= 1;
                if (Keyboard.current.aKey.isPressed)
                    movement.x -= 1;
                if (Keyboard.current.dKey.isPressed)
                    movement.x += 1;

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
                        hasPackage = false;
                        animator.SetBool("hasPackage", false);
                        if (deliveryManager != null)
                            deliveryManager.OnPackageDelivered(currentDropOffIndex);
                        packageSound.Play();
                        Debug.Log("Package Delivered");
                    }
                }
            }
        }

        void FixedUpdate()
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
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

        // collision penalty: colliding with NPC trigger freeze & PC jittering

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("NPC") && canMove)
            {
                GameManager.Instance.deductScore(200); 

                StartCoroutine(ApplyPenaltyRoutine());
            }
        }

        private System.Collections.IEnumerator ApplyPenaltyRoutine()
        {
            canMove = false;
            
            float elapsedTime = 0f;
            Vector3 originalPosition = transform.position;

            while (elapsedTime < penaltyDuration)
            {
                Vector3 jitter = new Vector3(
                    Random.Range(-jitterAmount, jitterAmount),
                    Random.Range(-jitterAmount, jitterAmount),
                    0f
                );

                transform.position = originalPosition + jitter;
                elapsedTime += Time.deltaTime;

                yield return null; 
            }

            transform.position = originalPosition;
            canMove = true;
        }

        // private void CollisionPenalty() { }
    }
}