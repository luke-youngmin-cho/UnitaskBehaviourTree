using Cysharp.Threading.Tasks;
using System;

namespace AISystems.BT
{
    /// <summary>
    /// This Execution task can be paused & resume.
    /// await while execution's running logic is finished and return.
    /// </summary>
    public abstract class Execution : Node, IPausable
    {
        public bool isPaused { get; set; }
        protected BehaviourTree tree;

        public Execution(BehaviourTree tree)
        {
            this.tree = tree;
        }

        public void Pause(bool pause)
        {
            isPaused = pause;
        }

        public override async UniTask<bool> Invoke()
        {
            await UniTask.WaitWhile(() => isPaused, PlayerLoopTiming.Update, tree.mainCTS.Token);

            tree.RegisterCurrentPausable(this);
            tree.RegisterCurrentExecution(this);

            bool result = await Execute();

            tree.UnregisterCurrentPausable(this);
            tree.UnregisterCurrentExecution(this);

            return result;
        }

        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnCancel() { }
        protected abstract UniTask<bool> Execute();
    }
}
