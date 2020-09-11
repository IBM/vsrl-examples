using System;
using System.IO;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnDogs : MonoBehaviour
{
    [NotNull] public List<GameObject> dogPrefabs;
    [HideInInspector] public FoxChickenConfig config = new FoxChickenConfig();
    [NotNull] public string target_name;
    [NotNull] public GameObject[] scenes;
    [NotNull] public GameObject Drone;
    public List<DogsByScene> currentDogsByScenes = new List<DogsByScene>();
    public List<TreesByScene> currentTreesByScenes = new List<TreesByScene>();
    private int numberOfDogs = 3;
    private List<GameObject> objectAssigned = new List<GameObject>();
    private float SAFE_COLLISION = 0.45f;
    private int RETRIES = 30;

    void Awake()
    {
        // Set environment variables
        string jsonPath = config.GetConfigPath();
        string json = null;
        try
        {
            json = File.ReadAllText(jsonPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        if (json != null)
        {
            config = JsonUtility.FromJson<FoxChickenConfig>(json);
            Debug.Log("Penalty: " + config.penalty);
        }
        else
        {
            Debug.Log("JSON NOT FOUND");
            config = null;
        }

        InitObjectsByScene();
        InitTreesValues();
        InitDogValues();
    }

    private void InitTreesValues()
    {
        var sceneCount = 0;
        foreach (var scene in scenes)
        {
            var trees = scene.transform.Find("Obstacles");
            var scenePosition = scene.transform.position;
            var i = 0;
            foreach (Transform treeTranform in trees.transform)
            {
                if (config != null && i < config.treePositions.Count)
                {
                    treeTranform.position = config.treePositions[i] + scenePosition;
                }
                else
                {
                    treeTranform.position = GetNewTreeRandomPosition() + scenePosition;
                }

                treeTranform.GetComponent<DragAndDrop>().ScenePosition = scenePosition;
                currentTreesByScenes[sceneCount].Trees.Add(treeTranform.gameObject);
                i++;
            }

            sceneCount++;
        }
    }

    private void InitDogValues()
    {
        var sceneCount = 0;

        List<int> prefabs = new List<int>();
        List<int> movements = new List<int>();
        List<Vector3> positions = new List<Vector3>();
        if (config != null)
        {
            prefabs = config.GetDogPrefabs();
            movements = config.GetDogMovements();
            positions = config.animatedPositions;
        }
        else
        {
            prefabs.Add(0);
            prefabs.Add(0);
            prefabs.Add(1);
            movements.Add(0);
            movements.Add(1);
            movements.Add(1);
        }

        foreach (var scene in scenes)
        {
            var scenePosition = scene.transform.position;
            for (int i = 0; i < numberOfDogs; i++)
            {
                var prefabType = prefabs[i];
                var prefab = dogPrefabs[prefabType];
                var position = positions.Count >= numberOfDogs ? positions[i] : GetNewDogRandomPosition();
                var newDog = Instantiate(prefab, position + scenePosition, transform.rotation);

                var movement = movements[i];

                newDog.GetComponent<DragAndDrop>().Ground = FindChildByName(scene, "House/Plane");
                newDog.GetComponent<DragAndDrop>().SceneCamera =
                    FindChildByName(scene, "Camera").GetComponent<Camera>();
                newDog.GetComponent<DragAndDrop>().ScenePosition = scenePosition;

                var circleDog = newDog.GetComponent<CircularMovement>();
                var chaseAgent = newDog.GetComponent<ChaseAgent>();
                var linear = newDog.GetComponent<LinearMovement>();

                if (movement == 0)
                {
                    circleDog.center = FindChildByName(scene, target_name);

                    chaseAgent.enabled = false;
                    linear.enabled = false;
                }
                else if (movement == 1)
                {
                    linear.numberOfSteps = 4;
                    linear.speed = 0.05f;
                    linear.SetNewStartPosition(newDog.transform.position);

                    chaseAgent.enabled = false;
                    circleDog.enabled = false;
                }
                else if (movement == 2)
                {
                    var targetAgent = FindChildByName(scene, "VSRL-Agent");
                    if (targetAgent == null)
                    {
                        targetAgent = FindChildByName(scene, "RL-Agent");
                    }

                    chaseAgent.agent = targetAgent;

                    circleDog.enabled = false;
                    linear.enabled = false;
                }

                currentDogsByScenes[sceneCount].Dogs.Add(newDog.gameObject);
                currentDogsByScenes[sceneCount].InitPosition.Add(newDog.transform.position);
            }

            sceneCount++;
        }
    }

    public void NotifyTreeMoved(string nameEmmiter, int id, Vector3 position)
    {
        foreach (var currentTreesByScene in currentTreesByScenes)
        {
            currentTreesByScene.Trees[id].GetComponent<DragAndDrop>().UpdatePosition(position);
        }
    }

    public void NotifyDogMoved(string nameEmmiter, int id, Vector3 position)
    {
        foreach (var currentDogsByScene in currentDogsByScenes)
        {
            currentDogsByScene.Dogs[id].GetComponent<DragAndDrop>().UpdatePosition(position);
        }
    }

    private void InitObjectsByScene()
    {
        foreach (var scene in scenes)
        {
            currentDogsByScenes.Add(new DogsByScene());
            currentTreesByScenes.Add(new TreesByScene());
        }
    }

    private GameObject FindChildByName(GameObject gameObject, string name)
    {
        var searchedTranform = gameObject.transform.Find(name);
        if (searchedTranform != null)
        {
            return searchedTranform.gameObject;
        }

        return null;
    }

    private Vector3 GetNewDogRandomPosition()
    {
        var x = -0.5 + Random.value * 1;
        var z = -1 - (Random.Range(0, 1) * 0.2f);
        return new Vector3((float) x, 0.9f, z);
    }

    private Vector3 GetNewTreeRandomPosition()
    {
        var x = -0.9 + Random.Range(0, 2f);
        var z = -0.15 - Random.Range(0, 1.3f);
        return new Vector3((float) x, 0.46f, (float) z);
    }


    public void ResetPosition()
    {
        foreach (var dogsByScene in currentDogsByScenes)
        {
            var dogIndex = 0;
            foreach (var dog in dogsByScene.Dogs)
            {
                Debug.Log("init dog " + dogIndex);
                var dogInitialPos = dogsByScene.InitPosition[dogIndex];
                dog.transform.position = dogInitialPos;
                dogIndex++;
            }
        }
    }

    public void RandomDogsPosition()
    {
        var newPositions = new Vector3[currentDogsByScenes[0].Dogs.Count];

        for (int i = 0; i < newPositions.Length; i++)
        {
            newPositions[i] = GetNewDogRandomPosition();
        }

        foreach (var dogsByScene in currentDogsByScenes)
        {
            var dogIndex = 0;
            foreach (var dog in dogsByScene.Dogs)
            {
                var positionAssigned = false;
                var counter = 0;
                while (!positionAssigned  && counter < RETRIES)
                {
                    var dogInitialPos = newPositions[dogIndex];
                    dog.transform.position = dogInitialPos;
                    Physics.autoSyncTransforms = true;
                    Physics.SyncTransforms();
                    if (PositionIsSafe(dog, objectAssigned))
                    {
                        objectAssigned.Add(dog);
                        positionAssigned = true;
                    }
                    else
                    {
                        newPositions[dogIndex] = GetNewDogRandomPosition();
                    }

                    counter++;
                }


                LinearMovement linear = dog.GetComponent<LinearMovement>();
                if (linear != null)
                {
                    linear.SetNewStartPosition(dog.transform.position);
                }

                CircularMovement circularMovement = dog.GetComponent<CircularMovement>();
                if (circularMovement != null)
                {
                    circularMovement.Reset();
                }

                dogIndex++;
            }
        }
    }

    private bool PositionIsSafe(GameObject currentDog, List<GameObject> objectAssigned)
    {
        foreach (var currentObject in objectAssigned)
        {
            if (Vector3.Distance(currentDog.transform.position, currentObject.transform.position) < SAFE_COLLISION)
//            if (currentCollider.bounds.Intersects(colliderToCheck.bounds))
            {
                Debug.Log(
                    $"collision {currentDog.name} with {currentObject.name} {currentObject.transform.position} {currentDog.transform.position}");
                return false;
            }
        }

        return true;
    }


    public void RandomTreesPosition()
    {
        Debug.Log("number of trees " + currentTreesByScenes[0].Trees.Count);
        var newPositions = new Vector3[currentTreesByScenes[0].Trees.Count];

        for (int i = 0; i < newPositions.Length; i++)
        {
            newPositions[i] = GetNewTreeRandomPosition();
        }

        foreach (var treesByScene in currentTreesByScenes)
        {
            var treeIndex = 0;
            foreach (var tree in treesByScene.Trees)
            {
                var positionAssigned = false;
                var counter = 0;
                while (!positionAssigned  && counter < RETRIES)
                {
                    var treeInitialPos = newPositions[treeIndex];
                    tree.transform.position = treeInitialPos;
                    Physics.autoSyncTransforms = true;
                    Physics.SyncTransforms();
                    if (PositionIsSafe(tree, objectAssigned))
                    {
                        positionAssigned = true;
                        objectAssigned.Add(tree);
                    }
                    else
                    {
                        newPositions[treeIndex] = GetNewTreeRandomPosition();
                    }

                    counter++;
                }

                treeIndex++;
            }
        }
    }

    public void ResetObjects()
    {
        objectAssigned.Clear();
        objectAssigned.Add(Drone);
    }

    public void ResetPositions()
    {
        if (config == null)
        {
            RandomTreesPosition();
            RandomDogsPosition();
            return;
        }

        foreach (var scene in currentTreesByScenes)
        {
            var i = 0;
            var scenePosition =  scenes[0].transform.position;
            foreach (var tree in scene.Trees)
            {
                tree.transform.position = config.treePositions.Count > i ? config.treePositions[i]+scenePosition : GetNewTreeRandomPosition();
                i++;
            }
        }

        foreach (var scene in currentDogsByScenes)
        {
            var i = 0;
            foreach (var dog in scene.Dogs)
            {
                dog.transform.position = config.animatedPositions.Count > i ? config.animatedPositions[i] : GetNewDogRandomPosition();
                i++;
            }
        }

    }
}

public class DogsByScene
{
    public List<GameObject> Dogs = new List<GameObject>();
    public List<Vector3> InitPosition = new List<Vector3>();
}

public class TreesByScene
{
    public List<GameObject> Trees = new List<GameObject>();
}
