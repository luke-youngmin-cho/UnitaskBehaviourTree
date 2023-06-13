using Cysharp.Threading.Tasks;

namespace Novike.AISystems.BT
{
    public class Sequence : Composite
    {
        public override async UniTask<bool> Invoke()
        {
            bool result;
            foreach (var child in children)
            {
                result = await child.Invoke();
                if (result == false)
                    return result;
            }

            return true;
        }
    }
}
