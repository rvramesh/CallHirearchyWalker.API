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
namespace CallHirearchyWalker.API
{
    public interface ICallHirearchy
    {
        void Add(MethodDetail key, MethodDetail value);
        void Add(MethodDetail key, System.Collections.Generic.List<MethodDetail> value);
        void Add(System.Collections.Generic.List<MethodDetail> details);
        IEnumerable<List<MethodDetail>> TracePath(MethodDetail detail);
    }
}
