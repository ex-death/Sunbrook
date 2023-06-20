using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishBehavior : MonoBehaviour
{
    List<Transform> body;
    Vector3 direction;
    public Rigidbody2D[] rb = new Rigidbody2D[3];

    public GameObject line;
    public GameObject hook;

    public Bait baitType;

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
    bool canBack;
    bool stiffen;

    void Awake()
    {
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
        damp = .93f;
        bodySpeed = 5;
        canReset = true;
        canZip = true;
        inWater = true;
        Reset();
    }

    void FixedUpdate()
    {
        if(mode == 0) //be a fish
        {
            if(DetectDepth() < .4f){
                FlipY();
                ZoneFace();
            }
            if(zoneDist > zoneRange)
                ZoneFace();
            if(Random.Range(0,1) < .1f) 
                Reset();
            if(Random.Range(0,1) < .2f)
                StartCoroutine(Zip());

        }
        if(mode == 1) // interested
        {
            HookFace();
            if(Random.Range(0,1) < .08f)
                StartCoroutine(Zip());
        }
        if(mode == 3) // run away
        {
            Reset();
            StartCoroutine(Zip());
            mode = 0;
        }

        inWater = DetectWater();
        DetectHook();

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
        if (Vector2.Distance(body[0].position, body[1].position) > .1f)
            body[1].rotation = Quaternion.Euler(new Vector3(0,0, AngleCalc(body[0].position, body[1].position)+90));
        if (Vector2.Distance(body[1].position, body[2].position) > .1f)
            body[2].rotation = Quaternion.Euler(new Vector3(0,0, AngleCalc(body[1].position, body[2].position)-90));
        // Vector2 b1offset = new Vector2(Mathf.Cos(Mathf.Rad2Deg * body[0].rotation.eulerAngles.z) *.5f,Mathf.Sin(Mathf.Rad2Deg * body[0].rotation.eulerAngles.z) * .5f);
        // Vector2 b2offset = new Vector2(Mathf.Cos(Mathf.Rad2Deg * body[1].rotation.eulerAngles.z) *.4f,Mathf.Sin(Mathf.Rad2Deg * body[1].rotation.eulerAngles.z) * .4f);
        // if (Vector2.Distance(body[0].position, body[1].position) > .6f)
        body[1].position = Vector2.Lerp(body[1].position, body[0].position + -body[0].right * .5f, Time.fixedDeltaTime * 30f);
        // if (Vector2.Distance(body[1].position, body[2].position) > .49f)
        body[2].position = Vector2.Lerp(body[2].position, body[1].position + body[1].up * .4f, Time.fixedDeltaTime * 30f);

        // if(inWater)
        // {
        //     foreach (Rigidbody2D i in rb)
        //     i.gravityScale = 0;
        // } else
        // {
        //     foreach (Rigidbody2D i in rb)
        //     i.gravityScale = 3;
        // }
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
        for(int i = 0; i < Random.Range(20, 200) ; i++)
            vel += moveSpeed * Time.fixedDeltaTime;
        StartCoroutine(ZipCoolDown());
        yield return null;
    }

    IEnumerator BackAway()
    {
        if(!canZip)
            yield break;
        for(int i = 0; i < Random.Range(20, 200) ; i++)
            vel -= moveSpeed * Time.fixedDeltaTime;
        StartCoroutine(BackCoolDown());
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
        yield return new WaitForSeconds(Random.Range(0.5f, 3f));
        canZip = true;;
    }

    IEnumerator BackCoolDown()
    {
        canBack = false;
        yield return new WaitForSeconds(Random.Range(0.5f, 3f));
        canBack = true;;
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
        direction = (direction + new Vector3 (Random.Range(-1,1),Random.Range(-1,1),0)).normalized;
    }

    void HookFace()
    {
        direction = (hook.transform.position - transform.position).normalized;
    }

// DETECTION

    void DetectHook()
    {
        if (!line.GetComponent<Line>().baiting)
        {
            mode = 0;
            return;
        }

        if (line.GetComponent<Line>().baitType != null)
            baitType = line.GetComponent<Line>().baitType;
        
        float hookDist = Vector2.Distance(body[0].position, line.transform.position);
        if (hookDist < baitType.stench)
            mode = 1;
        else if(mode == 1)
            mode = 3;
    }

    float DetectDepth()
    {
        int layerMask = 1 << 6; //Ground layer

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, Mathf.Infinity, layerMask);

        return hit.distance;
    }

    bool DetectWater()
    {
        int layerMask = 1 << 4;
        RaycastHit2D hit2 = Physics2D.Raycast(body[0].position, Vector2.zero, .1f, layerMask);
        if(hit2.collider != null)// == "Water")
            return true;
        return false;
    }


}
