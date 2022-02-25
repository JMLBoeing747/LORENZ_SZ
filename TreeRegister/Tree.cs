using System;
using System.Collections.Generic;

namespace TreeRegister
{
    public class Tree<T>
    {
        public T Node { get; set; }
        public List<Tree<T>> Wires { get; set; } = new List<Tree<T>>();
        public List<string> Keys { get; set; } = new List<string>();

        public Tree(T nodeValue)
        {
            Node = nodeValue;
        }

        public void AddNode(Tree<T> newNode)
        {
            Wires.Add(newNode);
        }

        public void AddNode(T newNode)
        {
            Wires.Add(new Tree<T>(newNode));
        }

        public void AddKey(string key)
        {
            Keys.Add(key);
        }

        public Tree<T> DeepCopy()
        {
            return new Tree<T>(this.Node)
            {
                Wires = this.Wires,
                Keys = this.Keys
            };
        }

        public Tree<T> FindTreeNode(T nodeToFind)
        {
            foreach (Tree<T> item in Wires)
            {
                if (item.Node.Equals(nodeToFind))
                    return item.DeepCopy();
            }
            return null;
        }

        public bool NodeIsInWires(T nodeToFind)
        {
            foreach (Tree<T> item in Wires)
            {
                if (item.Node.Equals(nodeToFind))
                    return true;
            }
            return false;
        }

        public void PrintTree(int padding)
        {
            Console.WriteLine(Node.ToString().PadLeft(padding));
            foreach (Tree<T> tree in Wires)
            {
                tree.PrintTree(padding + 1);
            }
        }
    }
}