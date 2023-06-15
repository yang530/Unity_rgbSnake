using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	//instances of game objects and variables
	public GameBoard gb;
	public GameState gameState = GameState.Title;
	
	Snake snake; // the snake
	Color cBackGround; //target color for the background to smoothly transit towards
	GameObject nodePrefab;
	GameObject blockPrefab;
	GameObject foodPrefab;
	List<GameObject> specFood; //list of special foods
	List<Color> cSpecFood; //list of special foods' target color

	public int score = 0;
	public int highScore;
	public float axisLast = 0.0f; //control axis value when it was registered last
	
	public MapSize mapSize = MapSize.Small;

	//pointers to menu elements
	public Text[] colorValUI;
	public Text mapSizeText;
	public Text scoreBoard;
	public Text snakeLen;
	public Text scoreText;
	public Text finalScore;
	public Text oldScoreText;
	public Text oldHiScore;

	public Transform miniMapCam;
	public Transform gameUI;
	public Transform titleScreen;
	public Transform pauseText;
	public Transform endGameText;

	// Use this for initialization the game manager
	void Awake () {

		gb = new GameBoard ();
		titleScreen.gameObject.SetActive (true);
		nodePrefab = Resources.Load ("Prefabs/Node") as GameObject;
		blockPrefab = Resources.Load ("Prefabs/Block") as GameObject;
		foodPrefab = Resources.Load ("Prefabs/Food") as GameObject;
		snake = GetComponent<Snake> ();
		cBackGround = new Color (Random.Range (0, 1f), Random.Range (0, 1f), Random.Range (0, 1f)); //A new RANDOM Color for the background
		mapSizeText.text = mapSize.ToString ();
		specFood = new List<GameObject> ();
		cSpecFood = new List<Color> ();

	}

	//only for testing within Unity
	void Start(){

		//StartGame ();

	}
	
	// Update is called once per frame
	void Update () {

		Control ();

		//game is at title screen
		if(gameState == GameState.Title){
			//generate a random color aproximately every two seconds
			if(Time.realtimeSinceStartup % 2 < Time.deltaTime){
				cBackGround = new Color (Random.Range (0, 1f), Random.Range (0, 1f), Random.Range (0, 1f));
			}
			SmoothColor();

		}
		//game is runing
		if(gameState == GameState.Runing){
			SmoothColorSpecFood();
			snake.Control ();
			snake.UpdateSnakePos();
			snake.MoveSnake ();
			snake.CamTrace ();
		}
		//show end game menu
		if(gameState == GameState.Ended){
			endGameText.gameObject.SetActive(true);
		}
		//all food is consumed
		if(gb.numFood <= 0 && gameState == GameState.Runing){
			EndGame();
		}

	}

	//Initialize play scene
	public void StartGame(){

		RenderBoard ();
		titleScreen.gameObject.SetActive (false);
		gameUI.gameObject.SetActive (true);
		Vector2 startPos = gb.FindStart ();
		snake.InitializeSnake (gb.GetTileCoordinate(startPos), gb.GetStartDir(startPos));
		scoreBoard.text = score.ToString ();
		Vector2 midPoint = gb.GetMidPoint ();
		miniMapCam.transform.position = new Vector3 (midPoint.x, midPoint.y, -10f);
		miniMapCam.GetComponent<Camera>().orthographicSize = Mathf.Max (gb.bHeight, gb.bWidth) / 4;
		gameState = GameState.Idle;
		pauseText.gameObject.SetActive(true);
		//load highscore from game file
		highScore = LoadHiScore();
		Debug.Log("Highscore loaded: " + highScore);

	}

	public void RestartGame(){
		Application.LoadLevel (0);
	}

	public void PauseGame(){

		if(gameState == GameState.Runing){
			
			gameState = GameState.Idle;
			pauseText.gameObject.SetActive(true);
			
		}
		else if(gameState == GameState.Idle){
			
			gameState = GameState.Runing;
			pauseText.gameObject.SetActive(false);
			
		}

	}

	public void updateRGBText(){

		colorValUI [0].text = ((int)(snake.snakeColor.r * 100)).ToString();
		colorValUI [1].text = ((int)(snake.snakeColor.g * 100)).ToString();
		colorValUI [2].text = ((int)(snake.snakeColor.b * 100)).ToString();

	}

	void SmoothColor(){

		Color currentColor = titleScreen.GetComponent<CanvasRenderer> ().GetColor ();
		currentColor += (cBackGround - currentColor) * Time.deltaTime;
		titleScreen.GetComponent<CanvasRenderer> ().SetColor (currentColor);

	}

	//basic keyboard function of the game
	void Control(){

		float axisH = Input.GetAxisRaw ("Horizontal");

		//Select Map Size with keyboard
		if(Mathf.Abs(axisH) > 0.1f && Mathf.Abs(axisLast) < 0.1f && gameState == GameState.Title){
			int keyDir = (int)(axisH / Mathf.Abs(axisH));
			Debug.Log("Key Axis " + keyDir);
			ChangeMapSize(keyDir);
		}

		if(Input.GetKeyUp(KeyCode.Return)){
			if(gameState == GameState.Title){
				StartGame();
			}
			if(gameState == GameState.Ended){
				RestartGame();
			}
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			PauseGame();
		}

		axisLast = axisH;

	}

	public void ChangeMapSize(int dir){

		int num = (int)mapSize;
		num = (num + dir) % 3;
		if(num < 0){
			num = 2;
		}
		mapSize = (MapSize)num;
		mapSizeText.text = mapSize.ToString ();

	}

	//Function to generate the gameboard
	void RenderBoard(){
	
		switch(mapSize){

		case MapSize.Small:
//			gb.bWidth = 40;
//			gb.bHeight = 40;
			break;
		case MapSize.Medium:
			gb = new GameBoard(60, 60);
			break;
		case MapSize.Huge:
			gb = new GameBoard(100, 100);
			break;

		}
		InitializeCSpecFood();

		gb.board = new Tile[gb.bWidth, gb.bHeight];
	
		for (int y = 0; y < gb.bHeight; y++){
	
			for (int x = 0; x < gb.bWidth; x++){
	
				gb.board[x, y] = new Tile(TileType.Block);
	
			}
	
		}
		//findPath (GetRandomPoint (), GetRandomPoint ());
		//gb.RandomCircuit (new Vector2(1, 1), 50);
		gb.CreateMaze();
		//Debug.Log (tEmpty);
	
		for (int y = 0; y < gb.bHeight; y++){
				
			for (int x = 0; x < gb.bWidth; x++){
	
				if(gb.board[x, y].type == TileType.Block){
	
					GameObject item = Object.Instantiate(blockPrefab) as GameObject;
					item.transform.position = gb.GetTileCoordinate(new Vector2((float)x, (float)y));
	
				}else if(gb.board[x, y].type == TileType.Start){
	
					GameObject item = Object.Instantiate(Resources.Load ("Prefabs/Test_Start")) as GameObject;
					item.transform.position = gb.GetTileCoordinate(new Vector2((float)x, (float)y));
	
				}else if(gb.board[x, y].type == TileType.End){
						
					GameObject item = Object.Instantiate(Resources.Load ("Prefabs/Test_End")) as GameObject;
					item.transform.position = gb.GetTileCoordinate(new Vector2((float)x, (float)y));
						
				}
				else if(gb.board[x, y].type == TileType.Food){
					
					GameObject item = Object.Instantiate(foodPrefab) as GameObject;
					item.transform.position = gb.GetTileCoordinate(new Vector2((float)x, (float)y));
					item.GetComponent<Renderer>().material.color = new Color (Random.Range(0.0f, 1.0f) , Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
					//if this food is special, add it to the special food list
					if(gb.board[x,y].isSpec == true){
						specFood.Add(item);
					}
					
				}
				
			}
				
		}
		//specFood Debug logs
		Debug.Log("---specFood list completed---");
		Debug.Log("specFood.Count: " + specFood.Count);
		Debug.Log("---specFood list Content---");
		for(int i=0; i < specFood.Count; i++){
				
				Debug.Log(specFood[i].GetComponent<Renderer>().material.color.ToString());

		}


		//place frame of the map
		for (int y = 0; y < gb.bHeight; y++){
			
			GameObject item = Object.Instantiate(blockPrefab) as GameObject;
			item.transform.position = gb.GetTileCoordinate(new Vector2((float)-1, (float)y));
			item = Object.Instantiate(blockPrefab) as GameObject;
			item.transform.position = gb.GetTileCoordinate(new Vector2((float)gb.bWidth, (float)y));
			
		}
		for (int x = -1; x <= gb.bWidth; x++){
			
			GameObject item = Object.Instantiate(blockPrefab) as GameObject;
			item.transform.position = gb.GetTileCoordinate(new Vector2((float)x, (float)-1));
			item = Object.Instantiate(blockPrefab) as GameObject;
			item.transform.position = gb.GetTileCoordinate(new Vector2((float)x, (float)gb.bHeight));
			
		}

	
	}

	//function to return highscore from game file
	public int LoadHiScore(){

		int returnHiScore = 0;

		switch(mapSize){

		case MapSize.Small:
			returnHiScore = PlayerPrefs.GetInt("highScoreS");
			break;
		case MapSize.Medium:
			returnHiScore = PlayerPrefs.GetInt("highScoreM");
			break;
		case MapSize.Huge:
			returnHiScore = PlayerPrefs.GetInt("highScoreH");
			break;
		default:
			break;
		}

		return returnHiScore;

	}

	//update highscore and change end game texts
	public void UpdateHiScore(){

		finalScore.text = score.ToString();
		oldHiScore.text = highScore.ToString();

		//if highscore is beaten
		if(score > highScore){
			//PlayerPrefs.SetInt("highScore", score); 
			//update highscore in game file
			switch(mapSize){

			case MapSize.Small:
				PlayerPrefs.SetInt("highScoreS", score);
				break;
			case MapSize.Medium:
				PlayerPrefs.SetInt("highScoreM", score);
				break;
			case MapSize.Huge:
				PlayerPrefs.SetInt("highScoreH", score);
				break;
			default:
				break;
			}

			scoreText.text = "New High Score!";
			oldScoreText.text = "Old Highscore Was:";
		}

	}

	public void EndGame (){

		gameState = GameState.Ended;
		UpdateHiScore();

	}

	//smoothly change color of an gameobject to the target color
	public void ObjSmoothColor(GameObject obj, Color cTarget){

		Color currentColor = obj.GetComponent<Renderer> ().material.color;
		currentColor += (cTarget - currentColor) * Time.deltaTime;
		obj.GetComponent<Renderer> ().material.color = currentColor;

	}

	public void InitializeCSpecFood(){

		for(int i=0; i < gb.numSpecFood; i++){
			
			cSpecFood.Add(new Color(Random.Range (0, 1f), Random.Range (0, 1f), Random.Range (0, 1f)));

		}


	}

	//smoothly change the colors of all special foods
	public void SmoothColorSpecFood(){

		//smoothly transit the color of each special food to a new random color
		for(int i=0; i < specFood.Count; i++){
			
			if (Time.realtimeSinceStartup % 1 < Time.deltaTime){
				cSpecFood[i] = new Color(Random.Range (0, 1f), Random.Range (0, 1f), Random.Range (0, 1f));
			}

			ObjSmoothColor(specFood[i], cSpecFood[i]);

		}

	}

	//remove an item from specFood list
	public bool ShrinkSpecFood (GameObject item){

		int indexFound = specFood.IndexOf(item); 

		if(indexFound != -1){
			return specFood.Remove(item);
		}

		return false;

	}


}

public enum GameState{

	Title, Idle, Runing, Ended,

};

public enum MapSize{

	Small, Medium, Huge,

};

