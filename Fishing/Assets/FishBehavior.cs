using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishBehavior : MonoBehaviour
{
    List<Transform> body;
    Vector3 direction;
    Rigidbody2D rb;

    public float moveSpeed;
    int strength;
    int viewRange;
    float zoneRange;
    // float dmin;
    // float dmax;

    float zoneDist; //distance from zone center
    float vel;
    public float damp;
    public float bodySpeed;
    int mode;

    bool inWater;
    bool canReset;
    bool canZip;

    void Awake()
    {
        // rb = GetComponent<Rigidbody2D>();
        body = new List<Transform>();
        foreach(Transform child in transform.parent)
            if(child.name != "Brain")
                body.Add(child);
    }

    void Start()
    {
        // dmin = .5f;
        // dmax = 5;
        zoneRange = 10;

        mode = 0;
        vel = 0;
        damp = .8f;
        bodySpeed = 5;
        canReset = true;
        canZip = true;
        Reset();
    }

    void FixedUpdate()
    {
        if(mode == 0) //be a fish
        {
            if(DetectDepth() < .4f)
                FlipY();
            if(zoneDist > zoneRange)
                ZoneFace();
            if(Random.Range(0,1) < .1f) 
                Reset();
            if(Random.Range(0,1) < .2f)
                StartCoroutine(Zip());

        }

        vel = vel * damp;
        transform.position += direction * vel;
        UpdateBody();
        zoneDist = transform.localPosition.magnitude; //distance from parent "zone spawn"
    }

// ANIMATION

    void UpdateBody()
    {
        // Debug.Log(Vector2.Distance(transform.position, body[0].position));
        if(Vector2.Distance(transform.position, body[0].position) > 0.2f)
        {
            body[0].rotation = Quaternion.Euler(new Vector3(0,0, AngleCalc(transform.position, body[0].position)));
            body[0].position += body[0].right * Time.fixedDeltaTime * bodySpeed; 
        } else
            body[0].position = Vector3.Lerp(body[0].position, transform.position, Time.fixedDeltaTime * 12f);
        body[1].rotation = Quaternion.Euler(new Vector3(0,0, AngleCalc(body[0].position, body[1].position)+90));
        body[2].rotation = Quaternion.Euler(new Vector3(0,0, AngleCalc(body[1].position, body[2].position)-90));
        if (Vector2.Distance(body[0].position, body[1].position) > .6f)
            body[1].position = Vector2.Lerp(body[1].position, body[0].position, Time.fixedDeltaTime * 12f);
        if (Vector2.Distance(body[1].position, body[2].position) > .49f)
            body[2].position = Vector2.Lerp(body[2].position, body[1].position, Time.fixedDeltaTime * 12f);
    }

    float AngleCalc(Vector3 target, Vector3 source)
    {
        float x = target.x - source.x;
        float y = target.y - source.y;

        return Mathf.Atan2(y,x) * Mathf.Rad2Deg;
    }

// MOVEMENT

    void Reset()
    {
        Debug.Log("Reset");
        if(!canReset)
            return;
        direction = Random.insideUnitCircle.normalized;
        direction = new Vector3(direction.x, direction.y, 0);
        moveSpeed = Random.Range(0.4f, 1f);
        StartCoroutine(ResetCoolDown());
    }

    IEnumerator Zip()
    {
        if(!canZip)
            yield break;
        for(int i = 0; i < Random.Range(20, 300) ; i++)
            vel += moveSpeed * Time.fixedDeltaTime;
        StartCoroutine(ZipCoolDown());
        yield return null;
    }

    IEnumerator ResetCoolDown()
    {
        canReset = false;
        yield return new WaitForSeconds(Random.Range(2, 5));
        canReset = true;
    }

    IEnumerator ZipCoolDown()
    {
        canZip = false;
        yield return new WaitForSeconds(Random.Range(0.4f, 2f));
        canZip = true;;
    }

    void FlipX()
    {
        direction.x = -direction.x;
    }

    void FlipY()
    {
        direction.y = -direction.y;
    }

    void ZoneFace()
    {
        direction = transform.parent.transform.position - transform.position;
        direction = (direction + new Vector3 (Random.Range(-2,2),Random.Range(-2,2),0)).normalized;
    }

// DETECTION

    float DetectDepth()
    {
        int layerMask = 1 << 6; //Ground layer

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, Mathf.Infinity, layerMask);

        return hit.distance;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        inWater = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        inWater = false;
        if (mode == 0)
        {
            FlipX();
        }
    }


}
