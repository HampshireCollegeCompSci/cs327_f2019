using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UndoHookUp : MonoBehaviour
{
    private void OnMouseUpAsButton()
    {
        UndoScript.undoScript.undo();
    }
}
