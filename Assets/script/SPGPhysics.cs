using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

public class SPGPhysics : MonoBehaviour
{
    // When passing a data collection into a Unity Job it has to be one of the new Unity Native types.
    // .net genrics (like List<T>) and plain arrays (like T[]) wil not work in jobs! 
    private NativeArray<float> lifetimes;
    private NativeArray<Vector3> velocities;
    private NativeArray<Vector3> positions;
    private NativeArray<Matrix4x4> renderMatrices;
    private Matrix4x4[] renderMatrixArray;
    private NativeArray<int> sleepingTimer;
    private NativeQueue<int> asleep;
    
    public int spgRate = 1;//1023; // the most we can fit into a single call to Graphics.DrawInstance

    public float spgTime = 1f;

    public Mesh mesh;
    public Material material;
    public Transform spawnDirection;

    // this job creates a batch of RaycastCommands we are going to use for
    // collision detection against the world. these can be sent to PhysX
    // as a batch that will be executed in a job, rather than us having to
    // call Physics.Raycast in a loop just on the main thread!
    struct PrepareRaycastCommands : IJobParallelFor
    {
        public float DeltaTime;

        public NativeArray<RaycastCommand> Raycasts;
        [ReadOnly]
        public NativeArray<float> Lifetimes;
        [ReadOnly]
        public NativeArray<Vector3> Velocities;
        [ReadOnly]
        public NativeArray<Vector3> Positions;


        public void Execute(int i)
        {
            // figure out how far the object we are testing collision for
            // wants to move in total. Our collision raycast only needs to be
            // that far.
            float distance = (Velocities[i] * DeltaTime).magnitude;
            Raycasts[i] = new RaycastCommand(Positions[i], Velocities[i], distance);
        }
    }

    // Integrate the velocity into all the objects positions. We use the
    // Raycast hit data to decide how much of the velocity to integrate -
    // we dont want to tunnel though the colliders in the scene!
    struct IntegratePhysics : IJobParallelFor
    {
        public float DeltaTime;
        [ReadOnly]
        public NativeArray<Vector3> Velocities;
        [ReadOnly]


        
        public NativeArray<RaycastHit> Hits;
        public NativeArray<int> Sleeping;
        public NativeArray<Vector3> Positions;
        public NativeArray<float> Lifetimes;

        public void Execute(int i)
        {
            if (Sleeping[i] > 0) // if the object is sleeping, we dont have to integrate anything
            {
                Sleeping[i]++;
                return;
            }

            if (Hits[i].normal == Vector3.zero)
            {
                // if there has been no colision, intergrate all of the velocity we want for this frame
                Positions[i] += Velocities[i] * (Velocities[i] * DeltaTime).magnitude;
                Lifetimes[i] += DeltaTime;
                //Debug.Log(Lifetimes[i]);
            }
            else
            {
                // there has been a collision! just move up to the point of the collion with a tiny offset for stability
                Positions[i] = Hits[i].point + new Vector3(0, 0.1f, 0);
            }
        }
    }

    
    // Respond to collisions
    struct CalculateCollisionResponse : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<RaycastHit> Hits;

        public NativeArray<float> Lifetimes;
        public NativeArray<Vector3> Velocities;
        public NativeArray<int> Sleeping;

