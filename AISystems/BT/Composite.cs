using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Novike.AISystems.BT
{
    public abstract class Composite : Node, IChildrenParent
    {
        public List<Node> children { get; set; }

        public Composite()
        {
            children = new List<Node>();
        }
    }
}
