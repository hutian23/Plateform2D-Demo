using System;
using UnityEngine;

namespace ET.Client.Platform
{
    public partial class PlayerController
    {
        private readonly int GroundMask;

        protected float varJumpTimer;
        protected float varJumpSpeed;
        protected int moveX;
        private float maxFall;
        private float fastMaxFall;

        private float dashCooldownTimer; //冲刺冷却时间计时器，为0时，可以再次冲刺

        // If you hit a wall, start this timer. If coast is clear within this timer, retain h-speed 
        private float dashRefillCooldownTimer; //如果碰撞到枪毙了，启动这个定时器，如果定时器内没碰到墙壁，就保持水平速度
        public int dashes;
        public int lastDashes;
        private float wallSpeedRetentionTimer;
        private float wallSpeedRetained;

        private bool onGround;
        private bool wasOnGround;

        public bool DashStartedOnGround { get; set; }

        public int ForceMoveX { get; set; }
        public float ForceMoveXTimer { get; set; }

        //如果你爬到一个移动的固体上，请修正到它旁边，直到你到达它上方
        public int HopWaitX; //If you climb hop onto a moving solid, snap to beside it until you get above it
        public float HopWaitXSpeed;

        public bool launched;
        public float launchedTimer;

        public float WallSlideTimer { get; set; } = Constants.WallSlideTime;
        public int WallSlideDir { get; set; }

        public JumpCheck JumpCheck { get; set; } //土狼时间
        public WallBoost WallBoost { get; set; }

        public FiniteStateMachine<BaseActionState> stateMachine;
        public ISpriteControl SpriteControl { get; private set; }

        //特效控制器
        public IEffectControl EffectControl { get; private set; }

        //音频控制器
        public ISoundControl SoundControl { get; private set; }
        public ICamera camera { get; private set; }

        public PlayerController(ISpriteControl spriteControl, IEffectControl effectControl)
        {
            this.SpriteControl = spriteControl;
            this.EffectControl = effectControl;

            this.stateMachine = new FiniteStateMachine<BaseActionState>((int)EActionState.Size);
            this.stateMachine.AddState(new NormalState(this));
            this.stateMachine.AddState(new DashState(this));
            this.stateMachine.AddState(new ClimbState(this));
            this.GroundMask = LayerMask.GetMask("Ground");

            this.Facing = Facings.Right;
            this.LastAim = Vector2.right;
        }

        public void RefreshAbility()
        {
            this.JumpCheck = new JumpCheck(this, Constants.EnableJumpGrace);

            if (!Constants.EnableWallBoost)
            {
                this.WallBoost = null;
            }
            else
            {
                this.WallBoost = this.WallBoost == null ? new WallBoost(this) : this.WallBoost;
            }
        }

        public void Init(Bounds bounds, Vector2 startPosition)
        {
            //根据进入的方式，决定初始状态
            this.stateMachine.State = (int)EActionState.Normal;
            this.lastDashes = this.dashes = 1;
            this.Position = startPosition;
            this.collider = normalHitbox;

            this.SpriteControl.SetSpriteScale(NORMAL_SPRITE_SCALE);

            this.bounds = bounds;
            this.cameraPosition = CameraTarget;
        }

