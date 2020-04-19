using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"GameOverTrigger.OnCollisionEnter2D other.gameObject={other.gameObject.name}");
        var isPlayer = other.gameObject.GetComponent<Movement>() != null;
        if (isPlayer)
        {
            StartCoroutine(Coroutine());
        }
    }

    private IEnumerator Coroutine()
    {
        var startFinishAnimation = StartFinishAnimation.instance;
        if (startFinishAnimation.finished) yield break;

        MusicStarter.instance.OneShot(MusicStarter.instance.audioFx.death);

        yield return startFinishAnimation.FinishAnimationCoroutine();

        var activeScene = SceneManager.GetActiveScene();
        Debug.Log($"Game Over. Restarting scene '{activeScene.name}'");
        SceneManager.LoadScene(activeScene.name);
    }
}
