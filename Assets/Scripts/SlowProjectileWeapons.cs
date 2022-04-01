using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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