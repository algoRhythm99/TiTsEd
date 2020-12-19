using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiTsEd.Common;

namespace TiTsEd.View
{
    /// <summary>
    /// Interaction logic for ObjectInTreeView.xaml
    /// </summary>
    public partial class ObjectInTreeView : UserControl
    {
        public static readonly DependencyProperty ObjectToVisualizeProperty =
            DependencyProperty.Register("ObjectToVisualize", typeof(object), typeof(ObjectInTreeView), new PropertyMetadata(null, OnObjectChanged));
        public static readonly DependencyProperty TreeNodesProperty =
            DependencyProperty.Register("TreeNodes", typeof(List<TreeNode>), typeof(ObjectInTreeView), new PropertyMetadata(null));
        public static readonly DependencyProperty RootNodeProperty =
            DependencyProperty.Register("RootNode", typeof(string), typeof(ObjectInTreeView), new PropertyMetadata(null, OnRootNodeChanged));

        public ObjectInTreeView()
        {
            InitializeComponent();
        }

        public object ObjectToVisualize
        {
            get
            {
                return (object) GetValue(ObjectToVisualizeProperty);
            }
            set
            {
                SetValue(ObjectToVisualizeProperty, value);
            }
        }

        public string RootNode
        {
            get
            {
                return (string) GetValue(RootNodeProperty);
            }
            set
            {
                SetValue(RootNodeProperty, value);
            }
        }

        private static void OnRootNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vm = (ObjectInTreeView) d;
            vm.RootNode = (string) e.NewValue;
        }


        private static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vm = (ObjectInTreeView) d;
            TreeNode tree = TreeNode.CreateTree(e.NewValue, vm.RootNode);
            vm.TreeNodes = new List<TreeNode>() { tree };
        }

        public List<TreeNode> TreeNodes
        {
            get
            {
                return (List<TreeNode>) GetValue(TreeNodesProperty);
            }
            set
            {
                SetValue(TreeNodesProperty, value);
            }
        }

    }
}
