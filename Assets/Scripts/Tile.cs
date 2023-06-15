using UnityEngine;
using System.Collections;

public class Tile {

	public TileType type = TileType.Block; //variable to indicate the type of the tile
	public bool isSpec = false; //variable to mark this tile as special

	public Tile( TileType t){

		type = t;

	}

}

public enum TileType{

	Empty, Block, Start, End, Food,

};
