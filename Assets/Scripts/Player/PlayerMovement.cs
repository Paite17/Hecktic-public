using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System.Collections.Generic;

public enum MovementState
{
    STATIONARY,
    WALKING,
    RUNNING,
    FROZEN,
    ELECTROCUTED,
    BELLY_FLOP,
    DEAD
}

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Player Stats")]
    [SerializeField] private float defaultSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float invincibleRunningSpeed;
    [SerializeField] private float giantMovementSpeed;
    private float acceleration;
    [SerializeField] private float defAcceleration;  // default acceleration
    [SerializeField] private float runningAcceleration;
    [SerializeField] private float jumpingAcceleration; // The acceleration used when jumping
    private float deceleration;
    [SerializeField] private float defDeceleration;   // default deceleration
    [SerializeField] private float slidingDeceleration;
    [SerializeField] private float jumpStunDuration;
    [SerializeField] private float bumpStunDuration;
    [SerializeField] private float bumpForce;  // the pushback force of bumping into another player
    [SerializeField] private float jumpPower;
    [SerializeField] private float bellyFlopStrength;
    [SerializeField] private float wallSlideSpeed;
    [SerializeField] private float bellyFlopWindupLength;
    [SerializeField] private float rockLaunchSpeed;  // rock powerup
    [SerializeField] private float rockThrowCooldownLength;
    [SerializeField] private float invincibilityLength;
    [SerializeField] private float giantLength;  // rather unfortunate name 
    [SerializeField] private float giantSize;   // the size in which you become when getting the giant powerup
    [SerializeField] private float giantGrowthDuration;
    [SerializeField] private float icePowerupCooldownLength;
    [SerializeField] private float iceBallMoveSpeed;
    [SerializeField] private float damageCooldownLength;
    [SerializeField] private float freezeButtonCheck;
    [SerializeField] private float mcGuffinLaunchSpeed;


    [Header("Player Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TMP_Text playerNameLabel;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private GameObject model;     // might be redundant
    [SerializeField] private InputAction jump;
    [SerializeField] private GameObject bodyObj;
    [SerializeField] private GameObject armsObj;
    [SerializeField] private Transform mcGuffinLaunchPoint;

    // need this for a thing
    [Header("Models and Other References")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private List<AudioClip> playerSounds;
    [SerializeField] private GameObject bellyFlopHitbox;
    [SerializeField] private GameObject iceBlock;

    [Header("Powerup Specific Object refs")]
    [SerializeField] private Transform rockLaunchPoint;
    [SerializeField] private GameObject electricityHitbox;
    [SerializeField] private List<Material> powerUpMaterials;
    [SerializeField] private Transform iceBallSpawn;
    //[SerializeField] private GameObject rockObj;   // not actually needed cus PhotonNetwork.Instantiate() doesn't take GameObjects as an argument


    //[SerializeField] private LayerMask WallLayer;

    [Header("Misc variables")]

    public MovementState movementState;

    public CharPowerupState powerupState;

    public Player thisPlayer;

    private Vector2 horizontal;

    private bool running;

    private bool isWallSliding;

    [SerializeField] private bool canMove = true;

    private UIScript ui;

    [SerializeField] private bool isFacingRight;

    private bool isBellyFlopping;

    private bool isWallJumping;

    private float wallJumpDirection;

    private float wallJumpTime = 0.2f;

    private float wallJumpCounter;

    private float wallJumpDuration = 0.4f;

    private Vector2 wallJumpPower = new Vector2(3f, 14f);

    private float mapLoopCooldown = 0.5f;

    private bool flopFinish;

    private float floppingTimer;

    private Vector2 moveVelocity;

    private Camera mainCamera;

    private bool freaky2 = false;

    private bool freaky3 = false;

    private bool isAirbourne;

    private bool isTurning;  // for when the player is turning and they're sliding a bit

    private bool isStunned;

    private float rockThrowCooldownTimer;

    private bool rockThrowActivated;  // true when rock throw has just been used, false when cooldown expires

    private PlayerStatus status;

    private bool bruh;

    private PlayerInput playerInput;

    private bool isInvincible;

    private Vector3 originalScale;

    private bool isGiant;

    private Vector3 originalGroundCheckPos;

    private float originalGroundCheckRadius;

    private float groundCheckRadius;

    private bool canDropMcGuffin;

    [SerializeField] private CharPowerupState originalPowerupState;   // used to revert to other powerup after invincibility ends

    private int matIndexRef;

    private float freezeTimer;

    

    private bool icePowerupActivated;  // used to determine if the ice ball cooldown needs to start

    private float icePowerupCooldownTimer;

    private bool tookDamage;   // identifying when to use damageCooldownTiemr

    private float damageCooldownTimer;

    private bool usingElectric;

    private Pipe currentPipe;

    private Parallax[] allBGs;

    private bool canJump;

    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }

    public GameObject LewisModel
    {
        get { return model; }
        set { model = value; }
    }

    public bool IsBellyFlopping
    {
        get { return isBellyFlopping; }
    }

    public bool IsInvincible
    {
        get { return isInvincible; }
        set { isInvincible = value; }
    }

    public bool IsGiant
    {
        get { return IsGiant; }
        set { IsGiant = value; }
    }

    public PlayerStatus Status
    {
        get { return status; }
        set { status = value; }
    }

    public float FreezeButtonCheck
    {
        get { return  freezeButtonCheck; } 
        set { freezeButtonCheck = value; }
    }

    public float CurrentSpeed
    {
        get { return currentSpeed; }
    }

    public float Acceleration
    {
        get { return acceleration; }
    }

    public float Deceleration
    {
        get { return deceleration; }
    }

    public bool IsFacingRight
    {
        get { return isFacingRight; }
    }

    public bool CanJump
    {
        get { return canJump; }
        set { canJump = value; }
    }

    private void OnEnable()
    {
        jump.Enable();
    }

    private void OnDisable()
    {
        jump.Disable();
    }

    private void Start()
    {
        canJump = true;
        originalGroundCheckPos = groundCheck.localPosition;
        originalGroundCheckRadius = 0.2f;
        groundCheckRadius = originalGroundCheckRadius;
        originalScale = transform.localScale;
        playerInput = GetComponent<PlayerInput>();
        status = GetComponent<PlayerStatus>();
        ui = FindObjectOfType<UIScript>();
        //anim = GetComponent<Animator>();
        currentSpeed = defaultSpeed;
        //playerNameLabel.text = PhotonNetwork.NickName;

        thisPlayer = PhotonNetwork.LocalPlayer;
        //FindObjectOfType<CameraFollow>().OnConnectionToGame(GetComponent<PhotonView>().ViewID);
        
        if (photonView.IsMine)
        {
            mainCamera = Camera.main;
            mainCamera.GetComponent<CameraFollow>().target = transform;
            mainCamera.GetComponent<CameraFollow>().hasConnected = true;
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), bellyFlopHitbox.GetComponent<Collider2D>());
        }
        acceleration = defAcceleration;
        deceleration = defDeceleration;

        allBGs = FindObjectsOfType<Parallax>();
        canDropMcGuffin = true;
    }


    // Update is called once per frame
    void Update()
    {
        mapLoopCooldown -= Time.deltaTime;

        if (ui.MenuActive)
        {
            canMove = false;
        }

        if (!isWallJumping)
        {
            
            Flip();
        }
        
        WallSlide();
        WallJump();

        // TODO: Animation controller

        if (isBellyFlopping)
        {
            floppingTimer += Time.deltaTime;
        }

        if (movementState == MovementState.BELLY_FLOP)
        {
            horizontal.x = 0;
        }

        if (!IsGrounded())
        {
            isAirbourne = true;
        }
        else
        {
            isAirbourne = false;
        }

        if (isAirbourne)
        {
            acceleration = jumpingAcceleration;
        }

        // direction change check
        if (horizontal.x <= -1 && rb.velocity.x > 0)
        {
            isTurning = true;
        }
        else if (horizontal.x >= 1 && rb.velocity.x < 0)
        {
            isTurning = true;
        }
        else
        {
            isTurning = false;
        }

        // sliding deceleration control
        if (isTurning)
        {
            deceleration = slidingDeceleration;
            
            if (!isAirbourne)
            {
                acceleration = slidingDeceleration;
            }
        }
        
        if (!isTurning && !isAirbourne)
        {
            deceleration = defDeceleration;
            acceleration = defAcceleration;
        }

        if (powerupState != CharPowerupState.GIANT)
        {
            anim.SetBool("Running", running);
        }
        else
        {
            anim.SetBool("Running", false);
        }
        anim.SetFloat("Movement", horizontal.x);
        anim.SetFloat("YVelocity", rb.velocity.y);
        anim.SetBool("Grounded", IsGrounded());
        anim.SetBool("Stunned", isStunned);
        anim.SetBool("Sliding", isTurning);
        anim.SetBool("Electric", usingElectric);


        if (rockThrowActivated)
        {
            rockThrowCooldownTimer += Time.deltaTime;
        }

        if (rockThrowCooldownTimer >= rockThrowCooldownLength)
        {
            rockThrowActivated = false;
            rockThrowCooldownTimer = 0;
        }

        if (icePowerupActivated)
        {
            icePowerupCooldownTimer += Time.deltaTime;
        }

        if (icePowerupCooldownTimer >= icePowerupCooldownLength)
        {
            icePowerupActivated = false;
            icePowerupCooldownTimer = 0;
        }

        if (tookDamage)
        {
            damageCooldownTimer += Time.deltaTime;
        }

        if (damageCooldownTimer >= damageCooldownLength)
        {
            tookDamage = false;
            damageCooldownTimer = 0;
        }

        if (movementState == MovementState.FROZEN)
        {
            canMove = false;
            freezeTimer += Time.deltaTime;

            if (freezeTimer >= 6f || freezeButtonCheck <= 0)
            {
                BreakFreeze();
            }
        }

        if (mapLoopCooldown <= 0)
        {
            anim.SetBool("Teleport", false);
        }

    }

    
    private void FixedUpdate()
    {
        if (isBellyFlopping)
        {
            StartBellyFlopping();
        }

        if (!isWallJumping)
        {
            moveVelocity = new Vector2(horizontal.x * currentSpeed, rb.velocity.y);

            if (horizontal.x != 0)
            {
                if (canMove)
                {
                    rb.velocity = new Vector2(Vector2.MoveTowards(new Vector2(rb.velocity.x, 0), new Vector2(moveVelocity.x, 0), acceleration * Time.fixedDeltaTime).x, rb.velocity.y);
                    
                }     
                
            }
            else
            {
                rb.velocity = new Vector2(Vector2.MoveTowards(new Vector2(rb.velocity.x, 0), new Vector2(0, 0), deceleration * Time.fixedDeltaTime).x, rb.velocity.y);
            }

            //rb.velocity = new Vector2(horizontal.x * currentSpeed, rb.velocity.y);
            //rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y);
        }
    }

    private void BreakFreeze()
    {
        movementState = MovementState.STATIONARY;
        FindObjectOfType<AudioManager>().Play("FreezeBreak");
        // TODO: disable ice
        freezeTimer = 0;
        canMove = true;
        bodyObj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
        armsObj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
        photonView.RPC("RevertMaterialColour", RpcTarget.All);
        iceBlock.SetActive(false);
    }

    [PunRPC]
    private void RevertMaterialColour()
    {
        bodyObj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
        armsObj.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 255);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "LeftSideLevelLoop":
                if (mapLoopCooldown <= 0f)
                {
                    //Vector3 oldPos = Camera.main.transform.position;

                    float playerOldX = transform.position.x;

                    Debug.Log("left side collision");
                    if (!GameManager.Instance.CurrentMapData.StageisVertical)
                    {
                        transform.position = new Vector2(GameManager.Instance.CurrentMapData.RightSideEdge, transform.position.y);
                    }
                    else
                    {
                        transform.position = new Vector2(transform.position.x, GameManager.Instance.CurrentMapData.RightSideEdge);
                    }

                    float playerNewX = transform.position.x;
                    if (photonView.IsMine)
                    {
                        foreach (var current in allBGs)
                        {
                            if (!current.dontBeFunny)
                            {
                                current.offsetX += playerNewX - playerOldX;
                            }

                            //current.likeUmIgnoreUpdateOrIgnoreCameraMoveSure = 10;
                        }
                        FindObjectOfType<CameraFollow>().UpdateLocation();
                    }
                    
                    mapLoopCooldown = 0.5f;
                    anim.SetBool("Teleport", true);
                }
                
                break;
            case "RightSideLevelLoop":
                if (mapLoopCooldown <= 0f)
                {
                    float playerOldX = transform.position.x;

                    Debug.Log("right side collision");
                    if (!GameManager.Instance.CurrentMapData.StageisVertical)
                    {
                        transform.position = new Vector2(GameManager.Instance.CurrentMapData.LeftSideEdge, transform.position.y);
                    }
                    else
                    {
                        transform.position = new Vector2(transform.position.x, GameManager.Instance.CurrentMapData.LeftSideEdge);
                    }

                    float playerNewX = transform.position.x;
                    
                    if (photonView.IsMine)
                    {
                        foreach (var current in allBGs)
                        {
                            current.offsetX += playerNewX - playerOldX;
                            //current.likeUmIgnoreUpdateOrIgnoreCameraMoveSure = 10;
                        }
                        FindObjectOfType<CameraFollow>().UpdateLocation();
                    }
                    

                    mapLoopCooldown = 0.5f;
                    anim.SetBool("Teleport", true);
                }
                break;
            case "KillHazard":
                Debug.Log("death lmao");
                if (photonView.IsMine)
                {
                    StartCoroutine(DeathSequence());
                }
                
                break;
            case "BellyFlop":
                // make sure its not the player's own
                rb.velocity = new Vector2(Vector2.right.x * bumpForce * 2, rb.velocity.y);
                photonView.RPC("BouncedOn", RpcTarget.All, photonView.ViewID);

                break;
            case "McGuffin":
                collision.gameObject.GetComponentInParent<McGuffin>().CollidingWithPlayer(this);
                break;
            case "Rock":
                // TODO: deal damage to this player
                if (powerupState != CharPowerupState.REGULAR)
                {
                    TakeDamage(false);
                }
                else
                {
                    TakeDamage(true);
                }
                break;
            case "Coin":
                if (photonView.IsMine)
                {
                    FindObjectOfType<AudioManager>().Play("CoinCollect");
                    status.AmountOfCoins++;
                    
                    if (status.AmountOfCoins >= 8)
                    {
                        status.AmountOfCoins = 0;
                        float percentage = (0.5f + ((100f * status.AmountOfMcGuffins) / GameManager.Instance.MaxMcGuffinCount));
                        //GameManager.Instance.SpawnPowerup(transform, percentage);
                        GameManager.Instance.photonView.RPC("SpawnPowerup", RpcTarget.All, percentage, photonView.ViewID);
                    }
                }

                collision.gameObject.SetActive(false);
                break;
            case "Powerup":
                if (powerupState != CharPowerupState.GIANT && powerupState != CharPowerupState.INVINCIBLE)
                {
                    // TODO: read powerup component to get correct powerup state
                    if (collision.GetComponentInParent<Powerup>().GivenPowerup == CharPowerupState.GIANT)
                    {
                        if (photonView.IsMine)
                        {
                            SetPowerupMaterial(0);

                            // hack fix for electric use when not in possesion of the powerup
                            usingElectric = false;
                            electricityHitbox.SetActive(false);
                            FindObjectOfType<AudioManager>().StopMusic("ElectricPowerup");
                        }

                        BecomeGiant();
                        //collision.gameObject.GetComponentInParent<Powerup>().CollidingWithPlayer(this);
                    }
                    else if (collision.GetComponentInParent<Powerup>().GivenPowerup == CharPowerupState.INVINCIBLE)
                    {
                        // TODO: invincible activation
                        if (photonView.IsMine)
                        {
                            originalPowerupState = powerupState;
                            SetPowerupMaterial(2);
                            BecomeInvincible();

                            // hack fix for electric use when not in possesion of the powerup
                            usingElectric = false;
                            electricityHitbox.SetActive(false);
                            FindObjectOfType<AudioManager>().StopMusic("ElectricPowerup");
                        }
                        //collision.gameObject.GetComponentInParent<Powerup>().CollidingWithPlayer(this);
                    }
                    else
                    {

                        // get correct index
                        switch (collision.GetComponentInParent<Powerup>().GivenPowerup)
                        {
                            case CharPowerupState.ROCK_THROW:
                                if (photonView.IsMine)
                                {
                                    SetPowerupMaterial(1);

                                    // hack fix for electric use when not in possesion of the powerup
                                    usingElectric = false;
                                    electricityHitbox.SetActive(false);
                                    FindObjectOfType<AudioManager>().StopMusic("ElectricPowerup");
                                }
                                break;
                            case CharPowerupState.ICE:
                                if (photonView.IsMine)
                                {
                                    SetPowerupMaterial(3);

                                    // hack fix for electric use when not in possesion of the powerup
                                    usingElectric = false;
                                    electricityHitbox.SetActive(false);
                                    FindObjectOfType<AudioManager>().StopMusic("ElectricPowerup");
                                }
                                break;
                            case CharPowerupState.ELECTRIC:
                                if (photonView.IsMine)
                                {
                                    SetPowerupMaterial(4);

                                }
                                break;
                            default:
                                if (photonView.IsMine)
                                {
                                    SetPowerupMaterial(0);

                                    // hack fix for electric use when not in possesion of the powerup
                                    usingElectric = false;
                                    electricityHitbox.SetActive(false);
                                    FindObjectOfType<AudioManager>().StopMusic("ElectricPowerup");
                                }
                                break;

                        }
                        
                    }

                    collision.gameObject.GetComponentInParent<Powerup>().CollidingWithPlayer(this);
                }
                else
                {
                    FindObjectOfType<AudioManager>().Play("Powerup");

                    collision.gameObject.GetComponentInParent<Powerup>().DestroyWithoutGivingPower();
                }
                
                break;
            case "Electricity":
                if (photonView.IsMine)
                {
                    if (powerupState != CharPowerupState.REGULAR)
                    {
                        TakeDamage(false);
                    }
                    else
                    {
                        TakeDamage(true);
                    }
                                     
                }              
                break;
            case "Pipe":
                currentPipe = collision.gameObject.GetComponent<Pipe>();
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Pipe":
                currentPipe = null;
                break;
        }
    }

    public void SetPowerupMaterial(int materialIndex)
    {
        if (materialIndex != 2)
        {
            matIndexRef = materialIndex;
        }
        bodyObj.GetComponent<Renderer>().material = powerUpMaterials[materialIndex];
        armsObj.GetComponent<Renderer>().material = powerUpMaterials[materialIndex];
        photonView.RPC("SetPowerupMaterialRPC", RpcTarget.All, materialIndex);
    }

    [PunRPC]
    private void SetPowerupMaterialRPC(int materialIndex)
    {
        bodyObj.GetComponent<Renderer>().material = powerUpMaterials[materialIndex];
        armsObj.GetComponent<Renderer>().material = powerUpMaterials[materialIndex];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PhotonView other = collision.gameObject.GetComponent<PhotonView>();
        
        if (mapLoopCooldown <= 0)
        {
            switch (collision.gameObject.tag)
            {
                case "Player":
                    if (mapLoopCooldown <= 0)
                    {
                        PlayerMovement otherPlr = other.gameObject.GetComponent<PlayerMovement>();
                        if (photonView.IsMine)
                        {

                            if (otherPlr.powerupState == CharPowerupState.GIANT || otherPlr.powerupState == CharPowerupState.INVINCIBLE)
                            {
                                Debug.LogWarning("Someone is giant or invincible");
                                //other.GetComponent<PlayerMovement>().TakeDamage(true);
                                /*if (otherPlr.powerupState == CharPowerupState.GIANT || otherPlr.powerupState == CharPowerupState.INVINCIBLE)
                                {
                                    photonView.RPC("TakeDamageFromPlayer", RpcTarget.All, other.ViewID, true);
                                } */
                                TakeDamage(true);


                            }
                        }

                        if (powerupState != CharPowerupState.GIANT && powerupState != CharPowerupState.INVINCIBLE && otherPlr.powerupState != CharPowerupState.GIANT && otherPlr.powerupState != CharPowerupState.INVINCIBLE)
                        {
                            Vector2 collisionDirection = collision.contacts[0].normal;
                            // checks if the collision happened from above this player
                            if (collision.relativeVelocity.y > 0f)
                            {
                                if (other != null)
                                {
                                    photonView.RPC("BouncedOn", RpcTarget.All, other.ViewID);

                                }
                            }
                            else
                            {
                                // checking if a collision is horizontal instead
                                if (Mathf.Abs(collisionDirection.x) > Mathf.Abs(collisionDirection.y))
                                {
                                    PlayerMovement otherPlayer = collision.gameObject.GetComponent<PlayerMovement>();

                                    if (otherPlayer != null)
                                    {
                                        photonView.RPC("HandleBump", RpcTarget.All, otherPlayer.photonView.ViewID);

                                    }
                                }
                            }
                        }
                    }
                    break;
                case "Rock":
                    TakeDamage(false);
                    break;
                case "Enemy":
                    if (photonView.IsMine)
                    {
                        if (powerupState != CharPowerupState.GIANT && powerupState != CharPowerupState.INVINCIBLE)
                        {
                            if (collision.gameObject.GetComponent<Enemy>().GloopState_ == GloopState.SHELL)
                            {
                                return;
                            }
                            else
                            {
                                Vector2 collisionDirection = collision.contacts[0].normal;
                                if (Mathf.Abs(collisionDirection.x) > Mathf.Abs(collisionDirection.y))
                                {
                                    if (collision.gameObject.GetComponent<Enemy>().GloopState_ == GloopState.SHELL || collision.gameObject.GetComponent<Enemy>().GloopState_ == GloopState.SLIDE)
                                    {
                                        if (collision.gameObject.GetComponent<Enemy>().SlideCooldown <= 0)
                                        {
                                            if (powerupState != CharPowerupState.REGULAR)
                                            {
                                                TakeDamage(false);
                                            }
                                            else
                                            {
                                                TakeDamage(true);
                                            }
                                        }                                      
                                    }
                                    else
                                    {
                                        if (powerupState != CharPowerupState.REGULAR)
                                        {
                                            TakeDamage(false);
                                        }
                                        else
                                        {
                                            TakeDamage(true);
                                        }
                                    }
                                }
                            }
                        }


                    }
                    break;

            }
        }
       
    }

    [PunRPC]
    private void RockMoment()
    {
        TakeDamage(false);
    }


    [PunRPC]
    private void HandleBump(int otherViewID)
    {
        PhotonView otherView = PhotonView.Find(otherViewID);
        if (otherView != null)
        {
            // get PlayerMovement of other
            PlayerMovement otherPlr = otherView.GetComponent<PlayerMovement>();

            if (otherPlr != null)
            {
                if (powerupState != CharPowerupState.GIANT && otherPlr.powerupState != CharPowerupState.GIANT)
                {
                    Rigidbody2D otherRB = otherPlr.GetComponent<Rigidbody2D>();

                    // calc the direction of being pushed based on position
                    Vector2 pushDir = transform.position.x > otherPlr.transform.position.x ? Vector2.right : Vector2.left;

                    // apply force
                    rb.velocity = new Vector2(pushDir.x * bumpForce, rb.velocity.y);
                    otherRB.velocity = new Vector2(-pushDir.x * bumpForce, otherRB.velocity.y);

                    // direction for mcguffin drop
                    Vector2 bounceDir = (otherRB.transform.position - transform.position).normalized;
                    Vector3 offsetMoment = new Vector3(0, 2, 0);
                    Vector2 dropPos = transform.position + offsetMoment;
                    photonView.RPC("DropMcGuffin", RpcTarget.All, dropPos, bounceDir, otherViewID);
                    // stun em both
                    StunPlayer(true);
                }
                else if (powerupState == CharPowerupState.GIANT && otherPlr.powerupState == CharPowerupState.GIANT)
                {
                    Rigidbody2D otherRB = otherPlr.GetComponent<Rigidbody2D>();

                    // calc the direction of being pushed based on position
                    Vector2 pushDir = transform.position.x > otherPlr.transform.position.x ? Vector2.right : Vector2.left;

                    // apply force
                    rb.velocity = new Vector2(pushDir.x * bumpForce, rb.velocity.y);
                    otherRB.velocity = new Vector2(-pushDir.x * bumpForce, otherRB.velocity.y);

                    // stun em both
                    StunPlayer(true);
                }
                
            }
        }

    }

    // jump input
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            if (canJump)
            {
                if (powerupState != CharPowerupState.GIANT)
                {
                    playerAudioSource.clip = playerSounds[0];
                    playerAudioSource.Play();
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                }
                else
                {
                    playerAudioSource.clip = playerSounds[11];
                    playerAudioSource.Play();
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower / 1.2f);
                }


                anim.SetBool("FlopBegin", false);
                anim.SetBool("FlopHappening", false);
                anim.SetBool("FlopDone", false);
            }        
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    // sprint input (May also include powerup activation? (no))
    public void Sprint(InputAction.CallbackContext context)
    {
        if (powerupState != CharPowerupState.GIANT)
        {
            if (context.performed)
            {
                movementState = MovementState.RUNNING;

                if (powerupState == CharPowerupState.INVINCIBLE)
                {
                    currentSpeed = invincibleRunningSpeed;
                }
                else
                {
                    currentSpeed = runningSpeed;
                }
                

                if (horizontal.x != 0)
                {
                    running = true;
                }

            }

            if (context.canceled)
            {
                anim.SetBool("Running", false);
                currentSpeed = defaultSpeed;
                running = false;
            }
        }
        
    }

    // tells whatever calls it if the player is grounded or not
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer);
    }



    private void WallJump()
    {
        if (powerupState != CharPowerupState.GIANT)
        {
            if (isWallSliding)
            {
                isWallJumping = false;
                // gotta multiply by 10 cus the model is small whoopsies

                wallJumpDirection = modelTransform.localScale.z * 10;

                wallJumpCounter = wallJumpTime;
                CancelInvoke(nameof(StopWallJump));
            }
            else
            {
                wallJumpCounter -= Time.deltaTime;
            }

            if (jump.WasPerformedThisFrame() && wallJumpCounter > 0f)
            {

                isWallJumping = true;
                rb.velocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
                wallJumpCounter = 0f;

                if (modelTransform.localScale.z != wallJumpDirection)
                {
                    isFacingRight = !isFacingRight;
                    Vector3 localScale = modelTransform.localScale;
                    localScale.z *= -1f;
                    modelTransform.localScale = localScale;
                }
                playerAudioSource.clip = playerSounds[2];
                playerAudioSource.Play();
                Invoke(nameof(StopWallJump), wallJumpDuration);
            }
        }
        else
        {
            return;
        }
        
    }

    private void StopWallJump()
    {
        isWallJumping = false;
    }

    private void WallSlide()
    {   
        if (IsWalled() && !IsGrounded() && horizontal.x != 0f)
        {

            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
            anim.SetBool("WallJump", true);

            if (!freaky2)
            {
                //playerAudioSource.clip = playerSounds[1];
                //playerAudioSource.Play();
                freaky2 = true;
            }
            
        }
        else
        {
            freaky2 = false;
            isWallSliding = false;
            anim.SetBool("WallJump", false);
            //playerAudioSource.Stop();
        }

       
    }

    // maybe also in crouching when anim is made?
    public void BellyFLop(InputAction.CallbackContext context)
    {
        
        if (!IsGrounded())
        {
            if (!flopFinish)
            {
                if (canMove)
                {
                    movementState = MovementState.BELLY_FLOP;
                    anim.SetBool("FlopBegin", true);
                    isBellyFlopping = true;
                    // clear rb force and float for a mo
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0;
                    rb.gravityScale = 0f;
                    flopFinish = true;
                }
                
            }
        }

        if (context.canceled)
        {
            flopFinish = false;
            
        }

    }

    // everybody do the flop *dies*
    private void StartBellyFlopping()
    {
        
        if (!freaky3)
        {
            if (powerupState != CharPowerupState.GIANT)
            {
                playerAudioSource.clip = playerSounds[5];
                playerAudioSource.Play();
            }
            freaky3 = true;
        }

        if (powerupState != CharPowerupState.GIANT)
        {
            groundCheckRadius = 0.5f;
        }
        

        bool freaky = false;
        // TODO: change this to last the speed of the windup animation
        if (floppingTimer >= bellyFlopWindupLength)
        {
            anim.SetBool("FlopHappening", true);
            rb.gravityScale = 2.3f;
            freaky = true;
        }
        
        if (freaky)
        {
            if (!IsGrounded())
            {
                rb.AddForce(Vector2.down * bellyFlopStrength, ForceMode2D.Impulse);
                bellyFlopHitbox.SetActive(true);
            }
            else
            {
                if (powerupState != CharPowerupState.GIANT)
                {
                    playerAudioSource.clip = playerSounds[6];
                    playerAudioSource.Play();
                }
                else
                {
                    playerAudioSource.clip = playerSounds[10];
                    playerAudioSource.Play();
                }
                anim.SetBool("FlopDone", true);
                isBellyFlopping = false;
                floppingTimer = 0;
                movementState = MovementState.STATIONARY;
                freaky3 = false;
                bellyFlopHitbox.SetActive(false);
                flopFinish = true;
                if (powerupState != CharPowerupState.GIANT)
                {
                    groundCheckRadius = 0.2f;
                }
                
            }
        }
       
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (movementState == MovementState.FROZEN)
        {
            if (context.canceled)
            {
                freezeButtonCheck--;
            }
        }

        if (canMove)
        {
            if (movementState != MovementState.BELLY_FLOP && movementState != MovementState.FROZEN)
            {
                // this is probably not the way to do this
                if (context.performed)
                {

                    if (movementState != MovementState.RUNNING)
                    {
                        movementState = MovementState.WALKING;
                        anim.SetBool("Walking", true);
                        anim.SetBool("FlopBegin", false);
                        anim.SetBool("FlopHappening", false);
                        anim.SetBool("FlopDone", false);
                    }
                    else
                    {
                        if (horizontal.x != 0)
                        {
                            running = true;
                        }
                    }
                }
                else if (context.canceled)
                {
                    movementState = MovementState.STATIONARY;
                    anim.SetBool("Walking", false);
                    horizontal = Vector2.zero;
                }


                horizontal = context.ReadValue<Vector2>();
            }
        }
        else
        {
            horizontal = Vector2.zero;
            anim.SetBool("Walking", false);
        }


    }

    private void Flip()
    {
        if (isFacingRight && rb.velocity.x < 0f || !isFacingRight && rb.velocity.x > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = modelTransform.localScale;
            localScale.z *= -1f;
            modelTransform.localScale = localScale;  
        }
    }

    public void PipeEnter()
    {
        if (photonView.IsMine)
        {
            if (currentPipe != null)
            {
                if (powerupState != CharPowerupState.GIANT)
                {
                    rb.velocity = Vector2.zero;
                    currentPipe.GoDownPipe(gameObject);
                }
                
            }
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(bellyFlopHitbox.activeSelf);
            stream.SendNext(electricityHitbox.activeSelf);
            stream.SendNext(powerupState);
            stream.SendNext(movementState);
            stream.SendNext(mapLoopCooldown);
            stream.SendNext(freezeButtonCheck);
            stream.SendNext(iceBlock.activeSelf);
        }
        else
        {
            bellyFlopHitbox.SetActive((bool)stream.ReceiveNext());
            electricityHitbox.SetActive((bool)stream.ReceiveNext());
            powerupState = (CharPowerupState)stream.ReceiveNext();
            movementState = (MovementState)stream.ReceiveNext();
            mapLoopCooldown = (float)stream.ReceiveNext();
            freezeButtonCheck = (float)stream.ReceiveNext();
            iceBlock.SetActive((bool)stream.ReceiveNext());
        }
    }

    // pressing the action button when theres a powerup with an action
    public void PowerupUse(InputAction.CallbackContext context)
    {
        Vector2 prevHoriz = horizontal;
        bool b = false;
        Debug.Log("PowerupUse() called!");
        if (canMove)
        {
            if (powerupState != CharPowerupState.INVINCIBLE)
            {
                switch (powerupState)
                {
                    case CharPowerupState.ROCK_THROW:
                        // TODO: spawn rock obj that goes on an arc
                        if (!rockThrowActivated)
                        {
                            if (photonView.IsMine)
                            {
                                FindObjectOfType<AudioManager>().Play("BoulderThrow");
                            }
                            GameObject tempProj = PhotonNetwork.Instantiate("Rock", rockLaunchPoint.position, rockLaunchPoint.rotation);
                            //tempProj.GetComponent<ThrowableRock>().InitialiseRock(this);
                            tempProj.GetComponent<ThrowableRock>().photonView.RPC("InitialiseRock", RpcTarget.All, photonView.ViewID);
                            tempProj.GetComponent<Rigidbody2D>().velocity = rockLaunchSpeed * rockLaunchPoint.up;
                            rockThrowActivated = true;
                        }

                        break;
                    case CharPowerupState.ELECTRIC:
                        // TODO: electric sfx
                        if (photonView.IsMine)
                        {
                            b = true;
                            FindObjectOfType<AudioManager>().Play("ElectricPowerup");
                            electricityHitbox.SetActive(true);
                            canMove = false;
                            horizontal = Vector2.zero;
                            usingElectric = true;

                        }
                        break;
                    case CharPowerupState.ICE:
                        if (!icePowerupActivated)
                        {
                            if (photonView.IsMine)
                            {
                                FindObjectOfType<AudioManager>().Play("IceBall");

                                if (PhotonNetwork.IsMasterClient)
                                {
                                    GameObject iceBall = PhotonNetwork.InstantiateRoomObject("IceBall", iceBallSpawn.position, Quaternion.identity);
                                    //iceBall.GetComponent<IceBallBehaviour>().IgnoreOwnerCollision(this);
                                    iceBall.GetComponent<IceBallBehaviour>().photonView.RPC("IgnoreOwnerCollision", RpcTarget.All, photonView.ViewID);
                                    Rigidbody2D iceRB = iceBall.GetComponent<Rigidbody2D>();

                                    float direction = isFacingRight ? 1f : -1f;

                                    iceRB.velocity = new Vector2(direction * iceBallMoveSpeed, rb.velocity.y);
                                    icePowerupActivated = true;
                                }
                                else
                                {
                                    photonView.RPC("AskHostToCreateIceBall", RpcTarget.MasterClient, isFacingRight, photonView.ViewID);
                                    icePowerupActivated = true;
                                }

                            }

                        }

                        break;
                }
            }
            else
            {
                switch (originalPowerupState)
                {
                    case CharPowerupState.ROCK_THROW:
                        // TODO: spawn rock obj that goes on an arc
                        if (!rockThrowActivated)
                        {
                            if (photonView.IsMine)
                            {
                                FindObjectOfType<AudioManager>().Play("BoulderThrow");
                            }
                            GameObject tempProj = PhotonNetwork.Instantiate("Rock", rockLaunchPoint.position, rockLaunchPoint.rotation);
                            //tempProj.GetComponent<ThrowableRock>().InitialiseRock(this);
                            tempProj.GetComponent<ThrowableRock>().photonView.RPC("InitialiseRock", RpcTarget.All, photonView.ViewID);
                            tempProj.GetComponent<Rigidbody2D>().velocity = rockLaunchSpeed * rockLaunchPoint.up;
                            rockThrowActivated = true;
                        }

                        break;
                    case CharPowerupState.ELECTRIC:
                        // TODO: electric sfx
                        if (photonView.IsMine)
                        {
                            b = true;
                            FindObjectOfType<AudioManager>().Play("ElectricPowerup");
                            electricityHitbox.SetActive(true);
                            canMove = false;
                            horizontal = Vector2.zero;
                            usingElectric = true;

                        }
                        break;
                    case CharPowerupState.ICE:
                        if (!icePowerupActivated)
                        {
                            if (photonView.IsMine)
                            {
                                FindObjectOfType<AudioManager>().Play("IceBall");

                                if (PhotonNetwork.IsMasterClient)
                                {
                                    GameObject iceBall = PhotonNetwork.InstantiateRoomObject("IceBall", iceBallSpawn.position, Quaternion.identity);
                                    //iceBall.GetComponent<IceBallBehaviour>().IgnoreOwnerCollision(this);
                                    iceBall.GetComponent<IceBallBehaviour>().photonView.RPC("IgnoreOwnerCollision", RpcTarget.All, photonView.ViewID);
                                    Rigidbody2D iceRB = iceBall.GetComponent<Rigidbody2D>();

                                    float direction = isFacingRight ? 1f : -1f;

                                    iceRB.velocity = new Vector2(direction * iceBallMoveSpeed, rb.velocity.y);
                                    icePowerupActivated = true;
                                }
                                else
                                {
                                    photonView.RPC("AskHostToCreateIceBall", RpcTarget.MasterClient, isFacingRight, photonView.ViewID);
                                    icePowerupActivated = true;
                                }

                            }

                        }

                        break;
                }
            }


        }

        // lol gotta fix a bug
        if (context.canceled)
        {
            if (b)
            {
                horizontal = prevHoriz;
            }
            usingElectric = false;
            canMove = true;
            electricityHitbox.SetActive(false);
            FindObjectOfType<AudioManager>().StopMusic("ElectricPowerup");
        }

    }

    [PunRPC]
    private void FreezePlayer()
    {
        Debug.Log("Freeze please");
        movementState = MovementState.FROZEN;
        FindObjectOfType<AudioManager>().Play("PlayerFrozen");
        freezeButtonCheck = Random.Range(3, 10);
        bodyObj.GetComponent<Renderer>().material.color = new Color32(0, 247, 255, 255);
        armsObj.GetComponent<Renderer>().material.color = new Color32(0, 247, 255, 255);
        photonView.RPC("UpdateFreezeMaterial", RpcTarget.All);
        iceBlock.SetActive(true);
    }

    [PunRPC]
    private void UpdateFreezeMaterial()
    {
        bodyObj.GetComponent<Renderer>().material.color = new Color32(0, 247, 255, 255);
        armsObj.GetComponent<Renderer>().material.color = new Color32(0, 247, 255, 255);
    }

    [PunRPC]
    private void AskHostToCreateIceBall(bool isRight, int thisID)
    {
        PhotonView view = PhotonView.Find(thisID);
        GameObject iceBall = PhotonNetwork.InstantiateRoomObject("IceBall", iceBallSpawn.position, Quaternion.identity);
        //iceBall.GetComponent<IceBallBehaviour>().IgnoreOwnerCollision(view.GetComponent<PlayerMovement>());
        iceBall.GetComponent<IceBallBehaviour>().photonView.RPC("IgnoreOwnerCollision", RpcTarget.All, view.ViewID);
        Rigidbody2D iceRB = iceBall.GetComponent<Rigidbody2D>();

        float direction = isRight ? 1f : -1f;

        iceRB.velocity = new Vector2(direction * iceBallMoveSpeed, rb.velocity.y);
        
    }
    private void BecomeInvincible()
    {
        if (photonView.IsMine)
        {
            if (isInvincible)
            {
                return;
            }

            if (running)
            {
                currentSpeed = invincibleRunningSpeed;
            }
            FindObjectOfType<AudioManager>().Play("Invincibility");
            FindObjectOfType<AudioManager>().ChangeVolume("Overworld1", 0f);
            FindObjectOfType<AudioManager>().ChangeVolume("Overworld2", 0f);
            FindObjectOfType<AudioManager>().ChangeVolume("Overworld3", 0f);
            isInvincible = true;
            Invoke(nameof(EndInvincible), invincibilityLength);
        }
    }


    private void EndInvincible()
    {
        if (photonView.IsMine)
        {
            FindObjectOfType<AudioManager>().StopMusic("Invincibility");
            FindObjectOfType<AudioManager>().ChangeVolume("Overworld1", 0.491f);
            FindObjectOfType<AudioManager>().ChangeVolume("Overworld2", 0.491f);
            if (running)
            {
                currentSpeed = runningSpeed;
            }
            powerupState = originalPowerupState;
            isInvincible = false;
            SetPowerupMaterial(matIndexRef);
        }
    }

    private void BecomeGiant()
    {
        if (isGiant)
        {
            return;
        }

        isGiant = true;
        

        // scale up
        Vector3 giantScale = originalScale * giantSize;
        originalGroundCheckPos = groundCheck.localPosition;

        // adjust radius
        float newGroundCheckRadius = originalGroundCheckRadius * giantSize;

        if (photonView.IsMine)
        {
            FindObjectOfType<AudioManager>().Play("GiantPowerup");
            currentSpeed = giantMovementSpeed;
            photonView.RPC("SetGiantScaleNetwork", RpcTarget.All, giantScale, newGroundCheckRadius);
            Invoke(nameof(RevertScale), giantLength);
        }
    }

    [PunRPC]
    private void SetGiantScaleNetwork(Vector3 newScale, float newRadius)
    {
        /*transform.localScale = newScale;
        groundCheckRadius = newRadius; */
        StartCoroutine(GrowPlayer(newScale, newRadius));
    }

    private IEnumerator GrowPlayer(Vector3 targetScale, float targetCheckRadius)
    {
        //currentSpeed = defaultSpeed;
        anim.SetBool("Growing", true);
        Vector2 prevHoriz = horizontal;
        float elapsedTime = 0f;
        Vector3 startingScale = transform.localScale;
        float startingRadius = groundCheckRadius;

        // smooth scale the player
        while (elapsedTime < giantGrowthDuration)
        {
            anim.SetBool("Walking", false);
            horizontal = Vector2.zero;
            canMove = false;
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / giantGrowthDuration;

            // its lerping time
            transform.localScale = Vector3.Lerp(startingScale, targetScale, t);

            // lerp the radius too
            groundCheckRadius = Mathf.Lerp(startingRadius, targetCheckRadius, t);

            yield return null;
        }

        //horizontal = prevHoriz;
        canMove = true;
        transform.localScale = targetScale;
        groundCheckRadius = targetCheckRadius;
        anim.SetBool("Growing", false);
    }

    // for when giant duration ends
    private void RevertScale()
    {
        if (photonView.IsMine)
        {
            FindObjectOfType<AudioManager>().Play("GiantPowerdown");
            currentSpeed = defaultSpeed;
            photonView.RPC("SetGiantScaleNetwork", RpcTarget.All, originalScale, originalGroundCheckRadius);
        }

        isGiant = false;
        powerupState = CharPowerupState.REGULAR;
    }

    [PunRPC]
    private void StartShrink(Vector3 targetScale, float targetRadius)
    {
        StartCoroutine(ShrinkPlayer(targetScale, targetRadius));
    }

    private IEnumerator ShrinkPlayer(Vector3 targetScale, float targetRadius)
    {
        float elapsedTime = 0f;
        Vector3 startingScale = transform.localScale;
        float startingRadius = groundCheckRadius;

        // time to revert back to normal
        while (elapsedTime < giantGrowthDuration) 
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / giantGrowthDuration;

            // its lerping time #2
            transform.localScale = Vector3.Lerp(startingScale, targetScale, t);

            // revert radius
            groundCheckRadius = Mathf.Lerp(startingRadius, targetRadius, t);

            yield return null;
        }

        transform.localScale = targetScale;
        groundCheckRadius = targetRadius;
    }

    public void TakeDamage(bool isDeath)
    {
        if (powerupState != CharPowerupState.GIANT && powerupState != CharPowerupState.INVINCIBLE)
        {

            Debug.LogWarning("yuh huh");
            if (!tookDamage)
            {
                if (isDeath)
                {
                    if (photonView.IsMine)
                    {
                        StartCoroutine(DeathSequence());
                    }

                }
                else
                {
                    if (photonView.IsMine)
                    {
                        SetPowerupMaterial(0);
                    }
                    powerupState = CharPowerupState.REGULAR;
                    FindObjectOfType<AudioManager>().Play("Powerdown");
                }

                if (photonView.IsMine)
                {
                    if (status.AmountOfMcGuffins > 0)
                    {
                        // TODO: call drop mcguffin
                        Vector2 bounceDir = (transform.position - transform.position).normalized;
                        Vector3 offsetMoment = mcGuffinLaunchPoint.position;
                        Vector2 dropPos = transform.position + offsetMoment;
                        photonView.RPC("DropMcGuffin", RpcTarget.All, dropPos, bounceDir, photonView.ViewID);
                    }
                }
                

                tookDamage = true;
            }
        }
        
    }

    [PunRPC]
    private void TakeDamageFromPlayer(int otherPlayer, bool isDeath)
    {
        PhotonView otherPlrView = PhotonView.Find(otherPlayer);
        PlayerMovement otherMovement = otherPlrView.GetComponent<PlayerMovement>();


        if (otherPlrView != null)
        {
            if (otherMovement.powerupState != CharPowerupState.GIANT && otherMovement.powerupState != CharPowerupState.INVINCIBLE)
            {
                if (!tookDamage)
                {
                    if (isDeath)
                    {
                        if (otherPlrView.IsMine)
                        {
                           otherMovement.StartCoroutine(DeathSequence());
                        }

                    }
                    else
                    {
                        if (otherPlrView.IsMine)
                        {
                            otherMovement.SetPowerupMaterial(0);
                        }
                        otherMovement.powerupState = CharPowerupState.REGULAR;
                        FindObjectOfType<AudioManager>().Play("Powerdown");
                    }


                    if (otherMovement.status.AmountOfMcGuffins > 0)
                    {
                        Vector2 bounceDir = (otherPlrView.transform.position - transform.position).normalized;
                        Vector3 offsetMoment = mcGuffinLaunchPoint.position;
                        Vector2 dropPos = transform.position + offsetMoment;
                        otherPlrView.RPC("DropMcGuffin", RpcTarget.All, dropPos, bounceDir, otherPlrView.ViewID);
                    }

                    otherMovement.tookDamage = true;
                }
            }
            
        }
       
    }

    // when you jump on/get jumped on by the other player
    [PunRPC]
    public void BouncedOn(int otherPlayer)
    {
        // upward force for the player bouncing on the other player
        rb.velocity = new Vector2(rb.velocity.x, jumpPower);

        // find the other player using their viewID
        PhotonView otherPlrView = PhotonView.Find(otherPlayer);

        if (otherPlrView != null)
        {

            // downward force for the other player being jumped on by you
            Rigidbody2D otherRB = otherPlrView.GetComponent<Rigidbody2D>();
            otherRB.velocity = new Vector2(otherRB.velocity.x, -(jumpPower / 2f));

            // direction for mcguffin drop
            Vector2 bounceDir = (otherRB.transform.position - transform.position).normalized;
            Vector3 offsetMoment = mcGuffinLaunchPoint.position;
            Vector2 dropPos = transform.position + offsetMoment;
            otherPlrView.RPC("DropMcGuffin", RpcTarget.All, dropPos, bounceDir, otherPlrView.ViewID);

            // stun
            otherPlrView.GetComponent<PlayerMovement>().StunPlayer(false);


        }
    }

    private void StunPlayer(bool isBump)
    {
        Debug.Log("le stun");
        // THE coroutine
        if (!isBump)
        {
            StartCoroutine(JumpStun());
        }
        else
        {
            StartCoroutine(BumpStun());
        }
        
    }

    // well ain't that just stunning?
    private IEnumerator JumpStun()
    {
        FindObjectOfType<AudioManager>().Play("PlayerBump");
        Debug.Log("le stun 2");
        canMove = false;
        isStunned = true;
        Vector2 oldHorizontal = horizontal;
        horizontal = Vector2.zero;

        yield return new WaitForSeconds(jumpStunDuration);

        canMove = true;
        isStunned = false;
        //horizontal = oldHorizontal;
    }

    private IEnumerator BumpStun()
    {
        FindObjectOfType<AudioManager>().Play("PlayerBump");
        Debug.Log("le stun 2");
        canMove = false;
        isStunned = true;
        Vector2 oldHorizontal = horizontal;
        horizontal = Vector2.zero;
        

        yield return new WaitForSeconds(bumpStunDuration);

        canMove = true;
        isStunned = false;
        //horizontal = oldHorizontal;
    }

    private IEnumerator DeathSequence()
    {
        bool b = false;

        horizontal = Vector2.zero;
        rb.velocity = Vector2.zero;
        if (!b)
        {
            // direction for mcguffin drop
            Vector2 bounceDir = Vector2.up;
            Vector3 offsetMoment = new Vector3(0, 2, 0);
            Vector2 dropPos = transform.position + offsetMoment;
            photonView.RPC("DropMcGuffin", RpcTarget.All, dropPos, bounceDir, photonView.ViewID);
            b = true;
            anim.SetBool("Death", true);

        }

        if (photonView.IsMine)
        {
            SetPowerupMaterial(0);
        }

        powerupState = CharPowerupState.REGULAR;
        if (!bruh)
        {
            if (!status.InfiniteLives)
            {
                // if live count suddenly stops completely then remove this if statement
                if (photonView.IsMine)
                {
                    status.AmountOfLives--;
                }               
            }           
            bruh = true;
        }

        if (isGiant)
        {
            photonView.RPC("SetGiantScaleNetwork", RpcTarget.All, originalScale, originalGroundCheckRadius);
        }
        
        if (status.AmountOfLives > 0 || status.InfiniteLives)
        {
            // first bit where the player floats for a moment (assuming it'll look a bit like the death in mario
            canMove = false;
            if (photonView.IsMine)
            {
                FindObjectOfType<AudioManager>().Play("PlayerDeath");
                FindObjectOfType<AudioManager>().StopMusic("Invincibility");
                FindObjectOfType<AudioManager>().ChangeVolume("Overworld1", 0f);
                FindObjectOfType<AudioManager>().ChangeVolume("Overworld2", 0f);
                photonView.RPC("MatchWideDeathAnnounce", RpcTarget.Others);
            }
            
            rb.gravityScale = 0; // change back to 2.3 later do not forget
            GetComponent<Collider2D>().enabled = false;

            yield return new WaitForSeconds(0.6f);

            rb.gravityScale = 2.3f;
            rb.velocity = new Vector2(rb.velocity.x, jumpPower / 1.5f);
            

            yield return new WaitForSeconds(1.8f);

            
            // bringing us back to the spawn area but not instantly "respawning"


            bool isP1 = (bool)PhotonNetwork.LocalPlayer.CustomProperties["isPlayer1"];
            switch (isP1)
            {
                case true:
                    transform.position = new Vector3(GameManager.Instance.CurrentMapData.Player1SpawnX, transform.position.y, transform.position.z);
                    break;
                case false:
                    transform.position = new Vector3(GameManager.Instance.CurrentMapData.Player2SpawnX, transform.position.y, transform.position.z);
                    break;
            }

            if (photonView.IsMine)
            {
                FindObjectOfType<AudioManager>().Play("GameStart");
            }

            anim.SetBool("Death", false);

            yield return new WaitForSeconds(1.35f);

            rb.velocity = Vector3.zero;
            GetComponent<Collider2D>().enabled = true;

            // doing the same thing again but now with the y axis :O
            switch (isP1)
            {
                case true:
                    transform.position = new Vector3(GameManager.Instance.CurrentMapData.Player1SpawnX, GameManager.Instance.CurrentMapData.Player1SpawnY, transform.position.z);
                    break;
                case false:
                    transform.position = new Vector3(GameManager.Instance.CurrentMapData.Player2SpawnX, GameManager.Instance.CurrentMapData.Player2SpawnY, transform.position.z);
                    break;
            }
            if (photonView.IsMine)
            {
                FindObjectOfType<AudioManager>().Play("PlayerRespawn");
            }
            
            canMove = true;
            if (photonView.IsMine)
            {
                FindObjectOfType<AudioManager>().ChangeVolume("Overworld1", 0.491f);
                FindObjectOfType<AudioManager>().ChangeVolume("Overworld2", 0.491f);
            }
            bruh = false;
            tookDamage = true;
            
            yield break;
        }
        else
        {
            photonView.RPC("MatchWideDeathAnnounce", RpcTarget.All);
            // end round
            GameManager.Instance.photonView.RPC("EndRound", RpcTarget.All);
        }
        
    }

    [PunRPC]
    public void MatchWideDeathAnnounce()
    {
        FindObjectOfType<AudioManager>().Play("OtherDeath");
    }

    [PunRPC]
    public void DropMcGuffin(Vector2 dropPos, Vector2 bounceDir, int targetID)
    {
        if (status.AmountOfMcGuffins > 0 && canDropMcGuffin == true)
        {

            status.AmountOfMcGuffins--;
            //PhotonView targetView = PhotonView.Find(targetID);
            //targetView.GetComponent<PlayerStatus>().AmountOfMcGuffins--;
            /*int targetID = photonView.ViewID;
            photonView.RPC("RemoveMcGuffinFromCount", RpcTarget.Others, photonView.ViewID); */

            if (PhotonNetwork.IsMasterClient)
            {
                GameObject dropped = PhotonNetwork.InstantiateRoomObject("McGuffin", mcGuffinLaunchPoint.position, mcGuffinLaunchPoint.rotation);

                dropped.GetComponent<McGuffin>().Type = McGuffinType.SECOND_HAND;
                dropped.GetComponent<Rigidbody2D>().velocity = mcGuffinLaunchSpeed * mcGuffinLaunchPoint.up;

            }
            else
            {
                photonView.RPC("DropMcGuffin", RpcTarget.MasterClient, dropPos, bounceDir, targetID);
            }

            // synchronise
            photonView.RPC("DropMcGuffin", RpcTarget.Others, dropPos, bounceDir, targetID);

        }
        StartCoroutine(McGuffinDropCooldown());
    }

    // an attempt at preventing the game from dropping more than one mcguffin from the player
    private IEnumerator McGuffinDropCooldown()
    {
        canDropMcGuffin = false;

        yield return new WaitForSeconds(4f);
        
        canDropMcGuffin = true;
    }
}