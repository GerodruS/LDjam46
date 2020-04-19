using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteTrigger : MonoBehaviour
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

        MusicStarter.instance.OneShot(MusicStarter.instance.audioFx.win);

        yield return startFinishAnimation.FinishAnimationCoroutine();

        var activeScene = SceneManager.GetActiveScene();
        Debug.Log($"Level#{activeScene.buildIndex} complete. Trying to load next scene. (Count={SceneManager.sceneCountInBuildSettings})");
        SceneManager.LoadScene(activeScene.buildIndex + 1);
    }
}
