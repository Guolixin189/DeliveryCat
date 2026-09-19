using UnityEngine;
using Unity.Cinemachine; 

public class AutoAssignCinemachineBrain : MonoBehaviour
{
    void Awake()
    {
        Camera mainCam = Camera.main;

        if (mainCam != null && mainCam.GetComponent<CinemachineBrain>() == null)
        {
            mainCam.gameObject.AddComponent<CinemachineBrain>();
        }
    }
}