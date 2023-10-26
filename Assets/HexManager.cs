using System;
using TMPro;
using UnityEngine;

internal class HexManager : MonoBehaviour
{
    private TextMeshPro text;
    internal int x;
    internal int y;

    internal void UpdateLabel(int i, int j)
    {
        x = i;
        y = j;
        text = GetComponentInChildren<TextMeshPro>();
        text.text = $"{x},{y}";
    }
}