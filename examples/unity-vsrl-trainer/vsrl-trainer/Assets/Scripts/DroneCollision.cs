using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneCollision : MonoBehaviour
{
    public GameObject targetDelivery;

    private GameObject agent;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.transform.Find("GameObject").gameObject;
    }

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animator.GetBool("delivery"))
        {
            Vector3 target = new Vector3(targetDelivery.transform.position.x,
                transform.position.y,
                targetDelivery.transform.position.z);

            transform.position = 
                Vector3.MoveTowards(
                transform.position,
                target,
                Time.deltaTime *4f);
        }
    }

    public void Reset()
    {
        transform.rotation = Quaternion.identity;
        transform.Find("GameObject").transform.rotation = Quaternion.identity;
        animator.SetBool("crash", false);
        animator.SetBool("delivery", false);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("dog") || other.gameObject.CompareTag("Obstacle"))
        {
            if (!animator.GetBool("delivery"))
            {
                animator.SetBool("crash", true);
            }
        }
        
        else if (other.gameObject.CompareTag("target"))
        {
            animator.SetBool("crash", false);
            animator.SetBool("delivery", true);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("dog") || other.gameObject.CompareTag("Obstacle"))
        {
            animator.SetBool("crash", false);
        }
    }
}