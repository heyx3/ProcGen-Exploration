using System.Diagnostics;

public static class Assert
{
    [Conditional("UNITY_EDITOR")]
    public static void IsTrue(bool expr, string str = "")
    {
        if (!expr)
        {
            UnityEngine.Debug.LogError("Assert failed" +
                                       (str.Length > 0 ? (": " + str) : ""));
            UnityEngine.Debug.Break();
        }
    }
}