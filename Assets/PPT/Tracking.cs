using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Tracking : MonoBehaviour {
	
	[System.Serializable]
	public class Tracker {
		public int virtualID = 0;
		public GameObject node;
		public Vector3 scale = new Vector3(1f, 1f, 1f);
	}
	
	public string trackingIP = "PPT0@171.64.33.43";
	public Tracker[] trackers = new Tracker[1];
	
	//public int intersenseSerialPort = 1;
	//public GameObject intersenseNode;
	public bool enableQuitESC = true;
	//public bool enableRotationResetR = false;
	public string wandIP = "PPT_WAND3@171.64.33.43:8945";
	public int[] wandButtons = new int[6];
	public float[] wandJoystick = new float[2];
		
	[DllImport ("Tracking")]
	private static extern void InitializePPT(string ppt_address, int max_trackers);
	[DllImport ("Tracking")]
	private static extern void TerminatePPT();
	[DllImport ("Tracking")]
	private static extern void UpdatePPT();
	[DllImport ("Tracking")]
	private static extern void FetchPPTPosition([In, Out] double[] out_position, int tracker_id);
	
	//[DllImport ("Tracking")]
	//private static extern void InitializeInterSense(int comm_port);
	//[DllImport ("Tracking")]
	//private static extern void TerminateInterSense();
	//[DllImport ("Tracking")]
	//private static extern void UpdateInterSense();
	//[DllImport ("Tracking")]
	//private static extern void FetchInterSenseQuaternion([In, Out] double[] out_quaternion);
	
	[DllImport ("Tracking")]
	private static extern void InitializeWand(string wand_address);
	[DllImport ("Tracking")]
	private static extern void TerminateWand();
	[DllImport ("Tracking")]
	private static extern void UpdateWand();
	[DllImport ("Tracking")]
	private static extern void FetchWandButtonStates([In, Out] int[] out_buttonstates);
	[DllImport ("Tracking")]
	private static extern void FetchWandAnalogData([In, Out] double[] out_analogdata);
	
	private double[] analog_data = new double[2];
	private double[] position = new double[3];
	private double[] quaternion = new double[4];
	private Quaternion quaternion_calibration = Quaternion.identity;
	
	void Awake () 
	{
		InitializePPT(trackingIP, trackers.Length + 8);
		//InitializeInterSense(intersenseSerialPort);
		InitializeWand(wandIP);
	}
	
	void Start()
	{
		
	}
	
	void Update()
	{
		UpdatePPT();
		for (int i = 0; i < trackers.Length; ++i) {
			if (trackers[i].node != null && trackers[i].virtualID > 0) {
				FetchPPTPosition(position, trackers[i].virtualID - 1);
				trackers[i].node.transform.localPosition = new Vector3(
					(float)(position[0] * trackers[i].scale.x), 
					(float)(position[1] * trackers[i].scale.y), 
					(float)(position[2] * trackers[i].scale.z)
				);
			}
		}
		
		//if (intersenseNode != null) {
		//	UpdateInterSense();
		//	FetchInterSenseQuaternion(quaternion);
			
		//	intersenseNode.transform.localRotation = Quaternion.Inverse(quaternion_calibration) * new Quaternion(
		//		(float)quaternion[1], 
		//		-(float)quaternion[2], 
		//		(float)quaternion[0], 
		//		-(float)quaternion[3]
		//	);
			
			// If "r" is pressed, calibrate/reset the head rotation
		//	if (enableRotationResetR && Input.GetKeyDown(KeyCode.R)) {
		//        quaternion_calibration = new Quaternion(
		//			(float)quaternion[1], 
		//			-(float)quaternion[2], 
		//			(float)quaternion[0], 
		//			-(float)quaternion[3]
		//		);
	    //	}
		//}
		
		UpdateWand();
		FetchWandButtonStates(wandButtons);
		
		FetchWandAnalogData(analog_data);
		wandJoystick[0] = (float)analog_data[0];
		wandJoystick[1] = -(float)analog_data[1];
		
		// If escape is pressed, quit
		if (enableQuitESC && Input.GetKeyDown(KeyCode.Escape)) {
        	Application.Quit();
    	}
	}
	
	void OnApplicationQuit()
	{
		TerminatePPT();
		//TerminateInterSense();
		TerminateWand();
	}
}
