using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Components
    private Rigidbody2D playerRigidbody;

    // Equipment
    // TODO Remember to change these to classes!
    // Not a final list!
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;
    public GameObject killstreak;
    public GameObject passive;
    // Slot for a primary-weapon specific gun mod (e.g. expanded clip, laser sight, corrosive rounds, etc)
    public GameObject primaryMod;
    // Idea: slot for a broadly applicable mod (e.g. extra ammo clips, targeting computer, etc)
    public GameObject equipment;

    // Attributes
    public const int MAX_HEALTH = 100;
    private int health;
    public const int MAX_ARMOR = 100;
    private int armor;

    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask ground;

    // True for facing right (positive X), false for left (negative X).
    public bool facingRight;
    // True if and only if the player has pressed jump, but the character has not actually jumped yet.
    private bool shouldJump;
    // True if and only if the player is standing on solid ground.
    private bool grounded;
    private bool crouching;

    private Weapon currentWeapon;
    public Weapon CurrentWeapon { get => currentWeapon; }
    private bool firing;
    private float timeToNextShot;
    private float movementAccuracyFactor;

    [SerializeField]
    private bool hasKillstreak;

    // Input
    float horizontalInput;
    float speed; 

    private const float BASE_SPEED = 10f;
    private const float MOVEMENT_AIM_COEFF = 2f;
    private const float CROUCH_SLOWDOWN = 0.8f;
    private const float CROUCH_AIM_COEFF = 1f;
    private const float AIR_SLOWDOWN = 0.7f;
    private const float AIR_AIM_COEFF = 2f;
    private const float JUMP_STRENGTH = 700f;

    // Start is called before the first frame update
    void Start()
    {
        // Init components
        playerRigidbody = GetComponent<Rigidbody2D>();
        currentWeapon = primaryWeapon.GetComponent<Weapon>();

        // Init input

        // Init equipment

        // Init attributes
        health = MAX_HEALTH;
        armor = MAX_ARMOR;
        speed = BASE_SPEED;
        facingRight = true;
        crouching = false;
        shouldJump = false;
        grounded = false;
        firing = false;
        timeToNextShot = 0;
        movementAccuracyFactor = 0;
        hasKillstreak = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Update world state information
        // Test if the player is on the ground
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, ground);

        // Take input
        // Update axes
        horizontalInput = Input.GetAxis("Horizontal"); // NB actually moving the player occurs in FixedUpdate

        // If the player is grounded and pressed jump this frame, initiate a jump
        if (grounded && Input.GetButtonDown("Jump"))
        {
            shouldJump = true;
        }

        // If the player released crouch this frame, then stop crouching
        if (Input.GetButtonUp("Crouch"))
        {
            crouching = false;
        }
        // If the player pressed crouch this frame, then start crouching
        else if (Input.GetButtonDown("Crouch"))
        {
            crouching = true;
        }

        // If the player released fire this frame, then stop firing if we were previously
        if (Input.GetButtonUp("Fire"))
        {
            firing = false;
        }
        // If the player pressed fire this frame, then begin firing
        else if (Input.GetButtonDown("Fire"))
        {
            firing = true;
        }

        // If the player pressed reload this frame, reload the current weapon
        if (Input.GetButtonDown("Reload"))
        {
            Reload();
        }

        // If the player pressed Swap Weapons this frame, then swap weapons
        if (Input.GetButtonDown("Swap Weapons"))
        {
            SwapWeapon();
        }

        // If the player pressed killstreak this frame and has a killstreak to use, then activate the killstreak
        if (hasKillstreak && Input.GetButtonDown("Killstreak"))
        {
            Killstreak();
        }



        // Fire weapon
        // TODO do we want any burst-fire weapons? Might need a switch(Enum) in here if so
        if (firing)
        {
            if (timeToNextShot <= 0)
            {
                // Movement and being in the air decrease accuracy (increases aim cone); crouching increases accuracy (shrinks aim cone)
                movementAccuracyFactor = (MOVEMENT_AIM_COEFF * Mathf.Abs(horizontalInput)) + (AIR_AIM_COEFF * (!grounded ? 1 : 0)) - (CROUCH_AIM_COEFF * (crouching ? 1 : 0));
                currentWeapon.Fire(facingRight, movementAccuracyFactor);
                timeToNextShot = currentWeapon.FireRate;

                // If the current weapon is not full auto, we shouldn't keep firing after the first shot
                // TODO would this be more efficient as a direct assignment?
                if (!currentWeapon.IsFullAuto)
                {
                    firing = false;
                }
            }
        }

        // Decrement timeToNextShot
        // This is OUTSIDE the main fire loop to a) allow for a cooldown between shots on non-automatic weapons and b) prevent fire rate circumvention via spam-clicking
        if (timeToNextShot > 0)
        {
            timeToNextShot -= Time.deltaTime;
        }



        // Update animations
        // Get mouse position info
        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);

        // Flip sprite so the player always faces towards the mouse cursor
        if ((facingRight && mousePos.x < screenPoint.x) || (!facingRight && mousePos.x > screenPoint.x))
        {
            FlipPlayerFacing();
        }

        // Point the weapon at the mouse cursor location
        Vector2 offset = new Vector2(mousePos.x - screenPoint.x, mousePos.y - screenPoint.y) * (facingRight ? 1 : -1);
        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        currentWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);


        // Crouching animation
        if (crouching)
        {

        }
    }

    void FixedUpdate()
    {
        // Update player position
        // Move the player left or right based on input and walk speed (move slower if crouching or in air)
        speed = BASE_SPEED;
        if (crouching)
        {
            speed *= CROUCH_SLOWDOWN;
        }
        else if (!grounded)
        {
            speed *= AIR_SLOWDOWN;
        }
        playerRigidbody.velocity = new Vector2(horizontalInput * speed, playerRigidbody.velocity.y);

        // If the player needs to jump, then jump
        if (shouldJump)
        {
            Jump();
        }
    }

    private void Jump()
    {
        Debug.Log("Jumped!");
        playerRigidbody.AddForce(new Vector2(0f, JUMP_STRENGTH));
        shouldJump = false;
    }

    private void Reload()
    {
        // TODO Play an animation based on currentWeapon.ReloadTime prior to changing any numbers
        Debug.Log("Reloaded current weapon!");
        currentWeapon.Reload();
    }

    private void SwapWeapon()
    {
        Debug.Log("Swapped weapons!");
    }

    private void Killstreak()
    {
        Debug.Log("Killstreak activated!");
        hasKillstreak = false;
    }

    private void FlipPlayerFacing()
    {
        // Debug.Log("Flipped!");
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
