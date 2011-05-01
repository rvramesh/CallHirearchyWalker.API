/************************************************************************************************
 * DICLAIMER : The below code is hacked over a weekend. This is not of a production quality
 * and it is proved to be right and not wrong. Please use this code AS/IS and I am not 
 * responsible for damages caused.  Software distributed under the License is distributed 
 * on an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or implied.
 * 
 * Written by Ramesh Vijayaraghavan, <contact@rvramesh.com>
 *
 *
 * You are free to use this in any way you want, in case you find this useful or working for you.
 * ***********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CallHirearchyWalker.API;

namespace CallHirearchyWalker.Client
{
    class NodeCollection : Dictionary<string, Node>
    {
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            builder.AppendLine("[");
            foreach (var item in this.Values)
            {
                builder.Append(item.ToString());
                i++;
                if (i != this.Values.Count)
                {
                    builder.AppendLine(",");
                }
            }
            builder.AppendLine("]");
            return builder.ToString();
        }
    }

    class Node
    {
        public string MethodName { get; private set; }
        public List<string> TraverseTo { get; private set; }

        public Node(string methodName)
        {
            this.MethodName = methodName;
            this.TraverseTo = new List<string>();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("{")
                   .AppendLine("    \"adjacencies\": [");
            int i = 0;
            foreach (var item in TraverseTo)
            {
                builder.AppendLine("       {")
                       .AppendLine("              \"nodeTo\": \"" + item + "\",")
                       .AppendLine("              \"nodeFrom\": \"" + this.MethodName + "\",")
                       .AppendLine("              \"data\": {}")
                       .Append("       }");
                i++;
                if (i != TraverseTo.Count)
                {
                    builder.AppendLine(",");
                }
                else
                {
                    builder.AppendLine();
                }
            }
            builder.AppendLine("    ],")
                .AppendLine("    \"data\": {},")
                .AppendLine("    \"id\": \"" + this.MethodName + "\",")
                .AppendLine("    \"name\": \"" + this.MethodName + "\"")
                .AppendLine("}");
            return builder.ToString();
        }
    }

    public class JSON
    {
        public static string GetJSON(ICallHirearchy ch, MethodDetail method)
        {
            NodeCollection collection = new NodeCollection();
            var items = ch.TracePath(method);
            if (items == null)
            {
                Console.WriteLine("No Callers detected");
                return collection.ToString();
            }

            List<MethodDetail> itemsDisplayed = new List<MethodDetail>();
            foreach (var item in items)
            {
                Node n = null;
                if (item.Count == 1)
                {
                    n = GetNode(collection, item[0]);
                }
                else
                {
                    for (int i = 1; i < item.Count; i++)
                    {
                        n = GetNode(collection, item[i - 1]);
                        n.TraverseTo.Add(GetNodeKey(item[i]));
                    }
                }
            }

            return collection.ToString();
        }

        private static Node GetNode(NodeCollection collection, MethodDetail item)
        {
            Node n;
            string key = GetNodeKey(item);
            if (!collection.TryGetValue(key, out n))
            {
                n = new Node(key);
                collection.Add(n.MethodName, n);
            }
            return n;
        }

        private static string GetNodeKey(MethodDetail item)
        {
            string key = item.FriendlyName.Replace(item.DeclaringTypeName.FullName + "::", string.Empty);
            return key;
        }
    }
}
