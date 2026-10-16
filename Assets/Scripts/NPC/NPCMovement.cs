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
        GameManager.Instance.GameOver("Game over! You rushed into people.", 40, 800);
        return;
    }

    direction *= -1;
    spriteRenderer.flipX = !spriteRenderer.flipX;
}
}