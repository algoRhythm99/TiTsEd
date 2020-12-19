using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using TiTsEd.Model;

namespace TiTsEd.Common
{
    public class TreeNode
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public List<TreeNode> Children
        {
            set;
            get;
        }

        public string ChildrenCountDisplay
        {
            get
            {
                if (null != Children)
                {
                    return String.Format("[{0}]", Children.Count.ToString() );
                }
                return "";
            }
        }
 
        public static TreeNode CreateTree(object obj, string rootNode = null)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = Int32.MaxValue;
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Formatting.Indented,
                new JsonSerializerSettings {
                    PreserveReferencesHandling = PreserveReferencesHandling.None
                });
            Dictionary<string, object> dic = jss.Deserialize<Dictionary<string, object>>(serialized);
            var root = new TreeNode();
            root.Name = rootNode ?? obj.GetType().ToString();
            BuildTree(dic, root);
            return root;
        }
 
        private static void BuildTree(object item, TreeNode node)
        {
            if (item is KeyValuePair<string, object>)
            {
                KeyValuePair<string, object> kv = (KeyValuePair<string, object>)item;
                TreeNode keyValueNode = new TreeNode();
                keyValueNode.Name = kv.Key;
                keyValueNode.Value = GetValueAsString(kv.Value);
                if (null == node.Children) node.Children = new List<TreeNode>();
                node.Children.Add(keyValueNode);
                BuildTree(kv.Value, keyValueNode);
            }
            else if (item is ArrayList)
            {
                ArrayList list = (ArrayList) item;
                int index = 0;
                foreach (object value in list)
                {
                    TreeNode arrayItem = new TreeNode();
                    arrayItem.Name = String.Format("[{0}]", index);
                    arrayItem.Value = "";
                    if (null == node.Children) node.Children = new List<TreeNode>();
                    node.Children.Add(arrayItem);
                    BuildTree(value, arrayItem);
                    index++;
                }
            }
            else if (item is Dictionary<string, object>)
            {
                Dictionary<string, object> dictionary = (Dictionary<string, object>)item;
                foreach (KeyValuePair<string, object> d in dictionary)
                {
                    BuildTree(d, node);
                }
            }
        }
 
        public static string GetValueAsString(object value)
        {
            if (null == value)
            {
                return "null";
            }

            var type = value.GetType();

            var ic = value as ICollection<object>;
            var arr = value as ArrayList;
            var ie = value as IEnumerable;
            var count = -1;

            if (null != ic)
            {
                count = ic.Count;
            }
            else if (null != arr)
            {
                count = arr.Count;
            }
            else if (null != ie)
            {
                count = 0;
                var enumerator = ie.GetEnumerator();
                if (null != enumerator)
                {
                    while (enumerator.MoveNext())
                    {
                        count++;
                    }
                }
            }
            if (value is String)
            {
                count = -1;
            }

            string ts = "";
            string vs;
            var ao = value as AmfObject;
            var aot = AmfTypes.String;
            if (null != ao)
            {
                aot = ao.AmfType;
                ts = String.Format(" [{0}] : ", aot.ToString());
            }

            if (count >= 0)
            {
                vs = String.Format("[{0}]", count);
            }
            else if (type.IsGenericType)
            {
                vs = "{}";
            }
            else
            {
                vs = value.ToString();
            }
            return String.Format("{0}{1}", ts, vs);
        }
    }
}
