using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 精灵控制器，用于和外部实现解耦
    /// </summary>
    public interface ISpriteControl
    {
        void Trail(int face);

        void Scale(Vector2 localScale);

        void SetSpriteScale(Vector2 localScale);
        
        Vector3 SpritePosition { get; }

        void Slash(bool enable);

        void DashFlux(Vector2 dir, bool enable);
        //设置头发颜色
        void SetHairColor(Color color);
        //爬墙下滑，生成灰尘
        void WallSlide(Color color, Vector2 dir);
    }
}