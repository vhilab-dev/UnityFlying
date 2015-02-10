using UnityEngine;
using System.Collections;

public class Locomotion : MonoBehaviour {
	
	public float speed = 25f;
	public float gravity = -10f;
	
	public bool flyingEnabled = false;
	public bool jetPackEnabled = false;
	
	public GameObject hmd;
	public GameObject hand1;
	public GameObject hand2;
	public Auralizer auralizer;

	private Transform PPT;
	
	void Awake()
	{
		PPT = this.transform;
		//hmd = GameObject.Find("HMD").gameObject;
		//hand1 = GameObject.Find("Hand 1").gameObject;
		//hand2 = GameObject.Find("Hand 2").gameObject;
		//auralizer = gameObject.GetComponent<Auralizer>();
	}
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.F)) {
			flyingEnabled = !flyingEnabled;
			jetPackEnabled = false;
			PPT.rigidbody.useGravity = false;
		}

		if (Input.GetKeyDown(KeyCode.J)) {
			jetPackEnabled = !jetPackEnabled;
			flyingEnabled = false;
			PPT.rigidbody.useGravity = true;
		}

		Vector3 v1 = hand1.transform.position - hmd.transform.position + new Vector3(0, 0.25f, 0);
		Vector3 v2 = hand2.transform.position - hmd.transform.position + new Vector3(0, 0.25f, 0);
		Vector3 direction = (v1 + v2).normalized; //get midpoint and normalize
		
		float angle = Vector3.Angle(v1, v2);

		//auralizer.soundObjects[0].volume = PPT.rigidbody.velocity.magnitude / speed; //(float)((180f - angle) / 180f * 0.75);

		if (flyingEnabled) {
			Vector3 s = (180f - angle) / 180f * speed * direction + new Vector3(0, gravity, 0);
			PPT.rigidbody.velocity = s;
		} else if (jetPackEnabled){
			Vector3 f = (180f - angle) / 180f * speed * direction;
			PPT.rigidbody.AddForce(f);
		} else {
			PPT.rigidbody.velocity = new Vector3();
			//auralizer.soundObjects[0].volume = 0f;
		}


		/*
			Vector3 handDiff = hand2.transform.position - hand1.transform.position;
			float yDiff = handDiff.y;
			float xMag = hand2.transform.position.x - hand1.transform.position.x;
			float rotationAngle = Mathf.Atan(yDiff / xMag) * Mathf.Rad2Deg;
			*/
		
		/*
			float terrainHeight = Terrain.activeTerrain.SampleHeight(PPT.position) 
				+ Terrain.activeTerrain.transform.position.y;
			
			if (direction.y > 0.25) {
				grounded = false;
			} else if (Mathf.Abs(PPT.position.y - terrainHeight) < 1.5) {
				grounded = true;
			}
			
			if (!grounded) {
				// Vector3 velocity = (180f - angle) / 180f * speed * direction;
				Vector3 velocity = (180f - angle) / 180f * speed * direction + new Vector3(0, gravity * Time.deltaTime, 0);
				Vector3 destination = PPT.position + velocity * Time.deltaTime;
				
				PPT.position = new Vector3(destination.x, destination.y, destination.z);
				//auralizer.soundObjects[0].volume = (float)((180f - angle) / 180f * 0.75);
				
			} else {
				//auralizer.soundObjects[0].volume = 0f;
			}
			*/
	}
}	