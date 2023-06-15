using UnityEngine;
using System.Collections;

public class Head : MonoBehaviour {

	public GameObject game;
	Snake snakeScript;

	// Use this for initialization
	void Start () {
	
		gameObject.AddComponent<Rigidbody2D> ();
		gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
		snakeScript = game.GetComponent<Snake>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D other){

		Debug.Log ("Collision with " + other.tag);

		if (other.tag == "Food") {

			other.tag = "Node";
			snakeScript.EatFood(other.gameObject);

		}else if(other.tag == "Node"){

			int index = snakeScript.nodes.IndexOf(other.gameObject);
			snakeScript.BreakFrom(index);

		} else {

			//Collide with wall, Game Over!
			game.GetComponent<GameManager>().EndGame();
			//game.GetComponent<GameManager>().gameState = GameState.Ended;
			//snakeScript.speed = 0;

		}

	}
}
