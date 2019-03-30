using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class spaceParticle : MonoBehaviour
{

    // Use this for initialization

    public float emitRadius = 1f;
    public float emitRate = 1f;
    public float particleSize = 1f;

    public float particleScale = 1f;

    private int refresh = 1;
    List<ParticleCollisionEvent> collisionEvents;

    ParticleSystem ps;

    TextMeshPro mText;
    void Start()
    {

        collisionEvents = new List<ParticleCollisionEvent>();
        ps = this.GetComponent<ParticleSystem>();
        //ps.playbackSpeed = simulationSpeed;

        var sh = ps.shape;
        sh.radius = emitRadius;
        //sh.scale = new Vector3(particleScale, particleScale, particleScale);

        var em = ps.emission;
        em.rateOverTime = emitRate;

        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.minParticleSize = particleSize;
        psr.maxParticleSize = particleSize;

    }

    // Update is called once per frame
    void Update() {
        
        if (Input.GetKey(KeyCode.R)) {
            refresh = 0;
            refreshState();
            //resetState();
            
        }
        // if (refresh == 1) {
        //     if (Input.GetKey(KeyCode.R)) {
        //         refresh = 0;
        //         Invoke("refreshState", 1f);
        //     }
        //     if (Input.GetKey(KeyCode.Z)) {
        //         Invoke("resetState", 0.1f);
        //     }
        // }
        // if (Input.GetKey(KeyCode.T)) {
        //     Invoke("showVal", 0.1f);
        // }
    }






    void refreshState() {
        GameObject[] masses = GameObject.FindGameObjectsWithTag("mass");
        ps = this.GetComponent<ParticleSystem>();
        var em = ps.emission;
        em.rateOverTime = 1;

        foreach (GameObject mass in masses) {

            float dist = Vector3.Distance(mass.transform.position, transform.position);
            Vector3 offset = mass.transform.position - transform.position;
            Mass cMass = mass.gameObject.GetComponent<Mass>();
            
            //offset = offset / cMass.density;
            float sqrLen = offset.magnitude;
        
            //float gval = cMass.density  / sqrLen;
			//float gval = (cMass.density/(sqrLen * em.rate.constant));
            float gval = (cMass.density) / sqrLen;

            //float totalGval = em.rate.constant + (gval-em.rate.constant);
            float totalGval = em.rate.constant + gval;
            mText = this.transform.Find("sTxt").GetComponent<TextMeshPro>();
            mText.text = totalGval.ToString();
            em.rateOverTime = totalGval; //old calc
            this.emitRate = totalGval;
            refresh = 1;
        }


    }




    void showVal() {
        if (this.transform.Find("sTxt").gameObject.active) {
            this.transform.Find("sTxt").gameObject.SetActive(false);
        }
        else {
            this.transform.Find("sTxt").gameObject.SetActive(true);
        }
		refresh = 1;
    }


    void resetState() {
        ps = this.GetComponent<ParticleSystem>();
        var em = ps.emission;
        em.rateOverTime = 1;
        mText.text = "1";
    }


    // void OnParticleCollision(GameObject other)
    // {
    //     int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);

    //     Rigidbody rb = other.GetComponent<Rigidbody>();
    //     int i = 0;
    // 	Debug.Log("===>"+other.name);
    //     while (i < numCollisionEvents)
    //     {
    //         if (rb)
    //         {
    //             Vector3 pos = collisionEvents[i].intersection;
    //             Vector3 force = collisionEvents[i].velocity * 10;
    //             rb.AddForce(force);
    //         }
    //         i++;
    //     }
    // }

    // void OnCollisionEnter (Collision col){
    // 	Debug.Log("==>z");
    // 	if(col.gameObject.name == "planet")
    // 	{
    // 		Debug.Log("==>");
    // 		//Destroy(col.gameObject);
    // 	}
    // }
}
