using UnityEngine;
using System.Collections;

public class Locomotion2 : MonoBehaviour {
	
	public float speed = 25f;
	public float gravity = -10f;
	
	public bool flyingEnabled = false;
	private bool grounded = true;
	
	private GameObject hmd;
	private GameObject hand1;
	private GameObject hand2;
	//private Auralizer auralizer;
	
	void Awake()
	{
		hmd = GameObject.Find("HMD").gameObject;
		hand1 = GameObject.Find("Hand 1").gameObject;
		hand2 = GameObject.Find("Hand 2").gameObject;
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
		}
		
		if (flyingEnabled) {
			Vector3 v1 = hand1.transform.position - hmd.transform.position + new Vector3(0, 0.25f, 0);
			Vector3 v2 = hand2.transform.position - hmd.transform.position + new Vector3(0, 0.25f, 0);
			Vector3 direction = Vector3.Normalize((v1 + v2) / 2.0f);

			Debug.Log (direction);
			float angle = Vector3.Angle(v1, v2);

			Vector3 handDiff = hand2.transform.position - hand1.transform.position;
			float yDiff = handDiff.y;
			float xMag = hand2.transform.position.x - hand1.transform.position.x;
			float rotationAngle = Mathf.Atan(yDiff / xMag) * Mathf.Rad2Deg;
			
			float terrainHeight = Terrain.activeTerrain.SampleHeight(transform.position) 
       					           + Terrain.activeTerrain.transform.position.y;
			
			if (direction.y > 0.25) {
				grounded = false;
			} else if (Mathf.Abs(transform.position.y - terrainHeight) < 1.5) {
				grounded = true;
			}
			
			if (!grounded) {
				// Vector3 velocity = (180f - angle) / 180f * speed * direction;
				Vector3 velocity = (180f - angle) / 180f * speed * direction + new Vector3(0, gravity * Time.deltaTime, 0);
				Vector3 destination = transform.position + velocity * Time.deltaTime;
				
				transform.position = new Vector3(destination.x, destination.y, destination.z);
				//auralizer.soundObjects[0].volume = (float)((180f - angle) / 180f * 0.75);

				transform.Rotate(direction, Mathf.Sign(yDiff) * rotationAngle);
			} else {
				//auralizer.soundObjects[0].volume = 0f;
			}
		} else {
			//auralizer.soundObjects[0].volume = 0f;
		}
	}
}
