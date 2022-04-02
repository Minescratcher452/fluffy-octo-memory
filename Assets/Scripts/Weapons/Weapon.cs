using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// This class acts as the root parent to all weapon scripts (primary and secondary).
public class Weapon : MonoBehaviour
{
    // Components
    public GameObject gunBarrel;
    private LineRenderer lr; 
    
    private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);    // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible


    // Localize the weapon name
    string nameLoc;
    // How many bullets per one magazine?
    [SerializeField]
    private int magazineSize;
    // How many magazines can the player carry at a time?
    [SerializeField]
    private int numMagazines;
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
    // 0 corresponds to 100% accuracy, i.e. 0 degrees of deviation
    [SerializeField]
    private float accuracy;
    // How much damage will the bullet do on impact? 
    // (This can be conceived of as a percentage, i.e. 20 damage = 20% of the target's max health, since by default health is 100.)
    [SerializeField]
    private int damage;

    // Getters
    public string Name { get => nameLoc; }
    public int MagazineSize { get => magazineSize; }
    public int NumMagazines { get => numMagazines; }
    public bool IsFullAuto { get => fullAuto; }
    public float FireRate { get => fireRate; }
    public float ReloadTime { get => reloadTime; }
    public float Range { get => range; }
    public float Accuracy { get => accuracy; }
    public float Damage { get => damage; }

    // TODO Each weapon should also store its art assets and fire, reload, and (if unique) "empty clip" sound files in some form.

    // Init
    // TODO This may be removable when testing is done
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
    }

    // Weapon fire logic
    public virtual void Fire(bool facingRight)
    {
        //Debug.Log("Firing!");

        // By default, fire straight forwards
        Vector2 direction = new Vector2(gunBarrel.transform.right.x, gunBarrel.transform.right.y) * (facingRight ? 1 : -1);

        // Pick a random angle in degrees between { -accuracy < 0 < accuracy }
        float inaccuracyOffset = Random.Range(-Accuracy, Accuracy) * Mathf.Deg2Rad;
        Debug.Log(inaccuracyOffset);

        // Rotate direction by inaccuracyOffset degrees anticlockwise
        // x2 = x1cos(B) - y1sin(B)
        // y2 = x1sin(B) + y1cos(B)
        // Note that a negative value of inaccuracyOffset will therefore result in clockwise rotation, so we get a nice symmetrical cone
        // https://matthew-brett.github.io/teaching/rotation_2d.html
        float x2 = (Mathf.Cos(inaccuracyOffset) * direction.x) - (Mathf.Sin(inaccuracyOffset) * direction.y);
        float y2 = (Mathf.Sin(inaccuracyOffset) * direction.x) + (Mathf.Cos(inaccuracyOffset) * direction.y);
        direction = new Vector2(x2, y2);
        Debug.Log(direction);

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

            // TODO If it's a character, damage them
        } else {
            // If we didn't hit anything, set the endpoint of the laser to its maximum range
            lr.SetPosition(1, gunBarrel.transform.position + new Vector3(direction.x * Range, direction.y * Range, 0));
        }
    }

    private IEnumerator ShotEffect()
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
}