using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Linq;

public class Auralizer : MonoBehaviour {
	
	[System.Serializable]
	public class SoundObject {
		public AudioClip clip;
		public GameObject node;
		public float volume = 1f;
		public float directionality = 0f;
		public bool loop = false;
		public bool play = false;
		
		private bool l_playing = false;
		private Vector3 l_position = Vector3.zero;
		private Quaternion l_rotation = Quaternion.identity;
		private float l_volume = 0f;
		
		public bool GetPlaying() { return l_playing; }
		public Vector3 GetPosition() { return l_position; }
		public Quaternion GetRotation() { return l_rotation; }
		
		public void SetPlaying(bool v) { l_playing = v; }
		public void SetPosition(Vector3 v) { l_position = v; }
		public void SetRotation(Quaternion v) { l_rotation = v; }
		
		public float GetVolume() { return l_volume; }
		public void SetVolume(float v) { l_volume = v; }
	}
	
	[System.Serializable]
	public class AmbientObject {
		public AudioClip clip;
		public float volume = 1f;
		public float reverberation = 0f;
		public bool play = false;

		private bool l_playing = false;
		private float l_volume = 0f;
		private float l_reverberation = 0f;
		
		public bool GetPlaying() { return l_playing; }
		public void SetPlaying(bool v) { l_playing = v; }
		
		public float GetVolume() { return l_volume; }
		public void SetVolume(float v) { l_volume = v; }
		
		public float GetReverberation() { return l_reverberation; }
		public void SetReverberation(float v) { l_reverberation = v; }
	}
	
	public string localPath = @"/Users/worldviz/Desktop/sound-system/";
	public string networkPath = @"Y:/sound-system/";
	public string cacheFolderPath = @"sounds/unity/";
	public string speakerConfigPath = @"auralizer/speakers-link.txt";
	public int broadcastPort = 7400;
	public float updateRate = 0.1f;
	public float movementThreshold = 0.1f;
	public float rotationThreshold = 3f;
	public AmbientObject ambientSound;
	public SoundObject[] soundObjects = new SoundObject[1];
	
	private UdpClient client;
	private IPEndPoint endpoint;
	private float l_time = 0f;
	private Vector3 l_position = Vector3.zero;
	private Quaternion l_rotation = Quaternion.identity;
	
	void SendCommand(string message)
	{
		byte[] message_buf = Encoding.ASCII.GetBytes(message);
		client.Send(message_buf, message_buf.Length, endpoint);
	}

    bool FilesAreEqual(FileInfo first, FileInfo second)
    {
		const int BYTES_TO_READ = sizeof(Int64);
		
        if (first.Length != second.Length)
            return false;

        int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

        using (FileStream fs1 = first.OpenRead())
        using (FileStream fs2 = second.OpenRead())
        {
            byte[] one = new byte[BYTES_TO_READ];
            byte[] two = new byte[BYTES_TO_READ];

            for (int i = 0; i < iterations; i++)
            {
                 fs1.Read(one, 0, BYTES_TO_READ);
                 fs2.Read(two, 0, BYTES_TO_READ);

                if (BitConverter.ToInt64(one,0) != BitConverter.ToInt64(two,0))
                    return false;
            }
        }
        return true;
    }
	
	void LoadSounds()
	{
		for (int i = 0; i < soundObjects.Length; ++i) {
			SoundObject sound = soundObjects[i];
			if (sound.clip != null) {
				string clipPath = Path.Combine(Application.dataPath, @"StreamingAssets/" + sound.clip.name + ".wav"); 
				if (!File.Exists(clipPath)) {
					Debug.LogError(clipPath + " cannot be found!");
				}
				
				string cachePath = Path.Combine(networkPath, cacheFolderPath);
				string soundPath = Path.Combine(cachePath, sound.clip.name + ".wav");
				if (File.Exists(soundPath)) {
					if (!FilesAreEqual(new FileInfo(clipPath), new FileInfo(soundPath))) {
						File.Copy(clipPath, soundPath, true);
					}
				} else {
					File.Copy(clipPath, soundPath, true);
				}
				
				SendCommand("setsound " + i + " \"" + sound.clip.name + ".wav\" " + sound.volume + " " + sound.directionality);
				
				sound.SetPosition(sound.node.transform.position);
				sound.SetRotation(sound.node.transform.rotation);
				SendCommand("move " + i + " "
					+ sound.node.transform.position.x + " "
					+ sound.node.transform.position.y + " "
					+ sound.node.transform.position.z + " "
					+ sound.node.transform.rotation.w + " "
					+ sound.node.transform.rotation.x + " "
					+ sound.node.transform.rotation.y + " "
					+ sound.node.transform.rotation.z + " "
				);
			}
		}
		
		if (ambientSound.clip != null) {
			string clipPath = Path.Combine(Application.dataPath, @"StreamingAssets/" + ambientSound.clip.name + ".wav");
			if (!File.Exists(clipPath)) {
				Debug.LogError(clipPath + " cannot be found!");
			}
			
			string cachePath = Path.Combine(networkPath, cacheFolderPath);
			string soundPath = Path.Combine(cachePath, ambientSound.clip.name + ".wav");
			if (File.Exists(soundPath)) {
				if (!FilesAreEqual(new FileInfo(clipPath), new FileInfo(soundPath))) {
					File.Copy(clipPath, soundPath, true);
				}
			} else {
				File.Copy(clipPath, soundPath, true);
			}
		}
	}
	
