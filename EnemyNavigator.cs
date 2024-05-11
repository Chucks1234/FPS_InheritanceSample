using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigator : MonoBehaviour
{
    Rigidbody body;
    NavMeshAgent agent;
    [SerializeField] float wanderRange;
    [SerializeField] FPSController player;
    [SerializeField] float playerSightRange = 3;
    [SerializeField] float attackRange = 1;
    [SerializeField]float recoveryTime;
    float elapsed = 0;

    public enum longStates
    {
        wander,
        pursuit,
        attack,
        recovery
    }
    public longStates steveStates;

    Vector3 startingLocation;



    // Start is called before the first frame update
    void Start()
    {
        steveStates = longStates.wander;

        agent = GetComponent<NavMeshAgent>();
        startingLocation = transform.position;

        GetRandomPointInRange();

        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (steveStates)
        {
            case longStates.wander:
                elapsed += Time.deltaTime;
                if(elapsed >= 5)
                {
                    elapsed = 0;
                    GoToRandomPoint();
                }
                if(Vector3.Distance(player.transform.position, this.transform.position) <= playerSightRange)
                {
                    steveStates = longStates.pursuit;
                }
                break;
            case longStates.pursuit:
                agent.SetDestination(player.transform.position);
                if (Vector3.Distance(player.transform.position, this.transform.position) >= playerSightRange)
                {
                    steveStates = longStates.wander;
                }
                else if(Vector3.Distance(player.transform.position, this.transform.position) <= attackRange)
                {
                    steveStates = longStates.attack;
                }
                break;
            case longStates.attack:
                body.isKinematic = false;
                body.AddForce(transform.forward * 3, ForceMode.Impulse);
                Debug.Log("BONK");
                body.isKinematic = true;
                steveStates = longStates.recovery;
                elapsed = 0;
                break;
            case longStates.recovery:
                elapsed += Time.deltaTime;
                agent.isStopped = true;
                if (elapsed >= recoveryTime)
                {
                    agent.isStopped = false;
                    steveStates = longStates.pursuit;
                }
                break;
        }
    }

    [ContextMenu("Do Thing")]
    public void GoToRandomPoint()
    {
        agent.SetDestination(GetRandomPointInRange());
    }

    Vector3 GetRandomPointInRange()
    {
        Vector3 offset = new Vector3(Random.Range(-wanderRange, wanderRange), 0 , Random.Range(-wanderRange, wanderRange));
        NavMeshHit hit;

        bool gotPoint = NavMesh.SamplePosition(startingLocation + offset, out hit, 1, NavMesh.AllAreas);
        //Debug.Log(gotPoint);
        if (gotPoint)
            return hit.position;

        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, wanderRange);
    }
}
