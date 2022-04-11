using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class acts as the parent to all "shotgun" (i.e. multiple bullets per "fire" interaction) weapons.
public class ShotgunWeapon : Weapon
{
    [SerializeField]
    private int pellets; // How many pellets should be fired in one blast?

    void Start()
    {
        // TODO: We need multiple LineRenderers to render all of the shot trails. Either create the components dynamically at runtime or set all shotguns to have the same # of pellets.

        // Deafult weapon init
        // Component init
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;

        // Initialize ammo
        maxAvailableAmmo = (NumMagazines - 1) * MagazineSize;
        currentMag = MagazineSize;
        reserveAmmo = MaxAvailableAmmo;
    }

    public override void Fire(bool facingRight, float movementAccuracyFactor)
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

        // Repeat *pellets* times
        for (int i = 0; i < pellets; i++)
        {
            // By default, fire straight forwards
            Vector2 direction = new Vector2(gunBarrel.transform.right.x, gunBarrel.transform.right.y) * (facingRight ? 1 : -1);

            // Pick a random angle in degrees between { -accuracy < 0 < accuracy }
            // NB shotgun pellet spread is not modified by movement
            float maxError = Accuracy + recoil;
            float inaccuracyOffset = Random.Range(-maxError, maxError) * Mathf.Deg2Rad;

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

        // Increment the recoil *after* firing to ensure correct accuracy on first shot.
        recoil += RecoilIncrement;
    }
}
