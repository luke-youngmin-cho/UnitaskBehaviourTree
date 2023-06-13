using Cysharp.Threading.Tasks;

namespace AISystems.BT
{
    public abstract class Node
    {
        public abstract UniTask<bool> Invoke();
    }
}
