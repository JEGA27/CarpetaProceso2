using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    private enum FSMStates
    {
        Patrol, Chase, Aim, Shoot, Evade
    }

    [SerializeField]
    private FSMStates currentState = FSMStates.Patrol;
    private int health = 100;
    private Vector3 destPos;

    public GameObject bullet;
    public Transform playerTransform;
    public GameObject bulletSpawnPoint;
    public Transform turretBase;
    public List<GameObject> pointList;
    public float curSpeed;
    public float rotSpeed = 150.0f;
    public float turretRotSpeed = 10.0f;
    public float maxForwardSpeed = 30.0f;
    public float maxBackwardSpeed = -30.0f;
    public float shootRate = 0.5f;
    private float elapsedTime;
    public float patrolRadius = 10f;
    private float patrolReachPlayer = 3f;
    public float chaseRadius = 25f;
    public float AttackRadius = 20f;
    private int index = -1;
    private bool underAttack = false;
    private bool sideFlag = false;

    // Start is called before the first frame update
    void Start()
    {
        FindNextPoint();
    }

    private void FindNextPoint()
    {
        print("Finding next point");
        index = (index + 1) % pointList.Count; //Random.Range(0, pointList.Count);
        destPos = pointList[index].transform.position;
    }
    private void FindPlayer()
    {
        //print("Finding player");
        destPos = playerTransform.position;
    }

    void Update()
    {
        switch (currentState)
        {
            case FSMStates.Patrol:
                UpdatePatrol();
                break;

            case FSMStates.Chase:
                UpdateChase();
                break;

            case FSMStates.Aim:
                UpdateAim();
                break;

            case FSMStates.Shoot:
                UpdateShoot();
                break;

            case FSMStates.Evade:
                UpdateEvade();
                break;
        }
    }

    void UpdatePatrol()
    {
        //Find another random patrol point if the current point is reached
        if (Vector3.Distance(transform.position, destPos) <= patrolRadius)
        {
            print("Reached the destination point -- calculating the next point");
            FindNextPoint();
        }
        //Check the distance with player tank, when the distance is near, transition to chase state
        else if (Vector3.Distance(transform.position, playerTransform.position) <= chaseRadius)
        {
            print("Switch to Chase state");
            currentState = FSMStates.Chase;
        }
        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);

    }

    void UpdateChase()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) <= patrolReachPlayer)
        {
            print("player reached");
            currentState = FSMStates.Aim;
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) >= chaseRadius)
        {
            print("player out of sight");
            currentState = FSMStates.Patrol;
        }
        else if(underAttack)
        {
            print("under attack");
            sideFlag = System.Convert.ToBoolean(Random.Range(0, 2));
            currentState = FSMStates.Evade;
        }
        FindPlayer();
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }
    void UpdateAim()
    {
        Debug.DrawRay(turretBase.position, destPos, Color.red);
        FindPlayer();
        Quaternion targetRotation = Quaternion.LookRotation(destPos - turretBase.position);
        turretBase.rotation = Quaternion.Slerp(turretBase.rotation, targetRotation, Time.deltaTime * rotSpeed);
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= 1.0f)
        {
          currentState = FSMStates.Shoot;
        }

    }

    void UpdateShoot()
    {

      elapsedTime += Time.deltaTime;
      if(elapsedTime >= shootRate)
      {
          elapsedTime = 0.0f;
          Instantiate(bullet, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
      }
      else
      {
          currentState = FSMStates.Chase;
      }
    }

    void UpdateEvade()
    {
        Vector3 lastPos = transform.position;
        Vector3 newPos;
        Vector3 evadePos;
        elapsedTime += Time.deltaTime;

        if(sideFlag)
        {
          Debug.Log("Right");
          evadePos = transform.right * 3;
          Quaternion targetRotation = Quaternion.LookRotation(evadePos - transform.position);
          transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
          transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
        }
        else
        {
          Debug.Log("Left");
          evadePos = transform.right * 5;
          Quaternion targetRotation = Quaternion.LookRotation(-evadePos - transform.position);
          transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
          transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
        }

        if(elapsedTime >= 2.0f)
        {
            currentState = FSMStates.Chase;
            underAttack = false;
            elapsedTime = 0f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Bullet")
        {
            underAttack = true;
        }
    }

    private void FixedUpdate()
    {

    }
}
