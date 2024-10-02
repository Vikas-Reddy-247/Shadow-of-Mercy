using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class GuardAI : MonoBehaviour
{
  public float walkSpeed = 1;
  public float runSpeed = 4;
  public float waitTime = 5f;
  public float resumePatrol = 11f;

  public float stopDistance = 7f;
  public float shortDistance = 2f;
  public float detectionRadius = 11f;       // The distance within which the Guard can detect the player
  public float detectionAngle = 147f;       // The angle within which the Guard can detect the player
  
  public bool isChasing = false;
  public bool isInvestigating = false;
  private bool savePosition = false;
  private bool discPosition = false;
  public bool isConscious = true;

  public Slider detectionSlider;
  private float sliderValue = 0; 
  public float sliderTransitionTime = 1.5f;
  public float detectionSpeedMultiplier = 11f;    //Time taken by guard to detect player
  public float fireForce = 700;

  public GameObject arrowPrefab;
  public Transform arrowSpawnPoint;

  public LayerMask viewMask;
  Transform player;
  Transform disc;
  Animator anim;
  public Transform pathHolder;
  private NavMeshAgent navMeshAgent;
  private Vector3 lastSeenPosition;
  private Vector3 lastdiscPosition;
  private Vector3 initialWaypointPosition;
  private Quaternion initialWaypointRotation;
  GuardHealth guardHealth;
  PlayerGeneral playerGen;
  GameManagement gameManagement;
  private Coroutine investigate;
  private Collider objCollider;
  public GameObject takedownButton;

  void Start()
  {
    player = GameObject.FindGameObjectWithTag("Player").transform;
    disc = GameObject.FindGameObjectWithTag("Disc").transform;
    playerGen = player.GetComponent<PlayerGeneral>();
    anim = GetComponent<Animator>();
    navMeshAgent = GetComponent<NavMeshAgent>();
    guardHealth = GetComponent<GuardHealth>();
    objCollider = GetComponent<Collider>();
    GameObject manage = GameObject.FindGameObjectWithTag("Relic");
    gameManagement = manage.GetComponent<GameManagement>();
    takedownButton.SetActive(false);

    //Stores the locations of placed Paths
    Vector3[] waypoints = new Vector3 [pathHolder.childCount];
    for (int i = 0; i < waypoints.Length; i++)
    {
      waypoints [i] = pathHolder.GetChild (i).position;
    }

    StartCoroutine(StartPatrolling(waypoints));
  }

  void Update()
  {
    if (PlayerDetected())
    {
      //Calculates the distance btw Guard and Player.
      float distanceToPlayer = Vector3.Distance(transform.position, player.position);
      float rate = Mathf.Clamp(1 / distanceToPlayer, 0.1f, 1) * detectionSpeedMultiplier;
      sliderValue = Mathf.MoveTowards(sliderValue, 1, rate * Time.deltaTime / sliderTransitionTime);

      //If detection meter is full then initiates chase.
      if (sliderValue >= 1)
      {
        isChasing = true;
      }
    }
    else
    {
      sliderValue = Mathf.MoveTowards(sliderValue, 0, Time.deltaTime / sliderTransitionTime);
    }
    detectionSlider.value = sliderValue;

    //If chasing is true then ChasePlayer method is called
    if (isChasing)
    {
      ChasePlayer();
      //Checks the optional objective of Undetected as player was detected
      gameManagement.detected = true;
    }

    //If Guard is in investigation and if player is detected then pause Investigation and resume Chasing
    if (isInvestigating && PlayerDetected())
    {
      StopCoroutine(Investigate());
      isChasing = true;
    }

    //The boolean is accessed from another script. If the Guard health is zero then specified animation runs and Unconscious method is called
    if(guardHealth.isUnconscious)
    {
      anim.SetTrigger("knocked");
      Unconscious();
    }

    //The boolean is accessed from another script. If the Guard is electrocuted then specified animation runs and Unconscious method is called
    if(guardHealth.iselectrocuted)
    {
      anim.SetTrigger("electrocute");
      Unconscious();
    }
    
    //The boolean is accessed from another script. If the Guard is attacked then Chasing is initiated
    if(guardHealth.isAlert)
    {
      isChasing = true;
      guardHealth.isAlert = false;
    }

    //Implementing ditraction mechanic when key is pressed
    if(Input.GetKeyDown(KeyCode.E))
    {
      Distraction();
    }
  }

  IEnumerator StartPatrolling(Vector3[] waypoints)
  {
    // If there's only one waypoint, stay stationary at that waypoint
    if (waypoints.Length == 1)
    {
        transform.position = waypoints[0];
        initialWaypointPosition = waypoints[0];
        initialWaypointRotation = transform.rotation;
        yield break; // Exit the coroutine, no need to continue patrolling
    }

    // If there are multiple waypoints, start patrolling
    StartCoroutine(FollowPath(waypoints));
  }


  //Patrols the set path using navmesh
  IEnumerator FollowPath(Vector3[] waypoints)
  {
    transform.position = waypoints[0];
    navMeshAgent.SetDestination(waypoints[0]);
    int targetWaypointIndex = 1;
    Vector3 targetWaypoint = waypoints[targetWaypointIndex];

    while (true)
    {          
      if(!isChasing && !isInvestigating && !guardHealth.isAlert)
      {
        anim.SetFloat("speed", 2);
        navMeshAgent.speed = walkSpeed;
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
          navMeshAgent.SetDestination(targetWaypoint);
          if (Vector3.Distance(transform.position, targetWaypoint) < 0.5f)
          {
            anim.SetFloat("speed", 0);
            targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
            targetWaypoint = waypoints[targetWaypointIndex];
            yield return new WaitForSeconds(waitTime);
          }             
        }
      }
      yield return null;
    }
  }

  bool PlayerDetected()
  {
    if (Vector3.Distance(transform.position, player.position) < detectionRadius)        //Checks if the player is in Guard's Detection Radius
    {
      Vector3 dirToPlayer = (player.position - transform.position).normalized;          //Checks if the player is in Guard's Detection Angle
      float angBtwGuardAndPlayer = Vector3.Angle (transform.forward, dirToPlayer);
      if (angBtwGuardAndPlayer < detectionAngle / 2f)
      {
        if (!Physics.Linecast (transform.position, player.position, viewMask))          //Checks if the Detection btw Guard and player is obstructed
        {
          return true;
        }
      }
    }
    return false;
  }

  //Chaing the player mechanic implementation
  void ChasePlayer()
  {

    float distanceToPlayer = Vector3.Distance(transform.position, player.position);

    //If Guard is provoked and player is in firing range then fires arrows
    if (PlayerDetected() && distanceToPlayer <= stopDistance)
    {
      transform.LookAt(player);
      anim.SetFloat("speed", 0);
      navMeshAgent.SetDestination(transform.position);
      anim.SetBool("shoot", true);
    }
    else
    {
      anim.SetFloat("speed", 4);
      anim.SetBool("shoot", false);
      navMeshAgent.speed = runSpeed;
      navMeshAgent.SetDestination(player.position);
    }

    //If Guard is provoked and player cannot be found StartInvestigation method is called
    if(!PlayerDetected())
    {
      StartInvestigation();
    }
  }
  
  //Instantiates the arrow toward the Player
  void FireArrow()
  {
    GameObject obj = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation) as GameObject;

    if (obj != null)
    {
      obj.name = "Arrow";
      Rigidbody arrowRb = obj.GetComponent<Rigidbody>();

      if (arrowRb != null)
      {
        arrowRb.AddForce(arrowSpawnPoint.forward * fireForce, ForceMode.Acceleration);
      }
      Destroy(obj, 1f);
    }
  }

  //In this method the Guard moves to the player's last seen position
  void StartInvestigation()
  {
    isInvestigating = true;
    detectionSlider.value = sliderValue;
    PlayerPosition();
    navMeshAgent.SetDestination(lastSeenPosition);
      
    // Start a coroutine to wait until the guard reaches the last seen position
    StartCoroutine(WaitForInvestigation());
  }

  IEnumerator WaitForInvestigation()
  {
      // Wait until the guard reaches the last seen position
      while (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
      {
          yield return null;
      }

      // Guard has reached the last seen position, trigger investigate animation
      anim.SetFloat("speed", 0);
      anim.SetBool("investigate", true);
      
      // Start the investigation coroutine
      investigate = StartCoroutine(Investigate());
  }


  IEnumerator Investigate()
  {
    yield return new WaitForSeconds(resumePatrol);
    anim.SetBool("investigate", false);
    investigate = null;
    isChasing = false;
    isInvestigating = false;
    transform.rotation = initialWaypointRotation;
    navMeshAgent.SetDestination(initialWaypointPosition);
  }

  public void Distraction()
  {
    float distanceToDisc = Vector3.Distance(transform.position, disc.position);
    {
      if(distanceToDisc <= detectionRadius)
      {
        DiscInvestigation();
      }
    }
  }

  //Guard moves to the position of the disc
  void DiscInvestigation()
  {
    isInvestigating = true;
    detectionSlider.value = sliderValue;
    DiscPosition();
    navMeshAgent.SetDestination(lastdiscPosition);
    anim.SetFloat("speed", 2);

    // Start a coroutine to wait until the guard reaches the last seen position
    StartCoroutine(WaitForDiscInvestigation());
  }

  //When Guard is near disc then Investigation process starts
  IEnumerator WaitForDiscInvestigation()
  {
    // Wait until the guard reaches the last seen position
    while (navMeshAgent.remainingDistance <= shortDistance)
    {
      yield return null;
    }

    // Guard has reached the last seen position, trigger investigate animation
    anim.SetFloat("speed", 0);
    anim.SetBool("investigate", true);
      
    // Start the investigation coroutine
    investigate = StartCoroutine(Investigate());
  }

  //When the massive object falls on the guard the animation runs and Unconscious method is called
  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Trap"))
    {
      anim.SetTrigger("boinked");
      Unconscious();
    }
  }

  //When player performs takedown mechanic the specific animation of guard runs and Unconscious method is called
  public void GetTakendown()
  {
    transform.rotation = player.rotation;
    transform.position = playerGen.guardLock.position;
    anim.SetTrigger("strangled");
    Unconscious();
  }

  //If Unconscious method is called the guard's health drops to zero, stops all coroutine, Collider is disabled
  void Unconscious()
  {
    guardHealth.guardHp = 0;
    detectionSlider.value = 0;
    guardHealth.healthSlider.gameObject.SetActive(false);
    isConscious = false;
    enabled = false;
    StopAllCoroutines();
    navMeshAgent.speed = 0;
    objCollider.enabled = false;
  }

  //If player is defeted then animations run and guard is disabled
  public void HandlePlayerKnocked()
  {
    anim.SetBool("shoot", false);
    anim.SetFloat("speed", 0);
    enabled = false;
  }

  //Records player position once when called
  void PlayerPosition()
  {
    if(!savePosition)
    {
      lastSeenPosition = player.position;
      savePosition = true;
    }
  }

  //Records disc position once when called
  void DiscPosition()
  {
    if(!discPosition)
    {
      lastdiscPosition = disc.position;
      discPosition = true;
    }
  }

  //When player collided to guard the chase is initiated
  private void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.CompareTag("Player"))
    {
      isChasing = true;
    }
  }

  //Visualises the lines of Radius, angle and patrol paths in scene window
  void OnDrawGizmos()
  {
    Vector3 startPosition = pathHolder.GetChild (0).position;       //Draw spheres where the paths are set
    Vector3 previousPosition = startPosition;

    foreach (Transform waypoint in pathHolder)
    {
      Gizmos.DrawSphere (waypoint.position, 0.3f);
      Gizmos.DrawLine (previousPosition, waypoint.position);
      previousPosition = waypoint.position;
    }
    Gizmos.DrawLine (previousPosition, startPosition);

    Gizmos.color = Color.red;
    Gizmos.DrawRay (transform.position, transform.forward * detectionRadius);       //Draw forward Detection Line
    Gizmos.DrawWireSphere(transform.position, detectionRadius);                     //Draw forward Detection Radius

    float halfFOV = detectionAngle * 0.5f;                                          // Draw the detection cone
    Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
    Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);

    Vector3 leftRayDirection = leftRayRotation * transform.forward;
    Vector3 rightRayDirection = rightRayRotation * transform.forward;

    Gizmos.DrawLine(transform.position, transform.position + leftRayDirection * detectionRadius);
    Gizmos.DrawLine(transform.position, transform.position + rightRayDirection * detectionRadius);
  }
}
