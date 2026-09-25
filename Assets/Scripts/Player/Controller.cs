using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public class SimpleCatController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 3f; 
        
        [Header("References")]
        public GameObject packageObject; // 拖入挂载在猫背上的包裹子物体

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
            sr = GetComponent<SpriteRenderer>(); // 获取 SpriteRenderer 用于翻转

            // 初始状态没有包裹
            if (packageObject != null)
            {
                packageObject.SetActive(false);
            }
        }

        void Update()
        {
            movement = Vector2.zero;

            // 1. 获取移动输入 (新版 Input System)
            if (Keyboard.current.wKey.isPressed) movement.y += 1;
            if (Keyboard.current.sKey.isPressed) movement.y -= 1;
            if (Keyboard.current.aKey.isPressed) movement.x -= 1;
            if (Keyboard.current.dKey.isPressed) movement.x += 1;

            // 2. 朝向翻转逻辑
            // 只有当有明确的横向输入时才翻转，如果是纯上下移动 (movement.x == 0)，则保持原样
            if (movement.x < 0)
            {
                // 向左走：X轴缩放改为 -1，整体镜像翻转
                transform.localScale = new Vector3(-1, 1, 1); 
            }
            else if (movement.x > 0)
            {
                // 向右走：恢复原始朝向
                transform.localScale = new Vector3(1, 1, 1);  
            }

            // 3. 动画状态更新
            // 只需要一个 Speed 参数来控制是播放 Idle 还是 Walk
            animator.SetFloat("Speed", movement.sqrMagnitude);

            // 4. 拾取/送达交互逻辑
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // 站在取货区且没拿包裹
                if (canPickUp && !hasPackage)
                {
                    hasPackage = true;
                    if (packageObject != null) packageObject.SetActive(true); // 显示包裹
                    animator.SetBool("hasPackage", true); // 同步给Animator，以防万一以后需要特殊动画
                    Debug.Log("Package Picked Up!");
                }
                // 站在交货区且拿着包裹
                else if (canDropOff && hasPackage)
                {
                    hasPackage = false;
                    if (packageObject != null) packageObject.SetActive(false); // 隐藏包裹
                    animator.SetBool("hasPackage", false);
                    Debug.Log("Package Delivered!");
                }
            }
        }

        void FixedUpdate()
        {
            // 处理物理移动
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }

        // --- 触发区检测 ---
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