using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LerpUtility
{
    public static IEnumerator Lerp(GameObject gameObject, Vector3 targetPosition, float smoothingSpeed)
    {
        // lerp until the player is close enough to the target position
        while (Vector3.Distance(gameObject.transform.position, targetPosition) > 0.01f)
        {
            gameObject.transform.position = Vector3.Lerp(
                gameObject.transform.position,
                targetPosition,
                smoothingSpeed * Time.deltaTime
            );

            yield return null;
        }

        gameObject.transform.position = targetPosition;
    }
}
