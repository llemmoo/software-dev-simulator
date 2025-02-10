using UnityEngine;
using UnityEngine.EventSystems;

public class SideMenuDraggableCodeBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform _parentAfterDrag;
    private Canvas _canvas;
    private BlockBehaviour _blockBehaviour;

    private Vector2 _dragOffset;
    private Vector2 _originalPosition;

    private bool _isOverEditor; // Tracks whether the block is over the "Editor" area

    [SerializeField] private GameObject _codeBlock; // Prefab to spawn
    [SerializeField] private RectTransform _editorArea; // Reference to the "Editor" RectTransform
    [SerializeField] private BlockType _blockType; // Which block type do we want to spawn?

    private void Awake()
    {
        // Find the closest Canvas in the hierarchy
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null)
        {
            Debug.LogError("No Canvas found in parent hierarchy!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");

        // Save the current parent and original position
        _parentAfterDrag = transform.parent;
        _originalPosition = GetComponent<RectTransform>().localPosition;

        // Temporarily move the dragged object to the top of the canvas hierarchy
        transform.SetParent(_canvas.transform);
        transform.SetAsLastSibling();

        // Calculate the offset between the mouse position and the block position
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        );
        _dragOffset = localPointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");

        // Convert mouse position to Canvas local space
        Vector2 localPointerPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPointerPosition
        );

        // Apply the offset to the dragged position
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = localPointerPosition - _dragOffset;

        // Check if the block is over the editor area
        _isOverEditor = RectTransformUtility.RectangleContainsScreenPoint(
            _editorArea,
            eventData.position,
            eventData.pressEventCamera
        );

        Debug.Log($"Is over editor: {_isOverEditor}");
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");


        if (_isOverEditor)
        {
            Debug.Log("Over editor; spawning block!");
            SpawnBlock(_blockType);
        }

        // Return to the original parent
        transform.SetParent(_parentAfterDrag);

        // Snap back to the original position
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.localPosition = _originalPosition;
    }
    private void SpawnBlock(BlockType blockType)
    {
        if (_codeBlock != null)
        {
            GameObject newBlock = Instantiate(_codeBlock, _editorArea);
            var blockBehaviour = newBlock.GetComponent<BlockBehaviour>();
            blockBehaviour.BlockType = blockType;
            RectTransform newBlockRect = newBlock.GetComponent<RectTransform>();

            if (newBlockRect != null)
            {
                // Reset position, scale, and size for visibility
                newBlockRect.localPosition = new Vector3(3,3,1); // Centered
                newBlockRect.localScale = Vector3.one;
                newBlockRect.SetAsLastSibling();

                Debug.Log("New block spawned!");
            }
            else
            {
                Debug.LogError("Spawned block does not have a RectTransform! Ensure the prefab is set up correctly.");
            }
        }
        else
        {
            Debug.LogError("Block prefab not assigned!");
        }
    }
}
