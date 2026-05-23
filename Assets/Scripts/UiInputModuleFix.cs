using UnityEngine;
using UnityEngine.InputSystem.UI;

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
