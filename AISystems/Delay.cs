using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AISystems.BT
{
    public class Delay : Node, IChildParent, IPausable
    {
        public Node child { get; set; }
        public bool isPaused { get; set; }

        private BehaviourTree _tree;
        private int _time;

        public Delay(BehaviourTree tree, int millisecond)
        {
            _tree = tree;
            _time = millisecond;
        }

        public override async UniTask<bool> Invoke()
        {
            float timeMark = Time.time;
            while (Time.time - timeMark > _time / 1000.0f)
            {
                await UniTask.WaitWhile(() => isPaused, PlayerLoopTiming.Update, _tree.mainCTS.Token);
            }

            return await child.Invoke();
        }

        public void Pause(bool pause)
        {
            isPaused = pause;
        }
    }
}
