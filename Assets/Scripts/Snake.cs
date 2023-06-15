using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Snake : MonoBehaviour {

	GameObject nodePrefab;
	public List<GameObject> nodes;
	List<Vector2> currentPos;
	List<Vector2> nextPos;
	Vector2 moveDir;
	float nodeSize;
	public float speed;
	float maxSpeed;
	float minSpeed;
	GameManager gmScript;
	public Color snakeColor;
	float snakeTimer;
	float snakeTimerMax;

	// Use this for initialization
	void Awake () {

		snakeTimerMax = 30f;
		snakeTimer = snakeTimerMax;
		gmScript = transform.GetComponent<GameManager> ();
		nodeSize = gmScript.gb.tileSize;
		minSpeed = 1f;
		maxSpeed = 3f * minSpeed;
		speed = minSpeed;
		nodePrefab = Resources.Load ("Prefabs/Node") as GameObject;
		nodes = new List<GameObject> ();
		currentPos = new List<Vector2> ();
		nextPos = new List<Vector2> ();
		snakeColor = new Color (Random.Range(0.0f, 1.0f) , Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)); 

	}
	
	// Update is called once per frame
	void Update () {
	
//		Control ();
//		UpdateSnakePos();
//		MoveSnake ();
//		CamTrace ();
//		ChangeSnakeColor ();
		CountDown ();

	}

	public void InitializeSnake(Vector2 startPos, Vector2 dir){

		moveDir = dir;

		for(int i = 0; i < 4; i++){

			GameObject item = Object.Instantiate(nodePrefab) as GameObject;
			item.transform.position = startPos + (-1 * moveDir.normalized * nodeSize * i);
			currentPos.Add(item.transform.position);
			nextPos.Add(item.transform.position);
			nodes.Add(item);
			item.GetComponent<Renderer>().material.color = snakeColor;

		}
		nodes [0].AddComponent<Head> ();
		nodes [0].GetComponent<Head> ().game = gameObject;
		Physics2D.IgnoreCollision (nodes[0].GetComponent<Collider2D>(), nodes[1].GetComponent<Collider2D>());

		transform.position = new Vector3 (nodes[0].transform.position.x, nodes[0].transform.position.y, transform.position.z);
		gmScript.snakeLen.text = nodes.Count.ToString();
		gmScript.updateRGBText ();
		Debug.Log (snakeColor);

	}

	public void EatFood(GameObject food){

		Color foodColor = food.GetComponent<Renderer>().material.color;

		//Physics2D.IgnoreCollision (nodes[0].collider2D, nodes[1].collider2D, false);
		food.GetComponent<Renderer>().material.color = snakeColor;

		float colorDiff = GetColorDiff (snakeColor, foodColor);
		Debug.Log (snakeColor + " | " + foodColor + " \n " + colorDiff);
		if(colorDiff > 0.8f){
			//Debug.Log(nodes.Count);
			BreakFrom((int)(nodes.Count / 2));
		}

		snakeColor = (snakeColor + foodColor) / 2;
		gmScript.updateRGBText ();
		Debug.Log ("Current Color: " + snakeColor);

		Vector3 newCurrentPos = currentPos [currentPos.Count - 1];
		Vector3 deltaMove = -1 * moveDir * nodeSize;
		newCurrentPos += new Vector3(deltaMove.x, deltaMove.y, 0);
		food.transform.position = nodes [nodes.Count - 1].transform.position + deltaMove;
		nodes.Add (food);
		nextPos.Add (currentPos [currentPos.Count - 1]);
		currentPos.Add (newCurrentPos);

		//nodes.Insert (1, food);
		//nodes [0].transform.position = food.transform.position;
		//currentPos.Insert (0, food.transform.position);
		//Vector3 newNextPos = nodeSize * moveDir;
		//newNextPos += food.transform.position;
		//nextPos.Insert (0, newNextPos);

		//Physics2D.IgnoreCollision (nodes[0].collider2D, nodes[1].collider2D);
		//ResetSnake ();
		ChangeSnakeColor ();

		//update some data on the game manager side
		gmScript.score += (int)(100 * Mathf.Clamp (snakeTimer / 5f, 1, snakeTimerMax / 5f) * nodes.Count * colorDiff);
		--gmScript.gb.numFood;
		snakeTimer = snakeTimerMax;
		UpdateSpeed ();
		gmScript.scoreBoard.text = gmScript.score.ToString ();
		gmScript.snakeLen.text = nodes.Count.ToString();
		Debug.Log(gmScript.ShrinkSpecFood (food));

	}

	public void CountDown(){

		if (snakeTimer < Time.deltaTime) {
			snakeTimer = 0;
		} else {
			snakeTimer -= Time.deltaTime;
		}

	}

	void ChangeSnakeColor(){

		for (int i = 0; i < nodes.Count; i++){

			nodes[i].GetComponent<Renderer>().material.color = snakeColor;

		}

	}

	Color ModRGB(Color c){

		//Color result = c;
		c.r %= 1.0f;
		c.g %= 1.0f;
		c.b %= 1.0f;
		c.a = 1f;
		return c;

	}

	float GetColorDiff(Color c1, Color c2){

		float rd = c1.r - c2.r;
		float gd = c1.g - c2.g;
		float bd = c1.b - c2.b;
		return Mathf.Sqrt (rd * rd + gd * gd + bd * bd);

	}

	void ResetSnake(){

		Vector2 vDis = new Vector3(nextPos[0].x, nextPos[0].y, 0) - nodes[0].transform.position;
		List<Vector2> resetTarget;

		if (vDis.magnitude < nodeSize) {
			resetTarget = nextPos;
		} else {
			resetTarget = currentPos;
		}

		for(int i = 0; i < nodes.Count; i++){
			
			nodes[i].transform.position = resetTarget[i];
			
		}

	}

	public void UpdateSnakePos(){

		Vector2 vDis = new Vector3(nextPos[0].x, nextPos[0].y, 0) - nodes[0].transform.position;

		if(vDis.magnitude < speed * Time.deltaTime){

			for(int i = 0; i < nodes.Count; i++){
				currentPos[i] = nextPos[i];
			}
			Vector2 newHead = nextPos[0] + moveDir.normalized * nodeSize;
			nextPos.Insert(0, newHead);
			nextPos.RemoveAt(nextPos.Count - 1);

		}

	}

	public void MoveSnake(){

		Vector3 dir = Vector3.zero;
		for(int i = 0; i < nodes.Count; i++){

			dir = new Vector3(nextPos[i].x, nextPos[i].y, 0) - nodes[i].transform.position;
			if(dir.magnitude < speed * Time.deltaTime){
				nodes[i].transform.position = nextPos[i];
			}
			else{
				nodes[i].transform.position += dir.normalized * Time.deltaTime * speed;
			}

		}

	}

	public void UpdateSpeed(){

		minSpeed = Mathf.Clamp((1f + (nodes.Count - 4) * 0.05f), 1, 2 * maxSpeed / 3);
		speed = minSpeed;

	}

	//control the snake movement with keyboard and mouse
	public void Control(){

		Vector2 newMove = moveDir;
		float axisH = Input.GetAxis ("Horizontal");
		float axisV = Input.GetAxis ("Vertical");
		bool isOverUI = EventSystem.current.IsPointerOverGameObject();

		if(Input.GetMouseButton(0) && isOverUI == false){

			Vector3 mouse2Snake = Input.mousePosition;
			mouse2Snake -= Camera.main.WorldToScreenPoint(nodes[0].transform.position);
			mouse2Snake.z = 0;
			float maxAxis = Mathf.Max(Mathf.Abs(mouse2Snake.x), Mathf.Abs(mouse2Snake.y));

			if(maxAxis == Mathf.Abs(mouse2Snake.x)){
				axisH = mouse2Snake.x;
				axisV = 0;
			}else if(maxAxis == Mathf.Abs(mouse2Snake.y)){
				axisV = mouse2Snake.y;
				axisH = 0;
			}

		}

		if(Mathf.Abs(axisH) > 0.1f && Mathf.Abs(axisV) < 0.1f){
			newMove = new Vector2(1f, 0) * axisH / Mathf.Abs(axisH);
		}
		if(Mathf.Abs(axisV) > 0.1f && Mathf.Abs(axisH) < 0.1f){
			newMove = new Vector2(0, 1f) * axisV / Mathf.Abs(axisV);
		}
		//Debug.Log (newMove);

		if (-1 * newMove.normalized != moveDir.normalized && newMove.normalized != moveDir.normalized && CanTurn (newMove) == true) {
			moveDir = newMove;
			ResetSnake ();
			speed = minSpeed;
		} else if (newMove.normalized == moveDir.normalized && (Mathf.Abs (axisH) > 0.1f || Mathf.Abs (axisV) > 0.1f)) {
			speed = maxSpeed;
		} else {
			speed = minSpeed;
		}

	}

	public void BreakFrom(int index){

		while(nodes.Count - 1 >= index){

			Destroy(nodes[nodes.Count - 1]);
			nodes.RemoveAt(nodes.Count - 1);
			nextPos.RemoveAt(nextPos.Count - 1);
			currentPos.RemoveAt(currentPos.Count - 1);

		}

	}

	bool CanTurn(Vector2 turnVect){

		Vector2 vDis = new Vector3(nextPos[0].x, nextPos[0].y, 0) - nodes[0].transform.position;
		Vector2 pHead;
		
		if (vDis.magnitude < nodeSize) {
			pHead = nextPos[0];
		} else {
			pHead = currentPos[0];
		}
		pHead += turnVect * nodeSize;
		pHead /= nodeSize;

		if(gmScript.gb.isOnBoard(pHead) == false){
			return false;
		}

		if ((gmScript.gb.board [(int)(pHead.x), (int)(pHead.y)]).type == TileType.Block) {
			return false;
		} else {
			return true;
		}

	}

	public void CamTrace(){

		Vector3 camMove = nodes [0].transform.position;
		camMove.z = transform.position.z;
		camMove -= transform.position;

		if (camMove.magnitude < 1.1f * speed * Time.deltaTime) {
			transform.position = nodes [0].transform.position;
			transform.position += new Vector3 (0, 0, -10);
		} else {
			transform.position += camMove.normalized * 1.1f * speed * Time.deltaTime;
		}

		//transform.position = nodes [0].transform.position;
		//transform.position += new Vector3 (0, 0, -10);

	}

}
