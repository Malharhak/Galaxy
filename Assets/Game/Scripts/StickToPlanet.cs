using UnityEngine;
using System.Collections;

public class StickToPlanet : MonoBehaviour {
	public float planetLerp = 1f;
	public float linkDistance = 10f;
	public float rotateCatchupSpeed = 1f;
	public float rotateSpeed = 0.1f;

	private Transform planet;
	private float lastPlanetChange;
	private Quaternion startQuat;
	private Vector3 startUp;

	private bool flying = false;
	private bool stickDown = true;
	private bool touchedSomething = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 dwn = transform.TransformDirection(Vector3.down);
		RaycastHit hit1;
		RaycastHit hit2;
		touchedSomething = false;
		if (stickDown && Physics.Raycast (transform.position, dwn, out hit1, linkDistance)) {
			linkToPlanet (hit1);
		}
		if (Physics.Raycast ( transform.position, transform.up, out hit2, linkDistance)) {
			if (!stickDown || hit2.distance < hit1.distance) {
				linkToPlanet (hit2);
			}
		}
		if (!touchedSomething && !flying) {
			transform.Rotate (rotateCatchupSpeed * Time.deltaTime, 0f, 0f);
		}
		touchedSomething = false;
	}
	void stopStickingDown () {
		stickDown = false;
	}
	void startStickingDown () {
		stickDown = true;
	}
	void linkToPlanet (RaycastHit planet) {
		if (planet.transform.tag == "Planet") {
			touchedSomething = true;
			Transform lastPlanet = this.planet;
			this.planet = planet.transform;
			if (this.planet != lastPlanet) {
				gameObject.SendMessage ("changePlanet", this.planet);
				lastPlanetChange = Time.time;
				startQuat = transform.rotation;
				startUp = transform.up;
			}
			flying = false;
			float frac = (Time.time - lastPlanetChange);
			transform.rotation = Quaternion.FromToRotation (transform.up, planet.normal) * transform.rotation;
			//transform.rotation = Quaternion.RotateTowards (transform.rotation, newRotation, 10f);
		}
	}

	void startFlying () {
		flying = true;
	}
	void stopFlying () {
		flying = false;
	}
}
