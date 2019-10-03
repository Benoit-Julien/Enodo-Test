using UnityEngine;
using System.Collections;
 
[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour {
 
    public Transform target;
    public float     distance = 5.0f;
    public float     xSpeed   = 120.0f;
    public float     ySpeed   = 120.0f;
 
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
 
    public float distanceMin = .5f;
    public float distanceMax = 15f;

    public float scrollSpeed = 1f;
 
    private float _x = 0.0f;
    private float _y = 0.0f;
 
    private void Start () 
    {
        Vector3 angles = transform.eulerAngles;
        _x = angles.y;
        _y = angles.x;
    }
    
    private void LateUpdate () 
    {
        if (target)
        {
            if (Input.GetMouseButton(0))
                UpdateCameraRotation();
            UpdateCameraPosition();
        }
    }
 
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    public void UpdateCameraPosition()
    {
        distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * scrollSpeed, distanceMin, distanceMax);
 
        RaycastHit hit;
        if (Physics.Linecast (target.position, transform.position, out hit)) 
            distance -=  hit.distance;
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = transform.rotation * negDistance + target.position;
        
        transform.position = position;
    }

    public void UpdateCameraRotation()
    {
        _x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
        _y -= Input.GetAxis("Mouse Y") * ySpeed            * 0.02f;
 
        _y = ClampAngle(_y, yMinLimit, yMaxLimit);
 
        Quaternion rotation = Quaternion.Euler(_y, _x, 0);
            
        transform.rotation = rotation;
    }
}