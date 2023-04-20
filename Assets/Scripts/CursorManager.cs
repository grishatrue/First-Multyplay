using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static bool isCursorVisibleStatic = true;

    [SerializeField] private KeyCode toggleButton = KeyCode.Escape;

    private void Awake()
    {
        SwitchCursorState(Cursor.visible);
    }

    private void Update()
    {
        ToggleCursor();
    }

    private void ToggleCursor()
    {
        if (Input.GetKeyDown(toggleButton))
        {
            SwitchCursorState(!isCursorVisibleStatic);
        }
    }

    public static void SwitchCursorState(bool b)
    {
        if (b)
        {
            isCursorVisibleStatic = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            isCursorVisibleStatic = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}