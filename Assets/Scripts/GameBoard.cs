using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameBoard {

	public Tile[,] board;

	public int bWidth;
	public int bHeight;
	public int numFood;
	public int numSpecFood;
	int tEmpty;
	int totalTiles;
	public float tileSize = 0.5f;
	float blockRatio  = 0.4f;


	// Use this for initialization
	public GameBoard () {
	
		tEmpty = 0;
		bWidth = 40;
		bHeight = 40;
		totalTiles = bWidth * bHeight;
		numFood = (bWidth + bHeight) / 2;
		numSpecFood = numFood / 5;

	}

	public GameBoard (int width, int height){

		tEmpty = 0;
		bWidth = width;
		bHeight = height;
		totalTiles = bWidth * bHeight;
		numFood = (bWidth + bHeight) / 2;
		numSpecFood = numFood / 5;

	}

	#region Maze Generating Codes 
	public Vector2[] GetAdjacentTiles(Vector2 pos){

		Vector2[] result = new Vector2[4];

		result [0] = pos + new Vector2 (-1, 0);
		result [1] = pos + new Vector2 (0, -1);
		result [2] = pos + new Vector2 (1, 0);
		result [3] = pos + new Vector2 (0, 1);

		return result;

	}

	public Vector2 GetStartDir(Vector2 pos){

		Vector2[] tiles = GetAdjacentTiles (pos);

		if(isOnBoard(tiles[0]) == true && board[(int)tiles[0].x, (int)tiles[0].y].type == TileType.Empty){
			return new Vector2(-1, 0);
		}
		if(isOnBoard(tiles[1]) == true && board[(int)tiles[1].x, (int)tiles[1].y].type == TileType.Empty){
			return new Vector2(0, -1);
		}
		if(isOnBoard(tiles[2]) == true && board[(int)tiles[2].x, (int)tiles[2].y].type == TileType.Empty){
			return new Vector2(1, 0);
		}
		if(isOnBoard(tiles[3]) == true && board[(int)tiles[3].x, (int)tiles[3].y].type == TileType.Empty){
			return new Vector2(0, 1);
		}
		return new Vector2 (0, 0);

	}

	public Vector2 FindStart(){

		Vector2 result = GetRandomPoint ();
		Vector2[] adjTiles = GetAdjacentTiles (result); 

		int i = 4;
		while(i >=4 || board[(int)result.x, (int)result.y].type != TileType.Empty){

			for(i = 0; i < 4; i++){
				if(isOnBoard(adjTiles[i]) == true && board[(int)adjTiles[i].x, (int)adjTiles[i].y].type == TileType.Empty){
					break;
				}
			}
			result = GetRandomPoint ();
			adjTiles = GetAdjacentTiles (result); 
		}
		return result;

	}

	public Vector2 GetTileCoordinate(Vector2 pIndex){

		return pIndex * tileSize;

	}

	public Vector2 GetMidPoint(){

		return new Vector2 ((bWidth - 1) * tileSize / 2, (bHeight - 1) * tileSize / 2);

	}

	int GetDistance(Vector2 start, Vector2 des){

		int result = 0; 
		result += Mathf.Abs((int)(des.x - start.x));
		result += Mathf.Abs((int)(des.y - start.y));

		return result;

	}

	Vector2 GetRandomPoint(){

		float x;
		float y;
		x = Random.Range (0, bWidth);
		y = Random.Range (0, bHeight);

		return new Vector2 (x, y);

	}

	void ChangeTiles(List<Vector2> list, TileType type){

		for(int i = 0; i < list.Count; i++){
			if (board [(int)list[i].x, (int)list[i].y].type == TileType.Block) {
				tEmpty ++;
			}
			board [(int)list[i].x, (int)list[i].y].type = type;
		}
		
	}
	
	List<Vector2> findPath(Vector2 start, Vector2 des){

		//Debug.Log ("start: " + start);
		//Debug.Log ("des: " + des);

		List<Vector2> newPath = new List<Vector2> ();

		if (start != des) {

			float deltaX = des.x - start.x;
			float deltaY = des.y - start.y;

			//Debug.Log ("deltaX: " + deltaX);
			//Debug.Log ("deltaY: " + deltaY);
			//Debug.Log ("\n\n\n");
			
			int xCoeff = (int)(deltaX / Mathf.Abs (deltaX));
			int yCoeff = (int)(deltaY / Mathf.Abs (deltaY));
			if(deltaX == 0){
				xCoeff = 0;
			}
			if(deltaY == 0){
				yCoeff = 0;
			}

			Vector2 current = start;
			newPath.Add(current);
			int moveDir;
			Vector2 nextV;
			Vector2 nextH;

			int loopCount = 0;
			while (current != des) {
//				Debug.Log("------------------------");
//				Debug.Log("current: " + current);
//				Debug.Log ("delta: " + (des - current) );

				moveDir = Random.Range (0, 2);
				//Debug.Log ("Dir: " + moveDir);
				nextH = current + new Vector2 (1, 0) * xCoeff;
				nextV = current + new Vector2 (0, 1) * yCoeff;
				
				if (moveDir == 0 && des.x - current.x != 0) {
					current = nextH;
					newPath.Add(current);
					//Debug.Log("next: " + current);
				} 
				if (moveDir == 1 && des.y - current.y != 0){
					current = nextV;
					newPath.Add(current);
					//Debug.Log("next: " + current);

				}

			}
			++loopCount;

		} else {
			board [(int)start.x, (int)start.y].type = TileType.Empty;
		}

//		Debug.Log ("Real Distance: " + GetDistance(start, des));
//		Debug.Log ("Path Count: " + newPath.Count);

		if(newPath.Count > 0){

			ChangeTiles(newPath, TileType.Empty);

//			Debug.Log ("Remove: " + newPath[newPath.Count - 1]);
//			Debug.Log ("Remove: " + newPath[0]);
//			Debug.Log ("Des: " + des);
//			Debug.Log ("Start: " + start);
			newPath.RemoveAt(newPath.Count - 1);
			newPath.RemoveAt(0);

			//ChangeTiles(newPath, TileType.Start);

		}

		return newPath;

	}

	List<Vector2> findPath(Vector2 start, Vector2 des, List<Vector2> oldPath){

		List<Vector2> newPath = new List<Vector2> ();
		List<Vector2> blocked = oldPath;
				
		if (start != des) {
					
			float deltaX = des.x - start.x;
			float deltaY = des.y - start.y;
					
			//Debug.Log ("deltaX: " + deltaX);
			//Debug.Log ("deltaY: " + deltaY);
			//Debug.Log ("\n\n\n");
		
			//Determine the direct to move on x and y axis
			int xCoeff = (int)(deltaX / Mathf.Abs (deltaX));
			int yCoeff = (int)(deltaY / Mathf.Abs (deltaY));
			if (deltaX == 0) {
				xCoeff = 0;
			}
			if (deltaY == 0) {
				yCoeff = 0;
			}
					
			Vector2 current = start;
			newPath.Add (current);
			
			int moveDir; //0 indicates moving horizontally, 1 is vertical
			Vector2 nextVp;
			Vector2 nextHp;
			Vector2 nextVs;
			Vector2 nextHs;

			int loopCount = 0;

			while (current != des) {

				Vector2 cDelta = des - current;

//				Debug.Log("------------------------");
//				Debug.Log("current: " + current);
//				Debug.Log ("delta: " + cDelta );

				if(cDelta.x * xCoeff < 0){
					xCoeff = (int)(cDelta.x / Mathf.Abs (cDelta.x));
				}
				if(cDelta.y * yCoeff < 0){
					yCoeff = (int)(cDelta.y / Mathf.Abs (cDelta.y));
				}

				//get adjecent tiles
				nextHp = current + new Vector2 (1, 0) * xCoeff;
				nextVp = current + new Vector2 (0, 1) * yCoeff;
				nextHs = current + new Vector2 (1, 0) * -xCoeff;
				nextVs = current + new Vector2 (0, 1) * -yCoeff;

				bool isBlockedHp = blocked.Exists ((Vector2 item) => item == nextHp) || isOnBoard(nextHp) == false;
				bool isBlockedVp = blocked.Exists ((Vector2 item) => item == nextVp) || isOnBoard(nextVp) == false;
				bool isBlockedHs = blocked.Exists ((Vector2 item) => item == nextHs) || isOnBoard(nextHs) == false;
				bool isBlockedVs = blocked.Exists ((Vector2 item) => item == nextVs) || isOnBoard(nextVs) == false;

				Vector2[] next = new Vector2[4]{nextHp, nextVp, nextHs, nextVs};
				bool[] isBlocked = new bool[4]{isBlockedHp, isBlockedVp, isBlockedHs, isBlockedVs}; 
				Vector2 nextCurrent = current;
				float currentCost = GetDistance(current, des);

				//look amongest the adjecent tiles for the one closest to the destination
				for(int i = 0; i < 4; i++){
					float moveCost = GetDistance(next[i], des);

					if(isBlocked[i] == false && (moveCost < currentCost || nextCurrent == current)){
						nextCurrent = next[i];
					}
					currentCost = GetDistance(nextCurrent, des);

				}

				blocked.Add(current);

				if(nextCurrent != current){
					current = nextCurrent;
					newPath.Add(nextCurrent);
				}else{
					if(newPath.Count - 1 < 0){
						Debug.Log("No Path!");
						break;
					}
					current = newPath[newPath.Count - 1];
					newPath.RemoveAt(newPath.Count - 1);
				}

				loopCount++;

			}

		} else {
			board [(int)start.x, (int)start.y].type = TileType.Empty;
		}

//		Debug.Log ("Real Distance: " + GetDistance(start, des));
//		Debug.Log ("Path Count: " + newPath.Count);

		if(newPath.Count > 0){

			ChangeTiles(newPath, TileType.Empty);

//			Debug.Log ("Remove: " + newPath[newPath.Count - 1]);
//			Debug.Log ("Remove: " + newPath[0]);
//			Debug.Log ("Des: " + des);
//			Debug.Log ("Start: " + start);
			newPath.RemoveAt(newPath.Count - 1);
			newPath.RemoveAt(0);
		}

		return newPath;


	}

	void LinkPath(List<Vector2> l){

		Vector2 currentPt = l [0];
		List<Vector2> mainPath = new List<Vector2>();

		if(l.Count >= 2){

			for(int i = 1; i < l.Count; i++){

				mainPath = findPath(currentPt, l[i]);
				currentPt = l[i];
				
			}

		}

	}

	public void RandomCircuit(int MaxLength){

		Vector2 start = Vector2.zero;
		Vector2 des = Vector2.zero;

		while(GetDistance(start, des) > MaxLength || GetDistance(start, des) < 4 || start.x == des.x || start.y == des.y){
			
			start = GetRandomPoint ();
			des = GetRandomPoint ();

		}
		//start = new Vector2 (0, 0);
		//des = new Vector2 (5, 5);
		//Debug.Log ("start: " + start);
		//Debug.Log ("des: " + des);

		//findPath (start, des);
		findPath (des, start, findPath (start, des));

	}

	public void RandomCircuit(Vector2 start, int MaxLength){

		Vector2 des = Vector2.zero;
		
		while(GetDistance(start, des) > MaxLength || GetDistance(start, des) < 4 || start.x == des.x || start.y == des.y){

			des = GetRandomPoint ();
			
		}
		
		//Debug.Log ("start: " + start);
		//Debug.Log ("des: " + des);
		
		//findPath (start, des);
		findPath (des, start, findPath (start, des));

	}

	//place x number of foods on the map
	void PlaceFood(int num){

		Vector2 foodPos = GetRandomPoint();
		for(int i = 0; i < num; i++){

			while(board[(int)foodPos.x, (int)foodPos.y].type != TileType.Empty){
				foodPos = GetRandomPoint();
			}
			board[(int)foodPos.x, (int)foodPos.y].type = TileType.Food;
			//make the first y foods placed special
			if(i < numSpecFood){ 
				board[(int)foodPos.x, (int)foodPos.y].isSpec = true; 
			}
		}

	}

	public void CreateMaze(){

		List<Vector2> startList = new List<Vector2> ();
		int eObj = (int)(0.7f * (bHeight + bWidth) / 2f);
		Vector2 start;


		for(int i = 0; i < eObj; i++){

			//Debug.Log (eObj - tEmpty);
			start = GetRandomPoint ();
			RandomCircuit(start, 50);
			startList.Add(start);
			Debug.Log ("Circuit " + i + " Created");

		}
		LinkPath (startList);
		PlaceFood (numFood);

	}
	#endregion 

	public bool isOnBoard(Vector2 pIndex){

		if(pIndex.x < 0 || pIndex.x >= bWidth){
			return false;
		}
		if(pIndex.y < 0 || pIndex.y >= bHeight){
			return false;
		}
		return true;

	}

}
