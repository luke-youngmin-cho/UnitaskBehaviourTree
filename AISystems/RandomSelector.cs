using Cysharp.Threading.Tasks;
using System;
using System.Linq;

namespace AISystems.BT
{
    public class RandomSelector : Composite
    {
        public override async UniTask<bool> Invoke()
        {
            bool result;
            foreach (var child in children.OrderBy(c => Guid.NewGuid()))
            {
                result = await child.Invoke();
                if (result == true)
                    return result;
            }

            return false;
        }
    }
}
