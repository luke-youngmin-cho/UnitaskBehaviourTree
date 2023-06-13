using RPG.Threading;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dev : Luke
/// Date : 2023-06-13
/// This behavior tree is designed as a model that can wait and resume tree search at a specific node. 
/// Can wait / pause / interrupt many tasks parallelly.
/// Unitask is used to minimize garbage collection.
/// Derive the standard nodes to make your own nodes.
/// Open mind in any opinion.
/// </summary>
namespace AISystems.BT
{
    /// <summary>
    /// How to build the tree : 
    /// 
    /// BehaviourTree bt = gameObject.AddComponenet<BehaviourTree>();
    /// bt.StartBuild()
    ///     .Select()
    ///         .Condition(Func<bool>)
    ///             .FunctionExecution(Func<Unitask<bool>>)
    ///         .FunctionExecution(Func<Unitask<bool>>)
    ///     .ExitCurrentComposite();
    ///     
    /// </summary>
    public class BehaviourTree : MonoBehaviour
    {
        public bool isRunning;
        private Node _root;
        public List<Execution> currentExecutions = new List<Execution>();
        public List<IPausable> currentPausables = new List<IPausable>();
        public DisposableCancellationTokenSource mainCTS;

        private ConcurrentQueue<Action> currentExecutionsWatingQueue = new ConcurrentQueue<Action>();
        private ConcurrentQueue<Action> currentPausablesWatingQueue = new ConcurrentQueue<Action>();
        public int currentAnimatorParameterID;


        //===========================================================================
        //                             Public Methods
        //===========================================================================

