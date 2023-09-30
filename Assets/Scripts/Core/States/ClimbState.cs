using System;
using System.Collections;
using UnityEngine;

namespace ET.Client.Platform
{
    public class ClimbState : BaseActionState
    {
        public ClimbState( PlayerController context) : base(EActionState.Climb, context)
        {
        }
        
        public override IEnumerator Coroutine()
        {
            yield return null;
        }

        public override void OnBegin()
        {
            ctx.Speed.x = 0;
            ctx.Speed.y *= Constants.ClimbGrabYMult;
            ctx.WallSlideTimer = Constants.WallSlideTime;
            ctx.WallBoost?.ResetTime();
            ctx.ClimbNoMoveTimer = Constants.ClimbNoMoveTime;
            
            //两个像素的吸附功能
            ctx.ClimbSnap();
        }

        public override void OnEnd()
        {
        }

        public override EActionState Update(float deltaTime)
        {
            ctx.ClimbNoMoveTimer -= deltaTime;
            //处理跳跃
            if (GameInput.Jump.Pressed() && (!ctx.Ducking || ctx.CanUnDuck))
            {
                if (ctx.MoveX == -(int)ctx.Facing)
                {
                    ctx.WallJump(-(int)ctx.Facing);
                }
                else
                {
                    ctx.ClimbJump();
                }
            }
            if (ctx.CanDash)
            {
                return this.ctx.Dash();
            }
            //放开抓取建,则回到normal状态
            if (!GameInput.Grab.Checked())
            {
                return EActionState.Normal;
            }
            //检测前面的墙面是否存在
            if (!ctx.CollideCheck(ctx.Position, Vector2.right * (int)ctx.Facing))
            {
                // if (ctx.Speed.y < 0)
                // {
                //     ClimbHop(); // 自动翻越墙面
                // }
                return EActionState.Normal;
            }

            {
                //Climbing
                float target = 0;
                bool trySlip = false;
                if (ctx.ClimbNoMoveTimer <= 0)
                {
                    if (ctx.MoveY == 1)
                    {
                        //往上爬
                        target = Constants.ClimbUpSpeed;
                        //向上攀爬的移动限制，顶上有碰撞或者SlipCheck
                        if (ctx.CollideCheck(ctx.Position, Vector2.up))
                        {
                            Debug.Log("=======ClimbSlip_Type1");
                            ctx.Speed.y = Mathf.Min(ctx.Speed.y, 0);
                            target = 0;
                            trySlip = true;
                        }
                        //如果在上面0.6米处存在障碍，且前上方0.1米处没有阻挡，仍然不允许向上
                        else if(ctx.ClimbHopBlockedCheck() && ctx.SlipCheck(0.1f))
                        {
                            Debug.Log("=====ClimbSlip_Type2");
                            ctx.Speed.y = Mathf.Min(ctx.Speed.y, 0);
                            target = 0;
                            trySlip = true;
                        }
                        //如果前上方没有阻挡，则进行ClimbHop
                        else if (ctx.SlipCheck())
                        {
                            //Hopping
                            ClimbHop();
                            return EActionState.Normal;
                        }
                    }
                    else if (ctx.MoveY == -1)
                    {
                        //往上爬
                        target = Constants.ClimbDownSpeed;

                        if (ctx.OnGround)
                        {
                            ctx.Speed.y = Mathf.Max(ctx.Speed.y, 0); // 落地时，Y轴速度>=0
                            target = 0;
                        }
                        else
                        {
                            //创建WallSlide粒子效果
                            ctx.PlayWallSlideEffect(Vector2.right*(int)ctx.Facing);
                        }
                    }
                    else
                    {
                        trySlip = true;
                    }
                }
                else
                {
                    trySlip = true;
                }
                
                //滑行
                if (trySlip && ctx.SlipCheck())
                {
                    Debug.Log("=======ClimbSlip_Type4");
                    target = Constants.ClimbSlipSpeed;
                }
                ctx.Speed.y = Mathf.MoveTowards(ctx.Speed.y, target, Constants.ClimbAccel * deltaTime);
            }
            //TrySlip导致下滑在碰到底部的时候 停止下滑
            if (ctx.MoveY != -1 && ctx.Speed.y < 0 && !ctx.CollideCheck(ctx.Position, new Vector2((int)ctx.Facing, -1)))
            {
                ctx.Speed.y = 0;
            }
            return state;
        }

        public void ClimbHop()
        {
            Debug.Log("======ClimbHop");
            
            //获取目标的落脚点
            bool hit = ctx.CollideCheck(ctx.Position, Vector2.right * (int)ctx.Facing);
            if (hit)
            {
                ctx.HopWaitX = (int)ctx.Facing;
                ctx.HopWaitXSpeed = (int)ctx.Facing * Constants.ClimbHopX;
            }
            else
            {
                ctx.HopWaitX = 0;
                ctx.Speed.x = (int)ctx.Facing * Constants.ClimbHopX;
            }
            ctx.Speed.y = Math.Max(ctx.Speed.y, Constants.ClimbHopY);
            ctx.ForceMoveX = 0;
            ctx.ForceMoveXTimer = Constants.ClimbHopForceTime;
        }
        
        public override bool IsCoroutine()
        {
            return false;
        }
    }
}