using UnityEngine;

namespace ET.Client.Platform
{
    public enum EGizmosDrawType
    {
        SlipCheck,
        ClimbCheck
    }
    public partial class PlayerController
    {
        public void Draw(EGizmosDrawType type)
        {
            switch (type)
            {
                case EGizmosDrawType.SlipCheck:
                    DrawSlipCheck();
                    break;
                case EGizmosDrawType.ClimbCheck:
                    DrawclimbCheck();
                    break;
            }
        }

        private void DrawSlipCheck()
        {
            int direct = Facing == Facings.Right ? 1 : -1;
            {
                //上下检查是否可以下滑
                Gizmos.color = Color.blue;
                Vector2 origin = this.Position + collider.position + Vector2.up * collider.size.y / 2f + Vector2.right * direct * (collider.size.x / 2f + STEP);
                Vector2 point1 = origin + Vector2.up * (-0.4f + 0.1f);
                Gizmos.DrawWireSphere(point1,0.1f);

                Gizmos.color = Color.red;
                Vector2 point2 = origin + Vector2.up * (0.4f + 0.1f);
                Gizmos.DrawWireSphere(point2,0.1f);
            }
        }

        private void DrawclimbCheck()
        {
            
        }
    }
}