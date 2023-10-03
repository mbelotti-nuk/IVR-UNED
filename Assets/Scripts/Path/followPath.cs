using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using Geometry;
using UnityEngine.Events;

public class followPath : MonoBehaviour
{
    // Start is called before the first frame update

    ThirdPersonController thirdPersonController;
    //GameObject player;
    [SerializeField]
    Maintenance maintenanceAction;
    [SerializeField]
    PathHolder pathHolder;
    [SerializeField]
    PauseManager pauseManager;

    Path path;


    /// <summary>
    /// Current main speed 
    /// </summary>
    public float speed;

    /// <summary>
    /// The line that t$$anonymous$$s follow.
    /// </summary>
    //public LineRenderer lineToFollow;

    /// <summary>
    /// So, we have to stop after the first lap?
    /// </summary>
    public bool justOnce = true;

    /// <summary>
    /// Internal variable, is the first lap completed?
    /// </summary>
    bool completed = false;

    /// <summary>
    /// Follow a smooth path.
    /// </summary>
    public bool smooth = false;

    /// <summary>
    /// the number of iterations that split each curve
    /// </summary>
    public int iterations = 10;
    public float radius = 0;

    /// <summary>
    /// The points of the line
    /// </summary>
    Vector3[] wayPoints;

    /// <summary>
    /// The Current Point
    /// </summary>
    int currentPoint = 0;

    UnityEvent StopEvent;


