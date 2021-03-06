﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CopterManagerTouch : MonoBehaviour {

	// Private stuff
	private Rigidbody2D copterBody;
	private DistanceJoint2D hookJoint;
	private bool once = false;
	private float 	currentAngle = 0f,  
					copterAngle = 0f, 
					copterScale = 0f, 
					persistence = 1f,
					tempHoldTime = 0f,
					currentPower,
					currentHealth,
					minDamage = -5f,
					currentFuel,
					idleConsumption = 1f;

	private GameObject hook;
	private LevelManager manager;
	private CargoManager cargo;
	private GameManager gameManager;
	private int 	rotationID1 = 255, 
					rotationID2 = 255;

	// Public values
	public GameObject 	hookPrefab, 
						hookAnchor, 
						brokenCopter, 
						explosion, 
						splash;

	public bool 	isHookDead = false,
					isHookDown = false, 
					isKill = false, 
					isSplash = false;

	public float 	maxTilt = 75f, 
					tiltSpeed = 50f, 
					returnSpeed = 5f,
					holdTime = 0.25f,
					rotationSensitivity = 0.5f,
					powerSensitivity = 0.5f,
					powerMultiplier = 100f,
					initialPower = 75f,
					hookDistance = 1.5f,
					reelSpeed = 1.5f,
					maxHealth = 100f,
					healPerSecond = 20f,
					maxFuel = 500f,
					reFuelPerSecond = 100f,
					minPower = 0f,
					maxPower = 120f;

	public void resetPower() {
		currentPower = initialPower;
	}
	
	// Use this for initialization
	void Start () {
		gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
		manager = GameObject.Find("LevelManagerO").GetComponent<LevelManager>();
		gameManager.load();
		copterBody = gameObject.GetComponent<Rigidbody2D>();
		tempHoldTime = holdTime;

		setupCopter(gameManager.getCopters(), gameManager.getCurrentCopter());
	}
	
	public void setupCopter (string[,] copterArray, int copterNumber){
		for (int h = 1; h < 10; h++) {
			Debug.Log (int.Parse(copterArray[copterNumber,h]));
		}
		Debug.Log(gameManager.getPlatformLevel());
		maxHealth = int.Parse(copterArray[copterNumber,15]);
		copterBody.mass = int.Parse(copterArray[copterNumber,10]);
		tiltSpeed = int.Parse(copterArray[copterNumber,16]);
		returnSpeed = tiltSpeed/40;
		maxTilt = int.Parse(copterArray[copterNumber,17]);
		maxFuel = int.Parse(copterArray[copterNumber,8]) + (((int.Parse(copterArray[copterNumber,9]) - int.Parse(copterArray[copterNumber,8])) / 10) * int.Parse(copterArray[copterNumber,5]));
		maxPower = int.Parse(copterArray[copterNumber,6]) + (((int.Parse(copterArray[copterNumber,7]) - int.Parse(copterArray[copterNumber,6])) / 10) * int.Parse(copterArray[copterNumber,4]));
		reFuelPerSecond = maxFuel / ((14 - gameManager.getPlatformLevel()) / 2);
		healPerSecond = maxHealth / ((14 - gameManager.getPlatformLevel()) / 2);
		manager.cargoSize = int.Parse(copterArray[copterNumber,11]);
		currentHealth = maxHealth;
		currentFuel = maxFuel;
		cargo = GetComponent<CargoManager>();
		hookJoint = GetComponent<DistanceJoint2D> ();
		hookJoint.anchor = hookAnchor.transform.localPosition;
		copterScale = gameObject.transform.localScale.x;
		resetPower();
	}

	//Update is called before void Update();
	void FixedUpdate () {
		copterAngle = gameObject.transform.eulerAngles.z;
	}

	// Update is called once per frame
	void Update () {

		// START INPUT ----------------------------------------------------------------------------------

		// Control system
		if (Input.touchCount > 0) {
			foreach (Touch touch in Input.touches){

				//Copter press and Joystick initialization since both cannot happen during the same frame
				if(touch.phase == TouchPhase.Began && 
				   gameObject.GetComponent<Collider2D>() == Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(touch.position)) && !isHookDead){
					if (isHookDown){
						isHookDown = false;
					} else {
						isHookDown = true;
					}
				}

				//Joystick initialization
				if (touch.phase == TouchPhase.Began) {
					currentAngle = copterAngle;
					if (rotationID1 == 255) {
						rotationID1 = touch.fingerId;
					} else if (rotationID2 == 255) {
						rotationID2 = touch.fingerId;
					}
				}

				//Joystick moved (JOYSTICK CONTROLS ROTATION AND POWER MANAGEMENT 
				if ((touch.fingerId == rotationID1 || touch.fingerId == rotationID2) && touch.phase == TouchPhase.Moved) {
					//Rotation
					if ((currentAngle < maxTilt || touch.deltaPosition.x < 0f) || 
					    (currentAngle > 360f-maxTilt || touch.deltaPosition.x > 0f)) currentAngle -= touch.deltaPosition.x * rotationSensitivity;

					if (currentAngle < 0f) currentAngle += 360f;
					else if (currentAngle > 360f) currentAngle -= 360f;
					
					if (currentAngle > maxTilt && currentAngle < 180f) currentAngle = maxTilt;
					else if (currentAngle < 360f - maxTilt && currentAngle > 180f) currentAngle = 360f - maxTilt;

					//Power Management
					if (currentPower <= maxPower && currentPower >= minPower && currentFuel > 0f) {
						if (touch.deltaTime != 0f) currentPower += (((touch.deltaPosition.y/Screen.height)*(maxPower-minPower))*powerSensitivity) * (Time.deltaTime / touch.deltaTime);
						else currentPower += (((touch.deltaPosition.y/Screen.height)*(maxPower-minPower))*powerSensitivity);
					}
				}

				//Control destruction
				if (touch.phase == TouchPhase.Ended) {
					if (touch.fingerId == rotationID1) {
						rotationID1 = 255;
					} else if (touch.fingerId == rotationID2) {
						rotationID2 = 255;
					}
					if (rotationID1 == 255 && rotationID2 == 255) {
						currentAngle = 0f;
						tempHoldTime = 0f;
					}
				}
			}

		}

		// END INPUT ------------------------------------------------------------------------------------

		// Helicopter angle management

		if (copterAngle != 0f && rotationID1 == 255) { // Return to 0°
			if (copterAngle > 180f) {
				gameObject.transform.Rotate (new Vector3 (0f, 0f, returnSpeed * Time.deltaTime * (360f - copterAngle) * persistence));
			} else if (copterAngle < 180f) {
				gameObject.transform.Rotate (new Vector3 (0f, 0f, -(returnSpeed * Time.deltaTime) * copterAngle * persistence));
			}
		} else if (copterAngle != currentAngle && rotationID1 != 255) { // Turn to currentAngle
			if (currentAngle < 180f) {
				if (copterAngle > 180f) {
					// Rotate CCW
					gameObject.transform.Rotate(new Vector3(0f, 0f, tiltSpeed * Time.deltaTime));
				} else if (copterAngle < 180f) {
					if (copterAngle < currentAngle) {
						// Rotate CCW
						gameObject.transform.Rotate (new Vector3 (0f, 0f, tiltSpeed * Time.deltaTime));
					} else if (copterAngle > currentAngle) {
						// Rotate CW
						gameObject.transform.Rotate(new Vector3(0f, 0f, -tiltSpeed * Time.deltaTime));
					}
				}
			} else if (currentAngle > 180f) {
				if (copterAngle < 180f) {
					// Rotate CW
					gameObject.transform.Rotate (new Vector3 (0f, 0f, -tiltSpeed * Time.deltaTime));
				} else if (copterAngle > 180f) {
					if (copterAngle < currentAngle) {
						// Rotate CCW
						gameObject.transform.Rotate (new Vector3 (0f, 0f, tiltSpeed * Time.deltaTime));
					} else if (copterAngle > currentAngle) {
						// Rotate CW
						gameObject.transform.Rotate(new Vector3(0f, 0f, -tiltSpeed * Time.deltaTime));
					}
				}
			}
		}

		// for limits
		if (currentPower < minPower) currentPower = minPower;
		if (currentPower > maxPower) currentPower = maxPower;
		
		if (minPower > maxPower) minPower = maxPower;
		if (maxPower < minPower) maxPower = minPower;

		//Copter direction mangement
		if (currentAngle > 180f && rotationID1 != 255) gameObject.transform.localScale = new Vector3(copterScale, gameObject.transform.localScale.y); 
		else if (currentAngle < 180f && rotationID1 != 255) gameObject.transform.localScale = new Vector3(-copterScale, gameObject.transform.localScale.y);

		if (currentFuel > 0f) changeFuel(-((idleConsumption + (currentPower/10)) * Time.deltaTime));
		else if (currentFuel <= 0f) {
			currentFuel = 0f;
			if (currentPower > 0f) currentPower -= currentPower * (currentPower/maxPower) * Time.deltaTime * 3f;
		}

		copterBody.AddForce (gameObject.transform.up * (currentPower*powerMultiplier) * Time.deltaTime);

		if (isHookDown && hook == null && !isHookDead) {
			once = true;
			hook = Instantiate (hookPrefab, gameObject.transform.position + new Vector3 (0f, -0.3f), Quaternion.identity) as GameObject;
			hookJoint.enabled = true;
			hookJoint.distance = hookDistance;
			hookJoint.connectedBody = hook.GetComponent<Rigidbody2D>();
		} else if (!isHookDown && once && Vector2.Distance (hook.transform.position, hookAnchor.transform.position) < 0.1 && !isHookDead) {
			cargo.cargoHookedCrates (hook);
			if (cargo.getCargoCrates() >= manager.cargoSize && hook.transform.childCount > 0){
				isHookDown = true;
				Debug.Log("Cargo full");
			} else if (hook.transform.childCount == 0){
				Destroy (hook);
				once = false;
			}
		} else if (!isHookDown && hook != null && !isHookDead) {
			hookJoint.distance -= reelSpeed * Time.deltaTime;
		} else if (isHookDown && hookJoint.distance != hookDistance) {
			hookJoint.distance = hookDistance;
		}
		if (!isHookDead && isHookDown && Vector2.Distance (hook.transform.position, hookAnchor.transform.position) > hookDistance*2) {
			killHook();
		} 

		if (tempHoldTime != holdTime) {
			tempHoldTime += Time.deltaTime;
			persistence = tempHoldTime/holdTime;
		}
		if (tempHoldTime > holdTime) {
			persistence = 1f;
			tempHoldTime = holdTime;
		}

		if ((gameObject.transform.position.y) < manager.getWaterLevel()){
			manager.levelFailed(2);
		}

		if (isKill) kill();
		if (isSplash) splashKill();
	}

	// Positive values to GAIN health and negative to LOSE health
	public void changeHealth (float amount) {
		if (amount < minDamage){
			currentHealth += amount;
			if (currentHealth <= 0) manager.levelFailed(1);
		} else if (amount > 0f) {
			if (currentHealth < maxHealth) currentHealth += amount;
			if (currentHealth > maxHealth) currentHealth = maxHealth;
		}
	}

	public float getHealth (){
		return currentHealth;
	}

	public float getFuel () {
		return currentFuel;
	}

	public float getReFuelSpeed () {
		return reFuelPerSecond;
	}

	public float getPower () {
		return currentPower;
	}

	public void changeFuel (float amount){
		currentFuel += amount;
	}

	private void killHook () {
		hookJoint.enabled = false;
		hook.GetComponent<DestroyHookOverTime>().doom();
		hook = null;
		isHookDead = true;
	}

	private void kill() {
		if (hook != null)
			killHook ();
		GameObject newCopter = (GameObject) Instantiate(brokenCopter, transform.position, Quaternion.identity);
		Instantiate(explosion, transform.position, Quaternion.identity);
		Rigidbody2D[] parts = newCopter.GetComponentsInChildren<Rigidbody2D>();
		foreach(Rigidbody2D part in parts){
			part.velocity += gameObject.GetComponent<Rigidbody2D>().velocity;
			part.angularVelocity += gameObject.GetComponent<Rigidbody2D>().angularVelocity;
		}
		newCopter.GetComponent<ExplodeParts>().enabled = true;
		currentHealth = 0f;
		Destroy (gameObject);
	}

	private void splashKill() {
		if (hook != null)
			killHook ();
		GameObject newCopter = (GameObject) Instantiate(brokenCopter, transform.position, Quaternion.identity);
		Vector3 splashPos = new Vector3(transform.position.x, manager.getWaterLevel()+0.5f);
		Instantiate(splash, splashPos, Quaternion.identity);
		Rigidbody2D[] parts = newCopter.GetComponentsInChildren<Rigidbody2D>();
		foreach(Rigidbody2D part in parts){
			part.velocity += gameObject.GetComponent<Rigidbody2D>().velocity;
			part.angularVelocity += gameObject.GetComponent<Rigidbody2D>().angularVelocity;
		}
		newCopter.GetComponent<ExplodeParts>().enabled = false;
		currentHealth = 0f;
		Destroy (gameObject);
	}
}
