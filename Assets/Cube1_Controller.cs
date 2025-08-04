using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CubeController : MonoBehaviour
{
    private Rigidbody rigidBody;
    private GameObject cubeB;
    private float cubeAMass;
    private float cubeBMass;
    private Rigidbody rbCubeB;
    private Vector3 startPositonCubeA;
 

    public float springConstant = 5.0f;

    private readonly float maxSwingTime = 5.0f; 
    private float startTime; 

    private Vector3 forceWind;


    private float springConstantBetweenCubs;

    private Vector3 cubeSize;
    private float lengthSpring;

    private bool hasWind;

    private float currentTimeStep;
    private List<List<float>> timeSeries;

    private float energieSpringCubeA;
    private float energieSpringCubeB;
    private float energieSpringTotal;
    private float energieKeneticCubeA;
    private float energieKeneticCubeB;
    private float energieKeneticTotal;
    private float energieTotal;
    private float momentumCubeA;
    private float momentumCubeB;
    private float momentumTotal;

    private float distanceToCubeB;

    
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        startPositonCubeA = transform.position;
        cubeB = GameObject.Find("Cube B");
        cubeSize = cubeB.transform.localScale;
        rbCubeB = cubeB.GetComponent<Rigidbody>();
        startTime = Time.time;
        energieSpringCubeA = 0;
        energieSpringCubeB = 0;
        lengthSpring = 1f;
        hasWind = true;
        cubeAMass = rigidBody.mass;
        cubeBMass = rbCubeB.mass;
        timeSeries = new List<List<float>>();

    }

    
    void FixedUpdate()
    {
        float elapsedTime = Time.time - startTime;

        if (elapsedTime < maxSwingTime)
        {
            
            Vector3 springForce = -springConstant * transform.position;
            
            rigidBody.AddForce(springForce);

        }        
        if (hasWind && elapsedTime >= maxSwingTime) 
        {
            InitializeWind();
            rigidBody.AddForce(forceWind);
            
            ComputeSpringConstant();
        }
        distanceToCubeB = Mathf.Abs(Vector3.Distance(transform.position, cubeB.transform.position)) - cubeSize.x;
        if(distanceToCubeB < lengthSpring)
        {
                      
            ApplySpringForce();
            hasWind = false;
            
        }
        

        energieKeneticCubeA = 0.5f * cubeAMass * Mathf.Pow(rigidBody.velocity.x, 2);
        
        energieKeneticCubeB = 0.5f * cubeBMass * Mathf.Pow(rbCubeB.velocity.x, 2);

        energieSpringTotal = energieSpringCubeB + energieSpringCubeA;

        energieKeneticTotal = energieKeneticCubeA + energieKeneticCubeB;

        energieTotal = energieKeneticCubeA + energieKeneticCubeB + energieSpringCubeA; 

        momentumCubeA = cubeAMass * rigidBody.velocity.x;
        momentumCubeB = cubeBMass * rbCubeB.velocity.x;
        momentumTotal = momentumCubeA + momentumCubeB;

        currentTimeStep += Time.deltaTime;
        List<float> newEntry = new() {
    currentTimeStep, rigidBody.position.x, rigidBody.velocity.x,
    energieKeneticCubeA, momentumCubeA, rbCubeB.position.x, rbCubeB.velocity.x,
    energieKeneticCubeB, momentumCubeB, energieKeneticTotal, energieSpringTotal ,energieTotal, momentumTotal
};
        timeSeries.Add(newEntry);

    }

    void ApplySpringForce()
    {
        Vector3 displacement = rbCubeB.transform.position - transform.position;
        
        Vector3 direction = displacement.normalized;
        float compression = lengthSpring - distanceToCubeB;
        Vector3 springForce = -springConstantBetweenCubs * compression * direction;
        
        rigidBody.AddForce(springForce);
        rbCubeB.AddForce(-springForce);

        energieSpringCubeA = 0.5f * springConstantBetweenCubs * Mathf.Pow(compression, 2);
    }

        
        void InitializeWind()
    {
        Vector3 cubeSize = transform.localScale;
        float frontalArea = cubeSize.y * cubeSize.z;
        float volume = cubeSize.x * cubeSize.y * cubeSize.z;
        float mass = rigidBody.mass;
        float density = mass / volume;
        float resistanceCoefficient = 1.3f;
        float velocity = 5;
        float scaleFactor = (-1.0f / 2.0f) * density * frontalArea * resistanceCoefficient * Mathf.Pow(velocity-rigidBody.velocity.x, 2);
        
        forceWind = scaleFactor * new Vector3(-1f, 0f, 0f);
    }

    void ComputeSpringConstant()
    {
        float kineticEnergy = 0.5f * cubeAMass * rigidBody.velocity.x * rigidBody.velocity.x;
        springConstantBetweenCubs = 2 * kineticEnergy / Mathf.Pow(lengthSpring,2);
        
    }

    void OnApplicationQuit()
    {
       // WriteTimeSeriesToCSV();
    }

    void WriteTimeSeriesToCSV()
    {
        using (var streamWriter = new StreamWriter("time_series.csv"))
        {
            streamWriter.WriteLine("t,x(t)_A,v(t)_A,E_kin(t)_A, p(t)_A,x(t)_B,v(t)_B,E_kin(t)_B, p(t)_B, E_kin_(t)T, E_spring_(t)T, E_total(t)T, p(t)_T");

            foreach (List<float> timeStep in timeSeries) {
                streamWriter.WriteLine(string.Join(",", timeStep));
                streamWriter.Flush();
            }
        }
    }
    
}