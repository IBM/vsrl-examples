using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class DragAndDrop : MonoBehaviour
{
    public Camera SceneCamera;

    private GameObject ground;

    public GameObject Ground
    {
        get => ground;
        set
        {
            ground = value;
            groundCollider = value.GetComponent<Collider>();
        }
    }

    public GameObject PublicGround;

    private static readonly int animRunning = 2;
    private static readonly int animLooking = 4;
    public Action<string, int, Vector3> onDragListener;
    public int Id { set; get; }
    private Animator animator;
    private Vector3 currentPosition;
    private Collider groundCollider;
    public int captureFrames = 8;
    private static readonly int _animation = Animator.StringToHash("animation");
    public Vector3 ScenePosition { get; set; }
    public Boolean canDragDrop { get; set; }
    public GameObject fence;
    private Collider myCollider;
    private bool tree;
    private Vector3 originalPosition;
    private GameObject[] blocked;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (PublicGround != null)
        {
            groundCollider = PublicGround.GetComponent<Collider>();
        }

        myCollider = transform.GetComponent<Collider>();
        tree = gameObject.tag == "Obstacle";
        if (tree)
        {
            blocked = new GameObject[] {
                GameObject.Find("Road"),
                GameObject.Find("Stone")
            };
        }
        originalPosition = transform.position;
    }

    void OnMouseDown()
    {
        if (!canDragDrop)
        {
            return;
        }

        transform.Translate(Vector3.up * 0.10f);
        GetComponent<Rigidbody>().isKinematic = true;
        if (animator != null)
        {
            animator.SetInteger(_animation, animLooking);
        }

        originalPosition = transform.position;
        readPosition();
        GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }


    void OnMouseDrag()
    {
        if (!canDragDrop)
        {
            return;
        }

        readPosition();
    }

    void OnMouseUp()
    {
        if (!canDragDrop)
        {
            return;
        }

        readPosition();
        GetComponent<Rigidbody>().isKinematic = false;
        if (animator != null)
        {
            animator.SetInteger(_animation, animRunning);
        }

        if (tree)
        {
            MoveIfInvalid();
        }
    }

    void readPosition()
    {
        Vector3 originalPosition = transform.position;
        // Debug.Log($"can drag and drop {canDragDrop}");
        if (!canDragDrop)
        {
            return;
        }

        RaycastHit hit;

        if (groundCollider.Raycast(SceneCamera.ScreenPointToRay(Input.mousePosition), out hit, 200.0F))
        {
            currentPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            if (isValid(currentPosition))
            {
                transform.position = currentPosition;

                if (gameObject.GetComponent<LinearMovement>())
                {
                    GetComponent<LinearMovement>().SetNewStartPosition(currentPosition);
                }
            }
            onDragListener?.Invoke(name, Id, transform.position);
        }
    }

    public void UpdatePosition(Vector3 position)
    {
        Vector3 newPosition = position + ScenePosition;
        if (isValid(newPosition))
        {
            transform.position = position + ScenePosition;
        }
    }

    private bool isValid(Vector3 position)
    {
        Bounds b = new Bounds(position, myCollider.bounds.size);
        var colliders = fence.GetComponentsInChildren<Collider>();
        foreach (var fenceCollider in colliders)
        {
            if (b.Intersects(fenceCollider.bounds))
            {
                return false;
            }
        }
        return true;
    }

    private void MoveIfInvalid()
    {
        foreach (GameObject block in blocked)
        {
            var collider = block.GetComponent<Collider>();

            Bounds b = new Bounds(new Vector3(transform.position.x, 0.47f, transform.position.z), myCollider.bounds.size);
            if (b.Intersects(collider.bounds))
            {
                transform.position = originalPosition;
                var meshRenderer = block.GetComponent<MeshRenderer>();
                if (meshRenderer)
                {
                    var original = meshRenderer.material.color;
                    StartCoroutine(MakeRed(meshRenderer, original));
                }
                else
                {
                    var renderers = block.GetComponentsInChildren<MeshRenderer>();
                    foreach (var renderer in renderers)
                    {
                        var original = renderer.material.color;
                        StartCoroutine(MakeRed(renderer, original));
                    }
                }

            }
        }
    }

    private IEnumerator MakeRed(MeshRenderer renderer, Color original)
    {
        renderer.material.color =  new Color32(0xB5, 0x98, 0x9A, 0x56);
        yield return new WaitForSeconds(.3f);
        renderer.material.color = original;
    }
}
