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

namespace CallHirearchyWalker.API
{
    public class CallHirearchy2 : Dictionary<string, List<string>>
    {

        // private Dictionary<MethodDetail, List<MethodDetail>> _dictionary = new Dictionary<MethodDetail, List<MethodDetail>>();
        private string _filterBy;
        public void Add(MethodDetail key, MethodDetail value)
        {
            List<string> detail = null;
            if (_filterBy == null ||
                key.MethodFullName.Contains(_filterBy))
            {
                if (base.TryGetValue(key.MethodFullName, out detail))
                {
                    var key1 = base.Keys.Where(item => item.Equals(key.MethodFullName)).FirstOrDefault();
                    if (key1 == null)
                    {
                        throw new Exception("Key Collision");
                    }
                    detail.Add(value.MethodFullName);
                }
                else
                {
                    detail = new List<string>();
                    detail.Add(value.MethodFullName);
                    base.Add(key.MethodFullName, detail);
                }
            }
        }

        public void Add(MethodDetail key, List<MethodDetail> value)
        {
            foreach (var item in value)
            {
                this.Add(key, item);
            }
        }

        public void Add(List<MethodDetail> details)
        {
            foreach (var item in details)
            {
                foreach (var childItem in item.UsedMethods)
                {
                    this.Add(childItem, item);
                }
            }
        }

        public CallHirearchy2(string filterBy)
        {
            _filterBy = filterBy;
        }

        public CallHirearchy2()
            : this(null)
        {

        }

        //public IEnumerable<List<string>> TracePath(MethodDetail method)
        //{
        //    foreach (var item in this[method.MethodFullName])
        //    {
        //        List<string> methods = new List<string>() { method.MethodFullName };
        //        yield return methods.AddRange((TracePath(item)));
        //    }
        //}

        //private List<List<string>> TracePath(string method)
        //{
        //    List<List<string>> result = new List<List<string>>();
        //    if (this.ContainsKey(method))
        //    {
        //        foreach (var item in this[method])
        //        {
        //            var tracePathResult = TracePath(item);
        //            foreach (var path in tracePathResult)
        //            {
        //                path.Insert(0, method);
        //            }
        //            return tracePathResult;
        //        }
        //    }
        //    result.Add(new List<string>() { method });
        //    return result;
        //}
        public IEnumerable<List<string>> TracePath(MethodDetail method)
        {
            List<List<string>> result = new List<List<string>>();

            foreach (var item in this[method.MethodFullName])
            {
                result = TracePath(item, new List<string>() { method.MethodFullName });
            }
            return result;
        }

        private List<List<string>> TracePath(string method, List<string> parent)
        {
            List<List<string>> result = new List<List<string>>();
            if (this.ContainsKey(method))
            {
                parent.Add(method);
                foreach (var item in this[method])
                {
                    var tracePathResult = TracePath(item, parent);
                    foreach (var tracePathResultItem in tracePathResult)
                    {
                        result.Add(tracePathResultItem);
                    }
                }
                return result;
            }
            List<string> res = new List<string>();
            res.AddRange(parent);
            res.Add(method);
            result.Add(res);
            return result;
        }
    }

    public class CallHirearchy : Dictionary<MethodDetail, List<MethodDetail>>, ICallHirearchy
    {
        private string _filterBy;
        private bool _locked = false;

        public void Add(MethodDetail key, MethodDetail value)
        {
            if (this._locked)
            {
                throw new Exception("Object has been finalized. Cannot Add");
            }

            List<MethodDetail> detail = null;
            if (_filterBy == null ||
                key.MethodFullName.Contains(_filterBy))
            {
                if (base.TryGetValue(key, out detail))
                {
                    var key1 = base.Keys.Where(item => item.Equals(key)).FirstOrDefault();
                    if (key1 == null)
                    {
                        throw new Exception("Key Collision");
                    }
                    detail.Add(value);
                }
                else
                {
                    detail = new List<MethodDetail>();
                    detail.Add(value);
                    base.Add(key, detail);
                }
            }
        }

        public new void Add(MethodDetail key, List<MethodDetail> value)
        {
            if (this._locked)
            {
                throw new Exception("Object has been finalized. Cannot Add");
            }

            foreach (var item in value)
            {
                this.Add(key, item);
            }
        }

        public void Add(List<MethodDetail> details)
        {
            if (this._locked)
            {
                throw new Exception("Object has been finalized. Cannot Add");
            }
            foreach (var item in details)
            {
                foreach (var childItem in item.UsedMethods)
                {
                    this.Add(childItem, item);
                }
            }
        }

        public CallHirearchy(string filterBy)
        {
            _filterBy = filterBy;
        }

        public CallHirearchy()
            : this(null)
        {

        }

        public void FinalizeObject()
        {
            this._locked = true;
        }


        public IEnumerable<List<MethodDetail>> TracePath(MethodDetail method)
        {
            if (this.ContainsKey(method))
            {
                List<List<MethodDetail>> result = new List<List<MethodDetail>>();
                foreach (var item in this[method])
                {
                    List<List<MethodDetail>> callResult = (TracePath(item, new List<MethodDetail>() { method }));
                    foreach (var i in callResult)
                    {
                        result.Add(i);
                    }
                }
                return result;
            }
            else
            {
                return null;
            }

        }

        private List<List<MethodDetail>> TracePath(MethodDetail method, List<MethodDetail> parent)
        {
            List<List<MethodDetail>> result = new List<List<MethodDetail>>();
            if (this.ContainsKey(method))
            {
                List<MethodDetail> myParent = new List<MethodDetail>(parent);
                myParent.Add(method);
                foreach (var item in this[method])
                {
                    var tracePathResult = TracePath(item, myParent);
                    foreach (var tracePathResultItem in tracePathResult)
                    {
                        result.Add(tracePathResultItem);
                    }
                }
                return result;
            }
            List<MethodDetail> res = new List<MethodDetail>();
            res.AddRange(parent);
            res.Add(method);
            result.Add(res);
            return result;
        }
    }
}
