using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class NavAgentExample : MonoBehaviour
{
    public AIWaypointNetwork WaypointNetwork = null;

    public int CurrentIndex = 0;

    public bool HasPath = false;
    public bool PathPending = false;
    public bool PathStale = false;
    public UnityEngine.AI.NavMeshPathStatus PathStatus = UnityEngine.AI.NavMeshPathStatus.PathInvalid;
    public AnimationCurve JumpCurve = new AnimationCurve();


    private UnityEngine.AI.NavMeshAgent _navAgent = null; 
    void Start()
    {
        _navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        /*_navAgent.updatePosition = false;
        _navAgent.updateRotation = false;*/

        if (WaypointNetwork == null) return;

        SetNextDestination(false);
    }

    void SetNextDestination (bool increment)
    {
        if (!WaypointNetwork) return;
        int incStep = increment?1:0;
        Transform nextWaypointTransform = null;


            int nextWaypoint = (CurrentIndex + incStep >= WaypointNetwork.Waypoints.Count) ? 0 : CurrentIndex + incStep;
            nextWaypointTransform = WaypointNetwork.Waypoints[nextWaypoint];

            if(nextWaypointTransform!=null)
            {
                CurrentIndex = nextWaypoint;
                _navAgent.destination = nextWaypointTransform.position;
                return;
            }      

        CurrentIndex++;
    }

    void Update()
    {
        HasPath = _navAgent.hasPath;
        PathPending = _navAgent.pathPending;
        PathStale = _navAgent.isPathStale;
        PathStatus = _navAgent.pathStatus;

        if( _navAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(2.0f));
            return;
        }

        if( _navAgent.remainingDistance<=_navAgent.stoppingDistance && !PathPending || PathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid  /* || PathStatus = UnityEngine.AI.NavMeshPathStatus.PathPartial */)
        {
            SetNextDestination(true);
        }

        else
            if(_navAgent.isPathStale)
            SetNextDestination(false);
    }

    IEnumerator Jump(float duration)
    {
        UnityEngine.AI.OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);
        float time = 0.0f;

        while (time <= duration)
        {
            float t = time / duration;
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + JumpCurve.Evaluate(t) * Vector3.up;
            time += Time.deltaTime;
            yield return null;
        }

        _navAgent.CompleteOffMeshLink();
    }
}