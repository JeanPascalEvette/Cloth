using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cloth : MonoBehaviour
{

    private List<Vector3> triangles = new List<Vector3>();
    private List<int> indices = new List<int>();

    public Particle[] particles;
    private List<Constraint> constraints = new List<Constraint>();

    [SerializeField]
    public int num_particles_width; // number of particles in "width" direction

    [SerializeField]
    public int num_particles_height; // number of particles in "height" direction


    [SerializeField]
    private float width;
    [SerializeField]
    private float height;

    [SerializeField]
    private GameObject ball;

    [SerializeField]
    private bool fourUnmovable = false;


    public float damping = 0.01f;
    public int constraintsIterations = 15;


    int timeStepSkip = 0;

    // Use this for initialization
    void Start()
    {
        particles = new Particle[num_particles_width * num_particles_height]; //I am essentially using this vector as an array with room for 
        // creating particles in a grid of particles from (0,0,0) to (width,-height,0)
        for (int x = 0; x < num_particles_width; x++)
        {
            for (int y = 0; y < num_particles_height; y++)
            {
                Vector3 pos = new Vector3(width * (x / (float)num_particles_width),
                                -height * (y / (float)num_particles_height),
                                0);
                particles[y * num_particles_width + x] = new Particle(this, pos); // insert particle in column x at y'th row
            }
        }

        // Connecting immediate neighbor particles with constraints (distance 1 and sqrt(2) in the grid)
        for (int x = 0; x < num_particles_width; x++)
        {
            for (int y = 0; y < num_particles_height; y++)
            {
                if (x < num_particles_width - 1) makeConstraint(getParticle(x, y), getParticle(x + 1, y));
                if (y < num_particles_height - 1) makeConstraint(getParticle(x, y), getParticle(x, y + 1));
                if (x < num_particles_width - 1 && y < num_particles_height - 1) makeConstraint(getParticle(x, y), getParticle(x + 1, y + 1));
                if (x < num_particles_width - 1 && y < num_particles_height - 1) makeConstraint(getParticle(x + 1, y), getParticle(x, y + 1));
            }
        }


        // Connecting secondary neighbors with constraints (distance 2 and sqrt(4) in the grid)
        for (int x = 0; x < num_particles_width; x++)
        {
            for (int y = 0; y < num_particles_height; y++)
            {
                if (x < num_particles_width - 2) makeConstraint(getParticle(x, y), getParticle(x + 2, y));
                if (y < num_particles_height - 2) makeConstraint(getParticle(x, y), getParticle(x, y + 2));
                if (x < num_particles_width - 2 && y < num_particles_height - 2) makeConstraint(getParticle(x, y), getParticle(x + 2, y + 2));
                if (x < num_particles_width - 2 && y < num_particles_height - 2) makeConstraint(getParticle(x + 2, y), getParticle(x, y + 2));
            }
        }


        // making the upper left most three and right most three particles unmovable
        for (int i = 0; i < 3; i++)
        {
            getParticle(0 + i, 0).offsetPos(new Vector3(0.5f, 0.0f, 0.0f)); // moving the particle a bit towards the center, to make it hang more natural - because I like it ;)
            getParticle(0 + i, 0).makeUnmovable();

            getParticle(num_particles_width - 1 - i, 0).offsetPos(new Vector3(-0.5f, 0.0f, 0.0f)); // moving the particle a bit towards the center, to make it hang more natural - because I like it ;)
            getParticle(num_particles_width - 1 - i, 0).makeUnmovable();



            if (fourUnmovable)
            {
                getParticle(0, num_particles_height - 1 - i).offsetPos(new Vector3(0.5f, 0.0f, 0.0f)); // moving the particle a bit towards the center, to make it hang more natural - because I like it ;)
                getParticle(0, num_particles_height - 1 - i).makeUnmovable();



                getParticle(num_particles_width - 1 - i, num_particles_height - 1 - i).offsetPos(new Vector3(-0.5f, 0.0f, 0.0f)); // moving the particle a bit towards the center, to make it hang more natural - because I like it ;)
                getParticle(num_particles_width - 1 - i, num_particles_height - 1 - i).makeUnmovable();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

        addForce(new Vector3(0, -0.098f, 0) * (Time.deltaTime)); // add gravity each frame, pointing down
        windForce(new Vector3(0.5f, 0, 0.3f) * (Time.deltaTime)); // generate some wind each frame

        timeStep();


        ballCollision(ball.transform.position, ball.transform.localScale.x); // resolve collision with the ball
    }
    

    void timeStep()
    {
        for (int u = 0; u < constraints.Count; u++)
        {

            constraints[u].checkTear();
             // checks value of cloth and tears.
        }
        for (int i = 0; i < constraintsIterations; i++) // iterate over all constraints several times
        {
            for (int u = 0; u < constraints.Count; u++)
            {
                constraints[u].satisfyConstraint(); // satisfy constraint.
            }
        }

        for (int u = 0; u < particles.Length; u++)
        {
            particles[u].timeStep(); // calculate the position of each particle at the next time step.
        }
    }

    public Particle getParticle(int x, int y) { return particles[y * num_particles_width + x]; }
    void makeConstraint(Particle p1, Particle p2) {
        var c = new Constraint(p1, p2);
        constraints.Add(c);
        p1.addConstraint(c);
        p2.addConstraint(c);
    }



    /* A private method used by drawShaded() and addWindForcesForTriangle() to retrieve the  
    normal vector of the triangle defined by the position of the particles p1, p2, and p3.
    The magnitude of the normal vector is equal to the area of the parallelogram defined by p1, p2 and p3
    */
    Vector3 calcTriangleNormal(Particle p1, Particle p2, Particle p3)
    {
        Vector3 pos1 = p1.getPos();
        Vector3 pos2 = p2.getPos();
        Vector3 pos3 = p3.getPos();

        Vector3 v1 = pos2 - pos1;
        Vector3 v2 = pos3 - pos1;

        return Vector3.Cross(v1, v2);
    }



    /* A private method used by windForce() to calcualte the wind force for a single triangle 
	defined by p1,p2,p3*/
    void addWindForcesForTriangle(Particle p1, Particle p2, Particle p3, Vector3 direction)
    {

        Vector3 normal = calcTriangleNormal(p1, p2, p3);
        Vector3 d = normal.normalized;
        Vector3 force = normal * (Vector3.Dot(d, direction));
        p1.addForce(force);
        p2.addForce(force);
        p3.addForce(force);
    }

    /* used to detect and resolve the collision of the cloth with the ball.
	This is based on a very simples scheme where the position of each particle is simply compared to the sphere and corrected.
	This also means that the sphere can "slip through" if the ball is small enough compared to the distance in the grid bewteen particles
	*/
    public void ballCollision(Vector3 center, float radius)
    {
        foreach (var particle in particles)
        {
            Vector3 v = particle.getPos() - center;
            float l = v.magnitude;
            if (v.magnitude < radius) // if the particle is inside the ball
            {
                particle.offsetPos(v.normalized * (radius - l)); // project the particle to the surface of the ball
            }
        }
    }

    /* used to add gravity (or any other arbitrary vector) to all particles*/
    public void addForce(Vector3 direction)
    {
        foreach (var particle in particles)
        {
            particle.addForce(direction); // add the forces to each particle
        }
    }
    /* used to add wind forces to all particles, is added for each triangle since the final force is proportional to the triangle area as seen from the wind direction*/
    public void windForce(Vector3 direction)
    {
        for (int x = 0; x < num_particles_width - 1; x++)
        {
            for (int y = 0; y < num_particles_height - 1; y++)
            {
                addWindForcesForTriangle(getParticle(x + 1, y), getParticle(x, y), getParticle(x, y + 1), direction);
                addWindForcesForTriangle(getParticle(x + 1, y + 1), getParticle(x + 1, y), getParticle(x, y + 1), direction);
            }
        }
    }

}