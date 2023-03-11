using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MDebug
{
    static public void Log(string formator, object args)
    {
        Debug.Log(string.Format(formator, args));
    }

    static public void Log(string formator, params object[] args)
    {
        Debug.Log(string.Format(formator, args));
    }
}
