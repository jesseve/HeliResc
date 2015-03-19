using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Testi : MonoBehaviour {
	
	public List<GameObject> go;		//Order of canvases: 1. MainMenu 2.LevelEnd 3. Upgrades
	private float[] canvasAnchorPoints;
	
	private Vector3[] panelAnchorPoints;
	
	public float speed = 10f;
	public bool showLevelEnd = false;
	
	private bool move;
	private bool drag;
	private Vector3 target;
	private Transform panel;
	
	private int current = 0;
	private Vector3 mouseStart;
	private Vector3 mouseEnd;
	private float swipeAmount;
	private Vector3 previousPosition;
	
	// Use this for initialization
	void Start () {
		swipeAmount = Screen.width * .2f;
		panel = go[0].transform.parent;
		Vector3[] vecs = new Vector3[4];
		GetComponent<RectTransform>().GetWorldCorners(vecs);
		foreach(Vector3 v in vecs)
			Debug.Log("Corner: " + v);
		//go[1].transform.position = vecs[3] + go[0].transform.position;
		canvasAnchorPoints = new float[go.Count];
		panelAnchorPoints = new Vector3[go.Count];
		for(int i = 0; i < go.Count; i++){
			canvasAnchorPoints[i] = go[0].transform.position.x + (i * vecs[3].x);
			panelAnchorPoints[i] = new Vector3(canvasAnchorPoints[i] - i * 4 * canvasAnchorPoints[0], go[0].transform.position.y);
			Debug.Log("Anchor point " + i + ": " + canvasAnchorPoints[i]);
			Debug.Log("Panel anchor point " + i + ": " + panelAnchorPoints[i]);
		}
		if(showLevelEnd){
			go[1].transform.position = new Vector3(canvasAnchorPoints[1], go[1].transform.position.y);
			go[2].transform.position = new Vector3(canvasAnchorPoints[2], go[2].transform.position.y);
		} else {
			go[2].transform.position = new Vector3(canvasAnchorPoints[1], go[2].transform.position.y);
		}
	}
	
	void Update() {
		if(Input.GetMouseButtonDown(0)){
			mouseStart = previousPosition = Input.mousePosition;
			drag = true;
			Debug.Log(mouseStart);
		}
		if(Input.GetMouseButtonUp(0)) {
			mouseEnd = Input.mousePosition;
			Debug.Log(mouseEnd);
			if(mouseEnd.x < mouseStart.x) {
				if(mouseStart.x - mouseEnd.x > swipeAmount)
					current = current < 2 ? current + 1 : 2;
			} else if(mouseEnd.x > mouseStart.x){
				if(mouseEnd.x - mouseStart.x > swipeAmount)
					current = current > 0 ? current - 1 : 0;
			}
			move = true;
			drag = false;
			target = panelAnchorPoints[current];
		}
		if(Input.GetMouseButton(0) && drag) {
			panel.position -= Vector3.right * ((Mathf.Abs(previousPosition.x) - Mathf.Abs(Input.mousePosition.x)));
			previousPosition = Input.mousePosition;
		}
		
		
		if(move) {
			float step = Time.deltaTime * speed;
			panel.position = Vector3.MoveTowards(panel.position, target, step);
		}
		if(Vector3.Distance(panel.position, target) < 10 && move) {
			move = false;
			Debug.Log ("Reach target");
			panel.position = target;
		}
	}
}