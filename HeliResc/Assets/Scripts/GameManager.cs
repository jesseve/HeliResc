﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {

	void Awake(){
		DontDestroyOnLoad(gameObject);
	}

	private const int copterAmount = 2; //FOR EVERY NEW COPTER, ADD 1 HERE

	private string 	playerName = "Anonymous";
	private int		playerFirst = 0,
					playerStars = 0, 
					playerCoins = 0, 
					playerPlatform = 1,
					currentCopter = 0;

	/*
	 * An array of Copter statistics (IN STRING!!!)
	 * First number goes as the CopterNumber (0 being DEFAULT)
	 * I couldn't make it visible to the inspector, so we'll have to work with this file
	 * Second number used as follows:
	 * 
	 * 0. CopterName
	 * 1. CopterCost
	 * 2. CopterPlatform
	 * 3. CopterUnlocked 		-> Save
	 * 4. CopterEngineLevel 	-> Save
	 * 5. CopterFuelTankLevel 	-> Save
	 * 6. CopterEngineDefaultPower
	 * 7. CopterEnginePowerMax
	 * 8. CopterFuelTankDefaultValue
	 * 9. CopterFuelTankMaxValue
	 * 10. CopterWeight
	 * 11. CargoSize
	 * 12. RopeLevel			-> Save
	 * 13. RopeDefaultValue
	 * 14. RopeMaxValue
	 * 15. CopterMaxHealth
	 * 16. TiltSpeed
	 * 17. MaxTilt
	 * 
	 */
	private string[,] copters = new string[copterAmount,18]{
		{
			"DefaultCopter", 	//NAME
			"0", 				//COST
			"1", 				//PLATFORM
			"1", 				//UNLOCKED (0/1)
			"1", 				//ENGINE LEVEL
			"1", 				//FUELTANK LEVEL
			"100", 				//ENGINE DEFAULT VALUE
			"200",				//ENGINE MAX VALUE
			"300", 				//FUELTANK DEFAULT VALUE
			"500",				//FUELTANK MAX VALUE
			"15",				//WEIGHT
			"2",				//CARGOSIZE
			"1",				//ROPE LEVEL
			"5",				//ROPE DEFAULT VALUE
			"15",				//ROPE MAX VALUE
			"100",				//MAXHEALTH / DURABILITY
			"100",				//MAX TILT SPEED
			"75"				//MAX TILT VALUE
		},{
			"WaterCopter",	 	//NAME
			"0", 				//COST
			"1", 				//PLATFORM
			"1", 				//UNLOCKED (0/1)
			"1", 				//ENGINE LEVEL
			"1", 				//FUELTANK LEVEL
			"100", 				//ENGINE DEFAULT VALUE
			"200",				//ENGINE MAX VALUE
			"300", 				//FUELTANK DEFAULT VALUE
			"500",				//FUELTANK MAX VALUE
			"15",				//WEIGHT
			"2",				//CARGOSIZE
			"1",				//ROPE LEVEL
			"5",				//ROPE DEFAULT VALUE
			"15",				//ROPE MAX VALUE
			"100",				//MAXHEALTH / DURABILITY
			"100",				//MAX TILT SPEED
			"75"				//MAX TILT VALUE
		}
	};

	// Use this for initialization
	void Start () {
		if (!PlayerPrefs.HasKey("First")) {
			save ();
		} else {
			load ();
		}
		updateMainMenuDebug();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void save () {
		PlayerPrefs.SetInt("First", 1);
		PlayerPrefs.SetString("Name", playerName);
		PlayerPrefs.SetInt("Stars", playerStars);
		PlayerPrefs.SetInt("Coins", playerCoins);
		PlayerPrefs.SetInt("Platform", playerPlatform);

		//Currently selected copter
		PlayerPrefs.SetInt("Copter", currentCopter);

		//CopterLevels and unlocks
		for (int i = 0; i < copterAmount; i++) {
			PlayerPrefs.SetInt("Copter"+i+"Unlocked", int.Parse(copters[i,3]));
			PlayerPrefs.SetInt("Copter"+i+"Enginelevel", int.Parse(copters[i,4]));
			PlayerPrefs.SetInt("Copter"+i+"Fueltanklevel", int.Parse(copters[i,5]));
			PlayerPrefs.SetInt("Copter"+i+"Ropelevel", int.Parse(copters[i,12]));
		}
	}

	public void load () {
		playerFirst = PlayerPrefs.GetInt("First", playerFirst);
		playerName = PlayerPrefs.GetString("Name", playerName);
		playerStars = PlayerPrefs.GetInt("Stars", playerStars);
		playerCoins = PlayerPrefs.GetInt("Coins", playerCoins);
		playerPlatform = PlayerPrefs.GetInt("Platform", playerPlatform);

		//Currently selected copter
		currentCopter = PlayerPrefs.GetInt("Copter", currentCopter);

		//CopterLevels and unlocks
		for (int i = 0; i < copterAmount; i++) {
			copters[i,3] = PlayerPrefs.GetInt("Copter"+i+"Unlocked").ToString();
			copters[i,4] = PlayerPrefs.GetInt("Copter"+i+"Enginelevel").ToString();
			copters[i,5] = PlayerPrefs.GetInt("Copter"+i+"Fueltanklevel").ToString();
			copters[i,12] = PlayerPrefs.GetInt("Copter"+i+"Ropelevel").ToString();
		}
	}

	public string getName () {
		return playerName;
	}
	public void setName (string name) {
		playerName = name;
		save ();
	}

	public int getStars () {
		return playerStars;
	}
	private void setStars (int stars) {
		playerStars = stars;
		save ();
	}
	public void addStars (int stars) {
		setStars (getStars() + stars);
	}

	public int getCoins () {
		return playerCoins;
	}
	private void setCoins (int coins) {
		playerCoins = coins;
	}
	public void addCoins (int coins) {
		setCoins (getCoins() + coins);
		save ();
	}

	public int getPlatformLevel () {
		return playerPlatform;
	}
	public void upgradePlatformLevel () {
		if (playerPlatform < 10)
			playerPlatform += 1;
		save ();
		updateMainMenuDebug();
	}

	public string[,] getCopters () {
		return copters;
	}
	public int getCopterAmount () {
		return copterAmount;
	}

	public int getCurrentCopter () {
		return currentCopter;
	}
	public void setCurrentCopter (int copter){
		currentCopter = copter;
		save ();
		updateMainMenuDebug();
	}
	public void swapCopter () {
		if (currentCopter < getCopterAmount()-1) setCurrentCopter(currentCopter+1);
		else setCurrentCopter(0);
	}

	public void upgradeCurrentEngine () {
		if (int.Parse(copters[currentCopter, 4]) < 10)
			copters[currentCopter, 4] = (int.Parse(copters[currentCopter, 4]) + 1).ToString();
		save ();
		updateMainMenuDebug();
	}

	public void upgradeCurrentFuelTank () {
		if (int.Parse(copters[currentCopter, 5]) < 10)
			copters[currentCopter, 5] = (int.Parse(copters[currentCopter, 5]) + 1).ToString();
		save ();
		updateMainMenuDebug();
	}

	public void upgradeCurrentRope () {
		if (int.Parse(copters[currentCopter, 12]) < 10)
			copters[currentCopter, 12] = (int.Parse(copters[currentCopter, 12]) + 1).ToString();
		save ();
		updateMainMenuDebug();
	}
	
	public void resetUpgrades () {
		copters[currentCopter, 4] = "1";
		copters[currentCopter, 5] = "1";
		copters[currentCopter, 12] = "1";
		playerPlatform = 1;
		save ();
		updateMainMenuDebug();
	}

	private void updateMainMenuDebug() {
		if (GameObject.Find("MainMenu") != null && GameObject.Find("MainMenu").transform.FindChild("DebugText") != null){
			GameObject.Find("MainMenu").transform.FindChild("DebugText").GetComponent<Text>().text = copters[currentCopter,0]+"\nEngine: " + copters[currentCopter, 4]+
				"\nFuel: " + copters[currentCopter, 5]+
				"\nPlatform: " + playerPlatform.ToString()+
				"\nRope: " + copters[currentCopter, 12];
		}
	}

	//CAREFUL!
	public void resetData () {
		PlayerPrefs.DeleteKey("First");
		PlayerPrefs.DeleteKey("Name");
		PlayerPrefs.DeleteKey("Stars");
		PlayerPrefs.DeleteKey("Coins");
		PlayerPrefs.DeleteKey("Platform");
		PlayerPrefs.DeleteKey("Copter");
		for (int i = 0; i < copterAmount; i++) {
			PlayerPrefs.DeleteKey("Copter"+i+"Unlocked");
			PlayerPrefs.DeleteKey("Copter"+i+"Enginelevel");
			PlayerPrefs.DeleteKey("Copter"+i+"Fueltanklevel");
			PlayerPrefs.DeleteKey("Copter"+i+"Ropelevel");
		}
		Application.LoadLevel("MainMenu");
	}

	public void goToUpgrades (int copter) {
		PlayerPrefs.SetInt("Copter", copter);
		Application.LoadLevel("UpgradeScreen");
	}

	public void startGame (string levelName) {
		//Debug.Log("Engine Level: " + copters[currentCopter,4] + " Fuel Level: " + copters[currentCopter,5] + " Platform Level: " + playerPlatform);
		//Debug.Log("Engine Level: " + PlayerPrefs.GetInt("Copter"+currentCopter+"Enginelevel").ToString() + " Fuel Level: " + PlayerPrefs.GetInt("Copter"+currentCopter+"Fueltanklevel").ToString() + " Platform Level: " + playerPlatform);
		save ();
		Application.LoadLevel(levelName);
	}
}
