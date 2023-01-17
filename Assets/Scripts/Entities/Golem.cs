using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RW.MonumentValley
{
    public class Golem : MonoBehaviour
    {
        //  time to move one unit
        [Range(0.25f, 2f)]
        [SerializeField] private float moveTime = 0.5f;

        // pathfinding fields
        // private Clickable[] clickables;
        private Pathfinder pathfinder;
        private Graph graph;
        private Node currentNode;
        private Node nextNode;

        // flags
        [SerializeField] private bool isMoving;
        [SerializeField] private bool isFrozen;

        [SerializeField] private List<Node> pivots;
        private int pivotIndex = 0;

        private void Awake()
        {
            //  initialize fields
            // clickables = FindObjectsOfType<Clickable>();
            pathfinder = FindObjectOfType<Pathfinder>();

            if (pathfinder != null)
            {
                graph = pathfinder.GetComponent<Graph>();
            }

            isMoving = false;
            isFrozen = false;
        }

        private void Start()
        {
            // always start on a Node
            SnapToNearestNode();
            nextNode = pivots[pivotIndex];
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isFrozen)
                    EnableFreeze(false);
                else
                    EnableFreeze(true);

            }

            if (isFrozen)
            {
                if (HasReachedNode(nextNode))
                {
                    StopAllCoroutines();
                    isMoving = false;
                    if (nextNode == pivots[pivotIndex])
                    {
                        currentNode = nextNode;
                        pivotIndex = (pivotIndex == pivots.Count - 1) ? 0 : pivotIndex + 1;
                        nextNode = pivots[pivotIndex];
                    }
                    else
                    {
                        currentNode = nextNode;
                        nextNode = pivots[pivotIndex];
                    }

                }
            }

            if (!isMoving && !isFrozen)
            {
                List<Node> newPath = pathfinder.FindPath(currentNode, nextNode);
                if (newPath.Count > 1)
                {
                    StartCoroutine(FollowPathRoutine(newPath));
                }
            }
            else if (!isFrozen && isMoving && HasReachedNode(pivots[pivotIndex]))
            {
                isMoving = false;
                pivotIndex = (pivotIndex == pivots.Count - 1) ? 0 : pivotIndex + 1;
                nextNode = pivots[pivotIndex];
            }
        }

        private IEnumerator FollowPathRoutine(List<Node> path)
        {
            // start moving
            isMoving = true;

            if (path == null || path.Count <= 1)
            {
                Debug.Log("GOLEM FollowPathRoutine: invalid path");
            }
            else
            {
                // UpdateAnimation();

                // loop through all Nodes
                for (int i = 0; i < path.Count; i++)
                {
                    // use the current Node as the next waypoint
                    nextNode = path[i];

                    // aim at the Node after that to minimize flipping
                    int nextAimIndex = Mathf.Clamp(i + 1, 0, path.Count - 1);
                    Node aimNode = path[nextAimIndex];
                    FaceNextPosition(transform.position, aimNode.transform.position);

                    // move to the next Node
                    yield return StartCoroutine(MoveToNodeRoutine(transform.position, nextNode));
                }
            }
        }

        //  lerp to another Node from current position
        private IEnumerator MoveToNodeRoutine(Vector3 startPosition, Node targetNode)
        {
            float elapsedTime = 0;

            // validate move time
            moveTime = Mathf.Clamp(moveTime, 0.1f, 5f);

            while (elapsedTime < moveTime && targetNode != null && !HasReachedNode(targetNode))
            {

                elapsedTime += Time.deltaTime;
                float lerpValue = Mathf.Clamp(elapsedTime / moveTime, 0f, 1f);

                Vector3 targetPos = targetNode.transform.position;
                transform.position = Vector3.Lerp(startPosition, targetPos, lerpValue);

                // if over halfway, change parent to next node
                if (lerpValue > 0.51f)
                {
                    transform.parent = targetNode.transform;
                    currentNode = targetNode;

                    // invoke UnityEvent associated with next Node
                    targetNode.gameEvent.Invoke();
                    //Debug.Log("invoked GameEvent from targetNode: " + targetNode.name);
                }

                // wait one frame
                yield return null;
            }
        }

        // snap to the nearest Node in Game view
        public void SnapToNearestNode()
        {
            Node nearestNode = graph?.FindClosestNode(transform.position);
            if (nearestNode != null)
            {
                currentNode = nearestNode;
                transform.position = nearestNode.transform.position;
            }
        }

        // turn face the next Node, always projected on a plane at the Player's feet
        public void FaceNextPosition(Vector3 startPosition, Vector3 nextPosition)
        {
            if (Camera.main == null)
            {
                return;
            }

            // convert next Node world space to screen space
            Vector3 nextPositionScreen = Camera.main.WorldToScreenPoint(nextPosition);

            // convert next Node screen point to Ray
            Ray rayToNextPosition = Camera.main.ScreenPointToRay(nextPositionScreen);

            // plane at player's feet
            Plane plane = new Plane(Vector3.up, startPosition);

            // distance from camera (used for projecting point onto plane)
            float cameraDistance = 0f;

            // project the nextNode onto the plane and face toward projected point
            if (plane.Raycast(rayToNextPosition, out cameraDistance))
            {
                Vector3 nextPositionOnPlane = rayToNextPosition.GetPoint(cameraDistance);
                Vector3 directionToNextNode = nextPositionOnPlane - startPosition;
                if (directionToNextNode != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(directionToNextNode);
                }
            }
        }

        // toggle between Idle and Walk animations
        // private void UpdateAnimation()
        // {
        //     if (playerAnimation != null)
        //     {
        //         playerAnimation.ToggleAnimation(isMoving);
        //     }
        // }

        // have we reached a specific Node?
        public bool HasReachedNode(Node node)
        {
            if (pathfinder == null || graph == null || node == null)
            {
                return false;
            }

            float distanceSqr = (node.transform.position - transform.position).sqrMagnitude;

            return (distanceSqr < 0.01f);
        }

        // have we reached the end of the graph?
        public bool HasReachedGoal()
        {
            if (graph == null)
            {
                return false;
            }
            return HasReachedNode(graph.GoalNode);
        }

        //  enable/disable controls
        public void EnableFreeze(bool state)
        {
            isFrozen = state;
        }
    }
}
