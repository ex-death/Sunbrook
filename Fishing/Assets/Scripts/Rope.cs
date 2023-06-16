using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public GameObject fishObj;

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float ropeSegLen = 0.25f;

    private float lineWidth = .1f;

    private bool isReeling;

    [SerializeField]
    private int segLen = 35;
    [SerializeField]
    private bool isBob;
    [SerializeField]
    private int bobLen = 5;
    [SerializeField]
    private float waterDampen = .8f;
    [SerializeField]
    private bool isBit = false;

    // Start is called before the first frame update
    void Start()
    {
        isReeling = false;
        isBit = false;

        this.lineRenderer = this.GetComponent<LineRenderer>();
        Vector3 ropeStartPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        for(int i = 0; i < segLen; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= ropeSegLen;
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.DrawRope();
    }

    void FixedUpdate()
    {
        this.Simulate();
        CheckTension();

        if(Input.GetMouseButton(0))
            StartCoroutine(Reel());
        if(Input.GetMouseButton(1))
            StartCoroutine(AddLine());
    }

    IEnumerator Reel()
    {
        if(segLen == 1)
            yield break;
        isReeling = true;
        ropeSegments.RemoveAt(0);
        segLen--;
        yield return new WaitForSeconds(.1f);
        isReeling = false;
    }

    IEnumerator AddLine()
    {
        Vector3 ropeStartPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isReeling = true;
        this.ropeSegments.Insert(0, new RopeSegment(ropeStartPoint));
        segLen++;
        yield return new WaitForSeconds(.1f);
        isReeling = false;
    }

    float CheckTension()
    {
        float tension = 0;

        for(int i = 0; i < segLen; i++)
        {

        }

        Vector2 startPos = ropeSegments[0].posNow;
        Vector2 endPos = ropeSegments[segLen - 1].posNow;

        if(Vector2.Distance(startPos, endPos) > (segLen * ropeSegLen))
            tension = (segLen * ropeSegLen) - Vector2.Distance(startPos, endPos);
        else
            tension = 0;

        Debug.Log(tension);
        return tension;
    }

    private void Simulate()
    {
        Vector2 gravityForce = new Vector2(0f, -2f);

        for(int i = 0; i < this.segLen; i++)
        {
            if(isReeling)
                return;
            RopeSegment firstSeg = this.ropeSegments[i];
            Vector2 velocity = firstSeg.posNow - firstSeg.posOld;
            
            if(firstSeg.posNow.y < 1.5f)
            {
                gravityForce = new Vector2(0f, -.005f);
                if(isBob && i != this.segLen - bobLen - 1)
                    velocity *= waterDampen;
            }
            else
                gravityForce = new Vector2(0f, -.8f);

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

            if(i == this.segLen - 1 && isBit)
                firstSeg.posNow = fishObj.transform.position;
                firstSeg.posOld = firstSeg.posNow;

            this.ropeSegments[i] = firstSeg;
        }

        for (int i = 0; i < 50; i++)
        {
            this.ApplyConstraints();
        }
    }

    private void ApplyConstraints()
    {
        RopeSegment firstSeg = this.ropeSegments[0];
        firstSeg.posNow = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.ropeSegments[0] = firstSeg;

        for(int i = 0; i < this.segLen - 1; i++)
        {
            if(isReeling)
                return;
            RopeSegment thisSeg = this.ropeSegments[i];
            RopeSegment nextSeg = this.ropeSegments[i + 1];

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
            if(isReeling)
                return;
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
