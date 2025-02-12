using UnityEngine;

public class PlayButtonScript : MonoBehaviour
{
    private CodeManager _codeManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _codeManager = CodeManager.Instance;
    }

    private void PlayCode()
    {
        _codeManager.RunCode();
    }


    void OnMouseDown()
    {
        Debug.Log("Play Button clicked");
        PlayCode();
    }
    
    
    
}
