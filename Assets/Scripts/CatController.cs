using UnityEngine;
using UnityEngine.InputSystem;

namespace player
{
    public class UFOController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // Move Up - W
            if (Keyboard.current.wKey.isPressed)
            {
                transform.position += new Vector3(0, 0.003f, 0);
            }

            // Move Down - S
            if (Keyboard.current.sKey.isPressed)
            {
                transform.position += new Vector3(0, -0.003f, 0);
            }

            // Move Left - A
            if (Keyboard.current.aKey.isPressed)
            {
                transform.position += new Vector3(-0.003f, 0, 0);
            }

            // Move Right - D
            if (Keyboard.current.dKey.isPressed)
            {
                transform.position += new Vector3(0.003f, 0, 0);
            }
        }
    }
}