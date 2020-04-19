using UnityEngine;

public class EnemySpawnerTrigger : MonoBehaviour
{
    public bool activate = true;
    public HomingMissileEnemy enemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"EnemySpawnerTrigger.OnCollisionEnter2D other.gameObject={other.gameObject.name}");

        var isPlayer = other.gameObject.GetComponent<Movement>() != null;
        if (isPlayer)
        {
            enemy.gameObject.SetActive(activate);
        }
    }
}
