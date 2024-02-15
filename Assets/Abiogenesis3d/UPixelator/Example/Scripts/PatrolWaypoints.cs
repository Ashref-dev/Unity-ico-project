using System.Collections.Generic;
using UnityEngine;

namespace Abiogenesis3d.UPixelator_Demo
{
public class PatrolWaypoints : MonoBehaviour
{
    public float stepSpeed = 0.1f;
    public float stoppingDistance = 0.25f;
    public List<Transform> waypoints = new List<Transform>();

    Transform waypoint;
    int index;

    public Vector3 GetWaypoint()
    {
        return waypoint != null ? waypoint.position : transform.position;
    }

    void Update()
    {
        if (stepSpeed < 0) stepSpeed = 0;

        if (waypoints.Count == 0) return;
        if (waypoint == null) waypoint = waypoints[index];

        // TODO: this can get bad if crossing one waypoint that is above another
        Vector3 distanceXZ = transform.position - waypoint.position;
        distanceXZ.y = 0;
        if (distanceXZ.magnitude < stoppingDistance)
        {
            index += 1;
            if (index >= waypoints.Count) index = 0;
            waypoint = waypoints[index];
        }

        float dt = Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, waypoint.position, stepSpeed * dt);
    }
}
}
