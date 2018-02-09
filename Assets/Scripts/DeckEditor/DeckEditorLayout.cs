using UnityEngine;

public class DeckEditorLayout : MonoBehaviour
{
    public const float WidthCheck = 1199f;
    public const float SelectorWidth = 177.5f;

    public Vector2 DeckButtonsPortraitPosition => new Vector2(0f, -(deckEditorLayout.rect.height + 87.5f));
    public Vector2 SortButtonPortraitPosition => new Vector2(15f, -(deckEditorLayout.rect.height + 87.5f));
    public static readonly Vector2 SortButtonLandscapePosition = new Vector2(187.5f, 0f);
    public static readonly Vector2 DeckButtonsLandscapePosition = new Vector2(-650f, 0f);
    public static readonly Vector2 SearchNamePortraitPosition = new Vector2(15f, 450f);
    public static readonly Vector2 SearchNameLandscapePosition = new Vector2(15f, 367.5f);
    public Vector2 SelectorButtonsPortraitPosition => new Vector2(GetComponent<RectTransform>().rect.width - SelectorWidth, 450f);
    public static readonly Vector2 SelectorButtonsLandscapePosition = new Vector2(675, 367.5f);

    public RectTransform sortButton;
    public RectTransform deckSelectorButtons;
    public RectTransform deckButtons;
    public RectTransform deckEditorLayout;
    public RectTransform selectorButtons;
    public RectTransform searchName;
    public SearchResults searchResults;

    void Start()
    {
        OnRectTransformDimensionsChange();
    }

    void OnRectTransformDimensionsChange()
    {
        if (!gameObject.activeInHierarchy)
            return;

        deckButtons.anchoredPosition = GetComponent<RectTransform>().rect.width < WidthCheck ? DeckButtonsPortraitPosition : DeckButtonsLandscapePosition;
        sortButton.anchoredPosition = GetComponent<RectTransform>().rect.width < WidthCheck ? SortButtonPortraitPosition : SortButtonLandscapePosition;
        deckSelectorButtons.anchoredPosition = (GetComponent<RectTransform>().rect.width < WidthCheck ? SortButtonPortraitPosition : SortButtonLandscapePosition) + new Vector2(SelectorWidth, 0);

        searchName.anchoredPosition = GetComponent<RectTransform>().rect.width < WidthCheck ? SearchNamePortraitPosition : SearchNameLandscapePosition;
        selectorButtons.anchoredPosition = GetComponent<RectTransform>().rect.width < WidthCheck ? SelectorButtonsPortraitPosition : SelectorButtonsLandscapePosition;
        searchResults.CurrentPageIndex = 0;
        searchResults.UpdateSearchResultsPanel();

        CardInfoViewer.Instance.IsVisible = false;
    }
}
