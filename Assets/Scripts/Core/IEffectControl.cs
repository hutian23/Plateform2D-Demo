using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 特效控制器，用于和外部实现解耦
    /// </summary>
    public interface IEffectControl
    {
        void DashLine(Vector3 position, Vector2 dir);

        //凌波微步特效
        void Ripple(Vector3 position);

        void CameraShake(Vector2 dir);
        
        //起跳灰尘
        void JumpDust(Vector2 position, Color color, Vector2 dir);
        
        void LandDust(Vector3 position, Color color);

        void SpeedRing(Vector3 position, Vector2 dir);
        //顿帧
        void Freeze(float time);
    }
}