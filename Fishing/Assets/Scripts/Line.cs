using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public GameObject fishObj;
    public GameObject spoutObj;

    public GameObject lineEnd;
    public GameObject hook;
    public GameObject bob;

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float ropeSegLen = 0.25f;

    private float lineWidth = .1f;

    private bool isReeling;
    public bool isRelease;

    [SerializeField]
    public int lineOut = 35; //length of rope unreeled

    [SerializeField]
    private int segLen = 200;
    [SerializeField]
    private bool isBob;
    [SerializeField]
    private int bobLen = 5;
    [SerializeField]
    private float waterDampen = .8f;
    [SerializeField]
    private bool isBit = false;

    int throwDelta;
    Vector2 oldHook;

    // Start is called before the first frame update
    void Start()
    {
        isReeling = false;
        isBit = false;

        this.lineRenderer = this.GetComponent<LineRenderer>();
        Vector3 ropeStartPoint = spoutObj.transform.position;//Camera.main.ScreenToWorldPoint(Input.mousePosition);

        for(int i = 0; i < segLen; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= ropeSegLen;
        }

        oldHook = hook.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        this.DrawRope();
    }

    void FixedUpdate()
    {
        this.Simulate();
        
        throwDelta = Mathf.RoundToInt(((Vector2)hook.transform.position - oldHook).magnitude*2);
        Debug.Log(throwDelta);
        oldHook = hook.transform.position;
        if(isRelease)
        {
            
            ThrowLine(throwDelta+1);
            float tens = CheckTension();
            // if(tens > -.27f)
            //     ThrowLine((int)((tens + 1) * 3f));
            
        }
        

        // if(Input.GetMouseButton(0))
        //     StartCoroutine(Reel());
        // if(Input.GetMouseButton(1))
        //     StartCoroutine(AddLine());
    }

    public IEnumerator Reel()
    {
        if(lineOut < bobLen || isReeling)
            yield break;
        isReeling = true;
        lineOut--;
        //ropeSegments.RemoveAt(0);
        //segLen--;
        yield return new WaitForSeconds(.03f);
        isReeling = false;
    }

    public IEnumerator AddLine()
    {
        if(lineOut >= segLen)
            yield break;
        //Vector3 ropeStartPoint = spoutObj.transform.position;//Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isReeling = true;
        lineOut++;
        //this.ropeSegments.Insert(0, new RopeSegment(ropeStartPoint));
        //segLen++;
        yield return new WaitForSeconds(0);
        isReeling = false;
    }

    public void ThrowLine(int len)
    {
        for(int i = 0; i < len; i++)
            StartCoroutine(AddLine());
    }

    float CheckTension()
    {
        float tension = 0;

        int lineOutNum = segLen - lineOut;
        Vector2 startPos = ropeSegments[lineOutNum].posNow;
        Vector2 endPos = ropeSegments[segLen - 1].posNow;

        // if(Vector2.Distance(startPos, endPos) >= (lineOut * ropeSegLen))
            tension = Vector2.Distance(startPos, endPos) - (lineOut * ropeSegLen);
        // else
        //     tension = 0;

        // Debug.Log(tension);
        return tension;
    }

    private void Simulate()
    {
        Vector2 gravityForce = new Vector2(0f, -2f);

        for(int i = 0; i < this.segLen; i++)
        {
            // if(isReeling)
            //     return;
            RopeSegment firstSeg = this.ropeSegments[i];
            Vector2 velocity = firstSeg.posNow - firstSeg.posOld;
            
            if(firstSeg.posNow.y < 1.5f) //if underwater
            {
                gravityForce = new Vector2(0f, -.005f);
                if(isBob && i != this.segLen - bobLen - 1)
                    velocity *= waterDampen;
            }
            else // if above water
                gravityForce = new Vector2(0f, -.8f);

            if(i == this.segLen - bobLen - 1) //bob location
                bob.transform.position = firstSeg.posNow;
            if(isBob && i == this.segLen - bobLen - 1 && firstSeg.posNow.y < 1.5f)// && i == this.segLen - 1)
            {
                gravityForce = new Vector2(0f, .8f);
            }
            // if(isBob && i == this.segLen - bobLen - 2 && firstSeg.posNow.y > 1.4f)// && i == this.segLen - 1)
            // {
            //     gravityForce = new Vector2(0f, 2f);
            //     velocity *= .2f;
            // }


            firstSeg.posOld = firstSeg.posNow;
            firstSeg.posNow += velocity;
            firstSeg.posNow += gravityForce * Time.deltaTime;

            if(i == this.segLen - 1 && isBit) //hooked and end
            {
                firstSeg.posNow = fishObj.transform.position;
                firstSeg.posOld = firstSeg.posNow;
            }
            if(i == this.segLen - 1) //end = hook
            {
                hook.transform.position = firstSeg.posNow;
                // firstSeg.posNow = hook.transform.position;
                // firstSeg.posOld = firstSeg.posNow;

                // hook.transform.rotation = Quaternion.Euler(0,0,0) * Quaternion.FromToRotation(firstSeg.posNow,this.ropeSegments[segLen-3].posNow-firstSeg.posNow);
            }
            this.ropeSegments[i] = firstSeg;
        }

        CheckTension();
        for (int i = 0; i < 50; i++)
        {
            this.ApplyConstraints();
        }
    }

    private void ApplyConstraints()
    {
        int lineOutNum = segLen - lineOut;
        RopeSegment firstSeg = this.ropeSegments[0];
        firstSeg.posNow = spoutObj.transform.position;//Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.ropeSegments[0] = firstSeg;

        for(int i = 0; i < this.segLen - 1; i++)
        {
            // if(isReeling)
            //     return;

            RopeSegment thisSeg = this.ropeSegments[i];
            RopeSegment nextSeg = this.ropeSegments[i + 1];

            if(i < segLen - lineOut)
            {
                thisSeg.posNow = spoutObj.transform.position;;
                this.ropeSegments[i] = thisSeg;
                nextSeg.posNow = spoutObj.transform.position;;
                this.ropeSegments[i+1] = nextSeg;
                continue;
            }

            float dist = (thisSeg.posNow - nextSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - this.ropeSegLen);
            Vector2 changeDir = Vector2.zero;

            if(dist > ropeSegLen)
            {
                changeDir = (thisSeg.posNow - nextSeg.posNow).normalized;
            } else if (dist < ropeSegLen)
            {
                changeDir = (nextSeg.posNow - thisSeg.posNow).normalized;
            }

            Vector2 changeAmt = changeDir * error;
            if ( i!=0)
            {
                thisSeg.posNow -= changeAmt * .5f;
                this.ropeSegments[i] = thisSeg;
                nextSeg.posNow += changeAmt * .5f;
                this.ropeSegments[i+1] = nextSeg;
            } else
            {
                nextSeg.posNow += changeAmt;
                this.ropeSegments[i+1] = nextSeg;
            }
        }

    }

    private void DrawRope()
    {
        float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[this.segLen];
        for(int i = 0; i < this.segLen; i++)
        {
            // if(isReeling)
            //     return;
            ropePositions[i] = this.ropeSegments[i].posNow;
        }
        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }
}