        /// <summary>
        /// Call Tick every after Update and FixedUpdate 
        /// </summary>
        public async void Run()
        {
            isRunning = true;
            mainCTS = new DisposableCancellationTokenSource();
            RefreshNodeListAsync().Forget();

            while (isRunning)
            {
                await Tick();
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, mainCTS.Token);
                await UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, mainCTS.Token);
            }
        }

        public async void Stop()
        {
            if (mainCTS != null)
            {
                IEnumerable<UniTask> tasks = currentExecutions.Select(x => UniTask.Create(async () =>
                {
                    x.OnCancel();
                    return;
                }));
                await UniTask.WhenAll(tasks);

                mainCTS.Cancel();
            }

            isRunning = false;
        }

        /// <summary>
        /// Call tick once.
        /// </summary>
        public async void RunOnce()
        {
            mainCTS = new DisposableCancellationTokenSource();
            RefreshNodeListAsync().Forget();
            await Tick();
        }

        /// <summary>
        /// Unit of searching tree task.
        /// </summary>
        public async UniTask<bool> Tick()
        {
            return await _root.Invoke();
        }

        /// <summary>
        /// Pause or Release current node
        /// </summary>
        public void Pause(bool pause)
        {
            foreach (var node in currentPausables)
            {
                node?.Pause(pause);
            }
        }

        /// <summary>
        /// Interrupt with specific node temporary
        /// Pause current and await other and then resume current again.
        /// </summary>
        public void Interrupt(Node node)
        {
            UniTask.Create(async () =>
            {
                Pause(true);
                await node.Invoke();
                Pause(false);
            });
        }

        /// <summary>
        /// Stop current task and invoke specific node forcely.
        /// </summary>
        public void InvokeForcely(Node node)
        {
            UniTask.Create(async () =>
            {
                Stop();
                mainCTS = new DisposableCancellationTokenSource();
                RefreshNodeListAsync().Forget();
                await node.Invoke();
                Run();
            });
        }


        public void RegisterCurrentExecution(Execution execution)
        {
            currentExecutionsWatingQueue.Enqueue(() => currentExecutions.Add(execution));
        }

        public void UnregisterCurrentExecution(Execution execution)
        {
            currentExecutionsWatingQueue.Enqueue(() => currentExecutions.Remove(execution));
        }

        public void RegisterCurrentPausable(IPausable pausable)
        {
            currentPausablesWatingQueue.Enqueue(() => currentPausables.Add(pausable));
        }

        public void UnregisterCurrentPausable(IPausable pausable)
        {
            currentPausablesWatingQueue.Enqueue(() => currentPausables.Remove(pausable));
        }


        private void Update()
        {
            foreach (var node in currentExecutions)
            {
                node?.OnUpdate();
            }
        }

        private void FixedUpdate()
        {
            foreach (var node in currentExecutions)
            {
                node?.OnFixedUpdate();
            }
        }

        /// <summary>
        /// Refresh executions and pausables every single frame.
        /// </summary>
        /// <returns></returns>
        private async UniTask RefreshNodeListAsync()
        {
            while (true)
            {
                while (currentExecutionsWatingQueue.TryDequeue(out Action action))
                {
                    action.Invoke();
                }

                while (currentPausablesWatingQueue.TryDequeue(out Action action))
                {
                    action.Invoke();
                }

                await UniTask.Yield(PlayerLoopTiming.PreUpdate, mainCTS.Token);
            }
        }


        #region Builder
        private Stack<Composite> _compositeStack;
        private Node _current;

        public BehaviourTree StartBuild()
        {
            _current = _root = new Root();
            _compositeStack = new Stack<Composite>();
            return this;
        }

        public void AttachAsChild(Node parent, Node child)
        {
            if (parent is Composite)
            {
                ((Composite)parent).children.Add(child);
            }
            else if (parent is IChildParent)
            {
                ((IChildParent)parent).child = child;
            }
            else
            {
                throw new Exception($"[BehaviourTree] : Failed to attach {child} as child. {parent} doesn't have child.");
            }
        }

        /// <summary>
        /// Ends the build for the current Composite and returns to the parent Composite.
        /// </summary>
        public BehaviourTree ExitCurrentComposite()
        {
            if (_compositeStack.Count > 1)
            {
                _compositeStack.Pop();
                _current = _compositeStack.Peek();
            }
            else if (_compositeStack.Count == 1)
            {
                _compositeStack.Pop();
                _current = null;
            }
            else
            {
                throw new Exception($"[BehaviourTree] : Failed to Exit composite. Composite stack is empty.");
            }
            return this;
        }

        public BehaviourTree Condition(Func<bool> func)
        {
            Node node = new Condition(func);
            AttachAsChild(_current, node);
            _current = node;            
            return this;
        }

        public BehaviourTree Execution(Func<UniTask<bool>> execute)
        {
            Node node = new FunctionExecution(this, execute);
            AttachAsChild(_current, node);

            if (_compositeStack.Count > 0)
                _current = _compositeStack.Peek();
            else
                _current = null;

            return this;
        }

        public BehaviourTree Execution(Func<bool> execute)
        {
            Node node = new FunctionExecution(this, () => UniTask.Create<bool>(async () =>
            {
                return execute.Invoke();
            }));
            AttachAsChild(_current, node);

            if (_compositeStack.Count > 0)
                _current = _compositeStack.Peek();
            else
                _current = null;

            return this;
        }

        public BehaviourTree Execution(Action execute)
        {
            Node node = new FunctionExecution(this, () => UniTask.Create<bool>(async () =>
            {
                execute.Invoke();
                return true;
            }));
            AttachAsChild(_current, node);

            if (_compositeStack.Count > 0)
                _current = _compositeStack.Peek();
            else
                _current = null;

            return this;
        }

        public BehaviourTree Success()
        {
            Node node = new Success();
            AttachAsChild(_current, node);

            if (_compositeStack.Count > 0)
                _current = _compositeStack.Peek();
            else
                _current = null;

            return this;
        }

        public BehaviourTree Failure()
        {
            Node node = new Failure();
            AttachAsChild(_current, node);

            if (_compositeStack.Count > 0)
                _current = _compositeStack.Peek();
            else
                _current = null;

            return this;
        }

        public BehaviourTree Selector()
        {
            Composite node = new Selector();
            AttachAsChild(_current, node);
            _compositeStack.Push(node);
            _current = node;
            return this;
        }

        public BehaviourTree RandomSelector()
        {
            Composite node = new RandomSelector();
            AttachAsChild(_current, node);
            _compositeStack.Push(node);
            _current = node;
            return this;
        }

        public BehaviourTree Sequence()
        {
            Composite node = new Sequence();
            AttachAsChild(_current, node);
            _compositeStack.Push(node);
            _current = node;
            return this;
        }

        public BehaviourTree RandomSequence()
        {
            Composite node = new RandomSequence();
            AttachAsChild(_current, node);
            _compositeStack.Push(node);
            _current = node;
            return this;
        }

        public BehaviourTree Parallel(int successPolicy)
        {
            Composite node = new Parallel(successPolicy);
            AttachAsChild(_current, node);
            _compositeStack.Push(node);
            _current = node;
            return this;
        }
        #endregion
    }
}