using System.Collections;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    #region private variables
    private Rigidbody playerRigidBody;
    private BoxCollider boxCollider;
    private AudioSource playerAudio;

    private Vector3 previousPos;

    private bool isMoving = false;
    private bool canJump = true;
    private bool isCrouching = false;
    private bool keepCrouching = false;
    private bool jumpInput;
    private bool inAirMoveInput;
    private bool doubleMove;
   
    private bool changeDirection;

    private float pretendToJumpTime;
    private float pretendToMoveTime;
    private float startMovingTime;
    private float lastRotation; //45 - left, 135 - right

    private float origColliderHeight;
    private float origColliderCenterY;
    private int moveCount = 0;
   

    //variables for mobiles devices
    private Vector2 startTouchPosition;
    private Vector2 currenttouchPosition;
    private Vector2 endTouchPosition;

    private bool stopTouch = false;
    private SwipeDirection swipeDirection;
    private PlayerPosition playerPosition;

    [Header("Mobile controlls")]
    [SerializeField] private float swipeRange = 50;
    [SerializeField] private float tapRange = 10;
    #endregion

    public bool isAlive { get; private set; }
    public Animator playerAnimation { get; private set; }

    #region serialize variables
    [Header("Movement")]
    [SerializeField] private float sideDashSpeed = 25f;
    [SerializeField] private float jumpForce = 300f;
    [Header("Collision Senses")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float groundCheckLength = 0.2f;
    [Header("Other")]
    [SerializeField] private float inputOnDelay = 0.2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float crouchTime = 1f;
    [Header("Visual")]
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private ParticleSystem dirtPartical;
    [SerializeField] private ParticleSystem eatParticle;
    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip crashSound;
    [SerializeField] private AudioClip crouchDashSound;
    #endregion  

    private enum SwipeDirection
    {
        none,
        left,
        right,
        up,
        down,
        tap
    }

    private enum PlayerPosition
    {
        left,
        middle,
        right
    }


    // Start is called before the first frame update
    void Start()
    {
        playerRigidBody= GetComponent<Rigidbody>();
        playerAnimation = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider>();
        isAlive = true;
        origColliderHeight = boxCollider.size.y;
        origColliderCenterY = boxCollider.center.y;
        playerPosition = PlayerPosition.middle;
    }

    private SwipeDirection Swipe()
    {
        var touchType = SwipeDirection.none;

        if(screenWasTouched && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startTouchPosition = Input.GetTouch(0).position;
        }

        if(screenWasTouched && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            currenttouchPosition = Input.GetTouch(0).position;
            var distance = currenttouchPosition - startTouchPosition;

            if (!stopTouch)
            {
                if (distance.x < -swipeRange)
                {
                    stopTouch = true;
                   
                    touchType = SwipeDirection.left;
                }
                else if(distance.x > swipeRange)
                {
                    stopTouch = true;
                    
                    touchType = SwipeDirection.right;
                }
                else if(distance.y > swipeRange)
                {
                    stopTouch = true;
                    
                    touchType = SwipeDirection.up;
                }
                else if(distance.y < -swipeRange)
                {
                    stopTouch = true;
                  
                    touchType = SwipeDirection.down;
                }
            }
        }

        if(screenWasTouched && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            stopTouch = false;

            endTouchPosition = Input.GetTouch(0).position;

            var distance = endTouchPosition - startTouchPosition;
            
            if(Mathf.Abs(distance.x) < tapRange && Mathf.Abs(distance.y) < tapRange)
            {
                print("right");
                touchType = SwipeDirection.tap;
            }
        }

        return touchType;
    }

    // Update is called once per frame
    void Update()
    {
            if (screenWasTouched || Input.anyKeyDown)
        {
            var touchType = Swipe();

            CheckMoveInput(touchType);
            CheckJumpInput(touchType);
            CheckCrouchInput(touchType);
        }

        PlayVisualEffects();
    }

    private void CheckMoveInput(SwipeDirection direction = SwipeDirection.none)
    {
        if (!isAlive || direction == SwipeDirection.none) return;

       /* if (inAirMoveInput && StandOnGround)
        {
            if (Time.time <= pretendToMoveTime + coyoteTime)
            {
                CheckInAirMove();
                return;
            }

            inAirMoveInput = false;
        }*/

        //moveCount++;
        //print("Move " + moveCount + " started from " + playerPosition.ToString());

        if (direction == SwipeDirection.left)
        {
            if (playerPosition == PlayerPosition.middle)
            {
                Move(45f, new Vector3(0, 0, 6));
            }
            else if (playerPosition == PlayerPosition.right)
            {
                Move(45f, new Vector3(0, 0, 0));
            }
            else if(playerPosition == PlayerPosition.left)
            {
                Move(45f, new Vector3(0, 0, 6));
            }
        }
        else if (direction == SwipeDirection.right)
        {
            if (playerPosition == PlayerPosition.middle)
            {
                Move(135f, new Vector3(0, 0, -6));
            }
            else if (playerPosition == PlayerPosition.left)
            {
                Move(135f, new Vector3(0, 0, 0));
            }
            else if (playerPosition == PlayerPosition.right)
            {
                Move(135f, new Vector3(0, 0, -6));
            }
        }
    }

  
  /*  private void CheckInAirMove()
    {
        inAirMoveInput = false;

        if (moveInputDirection)
        {
            Move(45f, new Vector3(0, 0, 6));
        }
        else Move(135f, new Vector3(0, 0, -6));
    }*/

    private void Move(float rotation, Vector3 newPos)
    {
        if (transform.position.z == Mathf.Abs(newPos.z)) return;
        
        if (isMoving)
        {
            if (lastRotation == rotation && !doubleMove)
            {
                doubleMove = true;
                return;
            }

            changeDirection = true;

            transform.rotation = Quaternion.Euler(0, rotation, 0);
            playerAudio.PlayOneShot(moveSound, 1f);

            StartCoroutine("ReturnToPreviousPosition", previousPos);

            return;
        }
        else
        {
            if (!changeDirection)
            {
                previousPos = transform.position;
                lastRotation = rotation;
            }

            transform.rotation = Quaternion.Euler(0, rotation, 0);
            playerAudio.PlayOneShot(moveSound, 1f);
            startMovingTime = Time.time;
            canJump = false;
            isMoving = true;

            if (doubleMove) doubleMove = false;

            StartCoroutine("LerpPosition", newPos);
        }
        
    }

    private void SetPlayerPosition(int zAxis)
    {
        switch (zAxis)
        {
            case 0:
                playerPosition = PlayerPosition.middle;
                break;
            case 6:
                playerPosition = PlayerPosition.left;
                break;
            case -6:
                playerPosition = PlayerPosition.right;
                break;
            default:
                break;
        }
    }

    private IEnumerator ReturnToPreviousPosition(Vector3 newPos)
    {
        while (Mathf.Abs(transform.position.z - newPos.z) > 0.05f)
        {
            float step = sideDashSpeed * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(transform.position.z, newPos.z, step));
            //transform.position = Vector3.MoveTowards(transform.position, newPos, step);

            yield return null;
        }

        changeDirection = false;
        SetPlayerPosition((int)newPos.z);
        transform.rotation = Quaternion.Euler(0, 90, 0);
        isMoving = false;

    }

        private IEnumerator LerpPosition(Vector3 newPos)
    {

        while (Mathf.Abs(transform.position.z - newPos.z) > 0.05f)
        {
            //transform.position.z != newPos.z
            if (changeDirection) break;
            
            float step = sideDashSpeed * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(transform.position.z, newPos.z, step));
            //transform.position = Vector3.MoveTowards(transform.position, newPos, step);

            yield return null;
        }

        //print("Move " + moveCount + " ended in " + playerPosition.ToString());

        if(!changeDirection)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
            isMoving = false;
            SetPlayerPosition((int)newPos.z);

            if (doubleMove && Mathf.Abs(previousPos.z) > 0.5f)
            {
                var left = true ? previousPos.z < 0 : previousPos.z > 0;
                DoubleMove(left);
            }
        }
    }

    private void DoubleMove(bool left)
    {
        if (left)
        {
            Move(45f, new Vector3(0, 0, 6));
        }
        else Move(135f, new Vector3(0, 0, -6));
    }

    private void CheckJumpInput(SwipeDirection direction = SwipeDirection.none)
    {
        if (!isAlive) return;

        if (jumpInput && StandOnGround)
        {
            if(Time.time <= pretendToJumpTime + inputOnDelay)
            {
                Jump();

                return;
            }

            jumpInput = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) || direction == SwipeDirection.up)
        {
            if (canJump && StandOnGround)
            {
                Jump();
            }
            else
            {
                pretendToJumpTime = Time.time;
                jumpInput = true;
            }
        }
    }

    private void Jump()
    {
        playerRigidBody.velocity = new Vector3(playerRigidBody.velocity.x, 0f, playerRigidBody.velocity.z);

        playerRigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        playerAnimation.SetTrigger("Jump_trig");
        playerAudio.PlayOneShot(jumpSound, 1.0f);

        jumpInput = false;
    }

    private void CheckCrouchInput(SwipeDirection direction = SwipeDirection.none)
    {
        if (!isAlive) return;

        if (direction == SwipeDirection.down || Input.GetKeyDown(KeyCode.LeftControl))
        {
            if(StandOnGround)
            {
                StartCoroutine(Crouch());
            }
            else
            {
                playerRigidBody.AddForce(Vector3.down * 1000, ForceMode.Impulse);
            }
            
        }
    }

    private IEnumerator Crouch()
    {
        // TODO Fix crouch
        if (isCrouching)
        {
            keepCrouching = true;
            yield return new WaitWhile(() => isCrouching);
        }

        if (!keepCrouching)
        {
            playerAnimation.SetBool("Crouch_b", true);
            playerAnimation.SetFloat("Speed_f", 0);

            boxCollider.size = new Vector3(boxCollider.size.x, origColliderHeight / 2, boxCollider.size.z);
            boxCollider.center = new Vector3(boxCollider.center.x, origColliderCenterY / 2, boxCollider.center.z);
            playerAudio.PlayOneShot(moveSound, 1f);
        }
        else keepCrouching = false;

        isCrouching = true;

        yield return new WaitForSeconds(crouchTime);

        isCrouching = false;

        if (!keepCrouching)
        {
            playerAnimation.SetFloat("Speed_f", 1);
            playerAnimation.SetBool("Crouch_b", false);

            boxCollider.size = new Vector3(boxCollider.size.x, origColliderHeight, boxCollider.size.z);
            boxCollider.center = new Vector3(boxCollider.center.x, origColliderCenterY, boxCollider.center.z);
        }

    }

    private void PlayVisualEffects()
    {
        if(StandOnGround && !dirtPartical.isPlaying)
        {
            dirtPartical.Play();
        }
        else if(!StandOnGround && dirtPartical.isPlaying)
        {
            dirtPartical.Stop();
        }
    }

    #region StatesChecks

    private bool StandOnGround => Physics.Raycast(groundCheck.position, Vector3.down, groundCheckLength, whatIsGround);

    private bool screenWasTouched => Input.touchCount == 1;

    private void OnCollisionEnter(Collision collision)
    {
        var tag = collision.gameObject.tag;

        if (tag != "Obstacle") return;

        GameManager.Instance.GameOver();

        if (GameManager.Instance.immortality)
        {
            collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.right * 250f, ForceMode.Impulse);

            GameManager.Instance.DestroyObstacle(collision.gameObject);
        }
      
    }

    private void OnTriggerEnter(Collider other)
    {
        var pickable = other.transform.GetComponentInChildren<Pickable>();

        if (pickable == null) return;

        pickable.Effect();

        if (pickable.gameObject.TryGetComponent<Edible>(out var edible))
        {
            var ps = eatParticle.main;
            ps.startColor = edible.color;

            eatParticle.Play();

        }

        GameObject.Destroy(pickable.gameObject);
    }
    public void Death()
    {
        explosionParticle.Play();
        dirtPartical.Stop();
        dirtPartical.gameObject.SetActive(false);
        playerAnimation.SetBool("Death_b", true);
        playerAnimation.SetInteger("DeathType_int", 1);
        isAlive = false;
        playerAudio.PlayOneShot(crashSound, 1.0f); 
    }

    #endregion

    #region junkyard

    /*
  */

    /*int check = (int)Mathf.Sign(newPos.z);

       if(check > 0)
       {
           if (transform.position.z + check >= newPos.z)
           {
               if(transform.rotation.y != 90)
               {
                   transform.rotation = Quaternion.Euler(0, 90, 0);
               }

               return;
           }
       }
       else if (transform.position.z + check <= newPos.z)
       {
           if (transform.rotation.y != 90)
           {
               transform.rotation = Quaternion.Euler(0, 90, 0);
           }

           return;
       }
       */



    /*if (isMoving && !doubleMove)
    {
        doubleMove = true;
        return;
    }

    if (!StandOnGround)
    {
        if(!inAirMoveInput)
        {
            inAirMoveInput = true;
            pretendToMoveTime = Time.time;
            return;
        }
        return; 
    }*/

    /*
   private bool InteractionCheck(out GameObject collider)
   {
       if(Physics.Raycast(interactCheck.position, Vector3.forward, out RaycastHit hit, interactionCheckDistance, whatIsInteractable))
       {
           collider = hit.collider.gameObject;
           return true;
       }

       collider = null;
       return false;

   }

   private void OnInteractableEnter(GameObject collision)
   {
       var tag = collision.tag;

       switch (tag)
       {
           case "Obstacle":

               GameManager.Instance.GameOver();

               if (GameManager.Instance.immortality)
               {
                   collision.GetComponent<Rigidbody>().AddForce(Vector3.right * 250f, ForceMode.Impulse);
               }

               break;
           case "Pickable":

               var pickable = collision.transform.GetComponentInChildren<Pickable>();

               pickable.Effect();

               if (pickable.gameObject.TryGetComponent<Edible>(out var edible))
               {
                   var ps = eatParticle.main;
                   ps.startColor = edible.color;

                   eatParticle.Play();
               }

               GameObject.Destroy(collision);
               break;
           default:

               break;
       }


   }
   */

    #endregion
}
