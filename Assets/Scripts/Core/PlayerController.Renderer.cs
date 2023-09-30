using UnityEngine;

namespace ET.Client.Platform
{
    public partial class PlayerController
    {
        public float DashTrailTimer { get; set; }

        public static Vector2 NORMAL_SPRITE_SCALE = Vector2.one;
        public static Vector2 DUCK_SPRITE_SCALE = new Vector2(1f, 0.75f);

        public Color NormalHairColor = new Color32(0xAC, 0x32, 0x32, 0xFF); //00FFA2
        public Color UsedHairColor = new Color32(0x44, 0xB7, 0xFF, 0xFF);
        public Color FlashHairColor = new Color(0.6f, 0.6f, 0.6f, 1);

        public Color HairColor { get; private set; }

        private float hairFlashTimer;

        private void UpdateHair(float deltaTime)
        {
            if (dashes == 0 && dashes < Constants.MaxDashes)
            {
                HairColor = Color.Lerp(HairColor, UsedHairColor, 6f * deltaTime);
            }
            else
            {
                Color color;
                if (lastDashes != dashes)
                {
                    color = FlashHairColor;
                    hairFlashTimer = .12f;
                }
                else if (hairFlashTimer > 0)
                {
                    color = FlashHairColor;
                    hairFlashTimer -= deltaTime;
                }
                else
                {
                    color = NormalHairColor;
                }

                HairColor = color;
            }

            // SpriteControl.SetHairColor(HairColor);

            lastDashes = dashes;
        }

        public void PlayWallSlideEffect(Vector2 dir)
        {
            Vector2 origin = this.Position + Vector2.up * -0.5f;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, 0.5f, GroundMask);
            Color color = Color.white;
            if (hit && hit.collider)
            {
                // color = hit.collider.GetComponent<Ground>().GroundColor;
                // SpriteControl.WallSlide(color, dir);
            }
        }

        //播放Dash特效
        public void PlayDashEffect(Vector3 position, Vector2 dir)
        {
            // EffectControl.Ripple(position);
            // EffectControl.CameraShake(dir);
        }

        /// <summary>
        /// 像下（假设平地起跳），向下投射碰撞和，在hit位置生成dust
        /// </summary>
        public void PlayJumpEffect(Vector3 position, Vector2 forward)
        {
            SpriteControl.Scale(new Vector2(.6f, 1.4f));

            Color color = Color.white;

            RaycastHit2D hit = Physics2D.BoxCast(position, collider.size * 0.8f, 0, -forward, 0.5f, GroundMask);
            if (hit && hit.collider)
            {
                color = hit.collider.GetComponent<Ground>().GroundColor;
                // EffectControl.JumpDust(position, color, forward);
            }
        }

        public void PlayTrailEffect(int face)
        {
            SpriteControl.Trail(face);
        }

        public void PlayFallEffect(float ySpeed)
        {
            float half = Constants.MaxFall * (Constants.FastMaxFall - Constants.MaxFall) * .5f;
            if (ySpeed <= half)
            {
                float spriteLerp = Mathf.Min(1f, (ySpeed - half) / Constants.FastMaxFall - half);
                Vector2 scale = Vector2.zero;
                scale.x = Mathf.Lerp(1f, 0.5f, spriteLerp);
                scale.y = Mathf.Lerp(1f, 1.5f, spriteLerp);
                SpriteControl.Scale(scale);
            }
        }

        public void PlayLandEffect(Vector3 position, float ySpeed)
        {
            float squish = Mathf.Min(ySpeed / Mathf.Abs(Constants.FastMaxFall), 1);
            float scaleX = Mathf.Lerp(1, 1.6f, squish);
            float scaleY = Mathf.Lerp(1, 0.4f, squish);
            SpriteControl.Scale(new Vector2(scaleX, scaleY));

            RaycastHit2D hit = Physics2D.BoxCast(position, collider.size * 0.8f, 0, Vector2.down, 0, GroundMask);
            Color color = Color.white;
            if (hit && hit.collider)
            {
                color = hit.collider.GetComponent<Ground>().GroundColor;
                // EffectControl.LandDust(Vector3.positiveInfinity, color);
            }
        }

        public void PlayerDashFluxEffect(Vector3 dir, bool enable)
        {
            SpriteControl.DashFlux(dir, enable);
        }

        public void PlayDuck(bool enable)
        {
            // if (enable)
            // {
            //     SpriteControl.Scale(new Vector2(1.4f, .6f));
            //     SpriteControl.SetSpriteScale(DUCK_SPRITE_SCALE);
            // }
            // else
            // {
            //     if (this.OnGround && MoveY != 1)
            //     {
            //         SpriteControl.Scale(new Vector2(.8f, 1.2f));
            //     }
            //
            //     SpriteControl.SetSpriteScale(NORMAL_SPRITE_SCALE);
            // }
        }
        
        public Vector3 SpritePosition
        {
            get => this.Position + Vector2.left * 0.6f;
        }
        
        public Vector2 RightPosition
        {
            get => this.Position + Vector2.right * 0.6f;
        }

        public Vector2 LeftPosition
        {
            get => this.Position + Vector2.left * 0.6f;
        }
    }
}