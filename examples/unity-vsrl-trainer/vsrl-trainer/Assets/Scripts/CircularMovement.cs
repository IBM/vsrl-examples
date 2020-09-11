using System;
using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CircularMovement : MonoBehaviour
{
    public float speed;
    public GameObject center;

    private float direction = 1.0f;
    // private float angularSpeed = 0f;
    // private float angle = 0f;
    // private float currentSpeed = 0f;
    // private float radius = 2;

    void Update()
    {

        if (center == null)
        {
            return;
        }

        var currentPosition = transform.position;
        transform.RotateAround(center.transform.position, Vector3.up, direction * speed * Time.deltaTime);
        var newPosition = transform.position;

        transform.rotation = Quaternion.LookRotation(newPosition - currentPosition);
    }

    public void Reset()
    {
        direction = 1.0f;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            direction *= -1.0f;
        }
    }
}
