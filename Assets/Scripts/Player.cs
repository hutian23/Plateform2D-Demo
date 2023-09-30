using UnityEngine;

namespace ET.Client.Platform
{
    /// <summary>
    /// 玩家类: 包含
    /// 1. 玩家显示器
    /// 2. 玩家控制器
    /// 允许内部交互
    /// </summary>
    public class Player
    {
        private PlayerRenderer playerRenderer;
        private PlayerController playerController;

        private IGameContext gameContext;

        public Player(IGameContext gameContext)
        {
            this.gameContext = gameContext;
        }

        //加载玩家实体
        public void Reload(Bounds bounds, Vector2 startPosition)
        {
            this.playerRenderer = Object.Instantiate(Resources.Load<PlayerRenderer>("PlayerRenderer"));
            this.playerRenderer.Reload();
            //初始化
            this.playerController = new PlayerController(playerRenderer, gameContext.EffectControl);
            this.playerController.Init(bounds,startPosition);

            PlayerParam playerParam = Resources.Load<PlayerParam>("PlayerParams");
            playerParam.SetReloadCallback(()=>this.playerController.RefreshAbility());
            playerParam.ReloadParams();
        }

        public void Update(float deltaTime)
        {
            playerController.Update(deltaTime);
            Render();
        }

        public void Render()
        {
            playerRenderer.Render(Time.deltaTime);

            Vector2 scale = playerRenderer.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (int)playerController.Facing;
            playerRenderer.transform.localScale = scale;
            playerRenderer.transform.position = playerController.Position;

            this.lastFrameOnGround = this.playerController.OnGround;
        }

        private bool lastFrameOnGround;

        public Vector2 GetCameraPosition()
        {
            if (this.playerController == null)
            {
                return Vector3.zero;
            }
            return playerController.GetCameraPosition();
        }
    }
}