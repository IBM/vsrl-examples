using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnvControl : MonoBehaviour
{
    [HideInInspector] public FoxChickenConfig config = new FoxChickenConfig();
    public GameObject tutorial;
    [HideInInspector] public Dynamic currentDogs = new Dynamic();
    [HideInInspector] public List<GameObject> currentTrees = new List<GameObject>();
    [HideInInspector] public List<GameObject> objectAssigned = new List<GameObject>();
    private Vector3 scenePosition;
    private float blockedMinX;
    private float blockedMaxX;
    private int RETRIES = 30;
    private float SAFE_DISTANCE = 0.45f;

    void Awake()
    {
        // Set initial config values, currentTrees, currentDogs.objects
        if (config.treePositions.Count == 0)
        {
            int i = 0;
            var trees = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (var tree in trees){
                config.treePositions.Add(tree.transform.position);

                var treeDrag = tree.GetComponent<DragAndDrop>();
                treeDrag.Id = i;
                treeDrag.onDragListener = NotifyTreeMoved;
                currentTrees.Add(tree);
                i ++;
            }
        }
        if (config.animatedPositions.Count == 0)
        {
            int i = 0;
            var dogs = GameObject.FindGameObjectsWithTag("dog");
            foreach (var dog in dogs){
                config.animatedPositions.Add(dog.transform.position);

                var dogDrag = dog.GetComponent<DragAndDrop>();
                dogDrag.Id = i;
                dogDrag.onDragListener = NotifyDogMoved;

                currentDogs.objects.Add(dog);
                i ++;
            }
        }

        Collider roadCollider = GameObject.Find("Road").GetComponent<Collider>();
        Collider stoneCollider = GameObject.Find("Stone").GetComponent<Collider>();
        blockedMinX = roadCollider.bounds.min.x;
        blockedMaxX = stoneCollider.bounds.max.x;

        scenePosition = GameObject.Find("Scene1").transform.position;
        SetDragDrop();

        tutorial.SetActive(true);
    }

    public void AssignRandomTreeValues()
    {
        var i = 0;
        foreach (GameObject tree in currentTrees)
        {
            var added = false;
            var counter = 0;
            var pos = GetNewTreeRandomPosition();
            while (!added && counter < RETRIES)
            {
                tree.transform.position = pos + scenePosition;
                Physics.autoSyncTransforms = true;
                Physics.SyncTransforms();
                if (PositionIsSafe(tree, objectAssigned))
                {
                    objectAssigned.Add(tree);
                    config.treePositions[i] = pos;
                    added = true;
                }
                else
                {
                    pos = GetNewTreeRandomPosition();
                }
                counter++;
            }

            if (counter == RETRIES)
            {
                Debug.Log("ALL RETRIES");
            }
            i += 1;
        }
    }

    public void AssignRandomDogValues()
    {
        var i = 0;
        var scenePosition = transform.position;
        foreach (GameObject dog in currentDogs.objects)
        {
            var added = false;
            var counter = 0;

            var pos = GetNewDogRandomPosition();
            while (!added && counter < RETRIES)
            {
                dog.transform.position = pos;
                Physics.autoSyncTransforms = true;
                Physics.SyncTransforms();
                if (PositionIsSafe(dog, objectAssigned))
                {
                    objectAssigned.Add(dog);
                    config.animatedPositions[i] = pos;
                    added = true;
                }
                else
                {
                    pos = GetNewDogRandomPosition();   
                }

                counter++;
            }

            if (counter == 10)
            {
                Debug.Log("NOT POSITION FOUNDED");
            }
            i += 1;
        }
    }


    private bool PositionIsSafe(GameObject currentDog, List<GameObject> objectAssigned)
    {
        var colliderToCheck = currentDog.GetComponent<Collider>();

        foreach (var currentObject in objectAssigned)
        {
            var currentCollider = currentObject.GetComponent<Collider>();

            if (Vector3.Distance(currentDog.transform.position, currentObject.transform.position) < SAFE_DISTANCE)
            {
                Debug.Log(
                    $"collision {currentDog.name} with {currentObject.name} {currentObject.transform.position} {currentDog.transform.position}");
                return false;
            }
        }

        return true;
    }
    
    private Vector3 GetNewDogRandomPosition()
    {
        var x = Random.Range(-0.5f, 0.5f);
        var z = Random.Range(-1.3f, -0.9f);
        return new Vector3((float) x, 0.46f, z);
    }

    private Vector3 GetNewTreeRandomPosition()
    {
        var x = Random.Range(-0.7f, 1.13f);
        var z = Random.Range(-1.3f, -0.12f);
        var position = new Vector3((float) x, 0.46f, (float) z);

        if (position.x > blockedMinX && position.x < blockedMaxX)
        {
            var useMax = Random.Range(0, 2) == 1;
            position.x = useMax ? Random.Range(blockedMaxX, 1.13f) : Random.Range(-0.7f, blockedMinX);
        }

        return position;
    }

    public void ResetDogPositions()
    {
        var dogIndex = 0;
        foreach (var dog in currentDogs.objects)
        {
            if (config.animatedPositions.Count <= dogIndex)
            {
                Debug.LogWarning("Total: " + config.animatedPositions.Count + "  Index: " + dogIndex);
            }
            else dog.transform.position = config.animatedPositions[dogIndex];

            CircularMovement circular = dog.GetComponent<CircularMovement>();
            if (circular != null)
            {
                circular.Reset();
            }
            
            dogIndex++;
        }
    }

    public void SetDragDrop()
    {
        foreach (var dog in currentDogs.objects)
        {
            dog.GetComponent<DragAndDrop>().canDragDrop = true;
        }

        foreach (var tree in currentTrees)
        {
            tree.GetComponent<DragAndDrop>().canDragDrop = true;
        }
    }

    public void NotifyDogMoved(string nameEmmiter, int id, Vector3 position)
    {
        config.animatedPositions[id] = position;
    }

    public void NotifyTreeMoved(string nameEmmiter, int id, Vector3 position)
    {
        config.treePositions[id] = position;
    }

}

public class Dynamic
{
    public List<GameObject> objects { get; }

    public Dynamic()
    {
        objects = new List<GameObject>();
    }

    public string GetDynamics()
    {
        string data = "";
        int i = 0;
        foreach (GameObject obj in objects)
        {
            if (obj.name.Contains("Dog"))
            {
                data += "Dog, ";
            }
            else
            {
                data += "Child, ";
            }

            if (obj.GetComponent<CircularMovement>().enabled)
            {
                data += "Circle\n";
            }
            else if (obj.GetComponent<LinearMovement>().enabled)
            {
                data += "Run\n";
            }
            else
            {
                data += "Chase\n";
            }

            i++;
        }

        return data;
    }
}