using System.Collections;

namespace ET.Client.Platform
{
    public enum EActionState
    {
        Normal,
        Dash,
        Climb,
        Size
    }

    public abstract class BaseActionState
    {
        protected EActionState state;
        protected PlayerController ctx;

        protected BaseActionState(EActionState state, PlayerController context)
        {
            this.state = state;
            this.ctx = context;
        }

        public EActionState State
        {
            get => state;
        }

        //每一帧都执行的逻辑
        public abstract EActionState Update(float deltaTime);

        public abstract IEnumerator Coroutine();

        public abstract void OnBegin();

        public abstract void OnEnd();

        public abstract bool IsCoroutine();
    }

    /// <summary>
    /// 有限状态机
    /// </summary>
    public class FiniteStateMachine<S> where S : BaseActionState
    {
        private S[] states;

        private int currState = -1;
        private int preState = -1;
        private Coroutine currentCoroutine;

        public FiniteStateMachine(int size)
        {
            this.states = new S[size];
            this.currentCoroutine = new Coroutine(true);
        }

        public void AddState(S state)
        {
            this.states[(int)state.State] = state;
        }

        public void Update(float deltaTime)
        {
            State = (int)this.states[this.currState].Update(deltaTime);
            if (this.currentCoroutine.Active)
            {
                this.currentCoroutine.Update(deltaTime);
            }
        }


        public int State
        {
            get
            {
                return this.currState;
            }
            set
            {
                if (this.currState == value)
                    return;
                this.preState = this.currState;
                this.currState = value;
                Logging.Log(
                    $"====Enter State[{(EActionState)this.currState}],leave State[{(EActionState)this.preState}]");
                if (this.preState != -1)
                {
                    Logging.Log($"====State[{(EActionState)this.preState}] OnEnd");
                    this.states[this.preState].OnEnd();
                }

                Logging.Log($"====State[{(EActionState)this.currState}] OnBegin");
                this.states[this.currState].OnBegin();
                if (this.states[this.currState].IsCoroutine())
                {
                    this.currentCoroutine.Replace(this.states[this.currState].Coroutine());
                    return;
                }

                this.currentCoroutine.Cancel();
            }
        }
    }
}