using Cysharp.Threading.Tasks;

namespace AISystems.BT
{
    public class Failure : Node
    {
        public override async UniTask<bool> Invoke()
        {
            return false;
        }
    }
}
