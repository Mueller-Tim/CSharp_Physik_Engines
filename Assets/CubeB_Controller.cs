using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using UnityEngine;

public class CubeB_Controller : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject B;
    public GameObject C;
    public GameObject C1;
    public GameObject C2;

    public Transform CPos;
    public Transform C1Pos;


    private Rigidbody rbB;
    private Rigidbody rbC;
    private Rigidbody rbC1;
    private Rigidbody rbC2;

    private Vector3 origin;

    private float currentTimeStep;

    private bool collisionCube = false;

    private float a;

    private List<List<float>> timeSeries;
    void Start()
    {
        rbB = B.GetComponent<Rigidbody>();
        rbC = C.GetComponent<Rigidbody>();
        rbC1 = C1.GetComponent<Rigidbody>();
        rbC2 = C2.GetComponent<Rigidbody>();

        rbB.angularDrag = 0;
        rbC.angularDrag = 0;
        rbC1.angularDrag = 0;
        rbC2.angularDrag = 0;

        var renderer = B.GetComponent<Renderer>();
        a = renderer.bounds.size.x;

        origin = calcOrigin();
        timeSeries = new List<List<float>>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        currentTimeStep += Time.deltaTime;
        float bahnDrehB = 0;
        float bahnDrehC = 0;

        float eigenDrehB = 0;
        float eigenDrehC = 0;

        float bahnDrehBC = 0;
        float eigenDrehBC = 0;
        if (!collisionCube)
        {
            bahnDrehB = AngularMomentum(rbB, origin).magnitude;
            bahnDrehC = AngularMomentum(rbC, origin).magnitude;

            eigenDrehB = eigenDrehImpulsAussen(rbB) * rbB.angularVelocity.magnitude;
            eigenDrehC = eigenDrehImpulsMitte(rbC) * rbC.angularVelocity.magnitude;
        }
        else
        {
            bahnDrehBC = AngularMomentumAfterCollision(rbC, origin).magnitude;
            eigenDrehBC = rbB.angularVelocity.magnitude * (eigenDrehImpulsMitte(rbC) + eigenDrehImpulsMitte(rbC1) + eigenDrehImpulsAussen(rbB) + eigenDrehImpulsAussen(rbC2));
        }
        List<float> newEntry = new() {
        currentTimeStep, bahnDrehB, bahnDrehC, bahnDrehBC, eigenDrehB, eigenDrehC, eigenDrehBC
    };
        timeSeries.Add(newEntry);
    }

    public Vector3 calcOrigin()
    {
        Vector3 distance = (C1Pos.position - CPos.position) / 2;

        return CPos.position + distance;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == C)
        {
            FixedJoint joint = C.AddComponent<FixedJoint>();
            joint.connectedBody = rbB;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            collisionCube = true;
        }
    }

    Vector3 AngularMomentum(Rigidbody rb, Vector3 origin)
    {
        Vector3 R = rb.transform.position - origin;
        Vector3 p = rb.velocity * rb.mass;
        Vector3 angular_momentum = Vector3.Cross(R, p);
        return angular_momentum;
    }

    Vector3 AngularMomentumAfterCollision(Rigidbody rb, Vector3 origin)
    {
        Vector3 R = new Vector3(rb.transform.position.x - origin.x, 0, 0);
        Vector3 p = new Vector3(rb.velocity.x * 4 * rb.mass, 0, 0);
        Vector3 angular_momentum = Vector3.Cross(R, p);
        return angular_momentum;
    }


    float eigenDrehImpulsMitte(Rigidbody rb)
    {
        return (float)((1 / 6.0) * rb.mass * Math.Pow(a, 2) + rb.mass * Math.Pow(a / 2, 2));
    }

    float eigenDrehImpulsAussen(Rigidbody rb)
    {
        return (float)((1 / 6.0) * rb.mass * Math.Pow(a, 2) + rb.mass * (Math.Pow(a, 2) + Math.Pow(a / 2.0, 2)));
    }


    void OnApplicationQuit()
    {
        WriteTimeSeriesToCSV();
    }

    void WriteTimeSeriesToCSV()
    {
        using (var streamWriter = new StreamWriter("time_series.csv"))
        {
            streamWriter.WriteLine("t,BahnDrehB, BahnDrehC, BahnDrehBC, EigenDrehB, EigenDrehC, EigenDrehBC");

            foreach (List<float> timeStep in timeSeries)
            {
                streamWriter.WriteLine(string.Join(",", timeStep));
                streamWriter.Flush();
            }
        }
    }
}
