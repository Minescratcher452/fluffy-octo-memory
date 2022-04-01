using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Components
    private Rigidbody2D playerRigidbody;

    // Equipment
    // TODO Remember to change these to structs or classes!
    // Not a final list!
    public int primaryWeapon;
    public int secondaryWeapon;
    public int killstreak;
    public int passive;
    // Slot for a primary-weapon specific gun mod (e.g. expanded clip, laser sight, corrosive rounds, etc)
    public int primaryMod;
    // Idea: slot for a broadly applicable mod (e.g. extra ammo clips, targeting computer, etc)
    public int equipment;

    // Attributes
    public const int MAX_HEALTH = 100;
    private int health;
    public const int MAX_ARMOR = 100;
    private int armor;

    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask ground;

    private bool crouching;

    // True if and only if the player has pressed jump, but the character has not actually jumped yet.
    private bool shouldJump;
    private bool grounded;
    private bool firing;

    // Input
    float horizontalInput;
    float speed;

    private const float BASE_SPEED = 10;
    private const float CROUCH_SLOWDOWN = 0.8f;
    private const float AIR_SLOWDOWN = 0.6f;
    private const float JUMP_STRENGTH = 550f;

    // Start is called before the first frame update
    void Start()
    {
        // Init components
        playerRigidbody = GetComponent<Rigidbody2D>();

        // Init input

        // Init equipment

        // Init attributes
        health = MAX_HEALTH;
        armor = MAX_ARMOR;
        speed = BASE_SPEED;
        crouching = false;
        shouldJump = false;
        grounded = false;
        firing = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Update world state information
        // Test if the player is on the ground
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, ground);

        // Take input
        // Update axes
        horizontalInput = Input.GetAxis("Horizontal");

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
        // NB actually moving the player occurs in FixedUpdate

        // If the player is grounded and pressed jump this frame, initiate a jump
        if (Input.GetButtonDown("Jump") && grounded)
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

        // If the player released fire this frame, then stop firing if the weapon is automatic
        if (Input.GetButtonUp("Fire"))
        {
            firing = false;
        }
        // If the player pressed fire this frame, then execute firing logic
        else if (Input.GetButtonDown("Fire"))
        {
            firing = true;
        }

        
    }

    void FixedUpdate()
    {
        // Update player position based on horizontal movement inputs (A/D)
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

    private void Fire()
    {
        // Input.mousePosition
        Debug.Log("Fired!");
    }
}