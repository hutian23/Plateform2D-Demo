using UnityEngine;

namespace ET.Client.Platform
{
    public static class Logging 
    {
        public static void Log(object message)
        {
            Debug.Log(message);
        }
    }
}