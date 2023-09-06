using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    private const float DEFAULT_TRANSITION_SPEED = 3f;
    private const float DEFAULT_SHOWING_SPEED_MULTIPLIER = 2f;
    private const float DEFAULT_DELAY_INPUT_TIME = 0.1f;
    private const float DEFAULT_Y_INFO_POSITION = 1.5f;
    public static MapManager instance { get; private set; }
    [SerializeField] private List<Button> buttons = new List<Button>();
    [SerializeField] private List<GameObject> info = new List<GameObject>();
    [SerializeField] private List<GameObject> pointOfInterestInfo = new List<GameObject>();
    [SerializeField] private List<CanvasGroup> canvasGroups = new List<CanvasGroup>();
    [SerializeField] private List<GameObject> prefabsToSpawn = new List<GameObject>();

    private List<GameObject> infoBoxList = new List<GameObject>();
    private List<string> pointNames = new List<string>();
    private CanvasGroup currentCG;
    private Image currentImageDisplaying;
    private Transform currentInfoParent;
    private Transform currentPointsOfInterest;
    private ShowingCharacterIcon showingIcon;

    private Coroutine co_fadingIn = null;
    private Coroutine co_fadingOut = null;
    public bool isOnButton { get; private set; } = false;
    public bool isFadingIn => co_fadingIn != null;
    public bool isFadingOut => co_fadingOut != null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Initialize();
        }   
        else
            DestroyImmediate(gameObject);
    }

    private void Initialize()
    {
        currentCG = canvasGroups[0];
        currentInfoParent = info[0].transform;
        currentPointsOfInterest = pointOfInterestInfo[0].transform;

        for (int i = 1; i < canvasGroups.Count; i++)
        {
            canvasGroups[i].gameObject.SetActive(false);
        }

        //Instantiate all info prefabs onto the scene
        StartInstantiatingPrefab();
    }

    private string GetGameObjectOnHovering()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0)
            {
                foreach (var go in raycastResults)
                {
                    return go.gameObject.name;
                }
            }
        }

        return null;
    }

    private void DisableAllButton()
    {
        foreach(Button button in buttons)
        {
            button.GetComponent<EventTrigger>().enabled = false;
        }
    }

    private void EnableAllButton()
    {
        foreach (Button button in buttons)
        {
            button.GetComponent<EventTrigger>().enabled = true;
        }
    }

    public GameObject GetPrefab(int index)
    {
        return prefabsToSpawn[index];
    }

    private void StartInstantiatingPrefab()
    {
        int infoCount = currentPointsOfInterest.transform.childCount;
        GameObject currentPrefab = prefabsToSpawn[0];
        Transform currentParent = currentInfoParent;

        foreach (var button in currentPointsOfInterest.GetComponentsInChildren<Button>())
        {
            pointNames.Add(button.name);
        }

        SpawnInfoBox(infoCount, currentPrefab, currentParent);
        AssignInfoBox();
    }

    private void ClearPrefabs()
    {
        foreach (Transform transform in currentInfoParent.transform)
        {
            Destroy(transform.gameObject);
        }

        currentPointsOfInterest = null;
        currentInfoParent = null;
        pointNames.Clear();
    }

    private void SpawnInfoBox(int infoCount, GameObject currentPrefab, Transform currentParent)
    {
        infoBoxList.Clear();
        List<Button> currentButtonList = currentPointsOfInterest.gameObject.GetComponentsInChildren<Button>().ToList();

        for (int i = 0; i < infoCount; i++)
        {
            Vector2 position = new Vector2(currentButtonList[i].transform.position.x, currentButtonList[i].transform.position.y + DEFAULT_Y_INFO_POSITION);
            GameObject go = Instantiate(currentPrefab, position, Quaternion.identity, parent: currentParent);
            go.name = pointNames[i];
            infoBoxList.Add(go);
        }

        currentButtonList.Clear();
    }

    private void AssignInfoBox()
    {
        foreach (var prefab in infoBoxList)
        {
            TextMeshProUGUI titleText = prefab.GetComponentInChildren<TextMeshProUGUI>();
            titleText.text = prefab.name;
        }
    }

    public void OnMouseOver()
    {
        Invoke("OnMouseHover", DEFAULT_DELAY_INPUT_TIME);
    }

    private void OnMouseHover()
    {
        Button hoveringButton = GetButton(GetGameObjectOnHovering());
        if (hoveringButton != null)
        {
            Image displayImage = GetInfo(hoveringButton);
            if (displayImage != null)
                StartCoroutine(ShowInfo(displayImage.GetComponent<CanvasGroup>()));
        }
        isOnButton = true;
    }

    public void OnMouseExit()
    {
        if (currentImageDisplaying != null)
        {
            StartCoroutine(HideInfo(currentImageDisplaying.GetComponent<CanvasGroup>()));
        }
        isOnButton = false;
    }

    private Image GetInfo(Button hoverButton)
    {
        List<Image> infoImages = currentCG.transform.GetChild(0).transform.GetChild(2).GetComponentsInChildren<Image>().ToList();

        foreach (Image image in infoImages)
        {
            if (image.name == hoverButton.name)
                return image; 
        }

        return null;
    }

    public Button GetButton(string name)
    {
        foreach (var button in buttons)
        {
            if (button.name == name)
                return button;
        }

        return null;
    }

    public CanvasGroup GetCanvasGroup(string name)
    {
        foreach(var group in canvasGroups)
        {
            if (group.name == name) 
                return group;
        }

        return null;
    }

    public void OnClickTransitioning()
    {
        Button triggeredButton = GetButton(EventSystem.current.currentSelectedGameObject.name);
        
        switch (triggeredButton.name)
        {
            case "Castle":
                Debug.Log("Click on castle");
                StartCoroutine(TransitioningMap(currentCG, canvasGroups[1], 1));
                break;
            case "Dungeon 1":
                Debug.Log("Click on dungeon 1");
                StartCoroutine(TransitioningMap(currentCG, canvasGroups[2], 2));
                break;
            case "Dungeon 2":
                Debug.Log("Click on dungeon 2");
                StartCoroutine(TransitioningMap(currentCG, canvasGroups[3], 3));
                break;
        }
    }

    public void OnClickBack()
    {
        Debug.Log("Back to main map");
        StartCoroutine(TransitioningMap(currentCG, canvasGroups[0], 0));
    }

    private IEnumerator ShowInfo(CanvasGroup targetCG)
    {
        if (co_fadingOut != null)
            ForceCompleteTransition(currentImageDisplaying.GetComponent<CanvasGroup>(), isFadingOut: true);
        if (targetCG.alpha != 1f)
        {
            currentImageDisplaying = targetCG.GetComponentInParent<Image>();
            yield return FadeInImage(targetCG, DEFAULT_SHOWING_SPEED_MULTIPLIER);
        }
    }

    private IEnumerator HideInfo(CanvasGroup targetCG)
    {
        if (co_fadingIn != null)
            StopCoroutine(co_fadingIn);
        if (targetCG.alpha != 0f)
        {
            yield return FadeOutImage(targetCG, DEFAULT_SHOWING_SPEED_MULTIPLIER);
        }
    }

    private IEnumerator TransitioningMap(CanvasGroup currentCG, CanvasGroup targetCG, int index)
    {
        DisableAllButton(); 

        if (currentImageDisplaying != null)
        {
            CanvasGroup infoCG = currentImageDisplaying.GetComponent<CanvasGroup>();
            if (infoCG.alpha != 0f)
            {
                if (co_fadingIn != null)
                {
                    StopCoroutine(co_fadingIn);
                    co_fadingIn = null;
                }
                yield return HideInfo(infoCG);
            }
        }      

        ClearPrefabs();

        yield return FadeOutImage(currentCG);
        currentCG.gameObject.SetActive(false);
        targetCG.gameObject.SetActive(true);
        yield return FadeInImage(targetCG);
        this.currentCG = targetCG;
        this.currentInfoParent = info[index].transform;
        this.currentPointsOfInterest = pointOfInterestInfo[index].transform;

        EnableAllButton();
        StartInstantiatingPrefab();
    } 

    public Coroutine FadeInImage(CanvasGroup targetGroup, float speed = 1f)
    {
        if (isFadingIn)
            return co_fadingIn;
        else if (isFadingOut)
        {
            StopCoroutine(co_fadingOut);
            co_fadingOut = null;
        }

        co_fadingIn = StartCoroutine(FadingImage(1f, targetGroup, speed));

        return co_fadingIn;
    }

    public Coroutine FadeOutImage(CanvasGroup targetGroup, float speed = 1f)
    {
        if (isFadingOut)
            return co_fadingOut;
        else if (isFadingIn)
        {
            StopCoroutine(co_fadingIn);
            co_fadingIn = null;
        }

        co_fadingOut = StartCoroutine(FadingImage(0f, targetGroup, speed));

        return co_fadingOut;
    }

    private IEnumerator FadingImage(float targetAlpha, CanvasGroup currentCG, float speed)
    {
        while (currentCG.alpha != targetAlpha)
        {
            currentCG.alpha = Mathf.MoveTowards(currentCG.alpha, targetAlpha, speed * DEFAULT_TRANSITION_SPEED * Time.deltaTime);
            yield return null;
        }

        co_fadingIn = null;
        co_fadingOut = null;
    }

    private void ForceCompleteTransition(CanvasGroup targetCG, bool isFadingOut = true)
    {
        if (isFadingOut)
            targetCG.alpha = 0f; 
        else
            targetCG.alpha = 1f;

        co_fadingIn = null;
        co_fadingOut = null;
    }
}
