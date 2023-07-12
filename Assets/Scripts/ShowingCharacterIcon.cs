using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowingCharacterIcon: MonoBehaviour
{
    private const int DEFAULT_INITIAL_ICON_SHOW = 4;
    private const int DEFAULT_SLOT_MOVING = 1;
    private const float DEFAULT_WAITING_TIME = 1f;

    [SerializeField] private int iconCount = 0;
    
    private MapManager mapManager => MapManager.instance;
    private RectTransform root;
    private Coroutine co_moving = null;
    private Coroutine co_repeating = null;
    private Vector2 initialPosition;
    private bool isMoving => co_moving != null;
    private bool isRepeating => co_repeating != null;

    void Start()
    {
        for (int i = 0; i < iconCount; i++)
        {
            Instantiate(mapManager.GetPrefab(1), parent: transform);
        }

        //Root is the Character Panel RectTransform
        root = GetComponent<RectTransform>();
        initialPosition = new Vector2(root.transform.position.x, root.transform.position.y);
    }

    private IEnumerator RepeatAnimatingIcon()
    {
        yield return new WaitForSeconds(DEFAULT_WAITING_TIME);

        for (int i = 1; i <= iconCount - DEFAULT_INITIAL_ICON_SHOW; i++)
        {
            Vector2 targetPos = new Vector2(root.transform.position.x - DEFAULT_SLOT_MOVING, root.transform.position.y);
            AnimateIcon(targetPos, isSmooth: false);

            yield return new WaitForSeconds(DEFAULT_WAITING_TIME);
        }           

        yield return new WaitForSeconds(DEFAULT_WAITING_TIME);
        AnimateIcon(initialPosition, speed: 50f, isSmooth: false);
        yield return new WaitForEndOfFrame();

        co_repeating = null;

        Update();
    }

    public Coroutine AnimateIcon(Vector2 position, float speed = 5f, bool isSmooth = false)
    {
        if (isMoving)
            StopCoroutine(co_moving);

        co_moving = StartCoroutine(AnimatingIcon(position, speed, isSmooth));

        return co_moving;
    }

    private IEnumerator AnimatingIcon(Vector2 targetPosition, float speed, bool isSmooth)
    {
        if (root == null)
            yield return null;

        while (root.transform.position.x != targetPosition.x)
        {
            Vector2 newPosition = isSmooth ?
                Vector2.Lerp(root.transform.position, targetPosition, speed * Time.deltaTime)
                : Vector2.MoveTowards(root.transform.position, targetPosition, speed * Time.deltaTime * 0.35f);

            root.transform.position = newPosition;

            if (isSmooth && Vector2.Distance(root.transform.position, targetPosition) <= 0.001f)
            {
                root.transform.SetLocalPositionAndRotation(targetPosition, Quaternion.identity);
                break;
            }

            yield return null;
        }
        Debug.Log($"Done moving to {root.transform.position}");
        co_moving = null;
    }

    public void SetOriginalPosition()
    {
        root.transform.position = initialPosition;
    }

    void Update()
    {
        if (mapManager.isOnButton)
        {
            if (isRepeating)
                return;

            if (co_repeating == null && iconCount > DEFAULT_INITIAL_ICON_SHOW)
                co_repeating = StartCoroutine(RepeatAnimatingIcon());

        }
        else
        {
            if (isRepeating)
            {
                StopCoroutine(co_repeating);
                if (isMoving)
                    StopCoroutine(co_moving);
                co_repeating = null;
                co_moving = null;
                SetOriginalPosition();
            }
        }
    }
}
