using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    float maxSpeed = 2;
    float acceleration = 8;
    float rotSpeed = 40; // deg per second
    Vector3 currentVelocity = Vector3.zero;

    public new CameraScript camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float direction = camera.GetDirection();
        float dirDiff = Helpers.CircularDifference(transform.rotation.eulerAngles.y, direction, 360);
        if (Mathf.Abs(dirDiff) > 30 || currentVelocity.magnitude > maxSpeed / 3)
        {
            float rotAmount = (dirDiff > 0 ? -1 : 1) * rotSpeed * Time.deltaTime;
            rotAmount = dirDiff > 0 ? Mathf.Min(rotAmount, dirDiff) : Mathf.Max(rotAmount, dirDiff);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotAmount, 0));
        }


        if (Input.GetButton("Horizontal"))
        {
            float x = currentVelocity.x + acceleration * Input.GetAxis("Horizontal") * Time.deltaTime;
            currentVelocity.x = Mathf.Clamp(x, -maxSpeed, maxSpeed);
        } else
        {
            float x = currentVelocity.x;
            x += acceleration * Time.deltaTime * (x == 0 ? 0 : x > 0 ? -.5f : .5f);
            currentVelocity.x = Mathf.Clamp(x, -maxSpeed, maxSpeed);
        }
        if (Input.GetButton("Vertical"))
        {
            float z = currentVelocity.z + acceleration * Input.GetAxis("Vertical") * Time.deltaTime;
            currentVelocity.z = Mathf.Clamp(z, -maxSpeed / 2, maxSpeed);
        } else
        {
            float z = currentVelocity.z;
            z += acceleration * Time.deltaTime * (z == 0 ? 0 : z > 0 ? -.5f : 1);
            currentVelocity.z = Mathf.Clamp(z, -maxSpeed, maxSpeed);
        }
        transform.position += transform.rotation * currentVelocity  * Time.deltaTime;
    }
}
