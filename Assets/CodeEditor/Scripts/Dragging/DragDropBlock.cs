using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropBlock : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Canvas _canvas;
    private RectTransform _sideMenuArea;
    private Vector2 _dragOffset;
    private bool _isOverSideMenu;
    private BlockBehaviour _thisBlockBehavior;
    private CodeManager _codeManager;

    [SerializeField] private float snapThreshold = 2f; // Distance threshold for snapping

    private void Start()
    {
        // Get references from UIManager Singleton
        _canvas = UIManager.Instance.mainCanvas;
        _sideMenuArea = UIManager.Instance.sideMenuArea;
        _thisBlockBehavior = GetComponent<BlockBehaviour>();
        name = _thisBlockBehavior.BlockType.ToString();
        _codeManager = CodeManager.Instance;
        _codeManager.AddBlock(this);

        if (_canvas == null || _sideMenuArea == null)
        {
            Debug.LogError("UIManager is missing required references.");
        }

    }
    
    
    private void Awake()
    {

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Temporarily move the dragged object to the top of the canvas hierarchy
        transform.SetParent(_canvas.transform);
        transform.SetAsLastSibling();

        // Calculate the drag offset
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out _dragOffset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Move block to follow the mouse cursor
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var localPointerPosition
        );

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = localPointerPosition - _dragOffset;

        // Check if block is over the side menu
        _isOverSideMenu = RectTransformUtility.RectangleContainsScreenPoint(
            _sideMenuArea,
            eventData.position,
            eventData.pressEventCamera
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isOverSideMenu)
        {
            Destroy(gameObject);
            _codeManager.DeleteBlock(this);
            Debug.Log("Block dropped in the side menu and destroyed.");
        }
        else
        {
            HandleSnap();
        }

        Debug.Log("End Drag");
    }

    private void HandleSnap()
    {
        // Find the nearest block within the snap threshold
        DragDropBlock nearestBlock = FindNearestBlock();

        if (nearestBlock != null)
        {
            SnapToBlock(nearestBlock);
        }
        else
        {
            Debug.Log("No valid snap target found. Drag ended.");
        }
    }

    private DragDropBlock FindNearestBlock()
    {
        DragDropBlock nearestBlock = null;
        float closestDistance = snapThreshold;

        foreach (var block in FindObjectsOfType<DragDropBlock>())
        {
            if (block == this) continue;

            var otherRect = block.GetComponent<RectTransform>();
            var currentRect = GetComponent<RectTransform>();

            if (otherRect != null && currentRect != null)
            {
                float distance = Vector2.Distance(currentRect.position, otherRect.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestBlock = block;
                }
            }
        }

        if (nearestBlock != null)
        {
            Debug.Log($"Nearest block: {nearestBlock.name}, Distance: {closestDistance}");
        }
        else
        {
            Debug.Log("No nearest block found within the threshold.");
        }

        return nearestBlock;
    }
    private void SnapToBlock(DragDropBlock targetBlock)
    {
        var targetRect = targetBlock.GetComponent<RectTransform>();
        var currentRect = GetComponent<RectTransform>();

        if (targetRect != null && currentRect != null)
        {
            // Calculate the relative Y position of the current block to the target block
            float targetBottomY = targetRect.anchoredPosition.y - (targetRect.rect.height / 2); // Get the bottom of the target block
            float targetTopY = targetRect.anchoredPosition.y + (targetRect.rect.height / 2);    // Get the top of the target block
            float currentHeight = currentRect.rect.height / 2; // Half the height of the current block

            // Determine whether the dragged block is above or below the target block
            Vector2 snapPosition;
            if (currentRect.position.y > targetRect.position.y)
            {
                // Dragged block is below, so snap above the target block
                snapPosition = new Vector2(
                    targetRect.anchoredPosition.x, // Align X-axis exactly
                    targetTopY + currentHeight // Snap above target block (add current height)
                );
                _thisBlockBehavior._childBlock = targetBlock;
            }
            else
            {
                // Dragged block is above, so snap below the target block
                snapPosition = new Vector2(
                    targetRect.anchoredPosition.x, // Align X-axis exactly
                    targetBottomY - currentHeight // Snap below target block (subtract current height)
                );
                _thisBlockBehavior._parentBlock = targetBlock;
            }

            Debug.Log($"Snapping to block: {targetBlock.name}, Position: {snapPosition}");

            currentRect.anchoredPosition = snapPosition;

            // Reparent the block to maintain hierarchy (optional)
            transform.SetParent(targetBlock.transform.parent);
        }
    }
}
