using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothLookAt : MonoBehaviour
{
    public GameObject targetObj;
    public int speed = 5;
 
void FixedUpdate()
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetObj.transform.position - transform.position);

        // Smoothly rotate towards the target point.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);
    }
}
