using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class PlayerScript : MonoBehaviour
{


    [Header("Movement parameters")]
    [SerializeField] public Vector3 velocity;
    [SerializeField] float speed;
    [SerializeField] float maxForce;

    [Header("Pursue information")]
    [SerializeField] private float predictionTime;

    [Header("Wander parameters")]
    [SerializeField] private float wanderRadius = 2f;
    [SerializeField] private float wanderDistance = 3f;
    [SerializeField] private float wanderJitter = 1f;

     [Header("Additional parameters")]
    [SerializeField] private float weight;
    [SerializeField] List<Transform> obstacles;

    private GameObject myTarget;
    private Vector3 wanderTarget = Vector3.zero;
    private Vector3 steering;
    private Vector3 desired;
    private LineRenderer lineyVelocity;
    private LineScript lineSteeringScript;
    private LineScript lineDesiredDirectionScript;

    public bool active = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        GameObject lineSteering = GameObject.Find("LineSteering");
        lineSteeringScript = lineSteering.GetComponent<LineScript>();

        lineSteeringScript.position = transform.position;
        lineSteeringScript.steering = steering;

        GameObject lineDesired = GameObject.Find("LineDesired");
        lineDesiredDirectionScript = lineDesired.GetComponent<LineScript>();

        lineDesiredDirectionScript.position = transform.position;
        lineDesiredDirectionScript.steering = steering;
        lineDesiredDirectionScript.SetColor(Color.gray);

        myTarget = GameObject.FindWithTag("myTarget");

         //Linerenderer stuff
        lineyVelocity = gameObject.AddComponent<LineRenderer>();
        lineyVelocity.startWidth = 0.1f;
        lineyVelocity.endWidth = 0.1f;
        lineyVelocity.material = new Material(Shader.Find("Sprites/Default"));
        lineyVelocity.startColor = Color.green;
        lineyVelocity.endColor = Color.green;
        lineyVelocity.sortingOrder = 0;
        lineyVelocity.sortingLayerName = "Default";


    }

    // Update is called once per frame
    void Update()
    {
        steering = Vector3.zero;

        if(Keyboard.current.aKey.wasPressedThisFrame){
            active = !active;
        }

        if(active){
            //Compute steering vector
            Debug.Log("Using steering");
            //steering = Seek(myTarget.transform.position);

            //steering = Flee(myTarget.transform.position);

            /*
            steering = Pursue(
                myTarget.transform, 
                myTarget.GetComponent<TargetScript>().velocity);
            */

            /*
            steering = Evade(
                myTarget.transform,
                myTarget.GetComponent<TargetScript>().velocity,
                predictionTime);
            */

            //steering = Wander();

            //steering = Hide(myTarget.transform, obstacles);

            steering = Hide(myTarget.transform, obstacles) * 0.5f + Flee(myTarget.transform.position) * 0.5f;

        }

        velocity += steering * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, speed);

        transform.position += velocity * Time.deltaTime;

        if (velocity != Vector3.zero)
            transform.up = velocity;
        
        DrawVelocityAndSteering();
    }

    Vector3 Seek(Vector3 targetPos)
    {

        //Obtener el seek dir o desired
        desired = (targetPos - transform.position).normalized * speed;
        Vector3 steering = desired - velocity;
        return Vector3.ClampMagnitude(steering, maxForce);
    }   


    Vector3 Flee(Vector3 targetPos)
    {
        desired = (transform.position - targetPos).normalized * speed;
        Vector3 steering = desired - velocity;
        return Vector3.ClampMagnitude(steering, maxForce);
    }

    Vector3 Pursue(Transform target, Vector3 velocity)
    {

        float distance = Vector3.Distance(target.position, transform.position);

        float newT = distance / speed;

        Vector3 futurePosition = target.position + velocity * newT;

        return Seek(futurePosition);
        
    }

    Vector3 Evade(Transform target, Vector3 velocity, float t)
    {
        float distance = Vector3.Distance(target.position, transform.position);

        float newT = distance / speed;

        Vector3 futurePosition = target.position + velocity * newT;

        return Flee(futurePosition);
    }


    Vector3 Wander()
    {
        wanderTarget += new Vector3(
            Random.Range(-1, 1) * wanderJitter,
            Random.Range(-1, 1) * wanderJitter,
            0
            );

        wanderTarget = wanderTarget.normalized * wanderRadius;

        Vector3 targetPos = transform.position + velocity * wanderDistance + wanderTarget;

        return Seek(targetPos);
    }

    Vector3 Hide(Transform target, List<Transform> obstacles)
    {
        float closestDist = Mathf.Infinity;
        Vector3 bestObstacle = Vector3.zero;

        foreach (Transform t in obstacles)
        {
            float dist = Vector3.Distance(transform.position, t.position);


            if(dist < closestDist)
            {
                closestDist = dist;
                bestObstacle = t.position;
            }
        }

        if(closestDist == Mathf.Infinity)
        {
            return Flee(target.position);
        }

        return Seek(bestObstacle);
    }

     void DrawVelocityAndSteering()
    {

        lineSteeringScript.position = transform.position + velocity;
        lineSteeringScript.steering = steering;
        lineSteeringScript.SetColor(Color.blue);
        
        lineDesiredDirectionScript.position = transform.position;
        lineDesiredDirectionScript.steering = desired;

        lineyVelocity.positionCount = 2;
        lineyVelocity.SetPosition(0, transform.position);
        lineyVelocity.SetPosition(1, transform.position + velocity);
    
    }    
}
