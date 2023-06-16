using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Line rodLine;
    private Animator anim;
    int faceDir;

    public int lineLen = 50;

    public Rigidbody2D selfRB;
    float moveSpeed = 30f;
    float vel;
    float damp;
    Vector2 moveDir;

    bool isCasting;

    //ragdoll components in boy
    public Rigidbody2D[] rb;
    public HingeJoint2D[] hjoint;

    void Awake()
    {
        rodLine = GetComponentInChildren<Line>();
        anim = GetComponentInChildren<Animator>();

        selfRB = GetComponent<Rigidbody2D>();
        rb = transform.Find("Body").GetComponentsInChildren<Rigidbody2D>();
        hjoint = transform.Find("Body").GetComponentsInChildren<HingeJoint2D>();

        for(int i = 0; i < rb.Length; i++)
            if(rb[i].name == "end")
                rb[i] = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        anim.enabled = true;
        DisableRagdoll();


        moveSpeed = 80f;
        vel = 0;
        damp = .74f;

        isCasting = false;
        faceDir = 1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        AnimatorManager();
        UpdateMovement();
    }

    void AnimatorManager()
    {
        if(true)
        {
            if (rodLine.lineOut > 10)
            {
                anim.SetBool("isCasted", true);

                if(moveDir.x > 0)
                {
                    if(transform.localScale.x == -1)
                        anim.SetBool("isWalkBack", false);
                    else
                        anim.SetBool("isWalkBack", true);
                }
                else if (moveDir.x < 0)
                {
                    if(transform.localScale.x == 1)
                        anim.SetBool("isWalkBack", false);
                    else
                        anim.SetBool("isWalkBack", true);
                }

            }else{
                anim.SetBool("isCasted", false);
                anim.SetBool("isWalkBack", false);
                if (selfRB.velocity.x > 0.1f)
                    transform.localScale = new Vector3(-1,1,1);
                if (selfRB.velocity.x < -0.1f)
                    transform.localScale = new Vector3(1,1,1);
            }

            if(vel > 0.3f)
                    
                    anim.SetBool("isWalking", true);
                else
                {
                    anim.SetBool("isWalking", false);
                    anim.SetBool("isWalkBack", false);
                    if(isCasting == true)
                    {
                        anim.SetBool("isCasting", true);
                    } else
                    {
                        anim.SetBool("isCasting", false);
                    }
                }
        }
        
    }

    //Action

    public void Casting()
    {
        isCasting = true;
        Debug.Log("Casting!");
        StartCoroutine(TimeCast());
    }
   
    public void Released()
    {
        isCasting = false;
        //rodLine.ThrowLine(lineLen);
        Debug.Log("Release!");
    }

    IEnumerator TimeCast()
    {
        yield return new WaitForSeconds(1f);
        if(isCasting)
            Released();
    }

    //Movement
    void UpdateMovement()
    {
        vel = vel * damp;
        selfRB.velocity = moveDir * vel;
    }


    public void Move(Vector2 dir)
    {
        moveDir = dir;
        vel += moveSpeed * Time.deltaTime;
    }

    //Ragdoll functions

    bool CheckRagdoll()
    {
        foreach(Rigidbody2D i in rb)
            if(i.simulated == false)
                return false;
        foreach(HingeJoint2D i in hjoint)
            if(i.enabled == false)
                return false;
        return true;
    }

    void EnableRagdoll()
    {
        foreach(Rigidbody2D i in rb)
            i.simulated = true;
        foreach(HingeJoint2D i in hjoint)
            i.enabled = true;
        selfRB.simulated = false;
    }

    void DisableRagdoll()
    {
        foreach(Rigidbody2D i in rb)
            i.simulated = false;
        foreach(HingeJoint2D i in hjoint)
            i.enabled = false;
        selfRB.simulated = true;
    }
}
