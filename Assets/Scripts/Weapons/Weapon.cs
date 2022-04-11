using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// This class acts as the root parent to all weapon scripts (primary and secondary).
public class Weapon : MonoBehaviour
{
    // Components
    [SerializeField]
    protected GameObject gunBarrel;
    protected LineRenderer lr;

    protected WaitForSeconds shotDuration = new WaitForSeconds(0.07f);    // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible

    // Immutable gun properties - can't be set readonly *and* initialized via inspector so remember to use only the getters even within class
    public readonly string nameLoc; // Localize the weapon name
    [SerializeField]
    private int magazineSize; // How many bullets per one magazine?
    [SerializeField]
    private int numMagazines; // How many magazines can the player carry at a time?
    private int maxAvailableAmmo; // The maximum "loose" ammo that can be carried ready for reload. Because one magazine is consumed in the weapon, equivalent to (numMagazines - 1) * MagazineSize and initialized as such in Start.
    // Can the fire button be held down to continuously fire?
    [SerializeField]
    private bool fullAuto;
    // How fast does the gun fire? (In ***seconds per bullet***).
    [SerializeField]
    private float fireRate;
    // How long does the gun take to reload? (In seconds).
    [SerializeField]
    private float reloadTime;
    // How far can gun hit a target? For raycast weapons, this is the ray length; for projectile weapons, the time until the projectile despawns itself.
    [SerializeField]
    private float range;
    // How accurate is the gun, in units of maximum degree of deviation from the line of sight? (I.E. half the angle of the cone containing all possible bullet paths?)
    // 0 corresponds to 0 degrees of deviation, i.e. 100% accuracy
    [SerializeField]
    private float accuracy;
    // How much does recoil affect aim, in units of maximum degree of deviation from the line of sight?
    // 0 corresponds to 0 degrees of deviation (continuous fire does not affect accuracy)
    [SerializeField]
    private float recoilIncrement;
    // How much damage will the bullet do on impact? 
    // (This can be conceived of as a percentage, i.e. 20 damage = 20% of the target's max health, since by default health is 100.)
    [SerializeField]
    private int damage;

    // Getters
    public string Name { get => nameLoc; }
    public int MagazineSize { get => magazineSize; }
    public int NumMagazines { get => numMagazines; }
    public int MaxAvailableAmmo { get => maxAvailableAmmo; }
    public bool IsFullAuto { get => fullAuto; }
    public float FireRate { get => fireRate; }
    public float ReloadTime { get => reloadTime; }
    public float Range { get => range; }
    public float Accuracy { get => accuracy; }
    public float RecoilIncrement { get => recoilIncrement; }
    public float Damage { get => damage; }

    // Mutable weapon properties
    // TODO remove serialization when UI implemented
    [SerializeField]
    protected int currentMag; // Ammo currently ready to be fired
    [SerializeField]
    protected int availableAmmo; // Ammo available to be loaded into the gun
    [SerializeField]
    protected float recoil; // Additional inaccuracy due to recoil, in degrees

    // TODO Each weapon should also store its art assets and fire, reload, and (if unique) "empty clip" sound files in some form.


    // This value is essentially the fire rate in RPS which should be a cutoff for weapons experiencing recoil inaccuracy. 
    private const float RECOIL_DECAY_COEFF = 3f;

    // Init
    // TODO This may be removable when testing is done
    void Start()
    {
        // Component init
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;

        // Initialize ammo
        maxAvailableAmmo = (NumMagazines - 1) * MagazineSize;
        currentMag = MagazineSize;
        availableAmmo = MaxAvailableAmmo;
    }

