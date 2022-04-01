using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killstreak : MonoBehaviour
{
    // Localize name
    string nameLoc;
    // Duration in seconds (0 = instantaneous)
    float duration;

    // TODO Each killstreak's art and sound assets should also be stored in this script or its Prefab.

    // What does the killstreak do when activated?
    public virtual void OnActivation(PlayerController player)
    {

    }

    // What does the killstreak do each frame while it is active?
    public virtual void OnUpdate(PlayerController player)
    {

    }

    // What does the killstreak do when it ends? (This should remove any buffs it applied when it was activated, for example.)
    public virtual void OnEnd(PlayerController player)
    {

    }
}
