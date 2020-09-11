using System;
using System.Collections.Generic;
using System.Linq;
using FoxChicken.Scripts;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using Random = UnityEngine.Random;


public class ChickenAgentTrainer : Agent
{
    private List<GameObject> dogs = new List<GameObject>();
    public TextRewardController textRewardController;
    public GameObject target;
    public SpawnDogs spawnDogs;

    public GameObject[] obstacles;
    private GameObject[] trees;

    private float accel = 5.0f;
    private float brake_accel = 1.0f;
    private float SAFE_SEP = .5f;
    private float SAFE_SEP_DOG = .5f;

    private Vector3 lookat = new Vector3(1f, 0f, 0f);
    private Vector3 initialPosition;

    private int trial = 0;

    private int NumCollisionsObj = 0;
    private int NumCollisionsPatrols = 0;
    public bool VSRLConstraints = false;
    private float min_w = 0;
    private float max_w = (float) (2 * Math.PI);

    private CollisionChecker _collisionChecker;

    public enum RewardStatus : int
    {
        Low = 1,
        Medium = 2,
        High = 3,
    }

    public enum PenaltyStatus : int
    {
        Low = 1,
        Medium = 2,
        High = 3,
    }

    // Default conditions
    [HideInInspector] public RewardStatus rewardStatus = RewardStatus.Low;
    [HideInInspector] public PenaltyStatus penaltyStatus = PenaltyStatus.Low;

    private float rewardValue = 0.0f;
    private float penaltyValue = 0.0f;


    private void Awake()
    {
        FoxChickenConfig config = spawnDogs.config;

        LoadConfig(config);

        // Finds and assigns rewardStatus and penaltyStatus using command line args.
        // Note: this only works if using an executable when training. Otherwise, the editor values are used.
        GetLevelArgs();

        // Show Reward and Penalty on terminal
        Debug.Log("Reward: " + rewardStatus.ToString() + " -- Penalty: " + penaltyStatus.ToString());

        // Show Reward and Penalty on screen
        if (textRewardController)
        {
            textRewardController.UpdateText(rewardStatus.ToString(), penaltyStatus.ToString());
        }
    }

    private void LoadConfig(FoxChickenConfig config)
    {
        if (config == null)
        {
            rewardStatus = RewardStatus.Low;
            penaltyStatus = PenaltyStatus.Low;
        }
        else
        {
            if (config.reward.ToLower() == "low")
            {
                rewardStatus = RewardStatus.Low;
            }
            else if (config.reward.ToLower() == "medium")
            {
                rewardStatus = RewardStatus.Medium;
            }
            else rewardStatus = RewardStatus.High;

            if (config.penalty.ToLower() == "low")
            {
                penaltyStatus = PenaltyStatus.Low;
            }
            else if (config.penalty.ToLower() == "medium")
            {
                penaltyStatus = PenaltyStatus.Medium;
            }
            else penaltyStatus = PenaltyStatus.High;
        }
    }


    void Start()
    {
        var position = transform.position;
        initialPosition = new Vector3(position.x, position.y, position.z);

        trees = GameObject.FindGameObjectsWithTag("Obstacle");

        SetRewards();

        SetPenalty();

        _collisionChecker = new CollisionChecker(gameObject, transform, dogs, new List<GameObject>(trees));
    }

    private void SetPenalty()
    {
        if (penaltyStatus == PenaltyStatus.Low)
        {
            penaltyValue = AIModels.LOW_PENALTY;
        }
        else if (penaltyStatus == PenaltyStatus.Medium)
        {
            penaltyValue = AIModels.MID_PENALTY;
        }
        else if (penaltyStatus == PenaltyStatus.High)
        {
            penaltyValue = AIModels.HIGH_PENALTY;
        }
    }

    private void SetRewards()
    {
        if (rewardStatus == RewardStatus.Low)
        {
            rewardValue = AIModels.LOW_REWARD;
        }
        else if (rewardStatus == RewardStatus.Medium)
        {
            rewardValue = AIModels.MID_REWARD;
        }
        else if (rewardStatus == RewardStatus.High)
        {
            rewardValue = AIModels.HIGH_REWARD;
        }
    }

    private void Update()
    {
        if (VSRLConstraints)
        {
            Vector3 controlSignal = _collisionChecker.SafeActions(GetComponent<Rigidbody>().velocity);
            GetComponent<Rigidbody>().velocity = controlSignal;
        }
    }

    public override void OnEpisodeBegin()
    {
        if (!dogs.Any())
        {
            dogs.AddRange(GameObject.FindGameObjectsWithTag("dog"));
            spawnDogs.ResetObjects();
        }
        else
        {
            Debug.Log("New positions");
            spawnDogs.ResetObjects();
            spawnDogs.ResetPositions();
        }

        Debug.Log("Trial: " + trial + "    Total Collisions Obj: " + NumCollisionsObj + "   Total Collisions Patrol: " +
                  NumCollisionsPatrols);
        NumCollisionsObj = 0;
        NumCollisionsPatrols = 0;
        // If agent fell, zero its momentum
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        GetComponent<Rigidbody>().velocity = Vector3.zero;

        RandomDronePosition();
        trial += 1;
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions

        var position = this.transform.position;
        float distanceToTarget = Vector3.Distance(position, target.transform.position);
        sensor.AddObservation(distanceToTarget);
        sensor.AddObservation(position.x);
        sensor.AddObservation(position.z);

        // Agent velocity
        sensor.AddObservation(GetComponent<Rigidbody>().velocity.x);
        sensor.AddObservation(GetComponent<Rigidbody>().velocity.z);

        foreach (var dog in dogs)
        {
            float distanceToEnemy = Vector3.Distance(position, dog.transform.position);
            sensor.AddObservation(dog.transform.position.x);
            sensor.AddObservation(dog.transform.position.z);
            sensor.AddObservation(distanceToEnemy);
        }

        foreach (var obstacle in obstacles)
        {
            float distanceToObstacle = Vector3.Distance(position, obstacle.transform.position);
            sensor.AddObservation(obstacle.transform.position.x);
            sensor.AddObservation(obstacle.transform.position.z);
            sensor.AddObservation(distanceToObstacle);
        }
    }

