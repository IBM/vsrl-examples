using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FoxChicken.Scripts
{
    public class CollisionChecker
    {
        private readonly float SAFE_SEP = .25f;
        private readonly float SAFE_SEP_DOG = .25f;
        
        private GameObject _gameObject;
        private Transform _transform;
        private List<GameObject> _dogs;
        private List<GameObject> _trees;

        public CollisionChecker(
            GameObject gameObject,
            Transform transform,
            List<GameObject> dogs,
            List<GameObject> trees
        )
        {
            _gameObject = gameObject;
            _transform = transform;
            _dogs = dogs;
            _trees = trees;
        }

        public Vector3 SafeActions(Vector3 action)
        {
            Vector3 nextPosition = getNextPosition(_gameObject, action);

            // Check collisions
            if (IsColliding(nextPosition))
            {
                // List<float> angles = RangeInt.RangeInt(0, 360, 10);
                List<int> angles = Enumerable.Range(0, 35).Select(x => x * 10).ToList();
                float mov = Random.value * 5f;

                List<Vector3> safeActions = new List<Vector3>();
                foreach (float angle in angles)
                {
                    Vector2 polar = new Vector2(angle, mov);
                    Vector3 tempAction = DegreeUtils.PolarToCartesian(polar);
                    Vector3 nextTempPos = this._transform.position + tempAction * Time.deltaTime;
                    if (!IsColliding(nextTempPos))
                    {
                        safeActions.Add(tempAction);
                    }
                }

                if (safeActions.Count > 0)
                {
                    float a = Random.value * safeActions.Count;
                    return safeActions[Random.Range(0, safeActions.Count - 1)];
                }
                else
                {
                    return Scape(angles, mov);
                }
            }
            else
            {
                return action;
            }
        }

        private Vector3 getNextPosition(GameObject _object, Vector3 velocity)
        {
            Vector3 newPosition = _object.transform.position + velocity * Time.deltaTime;
            return newPosition;
        }

        private bool IsColliding(Vector3 newPosition)
        {
            bool collideWithTrees = IsCollidingWithTrees(newPosition, SAFE_SEP);
            bool collideWithDogs = IsCollidingWithDogs(newPosition, SAFE_SEP_DOG);
            return (collideWithDogs || collideWithTrees);
        }

        private Vector3 Scape(List<int> angles, float mov)
        {
            // Look for the nearest object
            GameObject nearestObject = this._gameObject;
            float distance = 1000f;

            // Check trees
            foreach (GameObject tree in _trees)
            {
                float thisDistance = Vector3.Distance(tree.transform.position, _transform.position);
                if (thisDistance < distance)
                {
                    distance = thisDistance;
                    nearestObject = tree;
                }
            }

            // Check dogs
            foreach (GameObject dog in _dogs)
            {
                float thisDistance = Vector3.Distance(dog.transform.position, _transform.position);
                if (thisDistance < distance)
                {
                    distance = thisDistance;
                    nearestObject = dog;
                }
            }

            int selectedAngle = 0;
            // Get the safest action
            foreach (int angle in angles)
            {
                Vector2 polar = new Vector2(angle, mov);
                Vector3 tempAction = DegreeUtils.PolarToCartesian(polar);
                Vector3 nextTempPos = _transform.position + tempAction * Time.deltaTime;

                float newDistance = Vector3.Distance(nextTempPos, nearestObject.transform.position);
                if (newDistance > distance)
                {
                    selectedAngle = angle;
                    distance = newDistance;
                }
            }

            Vector2 polarAction = new Vector2(selectedAngle, mov);
            return DegreeUtils.PolarToCartesian(polarAction);
        }

        private bool IsCollidingWithTrees(Vector3 newPosition, float safeDistance)
        {
            foreach (GameObject tree in _trees)
            {
                Vector3 treePosition = tree.transform.position;
                if (Vector3.Distance(treePosition, newPosition) < safeDistance) return true;
            }

            return false;
        }

        private bool IsCollidingWithDogs(Vector3 newPosition, float safeDistance)
        {
            foreach (GameObject dog in _dogs)
            {
                Vector3 newDogPosition =
                    dog.transform.position + dog.GetComponent<Rigidbody>().velocity * Time.deltaTime;
                if (Vector3.Distance(newDogPosition, newPosition) < safeDistance) return true;
            }

            return false;
        }
    }
}