        public void Execute(int i)
        {
            // If there has been a collision, the RaycastHit normal will be
            // non zero. When we know there has been a collisoin we can
            // respond to it.
            //Debug.Log("==>"+Hits[i].normal);

            if (Hits[i].normal != Vector3.zero) {
                Velocities[i] = Vector3.Reflect(Velocities[i], Hits[i].normal);
                var angleBetweenNormalAndUp = Vector3.Angle(Hits[i].normal, Vector3.up); // returns a value between 0-180
                var lerp = angleBetweenNormalAndUp / 180f;
                Velocities[i] = Velocities[i] * Mathf.Lerp(0.5f, 1f, lerp);
            }else if (Lifetimes[i] > 1f) {
                    Velocities[i] = Vector3.zero;
                    Lifetimes[i] = 0;
                    Sleeping[i]++; // increment the sleeping counter. any value over 1 is sleeping. We can use this to respawn objects after they have been static for while
            }
        }
    }


    

    // Calculate and store a matrix we can use to draw each object. As we dont support rotation in this demo, we pretty
    // much only supply the position. A fixed scale is also applied to make sure the meshes we draw arnt giant.
    struct CalculateDrawMatricies : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> positions;
        public NativeArray<Matrix4x4> renderMatrices;

        public void Execute(int i)
        {
            renderMatrices[i] = Matrix4x4.TRS(positions[i], Quaternion.identity, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    struct FindSleepingObjects : IJobParallelFor
    {
        public NativeQueue<int>.Concurrent SleepQueue;
        [ReadOnly]
        public NativeArray<int> Sleeping;
            
        public void Execute(int index)
        {
            if (Sleeping[index] > 15)
            {
                SleepQueue.Enqueue(index);
            }
        }
    }

    CustomSampler sampler;
    IEnumerator Start()
    {
        // lets wait for half a second before we set off the system -- just to avoid any stutters
        yield return new WaitForSeconds(0.5f);

        // create a sampler so we can mesure performance
        sampler = CustomSampler.Create("SPGPhysics");
        
        
        // and set up all our NativeArrays with initial values
        lifetimes = new NativeArray<float>(spgRate, Allocator.Persistent);
        velocities = new NativeArray<Vector3>(spgRate, Allocator.Persistent);
        positions = new NativeArray<Vector3>(spgRate, Allocator.Persistent);
        sleepingTimer = new NativeArray<int>(spgRate, Allocator.Persistent);
        renderMatrices = new NativeArray<Matrix4x4>(spgRate, Allocator.Persistent);
        renderMatrixArray = new Matrix4x4[spgRate];
        asleep = new NativeQueue<int>(Allocator.Persistent);

        for (int i = 0; i < spgRate; i++) {
            Respawn(i);
        }
    }

    private void Respawn(int i)
    {
        // Random cannot be used from jobs so this always has to execute on the main thread.
        
        velocities[i] = Random.onUnitSphere; // spawn objects with same velocities
        lifetimes[i] = 0f;
        positions[i] = transform.position;
        //positions[i] = Random.onUnitSphere * 1f;
        sleepingTimer[i] = 0;
        renderMatrices[i] = Matrix4x4.identity;
    }

    void Update()
    {
        // only do an update if the data has been initialized
        if (!positions.IsCreated)
            return;

        sampler.Begin();


        //spgRate = Random.Range(11, 12);
        
        var deltaTime = Time.deltaTime;

        // FORCE ACCUMILATION

        // INTERGRATION AND COLLISION

        // Create temporary arrays for the raycast info. Lets use the TempJob allocator,
        // this means we have to dispose them when the job has finished - its short lived data
        var raycastCommands = new NativeArray<RaycastCommand>(spgRate, Allocator.TempJob);
        var raycastHits = new NativeArray<RaycastHit>(spgRate, Allocator.TempJob);

        // Lets schedule jobs to do a collision raycast for each object. One job Prepares all the raycast commands,
        // the second actually does the raycasts.
        var setupRaycastsJob = new PrepareRaycastCommands()
        {
            DeltaTime = deltaTime,
            Lifetimes = lifetimes,
            Positions = positions,
            Raycasts =  raycastCommands,
            Velocities = velocities
        };

        var setupDependency = setupRaycastsJob.Schedule(spgRate, 32 );
        
        var raycastDependency  = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, 32, setupDependency );

        // Now we know if there is a collision along our velocity vector, its time to integrate the velocity into
        // our objects for the current timeset.
        var integrateJob = new IntegratePhysics()
        {
            DeltaTime = deltaTime,
            Lifetimes = lifetimes,
            Positions = positions,
            Velocities = velocities,
            Sleeping = sleepingTimer,
            Hits = raycastHits
        };
        var integrateDependency  = integrateJob.Schedule(spgRate, 32, raycastDependency );

        // finally, respond to any collisions that happened in the lsat update step.
        var collisionResponeJob = new CalculateCollisionResponse()
        {
            Hits = raycastHits,
            Lifetimes = lifetimes,
            Velocities = velocities,
            Sleeping = sleepingTimer
        };
        var collisionDependency  = collisionResponeJob.Schedule(spgRate, 32, integrateDependency );

        // Now the physics is done, we need to create a drawing matrix for every object. This simple demo dosnt
        // implment roation, so only the translation values in the matrix reallllly matter.
        var renderMatrixJob = new CalculateDrawMatricies()
        {
            positions = positions,
            renderMatrices = renderMatrices
        };
        var matrixDependency = renderMatrixJob.Schedule(spgRate, 32, collisionDependency );

        // All the jobs we want to execute have been scheduled! By calling .Complete() on the last job in the
        // chain, Unity makes the main thread help out with scheduled jobs untill they are all complete. 
        // then we can move on and use the data caluclated in the jobs safely, without worry about data being changed
        // by other threads as we try to use it - we *know* all the work is done
        matrixDependency.Complete();

        // make sure we dispose of the temporary NativeArrays we used for raycasting
        raycastCommands.Dispose();
        raycastHits.Dispose();
        
        // lets schedule a job to figure out which objects are sleeping - this can run in the background whilst
        // we dispach the drawing commands for this frame.
        var sleepJob = new FindSleepingObjects()
        {
            Sleeping = sleepingTimer,
            SleepQueue = asleep
        };
        var sleepDependancy = sleepJob.Schedule(spgRate, 32, matrixDependency);

        //  lets actually issue a draw!
        renderMatrices.CopyTo(renderMatrixArray); // copy to a preallocated array, we would get garbage from ToArray()
        Graphics.DrawMeshInstanced(mesh, 0, material, renderMatrixArray);
        
        // DEBUG 1 - draw red lines showing object velocity
        for (int i = 0; i < spgRate; i++) {
            Debug.DrawLine(positions[i], positions[i] + velocities[i], Color.red, 0.016f, true);
        }

        // finally lets respawn any object that has been found to be asleep (ie not moved for 15 frames)
        // were going to respawn that object so there is a constant flow
        sleepDependancy.Complete();
        for (int i = asleep.Count; i != 0; i--)
        {
            int index = asleep.Dequeue();
            Respawn(index);
        }
        
        
        
        sampler.End();
    }

    private void OnDisable()
    {
        velocities.Dispose();
        lifetimes.Dispose();
        positions.Dispose();
        renderMatrices.Dispose();
        sleepingTimer.Dispose();
        asleep.Dispose();
    }
}