    void Update()
    {
        if (recoil > 0)
        {
            // Decrement recoil slowly
            recoil -= Time.deltaTime * RecoilIncrement * RECOIL_DECAY_COEFF;
        }

        if (recoil < 0)
        {
            // Floor recoil
            recoil = 0;
        }
    }
    // Weapon fire logic
    // This class uses raycasting to fire "instant" bullets. For slower projectiles like rockets, flames, etc. use SlowProjectileWeapon.
    // Laser line code adapted from https://learn.unity.com/tutorial/let-s-try-shooting-with-raycasts#
    public virtual void Fire(bool facingRight, float movementAccuracyFactor)
    {
        //Debug.Log("Firing!");

        // If this gun is are out of ammo, then play a clicking sound and don't bother to do anything else
        if (!HasAmmo())
        {
            // TODO Play clicking sound
            return;
        }

        // Decrement the ammo in the clip
        currentMag--;

        // By default, fire straight forwards
        Vector2 direction = new Vector2(gunBarrel.transform.right.x, gunBarrel.transform.right.y) * (facingRight ? 1 : -1);

        // Pick a random angle in degrees between { -accuracy < 0 < accuracy }, then modify it for movement, crouching, and recoil
        float maxError = Accuracy + recoil + movementAccuracyFactor;
        float inaccuracyOffset = Random.Range(-maxError, maxError) * Mathf.Deg2Rad;

        // Increment the recoil *after* calculating the offset to ensure correct accuracy on first shot.
        recoil += RecoilIncrement;

        // Rotate direction by inaccuracyOffset degrees anticlockwise
        // x2 = x1cos(B) - y1sin(B)
        // y2 = x1sin(B) + y1cos(B)
        // Note that a negative value of inaccuracyOffset will result in clockwise rotation, so we get a nice symmetrical cone
        // https://matthew-brett.github.io/teaching/rotation_2d.html
        float x2 = (Mathf.Cos(inaccuracyOffset) * direction.x) - (Mathf.Sin(inaccuracyOffset) * direction.y);
        float y2 = (Mathf.Sin(inaccuracyOffset) * direction.x) + (Mathf.Cos(inaccuracyOffset) * direction.y);
        direction = new Vector2(x2, y2);

        // Run the raycast
        RaycastHit2D hit = Physics2D.Raycast(gunBarrel.transform.position, direction, Range);
        Debug.DrawRay(gunBarrel.transform.position, direction, Color.blue, 5);

        // The laser visual effect should start from the gun barrel
        lr.SetPosition(0, gunBarrel.transform.position);

        StartCoroutine(ShotEffect());

        // If it hits something...
        if (hit.collider != null)
        {
            //Debug.Log(hit.point);

            // Set the end position for our visual laser
            lr.SetPosition(1, hit.point);

            // TODO If we hit a character, damage them
        }
        else
        {
            // If we didn't hit anything, set the endpoint of the laser to its maximum range
            lr.SetPosition(1, gunBarrel.transform.position + new Vector3(direction.x * Range, direction.y * Range, 0));
        }
    }

    protected IEnumerator ShotEffect()
    {
        // Play the shooting sound effect
        // gunAudio.Play();

        // Turn on our line renderer
        lr.enabled = true;

        //Wait for .07 seconds
        yield return shotDuration;

        // Deactivate our line renderer after waiting
        lr.enabled = false;
    }

    // Weapon reload logic
    // Ammo is stored as two values, one representing the ammo in the current magazine and the other the total ammo available for reloading.
    // If the gun is attempted to be reloaded while completely full, nothing happens.
    // If the gun is reloaded while not empty, any ammo in the current magazine goes into the total ammo pile.
    public void Reload()
    {
        // Exit immediately if reeloading is unneccessary (full magazine) or impossible (no available ammo)
        if (currentMag == MagazineSize || availableAmmo == 0)
        {
            return;
        }

        // If there are bullets in the current magazine still, we need to store them so we can add them back onto the pile
        int excess = 0;
        if (HasAmmo())
        {
            excess = currentMag;
        }

        // If we have too little available ammo for a full magazine, use everything that's left
        // TODO: If we have (ex) 10 rounds in the chamber and 15 in storage, what do we want to end up with after reloading?
        // 25 in the chamber, or 15 in the chamber and 10 in storage?
        // Currently, this does the latter.
        int reload = System.Math.Min(MagazineSize, availableAmmo);
        availableAmmo -= reload;
        currentMag = reload;

        // Add any leftovers from the old clip to storage
        AddAmmo(excess);
    }

    // Increases availableAmmo by newAmmo, without going over the weapon's maximum ammo capacity.
    public void AddAmmo(int newAmmo)
    {
        availableAmmo += newAmmo;
        if (availableAmmo > MaxAvailableAmmo)
        {
            availableAmmo = MaxAvailableAmmo;
        }
    }

    // Returns true if the current magazine has ammo and the weapon can be fired, false otherwise.
    public bool HasAmmo()
    {
        return currentMag > 0;
    }
}