using UnityEngine;

namespace ET.Client.Platform
{
    public enum Facings
    {
        Right = 1,
        Left = -1
    }

    public struct VitualJoyStick
    {
        public Vector2 Value
        {
            get => new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }

    public struct VisualButton
    {
        private KeyCode key;
        private float bufferTime;
        private bool consumed;
        private float bufferCounter;

        public VisualButton(KeyCode key) : this(key, 0)
        {
            
        }

        public VisualButton(KeyCode key, float bufferTime)
        {
            this.key = key;
            this.bufferTime = bufferTime;
            this.consumed = false;
            this.bufferCounter = 0f;
        }

        public void ConsumeBuffer()
        {
            this.bufferCounter = 0f;
        }

        public bool Pressed()
        {
            return Input.GetKeyDown(key) || (!this.consumed && (this.bufferCounter) > 0f);
        }

        public bool Checked()
        {
            return Input.GetKey(key);
        }

        public void Update(float deltaTime)
        {
            this.consumed = false;
            this.bufferCounter -= deltaTime;
            bool flag = false;
            if (Input.GetKeyDown(key))
            {
                this.bufferCounter = this.bufferTime;
                flag = true;
            }
            else if (Input.GetKey(key))
            {
                flag = true;
            }
            if (!flag)
            {
                this.bufferCounter = 0f;
                return;
            }
        }
    }
    
    public static class GameInput
    {
        public static VisualButton Jump = new VisualButton(KeyCode.Space, 0.08f);
        public static VisualButton Dash = new VisualButton(KeyCode.K, 0.08f);
        public static VisualButton Grab = new VisualButton(KeyCode.J);
        public static VitualJoyStick Aim = new VitualJoyStick(); // 方向
        public static Vector2 LastAim;
        
        //根据当前朝向，决定移动方向
        public static Vector3 GetAimVector(Facings defaultFacing = Facings.Right)
        {
            Vector2 value = GameInput.Aim.Value;

            if (value == Vector2.zero)
            {
                GameInput.LastAim = Vector2.right * (int)defaultFacing;
            }
            else
            {
                GameInput.LastAim = value;
            }
            
            return GameInput.LastAim.normalized;
        }

        public static void Update(float deltaTime)
        {
            Jump.Update(deltaTime);
            Dash.Update(deltaTime);
        }
    }
}