        public void Update(float deltaTime)
        {
            //更新各个组件中变量的状态
            {
                //Get Ground
                wasOnGround = onGround;
                if (Speed.y <= 0)
                {
                    this.onGround = CheckGround();
                }
                else
                {
                    this.onGround = false;
                }

                //Wall Slide
                if (this.WallSlideDir != 0)
                {
                    this.WallSlideTimer = Math.Max(this.WallSlideTimer - deltaTime, 0);
                    this.WallSlideDir = 0;
                }

                if (this.onGround && this.stateMachine.State != (int)EActionState.Climb)
                {
                    this.WallSlideTimer = Constants.WallSlideTime;
                }

                //WallBoost ， 不消耗体力WallJump
                this.WallBoost?.Update(deltaTime);

                //跳跃检查
                this.JumpCheck?.Update(deltaTime);

                //Dash
                {
                    if (dashCooldownTimer > 0)
                        dashCooldownTimer -= deltaTime;
                    if (dashRefillCooldownTimer > 0)
                    {
                        dashRefillCooldownTimer -= deltaTime;
                    }
                    else if (onGround)
                    {
                        RefillDash();
                    }
                }

                //Var Jump
                if (varJumpTimer > 0)
                {
                    varJumpTimer -= deltaTime;
                }

                //Force Move X
                if (ForceMoveXTimer > 0)
                {
                    ForceMoveXTimer -= deltaTime;
                    this.moveX = ForceMoveX;
                }
                else
                {
                    //输入
                    this.moveX = Math.Sign(Input.GetAxisRaw("Horizontal"));
                }

                //Facing
                if (moveX != 0 && this.stateMachine.State != (int)EActionState.Climb)
                {
                    Facing = (Facings)moveX;
                }
                //Aiming
                LastAim = GameInput.GetAimVector(Facing);

                //撞墙以后 速度保持， Wall Speed retention用于撞开
                if (wallSpeedRetentionTimer > 0)
                {
                    //WallSpeed保留了撞墙时的速度方向
                    if (Math.Sign(Speed.x) == -Math.Sign(wallSpeedRetained))
                    {
                        wallSpeedRetentionTimer = 0;
                    }
                    else if (!CollideCheck(Position, Vector2.right * Math.Sign(wallSpeedRetained)))
                    {
                        Speed.x = wallSpeedRetained;
                        wallSpeedRetentionTimer = 0;
                    }
                    else
                    {
                        wallSpeedRetentionTimer -= deltaTime;
                    }
                }

                //Hop Wait X
                if (this.HopWaitX != 0)
                {
                    if (Math.Sign(Speed.x) == -HopWaitX || Speed.y < 0)
                    {
                        this.HopWaitX = 0;
                    }
                    else if (!CollideCheck(Position, Vector2.right * this.HopWaitX))
                    {
                        Speed.x = this.HopWaitXSpeed;
                        this.HopWaitX = 0;
                    }
                }

                //launch particles
                if (launched)
                {
                    var sq = Speed.SqrMagnitude();
                    if (sq < Constants.LaunchedMinSpeedSq)
                        launched = false;
                    else
                    {
                        var was = launchedTimer;
                        launchedTimer += deltaTime;

                        if (launchedTimer >= .5f)
                        {
                            launched = false;
                            launchedTimer = 0;
                        }
                        else if (Calc.OnInterval(launchedTimer, was, 0.15f))
                        {
                            // EffectControl.SpeedRing(this.Position, this.Speed.normalized);
                        }
                    }
                }
                else
                {
                    launchedTimer = 0;
                }
            }

            //状态机更新逻辑
            stateMachine.Update(deltaTime);
            //更新位置
            UpdateCollideX(Speed.x * deltaTime);
            UpdateCollideY(Speed.y * deltaTime);

            UpdateHair(deltaTime);

            UpdateCamera(deltaTime);
        }

        //处理跳跃，跳跃时候，会给跳跃前方一个额外的速度
        public void Jump()
        {
            GameInput.Jump.ConsumeBuffer();
            this.JumpCheck?.ResetTime();
            this.WallSlideTimer = Constants.WallSlideTime;
            this.WallBoost?.ResetTime();
            this.varJumpTimer = Constants.VarJumpTime;
            this.Speed.x += Constants.JumpHBoost * moveX;
            this.Speed.y = Constants.JumpSpeed;
            this.varJumpSpeed = this.Speed.y;

            this.PlayJumpEffect(SpritePosition, Vector2.up);
        }

        //SuperJump，表示在地面上或者土狼时间内，Dash接跳跃。
        //数值方便和Jump类似，数值变大。
        //蹲伏状态的SuperJump需要额外处理。
        //Dash->Jump->Dush
        public void SuperJump()
        {
            GameInput.Jump.ConsumeBuffer();
            this.JumpCheck?.ResetTime();
            varJumpTimer = Constants.VarJumpTime;
            this.WallSlideTimer = Constants.WallSlideTime;
            this.WallBoost?.ResetTime();

            this.Speed.x = Constants.SuperJumpH * (int)Facing;
            this.Speed.y = Constants.JumpSpeed;

            if (Ducking)
            {
                Ducking = false;
                this.Speed.x *= Constants.DuckSuperJumpXMult;
                this.Speed.y *= Constants.DuckSuperJumpYMult;
            }

            varJumpSpeed = Speed.y;
            launched = true; //使用SuperJump?

            this.PlayJumpEffect(this.SpritePosition, Vector2.up);
        }

