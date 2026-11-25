using UnityEngine;

public class CursorMenu : MonoBehaviour
{
    private void LateUpdate()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}