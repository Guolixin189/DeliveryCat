using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public class SimpleCatController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 3f; 
        
        [Header("References")]
        public GameObject packageObject; 

        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer sr;
        private Vector2 movement;

        [Header("Status")]
        public bool hasPackage = false;
        private bool canPickUp = false;
        private bool canDropOff = false;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            sr = GetComponent<SpriteRenderer>(); 

            // initial state
            if (packageObject != null)
            {
                packageObject.SetActive(false);
            }
        }

        void Update()
        {
            movement = Vector2.zero;

            // 1. input
            if (Keyboard.current.wKey.isPressed) movement.y += 1;
            if (Keyboard.current.sKey.isPressed) movement.y -= 1;
            if (Keyboard.current.aKey.isPressed) movement.x -= 1;
            if (Keyboard.current.dKey.isPressed) movement.x += 1;

            // 2. direction flipping
            if (movement.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1); 
            }
            else if (movement.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);  
            }

            // 3. animation update between Idle and Walk
            animator.SetFloat("Speed", movement.sqrMagnitude);

            // 4. pick up / deliver logic
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (canPickUp && !hasPackage)
                {
                    hasPackage = true;
                    if (packageObject != null) packageObject.SetActive(true); 
                    animator.SetBool("hasPackage", true); 
                    Debug.Log("Package Picked Up!");
                }
                else if (canDropOff && hasPackage)
                {
                    hasPackage = false;
                    if (packageObject != null) packageObject.SetActive(false); 
                    animator.SetBool("hasPackage", false);
                    Debug.Log("Package Delivered!");
                }
            }
        }

        void FixedUpdate()
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }

        // Trigger for pickup zone
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("PickUpZone")) canPickUp = true;
            else if (other.CompareTag("DropOffZone")) canDropOff = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("PickUpZone")) canPickUp = false;
            else if (other.CompareTag("DropOffZone")) canDropOff = false;
        }
    }
}