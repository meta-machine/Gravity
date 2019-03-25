using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Mass : MonoBehaviour
{


    public float density = 2f;

    private int refresh = 1;

    private int freeze = 1;
    //	List<GameObject> closeSparticles = new List<GameObject>();
    List<spaceParticle> closeSparticles = new List<spaceParticle>();



	GameObject[] sparticles;
	ParticleSystem ps;
	TextMeshPro mText;

	void Start() {
		
    }
    void Update() {
       // RefreshSparticles();
        if (Input.GetKey(KeyCode.R)) {
            if (refresh == 1) {
                refresh = 0;
                //Invoke("nearBySparticles", 0.1f); 
                //Invoke("refreshPosition", 0.5f); 
                //Invoke("xFreezeMass", 0.5f);
            }
        }
    }


    void OnParticleCollision(GameObject spt)
    {
		RefreshSparticles();
        //Debug.Log("coll : " + spt.name);
    }


	void RefreshSparticles()
    {
		Vector3 offset;
		float sqrLen;
		float gval;
		float dist;
		
        Debug.Log("==>"+this.name);

		
		
		sparticles = GameObject.FindGameObjectsWithTag("sparticle");
		Collider mCollider = this.GetComponent<Collider>();

		foreach (GameObject sparticle in sparticles) {
            offset = transform.position - sparticle.transform.position;
			sqrLen = offset.sqrMagnitude;
			dist = Vector3.Distance(sparticle.transform.position, transform.position);

			gval = (this.density) / sqrLen;
			
			bool overlapping = mCollider.bounds.Contains(sparticle.transform.position);

			if(overlapping){
                gval = 0f;
            }

			ps = sparticle.GetComponent<ParticleSystem>();
			var em = ps.emission;
			float totalGval =  (gval);
			em.rateOverTime = totalGval; 

			mText = sparticle.transform.Find("sTxt").GetComponent<TextMeshPro>();
			mText.text = "" + gval.ToString();
        }
    }

	

	
	
	// // xFreezeMass is useless now
    // void xFreezeMass() {
    //     Rigidbody rb = this.GetComponent<Rigidbody>();
    //     if (freeze == 1) {
    //         freeze = 0;
    //         rb.constraints = RigidbodyConstraints.FreezeAll;
    //     } else if (freeze == 0) {
    //         freeze = 1;
    //         rb.constraints = RigidbodyConstraints.None;
    //         rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    //     }
    //     refresh = 1;
    // }

    // // nearBySparticles is useless now
	// void nearBySparticles() {
    //     //Debug.Log("===>" + this.name);
    //     GameObject[] sparticles = GameObject.FindGameObjectsWithTag("sparticle");
    //     GameObject spacebox = GameObject.Find("spacebox");
    //     init initSpace = spacebox.gameObject.GetComponent<init>();

    //     foreach (GameObject sparticle in sparticles) {
    //         float dist = Vector3.Distance(sparticle.transform.position, transform.position);
    //         if (dist <= (initSpace.minSpacing * 2)){
    //             spaceParticle sParti = sparticle.gameObject.GetComponent<spaceParticle>();
    //             closeSparticles.Add(sParti);
    //             //spaceParticle sParti = sparticle.gameObject.GetComponent<spaceParticle>();
    //         }
    //     }
    // }

    // // SortByEmitRate is useless now
	// static int SortByEmitRate(spaceParticle sp1, spaceParticle sp2)
    // {
    //     return sp1.emitRate.CompareTo(sp2.emitRate);
    // }

	// // refreshPosition is useless now
    // void refreshPosition() {
    //     closeSparticles.Sort(SortByEmitRate);
    //     closeSparticles.Reverse();
    //     //closeSparticles.Sort((p1,p2)=>p1.emitRate.CompareTo(p2.emitRate));
    //     int top3 = 3;
    //     float prevEmit = 0;

    //     List<Vector3> lstPos = new List<Vector3>();
    //     foreach (spaceParticle closeSp in closeSparticles) {
    //         if (closeSp.emitRate != Mathf.Infinity) {
    //             if (top3 > 0 && (prevEmit != closeSp.emitRate)) {
    //                 //Debug.Log(this.name + "=>" + closeSp.name + "=>" + closeSp.emitRate);
    //                 lstPos.Add(closeSp.transform.position);
    //                 top3--;
    //             }
    //             prevEmit = closeSp.emitRate;
    //         }
    //     }

    //     // float totalX = 0f;
    //     // float totalY = 0f;
    //     // float totalZ = 0f;

    //     // foreach(Vector3 vPos in lstPos) {
    //     // 	totalX += vPos.x;
    //     // 	totalY += vPos.y;
    //     // 	totalZ += vPos.z;
    //     // }

    //     // var centerX = totalX / lstPos.Count;
    //     // var centerY = totalY / lstPos.Count;
    //     // var centerZ = totalZ / lstPos.Count;

    //     //Vector3 nextPos = new Vector3(centerX, centerY, centerZ);
    //     //Vector3 nextPos = GetNextPos(lstPos[0],lstPos[1],lstPos[2]);

	// 	Vector3 nextPos = lstPos[0];
    //     this.transform.position = nextPos;
    //     // Debug.Log(lstPos[0]+" - "+lstPos[1]+" - "+lstPos[2]+"===>" + midAngle);
    //     // Debug.DrawLine(lstPos[0], lstPos[1], Color.red);
    //     // Debug.DrawLine(lstPos[1], lstPos[2], Color.red);
    //     // Debug.DrawLine(lstPos[2], lstPos[0], Color.red);
    //     refresh = 1;
    // }

	// // GetNormal is useless now
    // Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c) {
    //     // Find vectors corresponding to two of the sides of the triangle.
    //     Vector3 side1 = b - a;
    //     Vector3 side2 = c - a;

    //     // Cross the vectors to get a perpendicular vector, then normalize it.
    //     return Vector3.Cross(side1, side2).normalized;
    // }

	// // GetNextPos is useless now
    // Vector3 GetNextPos(Vector3 pos1, Vector3 pos2, Vector3 pos3) {
    //     Vector3 mPos = GetNormal(pos1, pos2, pos3);
    // 	Debug.Log("==>"+mPos);
    //     // Cross the vectors to get a perpendicular vector, then normalize it.
    //     return pos1;
    // }

	// // getMidAngle is useless now
    // float getMidAngle(Vector3 v1, Vector3 v2, Vector3 v3) {
    // 	 return Mathf.Atan2(
    //         Vector3.Dot(v3, Vector3.Cross(v1, v2)),
    //         Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    // }


}
