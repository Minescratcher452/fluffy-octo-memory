using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAbility : MonoBehaviour
{
    // The ability's name.
    string nameLoc;

    // TODO art assets and sound files 

    // This method will be called from Start by the PlayerController after all of its own initialization is done, and should be used to modify attributes like health, ammo, killstreak cooldown, etc.
    public virtual void OnMatchStart(PlayerController player)
    {

    }

    // TODO What other triggers could be used? OnPlayerShot? OnPlayerKilled? OnPlayerKillsEnemy?
}
