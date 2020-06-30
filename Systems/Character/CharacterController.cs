using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterController : MonoBehaviour
{
    [System.Serializable]
    public enum MovementDirection { Forward, Backward, Left, Right }
    public MovementDirection currentMoveDir = MovementDirection.Forward;
    MovementDirection lastMoveDirection = MovementDirection.Forward;
    float moveDir = 1;

    public Animator animator;
    public bool debugMode = false;
    public bool useAnimatorController = true;
    public float playerHeight;

    [System.Serializable]
    public class MovementOptions
    {
        public float movementSpeed;
        public float smoothSpeed;
        public bool smooth;
        public bool directional; // rotate on local forward or right depending on input vertical or horizontal
    }

    [System.Serializable]
    public class JumpOptions
    {
        public float jumpForce;
        public float jumpSpeed;
        public float jumpDecrease;
        public float incrementJumpFallSpeed = 0.1f;
    }

    [System.Serializable]
    public class PhsicsOptions
    {
        public float gravity = 2.5f;
        public LayerMask discludePlayer;
        public SphereCollider sphereCol;
    }

    [System.Serializable]
    public class AnimationSettings
    {
        public string shouldMove = "";
        public string velY = "";
        public string velX = "";
    }

    public MovementOptions movementOptions = new MovementOptions();
    public JumpOptions jumpOptions = new JumpOptions();
    public PhsicsOptions phsicsOptions = new PhsicsOptions();
    public AnimationSettings animSettings = new AnimationSettings();

    //Private
    //Movement Vectors
    Vector3 velocity;
    Vector3 move;
    Vector3 vel;

    // gravity
    bool grounded;
    Vector3 liftPoint = new Vector3(0, 1.2f, 0);
    Vector3 groundCheckPoint = new Vector3(0, -0.87f, 0);
    RaycastHit groundHit;
    CapsuleCollider _collider;


    void Start()
    {
        _collider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (InputManager.keyboard_axis.y > 0) currentMoveDir = MovementDirection.Forward;
        if (InputManager.keyboard_axis.y < 0) currentMoveDir = MovementDirection.Backward;

        Gravity();
        SimpleMove();
        Jump();
        FinalMove();
        UpdateAnimator();
        GroundChecking();
        CollisionCheck();
    }


    private void SimpleMove()
    {
        move = Vector3.ClampMagnitude(new Vector3(InputManager.keyboard_axis.x, 0, InputManager.keyboard_axis.y), 1);
        velocity += move;
    }

    private void FinalMove()
    {
        Vector3 vel = new Vector3(velocity.x, velocity.y, velocity.z) * movementOptions.movementSpeed;

        if (movementOptions.directional && currentMoveDir != lastMoveDirection)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y+180f, transform.localEulerAngles.z);
            moveDir *= -1;
            lastMoveDirection = currentMoveDir;
        }

        // if we are using root motion set this var to true,
        // so that movement is controlled only by animation i.e root motion
        if (!useAnimatorController)
        {
            vel = transform.TransformDirection(vel) * moveDir;
            transform.position += vel * Time.deltaTime;
            velocity = Vector3.zero;
        }
    }

    void UpdateAnimator()
    {
        bool move = Mathf.Abs(InputManager.keyboard_axis.y) > 0.1f || Mathf.Abs(InputManager.keyboard_axis.x) > 0.1f;

        animator.SetBool(animSettings.shouldMove, move);
        animator.SetFloat(animSettings.velX, InputManager.keyboard_axis.x);
        animator.SetFloat(animSettings.velY, InputManager.keyboard_axis.y);
    }

    private void Gravity()
    {
        if (grounded == false)
        {
            velocity.y -= phsicsOptions.gravity;
        }
        else
        {
            //currentGravity = 0;
        }
    }

    private void GroundChecking()
    {
        // Ray ray = new Ray(transform.TransformPoint(liftPoint), Vector3.down);
        Ray ray = new Ray( transform.position, Vector3.down);
        // debug this ray
        if (debugMode)
        {
            Debug.DrawRay(_collider.transform.position, Vector3.down, Color.green);
        }

        RaycastHit tempHit = new RaycastHit();

        if (Physics.SphereCast(ray, 0.17f, out tempHit, 20, phsicsOptions.discludePlayer))
        {
            GroundConfirm(tempHit);
        }
        else
        {
            grounded = false;
        }

    }

    private void GroundConfirm(RaycastHit tempHit)
    {
        Collider[] col = new Collider[3];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(groundCheckPoint), 0.55f, col, phsicsOptions.discludePlayer);

        grounded = false;

        for (int i = 0; i < num; i++)
        {
            if (col[i].transform == tempHit.transform)
            {
                groundHit = tempHit;
                grounded = true;

                //Snapping 
                if (inputJump == false)
                {
                    Vector3 avg = new Vector3(transform.position.x, (groundHit.point.y + playerHeight / 2), transform.position.z);

                    if (!movementOptions.smooth)
                    {
                        transform.position = avg;
                    }
                    else
                    {
                        transform.position = Vector3.Lerp(
                            transform.position,
                            new Vector3(transform.position.x,
                            (groundHit.point.y + playerHeight / 2),
                            transform.position.z),
                            (movementOptions.smoothSpeed) * Time.deltaTime);
                    }
                }
                break;
            }
        }

        if (num <= 1 && tempHit.distance <= 3.1f && inputJump == false)
        {
            if (col[0] != null)
            {
                Ray ray = new Ray(transform.TransformPoint(liftPoint), Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 3.1f, phsicsOptions.discludePlayer))
                {
                    if (hit.transform != col[0].transform)
                    {
                        grounded = false;
                        return;
                    }
                }
            }
        }
    }

    private void CollisionCheck()
    {
        Collider[] overlaps = new Collider[4];
        Collider myCollider = new Collider();
        int num = 0;
        if (phsicsOptions.sphereCol != null)
        {
            num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(phsicsOptions.sphereCol.center), phsicsOptions.sphereCol.radius, overlaps, phsicsOptions.discludePlayer, QueryTriggerInteraction.UseGlobal);
            myCollider = phsicsOptions.sphereCol;
        }

        for (int i = 0; i < num; i++)
        {
            Transform t = overlaps[i].transform;
            Vector3 dir;
            float dist;

            if (Physics.ComputePenetration(myCollider, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
            {
                Vector3 penetrationVector = dir * dist;
                Vector3 velocityProjected = Vector3.Project(velocity, -dir);
                transform.position = transform.position + penetrationVector;
                vel -= velocityProjected;
            }
        }
    }

    private float jumpHeight = 0;
    private float fallMultiplier = -1;
    private bool inputJump = false;

    private void Jump()
    {
        bool canJump = false;

        canJump = !Physics.Raycast(new Ray(transform.position, Vector3.up), playerHeight, phsicsOptions.discludePlayer);
        if (grounded && jumpHeight > 0.2f || jumpHeight <= 0.2f && grounded)
        {
            jumpHeight = 0;
            inputJump = false;
            fallMultiplier = -1;
        }

        if (grounded && canJump)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                inputJump = true;
                transform.position += Vector3.up * 0.2f;
                jumpHeight += jumpOptions.jumpForce;
            }
        }
        else
        {
            if (!grounded)
            {
                jumpHeight -= (jumpHeight * jumpOptions.jumpDecrease * Time.deltaTime) + fallMultiplier * Time.deltaTime;
                fallMultiplier += jumpOptions.incrementJumpFallSpeed;
            }
        }
        velocity.y += jumpHeight;
    }
}
