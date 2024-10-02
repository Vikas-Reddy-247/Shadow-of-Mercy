using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class DiscController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody discRb;
    private TechDisc techDisc;

    private Vector3 origPos;
    private Vector3 oriRot;
    private Vector3 pullPos;

    public Transform disc;
    public Transform hand;
    public Transform curvePoint;

    public bool aiming = false;
    public bool hasDisc = true;
    public bool isRetracting = false;
    public bool isAttacking = false;

    public float discForce = 240;
    public float cameraZoomOffset = .7f;
    public float discTurnSpeed = 100;
    private float time = 0.0f;
    public TrailRenderer trailRenderer;
    public Canvas crosshair;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        animator = GetComponent<Animator>();
        discRb = disc.GetComponent<Rigidbody>();
        techDisc = disc.GetComponent<TechDisc>();
        origPos = disc.localPosition;
        oriRot = disc.localEulerAngles;
        Aim(false, true, 0);
    }

    void Update()
    {
        //If aiming the aim animation is set to player and rotate in that direction else return to normal
        if (aiming)
        {
            Vector3 lookDirection = Camera.main.transform.forward;
            lookDirection.y = 0f; // Keeps the character's rotation flat on the ground
            transform.forward = lookDirection.normalized;
        }
        else
        {
            // Reset the player's rotation when not aiming
            transform.eulerAngles = new Vector3(Mathf.LerpAngle(transform.eulerAngles.x, 0f, 0.2f),transform.eulerAngles.y,transform.eulerAngles.z);
        }

        animator.SetBool("retracting", isRetracting);

        if(Input.GetMouseButtonDown(1) && hasDisc)
        {
            Aim(true, true, 0);
        }

        if(Input.GetMouseButtonUp(1) && hasDisc)
        {
            Aim(false, true, 0);
        }

        if (hasDisc)
        {
            //If player has disc and aiming and if pressed left mouse button then the throw animation is done.
            if (aiming && Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("throwing");
            }

        }
        else
        {
            //If player don't have disc and if pressed aim button player retracts the disc.
            if (Input.GetMouseButtonDown(1))
            {
                animator.SetBool("retracting", true);
                ReturnDisc();
            }

            //If player don't have disc and if disc is collided to the guard the disc retraction is occured automatically.
            if(techDisc.collidedToGuard)
            {
                animator.SetBool("retracting", true);
                ReturnDisc();
                techDisc.collidedToGuard = false;
            }
        }
        
        if(isRetracting)
        {
            //Calculations for disc retract using quadratic Bezier equation
            if(time < 1.0f)
            {
                disc.position = getBQCPoint(time, pullPos, curvePoint.position, hand.position);
                time += Time.deltaTime;
            }
            else
            {
                ResetDisc();
            }
        }
    }

    // Handling aim state and aim crosshair with fade effect.
    void Aim(bool state, bool changeCamera, float delay)
    {
        if (state != aiming && changeCamera)
        {
            // Access Cinemachine Virtual Camera
            Cinemachine.CinemachineVirtualCamera vcam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();

            if (vcam != null)
            {
                // Adjust the FOV based on the aiming state for a zoom-in effect
                vcam.m_Lens.FieldOfView -= state ? cameraZoomOffset : -cameraZoomOffset;
            }
        }

        aiming = state;
        animator.SetBool("aiming", aiming);

        float fade = state ? 1 : 0;
        StartCoroutine(ChangeCanvasAlpha(crosshair, fade, 0.2f));
    }



    // Coroutine for changing Canvas alpha over time
    private IEnumerator ChangeCanvasAlpha(Canvas canvas, float targetAlpha, float duration)
    {
        Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>();
        float[] startAlphas = new float[graphics.Length];
        for (int i = 0; i < graphics.Length; i++)
        {
            startAlphas[i] = graphics[i].color.a;
        }

        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            float newAlpha = Mathf.Lerp(0, targetAlpha, timeElapsed / duration);

            for (int i = 0; i < graphics.Length; i++)
            {
                Color color = graphics[i].color;
                color.a = Mathf.Lerp(startAlphas[i], newAlpha, timeElapsed / duration);
                graphics[i].color = color;
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < graphics.Length; i++)
        {
            Color color = graphics[i].color;
            color.a = targetAlpha;
            graphics[i].color = color;
        }
    }

    //Disc throwing mechanic handling
    public void ThrowDisc()
    {
        Aim(false, true, 1f);

        hasDisc = false;
        techDisc.activated = true;
        discRb.isKinematic = false;
        discRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        disc.parent = null;
        disc.eulerAngles = new Vector3(0, -90 +transform.eulerAngles.y, 0);
        discRb.transform.position += transform.right/5;
        discRb.AddForce(Camera.main.transform.forward * discForce + transform.up * 2, ForceMode.Impulse);
        trailRenderer.emitting = true;
    }


    //Disc Retracting mechanic handling
    void ReturnDisc()
    {
        pullPos = disc.position;
        discRb.Sleep();
        discRb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        time = 0.0f;
        discRb.velocity = Vector3.zero;
        discRb.isKinematic = true;
        techDisc.activated = true;
        isRetracting = true;
    }
    // Reset Disc when Retraction is complete
    void ResetDisc()
    {
        isRetracting = false;
        disc.parent = hand;
        techDisc.activated = false;
        disc.localEulerAngles = oriRot;
        disc.localPosition = origPos;
        hasDisc = true;
        trailRenderer.emitting = false;
    }

    //Quadratic Bezier equation formula
    Vector3 getBQCPoint (float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = (uu * p0) + (2* u * t * p1) + (tt * p2);
        return p;
    }
}
