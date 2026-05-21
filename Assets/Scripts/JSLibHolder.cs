using UnityEngine;
using System.Runtime.InteropServices;

public class JSLibHolder
{
    [DllImport("__Internal")]
    private static extern void CopyToClipboardJS(string text);

    public static void CopyToClipboard(string text)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CopyToClipboardJS(text);
#else
        GUIUtility.systemCopyBuffer = text;
#endif
    }
}