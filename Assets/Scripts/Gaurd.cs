using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gaurd : MonoBehaviour
{
    public static event System.Action OnGuardHasSpottedPlayer;


    public float speed;
    public float waitTime;

    public float turnSpeed = 90f;
    public float timeToSpotPlayer = .5f;


    public Transform pathHolder;
    Transform player;
    public LayerMask viewMask;

    public Light spotLight;
    public float viewDistance;
    float viewAngle;
    float playerVisibleTime;
    
    Color originalColor;


    private void Start()
    {
        originalColor = spotLight.color;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        viewAngle = spotLight.spotAngle;

        Vector3[] wayPoints = new Vector3[pathHolder.childCount];

        for (int i = 0; i < wayPoints.Length; i++)
        {
            wayPoints[i] = pathHolder.GetChild(i).position;
        }

        StartCoroutine(FollowPath(wayPoints));
    }

    private void Update()
    {
        if(CanSeePlayer())
        {
            playerVisibleTime += Time.deltaTime;
        }
        else
        {
            playerVisibleTime -= Time.deltaTime;
        }

        playerVisibleTime = Mathf.Clamp(playerVisibleTime, 0f, timeToSpotPlayer);
        spotLight.color = Color.Lerp(originalColor, Color.red, playerVisibleTime / timeToSpotPlayer);

        if(playerVisibleTime >= timeToSpotPlayer)
        {
            if(OnGuardHasSpottedPlayer!=null)
            {
                OnGuardHasSpottedPlayer();
            }
        }
    }

    bool CanSeePlayer()
    {
        if(Vector3.Distance(transform.position,player.position)<viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetGaurdandPlayer = Vector3.Angle(transform.forward, dirToPlayer);

            if(angleBetGaurdandPlayer<viewAngle/2f)
            {
                if(!Physics.Linecast(transform.position,player.position,viewMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

  

    IEnumerator TurnToFace(Vector3 target)
    {
        Vector3 dirToLookTarget = (target - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while(Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach(Transform waypoint in pathHolder)
        {
            Debug.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }

        Debug.DrawLine(previousPosition, startPosition);

        Gizmos.color = Color.red;Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

    IEnumerator FollowPath(Vector3[] wayPoints)
    {
        transform.position = wayPoints[0];

        int targetWayIndex = 1;
        Vector3 targetWayPoint = wayPoints[targetWayIndex];
        transform.LookAt(targetWayPoint);
        
        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWayPoint, speed * Time.deltaTime);
            if(transform.position==targetWayPoint)
            {
                targetWayIndex = (targetWayIndex + 1) % wayPoints.Length;
                targetWayPoint = wayPoints[targetWayIndex];

                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWayPoint));
            }
            yield return null;
        }
    }
}
