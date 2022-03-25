using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeGenerator : MonoBehaviour
{
	public List<GameObject> enemiesPrefabs;
	public int hordeCount = 10;
	public int positionSpread = 15;
	public Transform startPoint;

	public GameObject generateEnemy() {
		Vector3 position = startPoint.position;
		int dx = Random.Range( -positionSpread, positionSpread);
		int dz = Random.Range( -positionSpread, positionSpread);
		position += new Vector3( dx, 5, dz );

		int enemyIndex = Random.Range(0, enemiesPrefabs.Count);
		GameObject prefab = enemiesPrefabs[enemyIndex];
		GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

		return enemy;
	}

	public List<GameObject> generate(int enemiesCount = 0) {
		int num = enemiesCount;
		if( num == 0 )
			num = hordeCount;

		List<GameObject> result = new List<GameObject>();
		for( int i=0; i<num; i++ ) {
			GameObject enemy = generateEnemy();
			result.Add( enemy );
		}
		return result;
	}
}
