using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    public GameObject player;
    Vector2 lookingAngle; // In degrees, x: horizontal, y: vertical

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        lookingAngle.x += Input.GetAxis("Mouse X");
        lookingAngle.y -= Input.GetAxis("Mouse Y");
        transform.rotation = Quaternion.Euler(lookingAngle.y, lookingAngle.x, 0);
        transform.position = player.transform.position + new Vector3(0,2.2f,0) + transform.rotation * new Vector3(0, 0, -2.2f);
    }

    public float GetDirection() => lookingAngle.x;
}
