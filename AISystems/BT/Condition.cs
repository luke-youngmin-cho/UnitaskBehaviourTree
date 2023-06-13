using System;
using Cysharp.Threading.Tasks;

namespace Novike.AISystems.BT
{
    public class Condition : Node, IChildParent
    {
        public Node child { get; set; }
        private Func<bool> _condition;


        public Condition(Func<bool> condition)
        {
            _condition = condition;
        }

        public override async UniTask<bool> Invoke()
        {
            if (_condition.Invoke())
                return await child.Invoke();

            return false;
        }
    }
}
