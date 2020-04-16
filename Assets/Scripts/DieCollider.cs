using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieCollider : MonoBehaviour
{

    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            transform.GetComponentInParent<EnemyControl>().Die(EvolutionSystem.DIE_REASON.HURT_PLAYER);
        else if (other.CompareTag("Laser"))
        {
            Destroy(other.gameObject);
            transform.GetComponentInParent<EnemyControl>().Die(EvolutionSystem.DIE_REASON.LASER);
        }
    }

}
