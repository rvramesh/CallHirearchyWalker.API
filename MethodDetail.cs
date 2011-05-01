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
using System.Runtime.Serialization;

namespace CallHirearchyWalker.API
{
    [DataContract]
    public class DeclaringTypeDetail
    {
        [DataMember]
        public string FullName { get; private set; }
        [DataMember]
        public string FriendlyName { get; private set; }

        public DeclaringTypeDetail(string fullName, string friendlyName)
        {
            this.FullName = fullName;
            this.FriendlyName = friendlyName;
        }
    }


    [DataContract]
    public class MethodDetail
    {
        [DataMember]
        public string MethodFullName { get; private set; }
        //[DataMember]
        //public bool IsStatic { get; private set; }
        [DataMember]
        public List<MethodDetail> UsedMethods { get; private set; }
        [DataMember]
        public DeclaringTypeDetail DeclaringTypeName{ get; private set; }
        [DataMember]
        public string FriendlyName { get; private set; }

        public string Name { get { return this.FriendlyName.Remove(this.FriendlyName.IndexOf("(")); } }

        public override int GetHashCode()
        {
            return this.MethodFullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is MethodDetail)
                return (obj as MethodDetail).MethodFullName.Equals(this.MethodFullName);
            else
                return false;
        }

        public MethodDetail(string methodFullName, string friendlyName, List<MethodDetail> calledMethods, DeclaringTypeDetail declaringTypeName)
        {
            this.MethodFullName = methodFullName;
            this.UsedMethods = calledMethods;
            // this.IsStatic = isStatic;
            this.FriendlyName = friendlyName;
            this.DeclaringTypeName = declaringTypeName;
        }

        public MethodDetail(string methodFullName, DeclaringTypeDetail declaringTypeName)
            : this(methodFullName, null, null, declaringTypeName)
        {

        }

        public override string ToString()
        {
            return this.FriendlyName;
           // return this.ToString(0);

        }

        public string ToString(int indentCount)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbIndentString = new StringBuilder();
            for (int i = 0; i < indentCount; i++)
            {
                sbIndentString.Append("\t");
            }
            sb.Append(sbIndentString.ToString());
            sb.AppendLine(this.FriendlyName);

            if (this.UsedMethods != null)
            {
                foreach (var item in this.UsedMethods)
                {
                    sb.Append(item.ToString(indentCount + 1));
                }
            }

            return sb.ToString();
        }
    }
}
