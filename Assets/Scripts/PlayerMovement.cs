using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Movement Parameters")]
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float jumpPower = 16.0f;

    [Header("Wall Slide Parameters")]
    [SerializeField] public float wallSlideSpeed;

    [Header("Air Movement Parameters")]
    [SerializeField] public float movementForceInAir;
    [SerializeField] public float airDragMultiplier = 0.95f;

    [Header("Jump Parameters")]
    [SerializeField] public int amountOfJumps = 1;
    [SerializeField] public float variableJumpHeightMultiplier = 0.5f;

    [Header("Wall Jump Parameters")]
    [SerializeField] public Vector2 wallHopDirection;
    [SerializeField] public Vector2 wallJumpDirection;
    [SerializeField] public float wallHopForce;
    [SerializeField] public float wallJumpForce;


    private Rigidbody2D body;
    private Animator anim;
    private BoxCollider2D boxCollider;

    private float horizontalInput;
    private bool isWallSliding;
    private bool isGround;
    private bool isTouchingWall;
    private bool canJump;
    private int amountOfJumpsLeft;
    private int facingDirection = 1;

    private void Awake()
    {
        //Grab references for rigidbody and animator from object
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        amountOfJumpsLeft = amountOfJumps;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    private void Update()
    {
        checkInput();
        checkMovementDirection();
        updateAnimations();
        checkIfCanJump();
        checkIfWallSliding();
    }

    private void FixedUpdate()
    {
        applyMovement();
        checkSurroundings();
    }

    private void checkIfWallSliding() {
        if(isTouchingWall && !isGround && body.velocity.y < 0) {
            isWallSliding = true;
        } else {
            isWallSliding = false;
        }
    }

    private void checkSurroundings() {
        isGround = isGrounded();
        isTouchingWall = onWall();
    }

    private void checkIfCanJump() {
        if((isGround && body.velocity.y <= 0) || isWallSliding) {
            amountOfJumpsLeft = amountOfJumps;
        }

        if (amountOfJumpsLeft <= 0) {
            canJump = false;
        } else {
            canJump = true;
        }
    }

    private void checkMovementDirection() {
        flip();
    }

    private void updateAnimations() {
        //Set animator parameters
        anim.SetBool("run", horizontalInput != 0);
        anim.SetBool("grounded", isGrounded());
    }

    private void checkInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) {
            Jump();
        }
        if (Input.GetButtonUp("Jump")) {
            body.velocity = new Vector2(body.velocity.x, body.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void Jump()
    {
        if(canJump && !isWallSliding) {
            body.velocity = new Vector2(body.velocity.x, jumpPower);
            amountOfJumpsLeft--;
        }
         // wall hop
        else if (isWallSliding && horizontalInput == 0 && canJump) {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            body.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
        else if ((isWallSliding || isTouchingWall) && horizontalInput != 0 && canJump) {
            isWallSliding = false;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * horizontalInput, wallJumpForce * wallJumpDirection.y);
            body.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
    }

    private void applyMovement() {
        if (isGround){
            body.velocity = new Vector2(speed * horizontalInput, body.velocity.y);
        }
        else if (!isGround && !isWallSliding && horizontalInput != 0) {
            Vector2 forceToAdd = new Vector2(movementForceInAir * horizontalInput, 0);
            body.AddForce(forceToAdd);

            if(Mathf.Abs(body.velocity.x) > speed) {
                body.velocity = new Vector2(speed * horizontalInput, body.velocity.y);
            }
        } else if (!isGround && !isWallSliding && horizontalInput == 0) {
            body.velocity = new Vector2(body.velocity.x * airDragMultiplier, body.velocity.y);
        }
        if(isWallSliding) {
            if (body.velocity.y < -wallSlideSpeed) {
                body.velocity = new Vector2(body.velocity.x, -wallSlideSpeed);
            }
        }
    }

    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private void flip() {
        //Flip player when moving left-right
        if(!isWallSliding) {
            facingDirection*= -1;

            if (horizontalInput > 0.01f)
                transform.localScale = Vector3.one;
            else if (horizontalInput < -0.01f)
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}