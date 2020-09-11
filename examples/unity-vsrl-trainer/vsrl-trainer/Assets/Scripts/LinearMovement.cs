using UnityEngine;
using System.Collections;

public class LinearMovement : MonoBehaviour
{
    public int numberOfSteps;
    public float speed;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 prevPosition;
    private float currentSpeed = 0f;
    private bool flipped = false;

    private void Update()
    {
        if (startPosition == Vector3.zero)
        {
            return;
        }

        Vector3 currentPosition = transform.position;
        endPosition = startPosition + numberOfSteps * Vector3.forward;

        currentSpeed += speed * Time.deltaTime;
        var newPosition = Vector3.LerpUnclamped(startPosition, endPosition, Mathf.PingPong(currentSpeed, speed));
        transform.position = newPosition;

        Quaternion rotation = transform.rotation;
        if (transform.position.z < currentPosition.z && currentPosition.z < prevPosition.z)
        {
            if (!flipped)
            {
                rotation = Quaternion.Euler(0f, 180f, 0f);
                flipped = true;
            }
        }
        else if (transform.position.z > currentPosition.z && currentPosition.z > prevPosition.z)
        {
            if (flipped)
            {
                rotation = Quaternion.Euler(0f, 0f, 0f);
                flipped = !flipped;
            }
        }

        transform.rotation = rotation;
        prevPosition = currentPosition;
    }

    void OnMouseDown()
    {
        SetNewStartPosition(transform.position);
    }

    public void SetNewStartPosition(Vector3 newPosition)
    {
        startPosition = newPosition;
        startPosition.y = 0.46f;
        transform.position = startPosition;
    }
}
