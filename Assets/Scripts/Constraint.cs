using UnityEngine;
using System.Collections;

public class Constraint {

	private float rest_distance; // the length between particle p1 and p2 in rest configuration

    Particle p1;
    Particle p2; // the two particles that are connected through this constraint

    public bool isTorn
    {
        get; private set;
    }

	public Constraint(Particle _p1, Particle _p2)
    {
        p1 = _p1;
        p2 = _p2;
        isTorn = false;
        Vector3 vec = p1.getPos() - p2.getPos();
        rest_distance = vec.magnitude;
    }

    public bool checkTornFromParticle(Particle target)
    {
        return isTorn && ( p1 == target || p2 == target);
    }


    /* This is one of the important methods, where a single constraint between two particles p1 and p2 is solved
	the method is called by Cloth.time_step() many times per frame*/
    public void satisfyConstraint()
    {
        if (isTorn) return;
        Vector3 p1_to_p2 = p2.getPos() - p1.getPos(); // vector from p1 to p2

        if (Mathf.Abs(p1_to_p2.magnitude - rest_distance) < 0.005f) return;

        float current_distance = p1_to_p2.magnitude; // current distance between p1 and p2
        
        Vector3 correctionVector = p1_to_p2 * (1 - rest_distance / current_distance); // The offset vector that could moves p1 into a distance of rest_distance to p2
        Vector3 correctionVectorHalf = correctionVector * 0.5f; // Lets make it half that length, so that we can move BOTH p1 and p2.
        p1.offsetPos(correctionVectorHalf); // correctionVectorHalf is pointing from p1 to p2, so the length should move p1 half the length needed to satisfy the constraint.
        p2.offsetPos(-correctionVectorHalf); // we must move p2 the negative direction of correctionVectorHalf since it points from p2 to p1, and not p1 to p2.	
    }

    public void checkTear()
    {
        Vector3 p1_to_p2 = p2.getPos() - p1.getPos(); // vector from p1 to p2

        if (Mathf.Abs(p1_to_p2.magnitude - rest_distance) > 1.0f)
        {
            //p1.SetTorn();
            //p2.SetTorn();
            isTorn = true;
            //p1.resetAcceleration();
            //p2.resetAcceleration();
        }
    }
}
