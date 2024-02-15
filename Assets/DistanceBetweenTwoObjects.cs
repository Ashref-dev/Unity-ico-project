using System.Diagnostics;
using FIMSpace.Basics;
using UnityEngine;


[ExecuteInEditMode]
public class DistanceBetweenTwoObjects : MonoBehaviour
{
    

    [Space(4)]
    [Header("REAL")]

        //public float speed;
        private Rigidbody rb;
        public float speed;
        public GameObject obj;
        public float distanceBetweenObjects;

   



    void Start()
    {
        //rotationAngle = transform.eulerAngles.y;
            rb = GetComponent<Rigidbody>();
        }


    private void FixedUpdate()
    {
        distanceBetweenObjects = Vector3.Distance(transform.position, obj.transform.position);
        //moveDirectionLocal = Vector2.up;
            //bool w = Input.GetKey(KeyCode.W);


            if (distanceBetweenObjects > 0.51) {
               
                //Vector3 tempVect = new Vector3(0, 0, 1);
               
                rb.position = rb.position + transform.forward * speed * Time.fixedDeltaTime;

           

        }
        else
        {
             

            }
       


        //UnityEngine.Debug.DrawLine(transform.position, obj.transform.position, Color.green);
    }

    private void OnDrawGizmos()
    {
        //GUI.color = Color.black;
        //Handles.Label(transform.position - (transform.position - obj.transform.position) / 2, distanceBetweenObjects.ToString());
    }
}
