using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class init : MonoBehaviour
{
    private bool isCoroutineExecuting = false;
    public GameObject prefab;
    public float gridX = 11f;
    public float gridY = 11f;
    public float gridZ = 11f;
    public float minSpacing = 1f;
    public float maxSpacing = 1f;

	public float simulationSpeed = 1f;

    void Start()
    {

        for (int z = 0; z < gridZ; z++)
        {
            for (int y = 0; y < gridY; y++)
            {
                for (int x = 0; x < gridX; x++)
                {
                    Vector3 pos = new Vector3(x, z, y) * Random.Range(minSpacing, maxSpacing);
                    GameObject pts = Instantiate(prefab, pos, Quaternion.identity) as GameObject;
                    pts.name = "ps-" + z + "_" + y + "_" + x;
                    pts.transform.parent = this.transform;
					ParticleSystem ps = pts.GetComponent<ParticleSystem>();
        			ps.playbackSpeed = simulationSpeed;
                }
            }
        }

        //genPts();
    }

    // Update is called once per frame
	void Update(){

	}


    // void UpdateX()
    // {
    //     if(Input.GetKey(KeyCode.R)) {
    //     	GameObject[] allPts = new GameObject[transform.childCount];
    //     	GameObject[] planets;
    //     	planets = GameObject.FindGameObjectsWithTag("mass");


    //     	foreach (GameObject planet in planets) {
    //     		Mass planetMass = planet.gameObject.GetComponent<Mass>();
    //     		Debug.Log("==>"+planetMass._density);
    //     	}

    //     	int i = 0;
    //     	foreach (Transform child in transform) {
    //     		allPts[i] = child.gameObject;
    //     		i += 1;
    //     	}



    //     	//Now destroy them
    //     	foreach (GameObject child in allPts){
    //     		//DestroyImmediate(child.gameObject);
    //     		//Debug.Log(child.gameObject.name);
    //     	}
    //     	//Debug.Log(transform.childCount);
    //     }
    // }


    void genPts()
    {
        // for (int z = 0; z < gridZ; z++) {
        // 	for (int y = 0; y < gridY; y++) {
        // 		for (int x = 0; x < gridX; x++){
        // 			Vector3 pos = new Vector3(x, z, y) * Random.Range(minSpacing, maxSpacing);
        // 			GameObject pts = Instantiate(prefab, pos, Quaternion.identity) as GameObject;

        // 			pts.name = "ps-"+z+"_"+y+"_"+x; 
        // 			pts.transform.parent = this.transform;

        // 			ParticleSystem ps = pts.GetComponent<ParticleSystem>();
        // 			ps.playbackSpeed = simulationSpeed;

        // 			var sh = ps.shape;
        // 			sh.radius = emitRadius;
        // 			sh.scale = new Vector3(particleScale, particleScale, particleScale);

        // 			var em = ps.emission;
        // 			em.rateOverTime = Random.Range(1f, 1f);

        // 			// var cm = ps.colorOverLifetime;
        // 			// cm.color = 

        // 			// Gradient grad = new Gradient();
        // 			// grad.SetKeys( new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.red, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) } );

        // 			// cm.color = grad;

        // 			ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();

        // 			psr.minParticleSize = particleSize;
        // 			psr.maxParticleSize = particleSize;
        // 		}
        // 	}
        // }
    }



}
