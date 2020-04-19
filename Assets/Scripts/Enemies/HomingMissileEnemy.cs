using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class HomingMissileEnemy : MonoBehaviour
{
    public Transform target;
    public float force;
    public float maxVelocity;
    public float spawnDuration = 1;

    private Vector3 _startPosition;
    private Rigidbody2D _rigidbody2D;
    private Collider2D _collider2D;
    private Light2D _light2D;
    private float _pointLightInnerRadius;
    private float _pointLightOuterRadius;

    private void Awake()
    {
        _startPosition = transform.position;
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        _light2D = GetComponentInChildren<Light2D>();
        // target = target != null ? target : FindObjectOfType<Movement>().transform;

        if (_light2D != null)
        {
            _pointLightInnerRadius = _light2D.pointLightInnerRadius;
            _pointLightOuterRadius = _light2D.pointLightOuterRadius;
        }
    }

    private void Start()
    {
        _rigidbody2D.AddForce(Vector2.right * force);

        if (_light2D != null)
        {
            StartCoroutine(SpawnCoroutine());
        }
    }

    private IEnumerator SpawnCoroutine()
    {
        _light2D.pointLightInnerRadius = 0;
        _light2D.pointLightOuterRadius = 0;

        var time = spawnDuration;
        while (0 < time)
        {
            yield return null;
            time -= Time.deltaTime;
            _light2D.pointLightInnerRadius = Mathf.Lerp(_pointLightInnerRadius, 0, time);
            _light2D.pointLightOuterRadius = Mathf.Lerp(_pointLightOuterRadius, 0, time);
        }

        _light2D.pointLightInnerRadius = _pointLightInnerRadius;
        _light2D.pointLightOuterRadius = _pointLightOuterRadius;
    }

    private void Update()
    {
        var hasTarget = target != null;
        var currentPosition = transform.position;
        var targetPosition = hasTarget
            ? target.position
            : _startPosition;
        var vector = targetPosition - currentPosition;

        var f = force;
        vector.Normalize();
        _rigidbody2D.MoveRotation(Mathf.Lerp(_rigidbody2D.rotation, Vector2.SignedAngle(Vector2.up, vector), 0.1f));
        _rigidbody2D.AddForce(vector * f);
        var currentSqrVelocity = _rigidbody2D.velocity.sqrMagnitude;

        var mv = maxVelocity;
        if (mv * mv < currentSqrVelocity)
        {
            _rigidbody2D.velocity = _rigidbody2D.velocity.normalized * mv;
        }
    }
}
