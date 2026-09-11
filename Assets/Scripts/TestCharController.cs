using UnityEngine;
using UnityEngine.InputSystem;
 
namespace Q1 {
	public class UFOController : MonoBehaviour
	{
		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start() {
        
		}
 
		// Update is called once per frame
		void Update() {
			// Move Up
			if(Keyboard.current.upArrowKey.isPressed) {
				transform.position += new Vector3(0, 0.02f, 0);
			}
			
			// Move Down
			if(Keyboard.current.downArrowKey.isPressed) {
				transform.position += new Vector3(0, -0.02f, 0);
			}
			
			// Move Left
			if(Keyboard.current.leftArrowKey.isPressed) {
				transform.position += new Vector3(-0.02f, 0, 0);
			}
			
			// Move Right
			if(Keyboard.current.rightArrowKey.isPressed) {
				transform.position += new Vector3(0.02f, 0, 0);
			}
		}
	}
}
	