using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novike.AISystems.BT
{
    /// <summary>
    /// This is simple Execution task can delegate with Func. 
    /// </summary>
    public class FunctionExecution : Execution
    {
        private Func<UniTask<bool>> _func;

        public FunctionExecution(BehaviourTree tree, Func<UniTask<bool>> func) : base(tree)
        {
            _func = func;
        }

        protected override UniTask<bool> Execute()
        {
            return _func.Invoke();
        }
    }
}
