using UnityEngine;

namespace ET.Client.Platform
{
    /// <summary>
    /// 这里是unity下实现玩家表现的接口
    /// </summary>
    public class PlayerRenderer : MonoBehaviour , ISpriteControl
    {
        [SerializeField] public SpriteRenderer spriteRenderer;

        [SerializeField] public ParticleSystem vfxDashFlux;

        [SerializeField] public ParticleSystem vfxWallSlide;

        [SerializeField] public TrailRenderer hair;

        [SerializeField] public SpriteRenderer hairSprite01;

        [SerializeField] public SpriteRenderer hairSprite02;

        private Vector2 scale;
        private Vector2 currSpriteScale;
        
        public Vector3 SpritePosition
        {
            get => this.spriteRenderer.transform.position;
        }

        public void Reload()
        {
            
        }

        public void Render(float deltaTime)
        {
            float tempScaleX = Mathf.MoveTowards(scale.x, currSpriteScale.x, 1.75f * deltaTime);
            float tempScaleY = Mathf.MoveTowards(scale.y, currSpriteScale.y, 1.75f * deltaTime);
            this.scale = new Vector2(tempScaleX, tempScaleY);
            this.spriteRenderer.transform.localScale = scale;
        }
        
        public void Trail(int face)
        {
            // SceneEffectManager.Instance.Add(this.spriteRenderer, face, Color.white);
        }

        public void Scale(Vector2 localScale)
        {
            this.scale = localScale;
        }

        public void SetSpriteScale(Vector2 localScale)
        {
            this.currSpriteScale = localScale;
        }
        
        public void Slash(bool enable)
        {
            throw new System.NotImplementedException();
        }

        public void DashFlux(Vector2 dir, bool play)
        {
            if (play)
            {
                this.vfxDashFlux.transform.rotation = Quaternion.FromToRotation(Vector2.up,dir);
                this.vfxDashFlux.Play();
            }
            else
            {
                this.vfxDashFlux.transform.parent = this.transform;
                this.vfxDashFlux.Stop();
            }
        }

        public void SetHairColor(Color color)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(Color.black, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1, 0.0f), new GradientAlphaKey(1, 0.6f), new GradientAlphaKey(0, 1.0f) }
            );
            this.hair.colorGradient = gradient;
            this.hairSprite01.color = color;
            this.hairSprite02.color = color;
        }

        public void WallSlide(Color color, Vector2 dir)
        {
            //将向量从上方方向旋转到给定的方向 dir
            this.vfxWallSlide.transform.rotation = Quaternion.FromToRotation(Vector2.up,dir);
            var main = this.vfxWallSlide.main;
            main.startColor = color;
            this.vfxWallSlide.Emit(1);
        }
    }
}