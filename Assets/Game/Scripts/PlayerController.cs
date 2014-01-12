using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	public Transform cameraBox;

	public float mass = 0.3f;
	public float moveAcceleration = 0.3f;
	public float maxMoveSpeed = 2.0f;
	public float slowdownTime = 0.3f;

	public float jumpForce = 20;
	public float jumpReact = 5;

	public float jumpTimeMargin = 0.2f;

	public int jumpAllowed = 2;

	public float elevatorSpeed = 50f;

	private Vector3 acceleration = new Vector3(0f, 0f, 0f);
	private Vector3 speed = new Vector3 (0f, 0f, 0f);
	private float moveSpeed = 0f;
	private Vector3 lastPosition;
	private Vector3 lastSpeed;
	private Vector3 lastAcceleration;

	private bool floored = false;
	private int jumpCounter = 0;
	private float lastInput;
	private float lastAngle;
	private float stickAngle;
	private float lastJumpAsk;

	private bool elevating = false;
	private bool canElevate = false;
	private Vector3 elevateDirection;

	private PlanetInformations planetInfos;

	//debug
	private Vector3 lastDiff;
	// Use this for initialization
	void Start () {
		lastInput = Time.time;
		lastDiff = new Vector3(0, 0, 0);
		lastJumpAsk = Time.time;
	}
	
	// Update is called once per frame

	void processGravity () {
		if (!floored && !elevating) {
			acceleration += new Vector3(0f, -mass * ((planetInfos != null) ? planetInfos.gravity : 1), 0f);
		}

		speed += Time.deltaTime * acceleration;

		Vector3 deltaSpeed = speed * Time.deltaTime;
		transform.position += transform.right * deltaSpeed.x;
		transform.position += transform.up * deltaSpeed.y;
		transform.position += transform.forward * deltaSpeed.z;
	}

	void floor () {
		floored = true;
		jumpCounter = 0;
		elevating = false;
		SendMessage("startStickingDown");
		if (Time.time - lastJumpAsk < jumpTimeMargin) {
			jump ();
		}

	}

	void jump () {
		if (floored || jumpCounter < jumpAllowed) {
			acceleration.y += mass * jumpForce;
			speed.y += jumpReact;
			jumpCounter++;
			floored = false;
		} else {
			lastJumpAsk = Time.time;
		}
	}

	void processMove () {
		float zAxis = -Input.GetAxis ("Vertical");
		float xAxis = Input.GetAxis ("Horizontal");

		moveSpeed += (Mathf.Abs (xAxis) + Mathf.Abs (zAxis) ) * moveAcceleration * Time.deltaTime;
		if (moveSpeed > maxMoveSpeed) moveSpeed = maxMoveSpeed;
		                              
		if (Mathf.Abs (xAxis) > 0.2 || Mathf.Abs (zAxis) > 0.2) {
			lastInput = Time.time;
			Transform mainCamera = Camera.main.transform;

			Vector3 diff = (mainCamera.up * zAxis + mainCamera.right * xAxis);
			diff.Normalize();
			diff = transform.InverseTransformDirection(diff);
			diff.y = 0;
			diff = transform.TransformDirection(diff);
			transform.LookAt(transform.position + diff, transform.up);
			lastDiff = diff;
		}
		Debug.DrawRay (transform.position, lastDiff * 5);
		moveSpeed = Mathf.Lerp(moveSpeed, 0, (Time.time - lastInput) / slowdownTime);
		if (!elevating) {
			transform.Translate (new Vector3(0, 0, moveSpeed) * Time.deltaTime, Space.Self);
		}

	}

	void processElevators () {
		if (canElevate && !elevating && Input.GetButtonDown ("Fire3")) {
			elevate (elevateDirection);
		}
	}
	void Update () {
		lastPosition = transform.position;
		lastAcceleration = acceleration;
		lastSpeed = speed;

		processGravity();
		processMove();
		processElevators();

		if (Input.GetButtonDown ("Fire1")) {
			jump ();
		}

	}

	void OnTriggerEnter (Collider other) {
		if (other.tag == "Planet") {
			acceleration = Vector3.zero;
			speed = Vector3.zero;
			floor ();
			planetInfos = other.GetComponent<PlanetInformations>();
		} else if (other.tag == "Elevator") {
			canElevate = true;
			elevateDirection = other.transform.forward;

		}


	}

	void OnTriggerExit (Collider other) {
		if (other.tag == "Elevator") {
			canElevate = false;
		}
	}

	void elevate (Vector3 direction) {
		print ("Touched an elevator");
		elevating = true;
		canElevate = false;
		acceleration.y += elevatorSpeed;
		transform.rotation = Quaternion.FromToRotation (transform.up, direction) * transform.rotation;
		floored = false;
		SendMessage("stopStickingDown");

	}

	void OnGUI () {
		GUI.Label (new Rect (20, 20, 200, 20), "Accel: " + acceleration.ToString() );
		GUI.Label (new Rect (20, 40, 200, 20), "Speed: " + speed.ToString() );

	}
	void OnDrawGiwmosSelected () {

	}

	void changePlanet (Transform newPlanet) {
		print ("Changed planet");
		elevating = false;
		acceleration = Vector3.zero;
		speed = Vector3.zero;
		processGravity ();
	}

	
}
