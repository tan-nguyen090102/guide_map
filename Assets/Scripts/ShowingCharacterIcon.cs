using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowingCharacterIcon: MonoBehaviour
{
    private MapManager mapManager => MapManager.instance;

    void Start()
    {
        for (int i = 0; i < 8; i++)
        {
            Instantiate(mapManager.GetPrefab(1), parent: transform);
        }
        StartCoroutine(AnimatingIcon());
    }


    private IEnumerator AnimatingIcon()
    {
        for (int i = 1; i <= 8; i++)
        {
            yield return new WaitForSeconds(1);

            Vector3 newPosition = new Vector3(transform.position.x - i * 50, transform.position.y, transform.position.z);

            this.transform.SetLocalPositionAndRotation(newPosition, Quaternion.identity);
        }

        yield return new WaitForSeconds(1);

        Vector3 originalPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        transform.SetLocalPositionAndRotation(originalPosition, Quaternion.identity);

        yield return null;
    }
}
