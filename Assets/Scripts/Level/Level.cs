using UnityEngine;

namespace ET.Client
{
    public class Level : MonoBehaviour
    {
        public int levelId;

        public Bounds Bounds;

        public Vector2 StartPosition;
        
        
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(Bounds.center,Bounds.size); //整个关卡Bounds

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(StartPosition,0.5f); //起始点
        }
    }
}