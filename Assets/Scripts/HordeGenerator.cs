using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeGenerator : MonoBehaviour
{
	public List<GameObject> enemiesPrefabs;
	public int hordeCount = 10;
	public int positionSpread = 15;

    void Start()
    {
        generate(10);
    }

	public GameObject generateEnemy() {
		Vector3 position = transform.position;
		int dx = Random.Range( -positionSpread, positionSpread);
		int dz = Random.Range( -positionSpread, positionSpread);
		position += new Vector3( dx, 5, dz );

		int enemyIndex = Random.Range(0, enemiesPrefabs.Count);
		GameObject prefab = enemiesPrefabs[enemyIndex];
		GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
		return enemy;
	}

	public void generate(int num) {
		for( int i=0; i<num; i++ ) {
			GameObject enemy = generateEnemy();
		}
	}
}
