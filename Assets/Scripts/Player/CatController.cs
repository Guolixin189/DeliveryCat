using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public class CatController : MonoBehaviour
    {
        public float moveSpeed = 3f; 
        [Header("撞到行人减速")]
        public float slowFactor = 0.4f;     // 减速后速度倍率
        public float slowDuration = 2f;     // 减速持续秒数
        private float slowTimer = 0f;       // 减速剩余时间
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

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            gameManager = FindFirstObjectByType<GameManager>();
            deliveryManager = FindFirstObjectByType<DeliveryManager>();
        }

        void Update()
        {
            if (slowTimer > 0f)
                slowTimer -= Time.deltaTime;

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
                    hasPackage = false;
                    animator.SetBool("hasPackage", false); 
                    if (deliveryManager != null)
                        deliveryManager.OnPackageDelivered(currentDropOffIndex);
                    packageSound.Play();
                    Debug.Log("Package Delivered");
                }
            }
        }

        void FixedUpdate()
        {
            float speed = slowTimer > 0f ? moveSpeed * slowFactor : moveSpeed;
            rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
        }

        // 撞到行人时由 NPC 调用：减速 slowDuration 秒；减速中再被撞到会刷新时长
        public void ApplySlow()
        {
            slowTimer = slowDuration;
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
    }
}