using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OOBKillbox : MonoBehaviour
{
    public BoxCollider2D bc;
    private Color gizmoColor = Color.cyan;

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log(other);

        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Damage(1000, this.gameObject);
        }
    }

    // Draw the OOB box in the editor
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, bc.size);
    }
}
