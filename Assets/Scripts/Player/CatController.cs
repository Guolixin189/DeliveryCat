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

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            gameManager = FindFirstObjectByType<GameManager>();
        }

        void Update()
        {
            movement = Vector2.zero;

            // 1. 保留你的新输入系统逻辑，提取为方向向量
            if (Keyboard.current.wKey.isPressed) movement.y += 1;
            if (Keyboard.current.sKey.isPressed) movement.y -= 1;
            if (Keyboard.current.aKey.isPressed) movement.x -= 1;
            if (Keyboard.current.dKey.isPressed) movement.x += 1;

            // 2. 将速度传递给 Animator
            animator.SetFloat("Speed", movement.sqrMagnitude);

            // 3. 只有在移动时才更新 Animator 的方向，这样松开按键时会保持最后的朝向
            if (movement.sqrMagnitude > 0.01f)
            {
                animator.SetFloat("MoveX", movement.x);
                animator.SetFloat("MoveY", movement.y);
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // 情况1：在取货区，且手里没有包裹
                if (canPickUp && !hasPackage)
                {
                    hasPackage = true;
                    animator.SetBool("hasPackage", true); // 触发Animator瞬间切换贴图
                    Debug.Log("Package Picked Up");
                }
                // 情况2：在交货区，且手里有包裹
                else if (canDropOff && hasPackage)
                {
                    hasPackage = false;
                    animator.SetBool("hasPackage", false); // 触发Animator切回空手
                    gameManager.AddScore(100);
                    Debug.Log("Package Delivered +100");
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
                Debug.Log("Enter Drop Off Zone");
            }
        }

        // 当猫猫离开 Is Trigger 的碰撞体时自动执行
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