using Cysharp.Threading.Tasks;

namespace Novike.AISystems.BT
{
    public class Root : Node, IChildParent
    {
        public Node child { get; set; }

        public override async UniTask<bool> Invoke()
        {
            return await child.Invoke();
        }
    }
}
