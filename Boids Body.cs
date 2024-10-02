using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BoidsBody : MonoBehaviour
{
    public AudioSource drums;
    public AudioSource bass;
    public AudioLowPassFilter lowFilter;
    public Camera mainCamera;

    //Array of GameObjects called Body
    GameObject[] body;
    public const int numberOfBody = 150;
    //100 bodies
    BodyProperty[] bodyProperty;
    TrailRenderer trailRenderer;
    public float gravityConstant = 1;

    private float distanceCounter = 0;
    public float averageDistance = 0;
    

    [Range(0, 100)]
    public float pushForce;

    [Range(0, 100)]
    public float pushDistance;

    [Range(0, 1)]
    public float maxAcceleration;

    [Range(-50,50)]
    public float originX = 0;

    //We are defining the other variables for each body that we can adjust
    struct BodyProperty
    {
        public float mass;
        public float radius;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
    }
    void Start()
    {
        //I can change masses, initial position, initial velocity and more
        //Initialize the array
        body = new GameObject[numberOfBody];
        bodyProperty = new BodyProperty[numberOfBody];

        pushDistance = 3;
        maxAcceleration = .5f;
        // create new game objects based on specified amount.

        for (int i = 0; i < numberOfBody; i++)
        {
            
            //Will add each sphere to the array with a random position.
            //body[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body[i] = GameObject.Instantiate(GameObject.Find("SphereSounder"));
            //This will set their position to be equally spaced around a circle. the "10" is an arbitrary Z value. 
            float radius = 15;
            body[i].transform.position = new Vector3(radius * Mathf.Cos(Mathf.PI * 2 /numberOfBody * i), radius * Mathf.Sin(Mathf.PI * 2 / numberOfBody * i), 70);
            //float theta = Mathf.PI * 2 / numberOfBody * i;

            //The velocity of the bodies is randomized on startup! Different result everytime!!!!!

            bodyProperty[i].velocity = new Vector3(Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f));
            bodyProperty[i].mass = 1.0f;

            trailRenderer = body[i].AddComponent<TrailRenderer>();
            // Configure the TrailRenderer's properties
            trailRenderer.time = 20.0f;  // Duration of the trail
            trailRenderer.startWidth = 0.5f;  // Width of the trail at the start
            trailRenderer.endWidth = 0.0f;    // Width of the trail at the end
            // a material to the trail
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            // Set the trail color over time
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                //new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(Mathf.Cos(Mathf.PI * 2 / numberOfBody * i), Mathf.Sin(Mathf.PI * 2 / numberOfBody * i), Mathf.Tan(Mathf.PI * 2 / numberOfBody * i)), 0.80f) },

                new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.blue, 1.0f) },

                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }

            );

            trailRenderer.colorGradient = gradient;
        }
    }
    void Update()
    {
        distanceCounter = 0;
        for (int i = 0; i < body.Length; i++)
        {
            bodyProperty[i].acceleration = Vector3.zero;
        }

        //body.Length is the number of bodies in the body array
        for (int i = 0; i < body.Length; i++)
        {        
            for (int j = i + 1; j < body.Length; j++)
            {
                Vector3 distance = body[i].transform.position - body[j].transform.position;
                //Gravity = gravityConstant * mass1 * mass2 / (distance * distance)
                float m1 = bodyProperty[i].mass;
                float m2 = bodyProperty[j].mass;
                Vector3 Gravity = gravityConstant * m1 * m2 / (distance.magnitude * distance.magnitude) * distance.normalized;

                if(distance.sqrMagnitude > pushDistance)
                {
                    bodyProperty[i].acceleration -= Gravity / m1;
                    bodyProperty[j].acceleration += Gravity / m2;
                }
                else
                {
                    bodyProperty[i].acceleration += Gravity / m1 * pushForce;
                    bodyProperty[j].acceleration -= Gravity / m2 * pushForce;
                }

            }
            

            //What is the origin they are all flying towards? This makes them flock!
            Vector3 origin = new Vector3 (originX, 0, 0);
            Vector3 originTowards = origin - body[i].transform.position;

            distanceCounter += Vector3.Distance(origin, body[i].transform.position);

            bodyProperty[i].acceleration += originTowards / 100f;

            //Let's constrain acceleration so they don't go flying!
            if (bodyProperty[i].acceleration.magnitude > maxAcceleration)
            {
                bodyProperty[i].acceleration = bodyProperty[i].acceleration.normalized * maxAcceleration;
            }

            bodyProperty[i].velocity += bodyProperty[i].acceleration * Time.deltaTime;
            body[i].transform.position += bodyProperty[i].velocity;
        }
        averageDistance = distanceCounter / numberOfBody;
        Debug.Log ("Average distance:" + averageDistance);
        drums.volume = averageDistance / 500;
        lowFilter.cutoffFrequency = averageDistance * 22;
        bass.volume = averageDistance / 500;
        if (averageDistance > 600)
        {
            changeGradient();
            //mainCamera.transform.position = mainCamera.transform.position - (new Vector3(0, 0, 500));
            //update gradient to rainbow. Make a new function for this probably.
        }
        //drums.volume = averageDistance/max distance value I'm considering (150) (150/150 = 1) (100/150 = .66)
        //if distance from origin is high enough {
        //Make drums increase in strength and eventually make the gradients be rainbow
        /*Calculating distance from origin: Have a distance counter in the for loop  that adds up total vectors, afterwards divide by the number of bodies*/

    }

    public void changeGradient()
    {
        
        
        for (int i = 0; i < numberOfBody; i++)
        {
            trailRenderer = body[i].GetComponent<TrailRenderer>();
            Gradient gradient = new Gradient();
            gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(Mathf.Cos(Mathf.PI * 2 / numberOfBody * i), Mathf.Sin(Mathf.PI * 2 / numberOfBody * i), Mathf.Tan(Mathf.PI * 2 / numberOfBody * i)), 0.80f) },

            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) } );
            trailRenderer.colorGradient = gradient;

        }
    }
}
