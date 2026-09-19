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

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
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

            // 4. 按下空格键测试切换包裹动画 (wasPressedThisFrame 确保按一下只触发一次)
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                hasPackage = !hasPackage;
                animator.SetBool("HasPackage", hasPackage);
            }
        }

        void FixedUpdate()
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }
}