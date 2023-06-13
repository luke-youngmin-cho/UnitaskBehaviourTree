using Cysharp.Threading.Tasks;
using System;
using System.Linq;

namespace Novike.AISystems.BT
{
    public class RandomSequence : Composite
    {
        public override async UniTask<bool> Invoke()
        {
            bool result;
            foreach (var child in children.OrderBy(c => Guid.NewGuid()))
            {
                result = await child.Invoke();
                if (result == false)
                    return result;
            }

            return true;
        }
    }
}
