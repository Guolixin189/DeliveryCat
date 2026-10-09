using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public float speed = 2f;
    public bool isGameOver = false;

    private int direction = -1; 
    private SpriteRenderer spriteRenderer;

    void Start(){
         spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void FixedUpdate()
    {
        if (isGameOver)
            return;

        transform.position += Vector3.right * direction * speed * Time.fixedDeltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Player"))
    {
        // 不再直接结束游戏：让玩家减速，行人被弹开
        player.CatController cat = collision.gameObject.GetComponent<player.CatController>();
        if (cat != null)
            cat.ApplySlow();
    }

    direction *= -1;
    spriteRenderer.flipX = !spriteRenderer.flipX;
}
}