using UnityEngine;

public class EnemyActivationTrigger : MonoBehaviour
{
    public bool waitForPlayer = true;
    public HomingMissileEnemy enemy;
    public Transform target;
    public float force;
    public float maxVelocity;
    public bool deactivateOnExit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"GameOverTrigger.OnCollisionEnter2D other.gameObject={other.gameObject.name}");

        if (!waitForPlayer && other.gameObject == enemy.gameObject)
        {
            enemy.target = target;
            enemy.force = force;
            enemy.maxVelocity = maxVelocity;
        }
        else if (waitForPlayer)
        {
            var isPlayer = other.gameObject.GetComponent<Movement>() != null;
            if (isPlayer)
            {
                enemy.target = target;
                enemy.force = force;
                enemy.maxVelocity = maxVelocity;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!deactivateOnExit) return;

        var isPlayer = other.gameObject.GetComponent<Movement>() != null;
        if (isPlayer)
        {
            enemy.target = null;
        }
    }
}
