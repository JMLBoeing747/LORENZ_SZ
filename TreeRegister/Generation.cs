using System.Collections.Generic;

namespace TreeRegister
{
    public static class Generation
    {
        public static List<string> ReadTreeRegister(string RegisterFilePath)
        {
            int maxDeep = 0;

            List<string> listOfRetrievedValues = new List<string>();
            if (System.IO.File.Exists(RegisterFilePath))
            {
                string[] allLinesRegister = System.IO.File.ReadAllLines(RegisterFilePath);
                //Validate maxDeep...
                foreach (string line in allLinesRegister)
                    if (line.Length % 3 == 0 && line.Length / 3 > maxDeep && line[line.Length - 3] == '[' && line[line.Length - 1] == ']')
                        maxDeep = line.Length / 3;

                foreach (string line in allLinesRegister)
                    if (line.Length > maxDeep * 3)
                        if (line.Substring((maxDeep - 1) * 3, 3) == " └─" || line.Substring((maxDeep - 1) * 3, 3) == " ├─")
                            listOfRetrievedValues.Add(line[(maxDeep * 3)..]);
            }
            else
                System.Console.WriteLine(RegisterFilePath + " does not exists.");
            return listOfRetrievedValues;
        }

        public static Tree<char> TreeLastCorrespNode(string value, Tree<char> tree, int index, ref int deep)
        {
            foreach (Tree<char> item in tree.Wires)
                if (item.Node.Equals(value[index]))
                {
                    deep++;
                    return TreeLastCorrespNode(value, item, index + 1, ref deep);
                }
            return tree;
        }

        public static IEnumerable<Queue<bool>> IteratorQueuesLastNode(Tree<char> nodeRoot, Queue<bool> queueRoot, int deep, int maxDeep)
        {
            if (deep < maxDeep)
                for (int n = 0; n < nodeRoot.Wires.Count; n++)
                {
                    Queue<bool> newQueueRoot = new Queue<bool>(queueRoot);
                    newQueueRoot.Enqueue(n + 1 == nodeRoot.Wires.Count);
                    yield return newQueueRoot;
                    foreach (Queue<bool> item in IteratorQueuesLastNode(nodeRoot.Wires[n], newQueueRoot, deep + 1, maxDeep))
                        yield return item;
                }
            else
                for (int k = 0; k < nodeRoot.Keys.Count; k++)
                {
                    Queue<bool> keyQueue = new Queue<bool>(queueRoot);
                    keyQueue.Enqueue(k + 1 == nodeRoot.Keys.Count);
                    yield return keyQueue;
                }
        }

        public static void CreateFormatedLines(List<string> linesToWriteList, Tree<char> treeRoot, Queue<Queue<bool>> allqueuesFromRegister, int order, int maxDeep)
        {
            void FormatLine(ref string lineToFormat, Queue<bool> queueNode, int order)
            {
                //Preformating...
                for (int q = 0; q < order; q++)
                    lineToFormat += queueNode.Dequeue() ? "   " : " │ ";
                //formating...
                while (queueNode.Count > 0)
                    lineToFormat += queueNode.Dequeue() ? " └─" : " ├─";
            }

            if (order < maxDeep)
                for (int n = 0; n < treeRoot.Wires.Count; n++)
                {
                    string formatLine = default;
                    Queue<bool> queueNode = allqueuesFromRegister.Dequeue();
                    FormatLine(ref formatLine, queueNode, order);
                    formatLine += "[" + treeRoot.Wires[n].Node + "]";
                    linesToWriteList.Add(formatLine);
                    CreateFormatedLines(linesToWriteList, treeRoot.Wires[n], allqueuesFromRegister, order + 1, maxDeep);
                }
            else
                for (int k = 0; k < treeRoot.Keys.Count; k++)
                {
                    string formatLine = default;
                    Queue<bool> queueNode = allqueuesFromRegister.Dequeue();
                    FormatLine(ref formatLine, queueNode, order);
                    formatLine += treeRoot.Keys[k];
                    linesToWriteList.Add(formatLine);
                }
        }

        public static void StripOutBranchDeep(List<string> linesToWriteList, int deepToStripOut)
        {
            for (int ln = 0; ln < linesToWriteList.Count; ln++)
                linesToWriteList[ln] = linesToWriteList[ln][(deepToStripOut * 3 + 3)..];
        }

        public static List<string> GenerateTreeRegister(List<string> listOfValues, int maxDeep)
        {
            Tree<char> RegisterTree = new Tree<char>('#');
            foreach (string value in listOfValues)
            {
                int order = 0;
                Tree<char> lastTree = TreeLastCorrespNode(value, RegisterTree, 0, ref order);
                for (int n = order; n < maxDeep; n++)
                {
                    if (n >= value.Length)
                    {
                        lastTree.AddNode('*');
                        lastTree = lastTree.FindTreeNode('*');
                    }
                    else
                    {
                        lastTree.AddNode(value[n]);
                        lastTree = lastTree.FindTreeNode(value[n]);
                    }
                }
                lastTree.AddKey(value);
            }

            //Preparing lines to write...
            Queue<Queue<bool>> allQueuesForRegister = new Queue<Queue<bool>>();
            foreach (Queue<bool> queueItem in IteratorQueuesLastNode(RegisterTree, new Queue<bool>(), 0, maxDeep))
                allQueuesForRegister.Enqueue(queueItem);

            List<string> linesToWrite = new List<string>();
            CreateFormatedLines(linesToWrite, RegisterTree, allQueuesForRegister, 0, maxDeep);

            //Finalizing...
            StripOutBranchDeep(linesToWrite, 0);
            return linesToWrite;
        }

        public static List<string> ReadMultipleTreeRegisters(params string[] filePaths)
        {
            //Check for similarities in filePaths...
            for (int i = 0; i < filePaths.Length - 1; i++)
                for (int j = i + 1; j < filePaths.Length; j++)
                    filePaths[j] = filePaths[i] == filePaths[j] ? null : filePaths[j];

            //Create the big list of values...
            List<string> listOfAllRetrievedValues = new List<string>();
            foreach (string path in filePaths)
                if (path != null)
                    listOfAllRetrievedValues.AddRange(ReadTreeRegister(@path));

            //Check for similarities into the big list...
            for (int i = 0; i < listOfAllRetrievedValues.Count - 1; i++)
                for (int j = i + 1; j < listOfAllRetrievedValues.Count; j++)
                    if (listOfAllRetrievedValues[i] == listOfAllRetrievedValues[j])
                    {
                        listOfAllRetrievedValues.RemoveAt(j);
                        j--;
                    }

            return listOfAllRetrievedValues;
        }
    }
}

/*
            │   179
            ║   186
            └   192
            ┬   194
            ├   195
            ─   196
            ╚   200
            ╦   203
            ╠   204
            ═   205
            */