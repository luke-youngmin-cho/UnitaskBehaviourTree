using Cysharp.Threading.Tasks;

namespace Novike.AISystems.BT
{
    public class Success : Node
    {
        public override async UniTask<bool> Invoke()
        {
            return true;
        }
    }
}
