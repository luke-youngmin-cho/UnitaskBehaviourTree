using Cysharp.Threading.Tasks;
using RPG.GameElements;
using UnityEngine;

namespace AISystems.BT
{
    /// <summary>
    /// Example that how to implement exeuction for an action with animation motion
    /// In this example, BehaviourTree has an field to remember currentAnimatorParameterID 
    /// but if you don't want to add dependency about the tree in every single node, 
    /// using static field to remember it is also can be a solution.
    /// </summary>
    public abstract class Motion : Execution
    {
        protected Transform owner;
        protected AnimatorWrapper animator;
        protected MovementBase movement;
        protected Rigidbody rigidbody;
        protected int animatorParameterID;


        public Motion(BehaviourTree tree) : base(tree)
        {
            owner = tree.transform;
            animator = tree.GetComponent<AnimatorWrapper>();
            movement = tree.GetComponent<MovementBase>();
            rigidbody = tree.GetComponent<Rigidbody>();
        }

        public override async UniTask<bool> Invoke()
        {
            if (animatorParameterID == tree.currentAnimatorParameterID)
                return false;

            if (animator.IsInTransition(0))
                return false;

            Debug.Log($"{GetType()} invoked");

            animator.SetBool(animatorParameterID, true);
            animator.SetBool(tree.currentAnimatorParameterID, false);
            tree.currentAnimatorParameterID = animatorParameterID;

            await UniTask.WaitUntil(() => animator.IsInTransition(0));

            return await base.Invoke();
        }
    }
}