    void Start()
    {
        if (StopEvent == null)
            StopEvent = new UnityEvent();

        StopEvent.AddListener(stopDoingThings);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && StopEvent != null)
        {
            StopEvent.Invoke();
        }
        else if(!pauseManager.isOnPath())
        {
            StopEvent.Invoke();
        }
    }



    void stopDoingThings()
    {
        StopAllCoroutines();
    }


    public void followSelectedPath()
    {

        thirdPersonController = GetComponent<ThirdPersonController>();

        path = pathHolder.getSelectedPath();

        if (path == null) return;

        StartCoroutine(pathFollowerCoroutine(path));
    }


    IEnumerator pathFollowerCoroutine(Path path)
    {
        IEnumerator[] coroutines = new IEnumerator[path.inventory.Count];

        for(int i = 0; i < path.inventory.Count; i++)
        {
            var actionW = path.inventory[i].action as Walk; 
            if (actionW != null)
            {
                Vector3[] way = new Vector3[actionW.line.GetComponent<LineRenderer>().positionCount];
                actionW.line.GetComponent<LineRenderer>().GetPositions(way);
                coroutines[i] = FOLLOW(way);
                continue;
            }
            var actionM = path.inventory[i].action as Mantain;
            if(actionM != null)
            {
                coroutines[i] = MANTAINANCE(actionM.time*60f);
            }
        }


        maintenanceAction.resetDose();
        maintenanceAction.activateDosimeter();
        
        foreach(IEnumerator coroutine in coroutines)
        {
            yield return StartCoroutine(coroutine);
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                break;
            }
        }
    }

    IEnumerator FOLLOW(Vector3[] way)
    {
        wayPoints = way; currentPoint = 0;

        if (wayPoints.Length == 0) yield break;



        maintenanceAction.resumeCounting();

        // take to first point
        CharacterController _controller = GetComponent<CharacterController>();
        _controller.enabled = false;
        transform.position = Current(currentPoint);
        _controller.enabled = true;
        IncreaseIndex();

        if (0 < wayPoints.Length)
        {
            while(currentPoint < wayPoints.Length)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    break;
                }
                if (smooth)
                {
                    FollowSmooth();
                    yield return null;
                }
                else
                {
                    FollowClumsy();
                    yield return null;
                }
            }

            

        }
        thirdPersonController.stop();
        maintenanceAction.stopCounting();
        currentPoint = 0;
        Debug.Log("FINISHED");
    }


    IEnumerator MANTAINANCE(float deltaTime)
    {
        maintenanceAction.stopCounting();
        maintenanceAction.deltaCounting(deltaTime);
        yield return null;

    }



    // Small methods to make job easy to read


    // about the array of points

    /// <summary>
    /// Prevoius the specified Index, the points.Length must be > 0.
    /// </summary>
    /// <param name="Index">index on array points.</param>
    Vector3 Prevoius(int index)
    {
        if (0 == index)
        {
            return wayPoints[wayPoints.Length - 1];
        }
        else
        {
            return wayPoints[index - 1];
        }
    }

    /// <summary>
    /// Current at the specified Index.
    /// </summary>
    /// <param name="Index">index on array points.</param>
    Vector3 Current(int index)
    {
        return wayPoints[index];
    }


    /// <summary>
    /// Next of the specified index.
    /// </summary>
    /// <param name="Index">index on array points.</param>
    Vector3 Next(int index)
    {
        if (wayPoints.Length == index + 1)
        {
            return wayPoints[0];
        }
        else
        {
            return wayPoints[index + 1];
        }
    }

    void IncreaseIndex()
    {
        currentPoint++;
        if (currentPoint == wayPoints.Length)
        {
            if (justOnce)
                completed = true;
            else
                currentPoint = 0;
        }
    }



    //about 3d geometry

    /// <summary>
    /// Non smooth following
    /// </summary>
    void FollowClumsy()
    {
        transform.LookAt(Current(currentPoint));

        thirdPersonController.MoveTo(speed, Current(currentPoint)- transform.position);
        transform.position = Vector3.MoveTowards(transform.position, Current(currentPoint), speed * Time.deltaTime);
        
        
        //if is close to the waypoint, pass to the next, if is the last, stop following
        if ((transform.position - Current(currentPoint)).sqrMagnitude < (speed * Time.deltaTime) * (speed * Time.deltaTime))
        {
            IncreaseIndex();
        }
    }

    int i = 1;


    //the function try, just try, to apply the quadratic beizer algorithm, but thos is based on number of subdivisions, not by speed, so, 
    //the speed varies, usually on closed trurns so, to minimize it I put the splits dependig of speed, but, still is a problem

    void FollowSmooth()
    {
        Vector3 anchor1 = Vector3.Lerp(Prevoius(currentPoint), Current(currentPoint), .5f);
        Vector3 anchor2 = Vector3.Lerp(Current(currentPoint), Next(currentPoint), .5f);

        if (i < iterations)
        {
            float currentProgress = (1f / (float)iterations) * (float)i;
            transform.LookAt(Vector3.Lerp(anchor1, Current(currentPoint), currentProgress));
            thirdPersonController.MoveTo(speed, Vector3.Lerp(Current(currentPoint), anchor2, currentProgress) - Vector3.Lerp(anchor1, Current(currentPoint), currentProgress) );
            transform.position = Vector3.Lerp(Vector3.Lerp(anchor1, Current(currentPoint), currentProgress), Vector3.Lerp(Current(currentPoint), anchor2, currentProgress), currentProgress);
            i++;
        }
        else
        {
            i = 1;
            IncreaseIndex();
            Vector3 absisa = Vector3.Lerp(Vector3.Lerp(anchor1, Current(currentPoint), .5f), Vector3.Lerp(Current(currentPoint), anchor2, .5f), .5f);
            float it = (((anchor1 - absisa).magnitude + (anchor2 - absisa).magnitude) / (speed * Time.deltaTime));
            iterations = (int)it;
        }
    }

    /// <summary>
    /// you can also split the vertexs of the LineRenderer, and you know how to assign it, with setvertex
    /// </summary>
    /// <returns>The vertex.</returns>
    /// <param name="numSplit">Number split.</param>


    Vector3[] SplitVertex(int numSplit)
    {
        Vector3[] ret = new Vector3[numSplit * wayPoints.Length];
        for (int oldPoint = 0; oldPoint < wayPoints.Length; oldPoint++)
        {
            Vector3 anchor1 = Vector3.Lerp(Prevoius(oldPoint), Current(oldPoint), .5f);
            Vector3 anchor2 = Vector3.Lerp(Current(oldPoint), Next(oldPoint), .5f);

            for (int j = 1; j < numSplit; j++)
            {
                float currentProgress = (1f / (float)iterations) * (float)i;
                ret[oldPoint * numSplit + j] = Vector3.Lerp(Vector3.Lerp(anchor1, Current(oldPoint), currentProgress), Vector3.Lerp(Current(oldPoint), anchor2, currentProgress), currentProgress);
            }
            IncreaseIndex();
        }
        return ret;
    }







}
