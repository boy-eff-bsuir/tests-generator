using DirectoryScanner.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryScanner.Application.Services
{
    public class TreeTraverseService
    {
        public void PostorderTraverse(Tree tree, Action<Node> action)
        {
            RecursivePostorderTraverse(tree.Root, action);
        }

        private void RecursivePostorderTraverse(Node node, Action<Node> action)
        {
            foreach(var child in node.Children)
            {
                RecursivePostorderTraverse(child, action);
            }
            action.Invoke(node);
        }
    }
}