        //在墙边情况下的跳跃，主要需要考虑当前跳跃朝向
        public void WallJump(int dir)
        {
            GameInput.Jump.ConsumeBuffer();
            Ducking = false;
            this.JumpCheck?.ResetTime();
            varJumpTimer = Constants.VarJumpTime;
            this.WallSlideTimer = Constants.WallSlideTime;
            this.WallBoost?.ResetTime();
            if (moveX != 0)
            {
                this.ForceMoveX = dir;
                this.ForceMoveXTimer = Constants.WallJumpForceTime;
            }

            Speed.x = Constants.WallJumpHSpeed * dir;
            Speed.y = Constants.JumpSpeed;
            varJumpSpeed = Speed.y;

            //墙壁粒子效果
            if (dir == -1)
            {
                this.PlayJumpEffect(this.RightPosition, Vector2.left);
            }
            else
            {
                this.PlayJumpEffect(this.LeftPosition, Vector2.right);
            }
        }

        public void ClimbJump()
        {
            if (!onGround)
            {
            }

            Jump();
            WallBoost?.Active();
        }

        //在墙边dash时，当前按住上，不按住左右时，执行SuperWallJump
        public void SuperWallJump(int dir)
        {
        }

        public float WallSpeedRetentionTimer
        {
            get { return this.wallSpeedRetentionTimer; }
            set { this.wallSpeedRetentionTimer = value; }
        }

        public Vector2 Speed;

        public object Holding => null;

        public bool OnGround => this.onGround;
        public Color groundColor = Color.white;
        public Color GroundColor => this.groundColor;

        public Vector2 Position { get; private set; }

        //表示进入爬墙状态有0.1秒时间，不发生移动，为了让玩家看清发生了爬墙的动作
        public float ClimbNoMoveTimer { get; set; }
        public float VarJumpSpeed => this.varJumpSpeed;

        public float VarJumpTimer
        {
            get { return this.varJumpTimer; }
            set { this.varJumpTimer = value; }
        }

        //记录了当前帧的输入数据
        public int MoveX => moveX;
        public int MoveY => Math.Sign(Input.GetAxisRaw("Vertical"));

        public float MaxFall
        {
            get => maxFall;
            set => maxFall = value;
        }

        public float DashCooldownTimer
        {
            get => dashCooldownTimer;
            set => dashCooldownTimer = value;
        }

        public float DashRefillCooldownTimer
        {
            get => dashRefillCooldownTimer;
            set => dashRefillCooldownTimer = value;
        }

        public Vector2 LastAim { get; set; }
        public Facings Facing { get; set; } //当前朝向

        public EActionState Dash()
        {
            this.dashes = Mathf.Max(0, this.dashes - 1);
            GameInput.Dash.ConsumeBuffer();
            return EActionState.Dash;
        }

        public bool CanDash
        {
            get { return GameInput.Dash.Pressed() && dashCooldownTimer <= 0 && this.dashes > 0; }
        }

        public bool RefillDash()
        {
            if (this.dashes < Constants.MaxDashes)
            {
                this.dashes = Constants.MaxDashes;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetState(int state)
        {
            this.stateMachine.State = state;
        }

        public bool Ducking
        {
            get { return this.collider == this.duckHitbox || this.collider == this.duckHurtbox; }
            set
            {
                if (value)
                {
                    this.collider = this.duckHitbox;
                    return;
                }
                else
                {
                    this.collider = this.normalHitbox;
                }
            }
        }

        //能否退出duck状态? 如果检测到头顶是天花板，则不能解除Duck
        public bool CanUnDuck
        {
            get
            {
                if (!Ducking)
                {
                    return true;
                }

                Rect lastCollider = this.collider;
                this.collider = normalHitbox;
                bool noCollide = !CollideCheck(this.Position, Vector2.zero);
                this.collider = lastCollider;
                return noCollide;
            }
        }

        public bool DuckFreeAt(Vector2 at)
        {
            Vector2 oldP = Position;
            Rect oldC = this.collider;
            Position = at;
            this.collider = duckHitbox;

            bool ret = !CollideCheck(this.Position, Vector2.zero);

            this.Position = oldP;
            this.collider = oldC;

            return ret;
        }

        public bool IsFall
        {
            get
            {
                //上一帧不在地上，这一帧中在
                return !this.wasOnGround && this.OnGround;
            }
        }
    }
}