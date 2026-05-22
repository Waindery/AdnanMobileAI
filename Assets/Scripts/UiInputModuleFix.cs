using UnityEngine;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Ensures UI clicks work with the Input System package (project uses Input System only).
/// </summary>
[DefaultExecutionOrder(-1000)]
public class UiInputModuleFix : MonoBehaviour
{
    private void Awake()
    {
        InputSystemUIInputModule module = GetComponent<InputSystemUIInputModule>();
        if (module == null)
            return;

        module.AssignDefaultActions();
        module.enabled = true;
    }
}
