using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Particle
{

    private bool movable;
    private float mass;
    private Vector3 position;
    private Vector3 previous_position;
    private Vector3 acceleration;
    private Vector3 accumulated_normal;

    private Cloth parent;
    private List<Constraint> constraints = new List<Constraint>();

    public bool deadParticle { get; private set; }

    public Particle(Cloth _parent, Vector3 pos)
    {
        parent = _parent;
        position = pos;
        previous_position = pos;
        acceleration = Vector3.zero;
        mass = 1;
        movable = true;
        deadParticle = false;
        accumulated_normal = Vector3.zero;
    }

    public void addConstraint(Constraint c)
    {
        constraints.Add(c);
    }

    public bool getIsConstraintTorn(Particle p)
    {
        foreach (var c in constraints)
            if (c.checkTornFromParticle(p))
                return true;
        return false;
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
        if (movable && !deadParticle)
        {
            Vector3 temp = position;
            position = position + (position - previous_position) * (1.0f - parent.damping) + acceleration;// * (Time.deltaTime);
            previous_position = temp;
            acceleration = Vector3.zero; // acceleration is reset since it HAS been translated into a change in position (and implicitely into velocity)

            int aliveConstraints = 0;
            foreach (Constraint c in constraints)
                if (c.isTorn == false)
                    aliveConstraints++;
            if (aliveConstraints == 0)
                deadParticle = true;


        }
    }


    public void tearConstraints(int x, int y)
    {
        foreach (Constraint c in constraints)
        {
            //if(c.GetOtherParticle(this) == parent.getParticle(x, y + 1) ||
            //    c.GetOtherParticle(this) == parent.getParticle(x, y - 1))
            c.tearConstraint();
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