    private bool ApplyConstraint(GameObject tree, Vector3 position)
    {
        float distance = Vector3.Distance(tree.transform.position, position);

        var deltaTime = Time.deltaTime;
        var velocityMagnitude = Vector3.Magnitude(GetComponent<Rigidbody>().velocity);


        return (velocityMagnitude >= 0 &&
                (2 * brake_accel * (distance - SAFE_SEP) > velocityMagnitude * velocityMagnitude +
                 (accel + brake_accel) * (accel * deltaTime * deltaTime + 2 * deltaTime * velocityMagnitude)))
               || (velocityMagnitude < 0 && velocityMagnitude + accel * deltaTime < 0 &&
                   2 * brake_accel * (distance - SAFE_SEP) >= 0)
               || (velocityMagnitude < 0 && velocityMagnitude + accel * deltaTime >= 0 &&
                   2 * brake_accel * (distance - SAFE_SEP) >=
                   (accel + brake_accel) * (accel * deltaTime * deltaTime));
    }

    private bool AccelIsSafe(Vector3 positionToCheck)
    {
        foreach (var tree in trees)
        {
            if (!ApplyConstraint(tree, positionToCheck))
            {
                return false;
            }
        }

        foreach (var dog in dogs)
        {
        }

        return true;
    }


    public override void OnActionReceived(float[] vectorAction)
    {
        // Actions, size = 2k
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        if (VSRLConstraints)
        {
            controlSignal = _collisionChecker.SafeActions(controlSignal);
        }

        GetComponent<Rigidbody>().velocity = controlSignal;

        // Update agent rotation
        if (GetComponent<Rigidbody>().velocity != Vector3.zero)
        {
            lookat = GetComponent<Rigidbody>().velocity;
        }

        transform.rotation = Quaternion.LookRotation(lookat);

        // Rewards
        if (transform.position.y < -1f)
        {
            AddReward(AIModels.FALL_PENALTY);
            EndEpisode();
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
        if (distanceToTarget > 5f) // IS THIS STILL VALID?
        {
            AddReward(AIModels.FALL_PENALTY);
            EndEpisode();
        }


        if (CollideWithDog())
        {
            AddReward(penaltyValue);
            NumCollisionsPatrols += 1;
            if (spawnDogs.config != null && spawnDogs.config.endAtCollision)
            {
                EndEpisode();
            }
        }

        if (CollideWithObstacles())
        {
            AddReward(AIModels.STATIC_COLLISION_PENALTY);
            NumCollisionsObj += 1;
            if (spawnDogs.config != null && spawnDogs.config.endAtCollision)
            {
                EndEpisode();
            }
        }

        if (CollideWithObject(target))
        {
            AddReward(rewardValue);
            EndEpisode();
        }

        AddReward(AIModels.GOAL_DISTANCE_PENALTY * distanceToTarget);
    }

    // Update is called once per frame
    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }

    private bool CollideWithDog()
    {
        bool collision = false;
        foreach (var dog in dogs)
        {
            collision = collision || CollideWithObject(dog);
        }

        return collision;
    }

    private bool CollideWithObstacles()
    {
        bool collision = false;
        foreach (var obstacle in obstacles)
        {
            collision = collision || CollideWithObject(obstacle);
        }

        return collision;
    }

    private bool CollideWithObject(GameObject otherObject)
    {
        return transform.GetComponent<Collider>().bounds.Intersects(otherObject.GetComponent<Collider>().bounds);
    }

    private void GetLevelArgs()
    {
        string[] arguments = Environment.GetCommandLineArgs();
        string rewardArgName = "--rewardLevel";
        string penaltyArgName = "--penaltyLevel";
        for (int i = 0; i < arguments.Length; i++)
        {
            if (arguments[i].Equals(rewardArgName) && arguments.Length > i + 1)
            {
                var status = arguments[i + 1];
                if (status.ToLower() == "low")
                {
                    rewardStatus = (RewardStatus) 1;
                }
                else if (status.ToLower() == "medium")
                {
                    rewardStatus = (RewardStatus) 2;
                }
                else
                {
                    rewardStatus = (RewardStatus) 3;
                }
            }
            else if (arguments[i].Equals(penaltyArgName) && arguments.Length > i + 1)
            {
                var status = arguments[i + 1];
                if (status.ToLower() == "low")
                {
                    penaltyStatus = (PenaltyStatus) 1;
                }
                else if (status.ToLower() == "medium")
                {
                    penaltyStatus = (PenaltyStatus) 2;
                }
                else
                {
                    penaltyStatus = (PenaltyStatus) 3;
                }
            }
        }
    }


    private void RandomDronePosition()
    {
        float x = -0.9f + Random.Range(0f, 2f);
        float z = -0.15f - Random.Range(0f, 1.3f);
        transform.position = new Vector3((float) x, initialPosition.y, z);
    }
}