using System;
using System.Collections;
using UnityEngine;

public class StartFinishAnimation : MonoBehaviour
{
    public static StartFinishAnimation instance;

    public GameObject root;
    public float duration;
    public float minScale;
    public float maxScale;
    public SpriteMask spriteMask;

    [HideInInspector]
    public bool finished;

    public void PlayStart()
    {
        StartCoroutine(StartAnimationCoroutine());
    }

    public IEnumerator FinishAnimationCoroutine()
    {
        Time.timeScale = 0;

        if (finished) yield break;

        finished = true;
        root.SetActive(true);

        var player = FindObjectOfType<Movement>();
        var target = player != null ? player.transform : transform;

        var time = duration;
        while (0 < time)
        {
            var scale = Mathf.Lerp(minScale, maxScale, time / duration);
            spriteMask.transform.localScale = new Vector3(scale, scale, 1);
            spriteMask.transform.position = target.position;
            yield return null;
            time -= Time.unscaledDeltaTime;
        }
    }

    private void Awake()
    {
        root.SetActive(true);
        instance = this;
    }

    private void Start()
    {
        PlayStart();
    }

    private IEnumerator StartAnimationCoroutine()
    {
        Time.timeScale = 1;

        root.SetActive(true);

        var player = FindObjectOfType<Movement>();
        var target = player != null ? player.transform : transform;

        var time = duration;
        while (0 < time)
        {
            var scale = Mathf.Lerp(maxScale, minScale, time / duration);
            spriteMask.transform.localScale = new Vector3(scale, scale, 1);
            spriteMask.transform.position = target.position;
            yield return null;
            time -= Time.unscaledDeltaTime;
        }

        root.SetActive(false);
    }
}
