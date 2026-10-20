using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 boundsMin;
    public Vector2 boundsMax;
    public float smoothTime = 0.15f;

    Camera cam;
    Vector3 velocity;

    void Awake() => cam = GetComponent<Camera>();

    void Start() => transform.position = Clamp(target.position);

    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, Clamp(target.position), ref velocity, smoothTime);
    }

    Vector3 Clamp(Vector3 pos)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float x = boundsMax.x - boundsMin.x <= halfW * 2f ? (boundsMin.x + boundsMax.x) / 2f : Mathf.Clamp(pos.x, boundsMin.x + halfW, boundsMax.x - halfW);
        float y = boundsMax.y - boundsMin.y <= halfH * 2f ? (boundsMin.y + boundsMax.y) / 2f : Mathf.Clamp(pos.y, boundsMin.y + halfH, boundsMax.y - halfH);
        return new Vector3(x, y, -10f);
    }
}
