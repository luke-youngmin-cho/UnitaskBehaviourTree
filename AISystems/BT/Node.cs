using Cysharp.Threading.Tasks;

namespace Novike.AISystems.BT
{
    public abstract class Node
    {
        public abstract UniTask<bool> Invoke();
    }
}
