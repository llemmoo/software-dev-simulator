using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;
public enum BlockType
{
    Output,
    Variable,
    IfElse
}

public class BlockBehaviour : MonoBehaviour
{
    private Image _blockImage;
    public string _blockInputFieldText;
    public string _code;
    public DragDropBlock _parentBlock;
    public DragDropBlock _childBlock;

    public BlockType BlockType { get; set; } // Determines block's behavior
    [SerializeField] private TMP_InputField _blockInputField; // Our input field
    [SerializeField] private TMP_Text _blockTextField; // Text on the block to project block behaviour

    private void Start()
    {
        _blockImage = GetComponent<Image>();
        SetDefaultBlockText();
        Debug.Log(BlockType.ToString());
    }

    // Update is called once per frame
    private void Update()
    {
        _blockInputFieldText = _blockInputField.text;
        SetInputStringToCodeBlock(_blockInputFieldText);
    }

    private void SetDefaultBlockText()
    {
        _blockTextField.text = BlockType switch
        {
            BlockType.Output => "Output",
            BlockType.Variable => "Variable",
            BlockType.IfElse => "IfElse",
            _ => _blockTextField.text
        };
    }
    private void SetInputStringToCodeBlock(string inputFieldtext)
    {
        _code = BlockType switch
        {
            BlockType.Output => "Console.WriteLine(\"" + inputFieldtext + "\");",
            BlockType.Variable => "var x = \"" + inputFieldtext + "\";",
            BlockType.IfElse => "Console.WriteLine(\"" + inputFieldtext + "\");",
            _ => _code
        };
    }
}
