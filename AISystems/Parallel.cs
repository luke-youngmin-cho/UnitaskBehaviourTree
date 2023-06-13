using Cysharp.Threading.Tasks;
using System.Diagnostics;

namespace AISystems.BT
{
    internal class Parallel : Composite
    {
        private int _successPolicy;


        public Parallel(int successPolicy)
        {
            _successPolicy = successPolicy;
        }

        public override async UniTask<bool> Invoke()
        {
            bool result;
            int successCount = 0;

            foreach (var child in children)
            {
                result = await child.Invoke();
                if (result)
                {
                    successCount++;
                }
            }

            if (successCount >= _successPolicy)
                return true;
            else
                return false;
        }
    }
}
