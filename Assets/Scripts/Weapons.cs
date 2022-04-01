using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class acts as the root parent to all weapon scripts (primary and secondary).
public class Weapon : MonoBehaviour
{
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
    // How accurate is the gun, in terms of maximum degree of deviation from the line of sight? (I.E. how wide is the cone containing all possible bullet paths?)
    // 0 corresponds to 100% accuracy
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

    // Weapon fire logic
    public virtual void Fire()
    {
        // Pick a random angle between { -accuracy < 0 < accuracy }

        // TODO Raytracing magic goes here
    }
}

// This class acts as the parent to all *non-raytraced* weapons (i.e. flamethrowers, rocket launchers, etc., whose projectiles do not travel "instantly".)
public class SlowProjectileWeapon : Weapon
{
    [SerializeField]
    private GameObject projectile;

    public override void Fire()
    {
        // Pick a random angle between { -accuracy < 0 < accuracy }

        // TODO GameObject instantiation goes here
    }
}