	void Awake()
	{
		client = new UdpClient();
		endpoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
	
		if (!Directory.Exists(networkPath)) {
			Debug.LogError("Network path cannot be found! Is the Auralizer connected?");
			return;
		}
		
		if (!Directory.Exists(Path.Combine(networkPath, cacheFolderPath))) {
			Debug.LogWarning("Cache folder does not exist. Creating new folder.");
			Directory.CreateDirectory(Path.Combine(networkPath, cacheFolderPath));
		}
		
		// Enable sound system.
		SendCommand("enable 1");
		SendCommand("setpath " + Path.Combine(localPath, cacheFolderPath));
		SendCommand("speakers " + Path.Combine(localPath, speakerConfigPath));
		SendCommand("test none");
		SendCommand("stopall");
		
		LoadSounds();
		
		l_position = transform.position;
		l_rotation = transform.rotation;
		SendCommand("view "
			+ l_position.x + " " + l_position.y + " " + l_position.z + " "
			+ l_rotation.w + " " + l_rotation.x + " " + l_rotation.y + " " + l_rotation.z
		);
	}
	
	void Start() 
	{
		
	}
	
	void Update() 
	{
		
		float c_time = Time.time;
		if (c_time - l_time > updateRate) {
			l_time = c_time;
			
			if (Vector3.Distance(transform.position, l_position) > movementThreshold
				|| Quaternion.Angle (transform.rotation, l_rotation) > rotationThreshold) {
				l_position = transform.position;
				l_rotation = transform.rotation;
				SendCommand("view "
					+ l_position.x + " " + l_position.y + " " + l_position.z + " "
					+ l_rotation.w + " " + l_rotation.x + " " + l_rotation.y + " " + l_rotation.z
				);
			}
			
			if (ambientSound.clip != null) {
				if (ambientSound.play && !ambientSound.GetPlaying()) {
					ambientSound.SetPlaying(true);
					SendCommand("setambient " + " \"" + ambientSound.clip.name + ".wav\" "
						+ ambientSound.volume + " " + ambientSound.reverberation);
				}
				if (!ambientSound.play && ambientSound.GetPlaying()) {
					ambientSound.SetPlaying(false);
					SendCommand("setambient " + " \"" + ambientSound.clip.name + ".wav\" 0 0");
				}
				if (ambientSound.GetPlaying()) {
					if ((ambientSound.GetVolume() != ambientSound.volume) ||
						(ambientSound.GetReverberation() != ambientSound.reverberation)) {
						ambientSound.SetVolume(ambientSound.volume);
						ambientSound.SetReverberation(ambientSound.reverberation);
						SendCommand("setambient " + " \"" + ambientSound.clip.name + ".wav\" "
						+ ambientSound.volume + " " + ambientSound.reverberation);
					}
				}
			}
			
			for (int i = 0; i < soundObjects.Length; ++i) {
				SoundObject sound = soundObjects[i];
				if (sound.clip != null && sound.node != null) {
					if (sound.play && !sound.GetPlaying()) {
						sound.SetPlaying(true);
						SendCommand("play " + i + " " + (sound.loop ? 1 : 0));
					}
					if (!sound.play && sound.GetPlaying()) {
						sound.SetPlaying(false);
						SendCommand("stop " + i);
					}
					if (sound.play && 
						(Vector3.Distance(sound.GetPosition(), sound.node.transform.position) > movementThreshold
						|| Quaternion.Angle(sound.GetRotation(), sound.node.transform.rotation) > rotationThreshold)) {
						sound.SetPosition(sound.node.transform.position);
						sound.SetRotation(sound.node.transform.rotation);
						SendCommand("move " + i + " "
							+ sound.node.transform.position.x + " "
							+ sound.node.transform.position.y + " "
							+ sound.node.transform.position.z + " "
							+ sound.node.transform.rotation.w + " "
							+ sound.node.transform.rotation.x + " "
							+ sound.node.transform.rotation.y + " "
							+ sound.node.transform.rotation.z + " "
						);
					}
					if (sound.play && (sound.volume != sound.GetVolume())) {
						sound.SetVolume(sound.volume);
						SendCommand("volume " + i + " " + sound.volume);
					}
				}
			}
		}
	}
	
	void OnApplicationQuit()
	{
		// Disable sound system.
		SendCommand("test none");
		SendCommand("stopall");
		SendCommand("enable 0");
		client.Close();
	}
}
