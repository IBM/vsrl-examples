/*

Copyright (C) 2020 IBM. All Rights Reserved.

See LICENSE.txt file in the root directory
of this source tree for licensing information.

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseAgent : MonoBehaviour
{
    public GameObject agent;
    public float speed = 0.2f;
    public GameObject ground;

    // Update is called once per frame
    void Update()
    {

        if (agent == null)
        {
            return;
        }

        SetRotation();
        Vector3 newPosition = Vector3.MoveTowards(transform.position, agent.transform.position, speed*Time.deltaTime);
        newPosition.y = transform.position.y;

        transform.position = newPosition;
    }

    void SetRotation() {

        Vector3 lookPosition = agent.transform.position - transform.position;
        lookPosition.y = 0;
        var rotation = Quaternion.LookRotation(lookPosition);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);
    }

}
