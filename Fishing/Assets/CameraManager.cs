using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    Camera cam;
    [SerializeField]
    GameObject player;
    [SerializeField]
    GameObject hook;

    float dist;
    public int offset;
    public float multiplier;
    public float camSpeed = 5;

    Vector3 targetPos;

    void Awake()
    {
        cam = Camera.main;
    }

    // Start is called before the first frame update
    void Start()
    {
        offset = -10;
        multiplier = .6f;
    }

    // Update is called once per frame
    void Update()
    {
        cam.orthographicSize = 7.5f;

        targetPos = (hook.transform.position + player.transform.position)/2;

        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, camSpeed *Time.deltaTime);
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, -10);

        dist = (player.transform.position - hook.transform.position).magnitude;
        cam.orthographicSize = dist * multiplier + offset;
        if(cam.orthographicSize < 7.5f)
            cam.orthographicSize = 7.5f;
    }
}
