using UnityEngine;
using System.Collections;

public class Particle
{

    private bool movable;
    private float mass;
    private Vector3 position;
    private Vector3 previous_position;
    private Vector3 acceleration;
    private Vector3 accumulated_normal;

    private Cloth parent;

    public Particle(Cloth _parent, Vector3 pos)
    {
        parent = _parent;
        position = pos;
        previous_position = pos;
        acceleration = Vector3.zero;
        mass = 1;
        movable = true;
        accumulated_normal = Vector3.zero;
    }


    public void addForce(Vector3 f)
    {
        acceleration += f / mass;
    }

    /* This is one of the important methods, where the time is progressed a single step size (TIME_STEPSIZE)
	   The method is called by Cloth.time_step()
	   Given the equation "force = mass * acceleration" the next position is found through verlet integration*/
    public void timeStep()
    {
        if (movable)
        {
            Vector3 temp = position;
            position = position + (position - previous_position) * (1.0f - parent.damping) + acceleration * (Time.deltaTime*Time.deltaTime);
            previous_position = temp;
            acceleration = Vector3.zero; // acceleration is reset since it HAS been translated into a change in position (and implicitely into velocity)	
        }
    }



    public Vector3 getPos() { return position; }

    public void resetAcceleration() { acceleration = Vector3.zero; }

    public void offsetPos(Vector3 v) { if(movable) position += v;}

    public void makeUnmovable() { movable = false; }

    public void addToNormal(Vector3 normal)
    {
        accumulated_normal += normal.normalized;
    }

    public Vector3 getNormal() { return accumulated_normal; } // notice, the normal is not unit length

    public void resetNormal() { accumulated_normal = Vector3.zero; }
}
