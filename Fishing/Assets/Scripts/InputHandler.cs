using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class InputHandler : MonoBehaviour
{
    [SerializeField]
    private Line line;
    [SerializeField]
    private PlayerActions pa;
    private InputAction MoveAction;
    private InputAction WheelAction;
    private InputAction ScrollAction;
    private InputAction AutoReelAction;
    private InputAction CastAction;
    private InputAction ReleaseAction;

    private PlayerController pc;

    void Awake()
    {
        pa = new PlayerActions();
        pc = GetComponent<PlayerController>();

        MoveAction = pa.Gameplay.Movement;
        WheelAction = pa.Gameplay.ReelWheel;
        ScrollAction = pa.Gameplay.Reel;
        AutoReelAction = pa.Gameplay.AutoReel;
        CastAction = pa.Gameplay.Cast;
        ReleaseAction = pa.Gameplay.Release;

        CastAction.performed += ctx => pc.Casting();
        CastAction.canceled += ctx => pc.Released();


    }

    void OnEnable()
    {
        pa.Enable();
    }

    void OnDisable()
    {
        pa.Disable();
    }

    void Update()
    {
        CheckCasting();
        CheckMovement();
        CheckReelWheel();
        CheckScrollWheel();
        CheckRelease();
    }

    void CheckCasting()
    {
        // if (CastAction.performed)

    }

    void CheckMovement()
    {
        Vector2 moveDir = MoveAction.ReadValue<Vector2>();
        if(moveDir.magnitude > .1)
            pc.Move(moveDir);
    }

    Vector2 reelPos;
    Vector2 oldReelPos;


    //needs test
    void CheckReelWheel()
    {
        float reelChange = 0;
        float oldAngle = 0;
        float newAngle = 0;
        
        reelPos = WheelAction.ReadValue<Vector2>();
        if(reelPos.magnitude < .5f)
            return;
        if(reelPos.x >= 0)
        {
            oldAngle = Vector2.Angle(Vector2.up, oldReelPos);
            newAngle = Vector2.Angle(Vector2.up, reelPos);
        }else
        {
            oldAngle = Vector2.Angle(Vector2.down, oldReelPos);
            newAngle = Vector2.Angle(Vector2.down, reelPos);
        }
        reelChange = newAngle - oldAngle;

        if(reelChange > 40)
            StartCoroutine(line.Reel());
        oldReelPos = reelPos;
    }

    void CheckScrollWheel()
    {
        float scroll = ScrollAction.ReadValue<float>();
        float press = AutoReelAction.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0 || press > 0)
            StartCoroutine(line.Reel());
    }

    void CheckRelease()
    {
        float rel = ReleaseAction.ReadValue<float>();
        if(rel > 0)
            line.isRelease = true;
        else
            line.isRelease = false;
    }
}
