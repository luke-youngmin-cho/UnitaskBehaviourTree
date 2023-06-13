using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novike.AISystems.BT
{
    public abstract class Decorator : Node, IChildParent
    {
        public Node child { get; set; }

        public override async UniTask<bool> Invoke()
        {
            return await Decorate(await child.Invoke());
        }

        public abstract UniTask<bool> Decorate(bool childResult);
    }
}
