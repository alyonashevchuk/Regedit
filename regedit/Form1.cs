using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace regedit
{
    public partial class Form1 : Form
    {
        private TreeNode root;
        private TreeNode KeyToNode(RegistryKey key)
        {
            return KeyToNode(key, key.Name);
        }
        private TreeNode KeyToNode(RegistryKey key, string shortName)
        {
            var node = new TreeNode(shortName, 0, 1);
            node.Tag = key;
            if (key.SubKeyCount > 0)
            {
                foreach (var subkey in key.GetSubKeyNames())
                {
                    try
                    {
                        node.Nodes.Add(GetNode(key.OpenSubKey(subkey), subkey));
                    }
                    catch (System.Security.SecurityException exc)
                    {
                        var restrictedNode = new TreeNode(subkey, 3, 3);
                        restrictedNode.ToolTipText = exc.Message;
                        node.Nodes.Add(restrictedNode);
                    }
                }
            }
            return node;
        }
        private TreeNode GetNode(RegistryKey key, string shortname)
        {
            var node = new TreeNode(shortname);
            node.Tag = key;
            foreach (var subkey in key.GetSubKeyNames())
            {
                try
                {
                    node.Nodes.Add(GetNode(key.OpenSubKey(subkey), subkey));
                }
                catch (System.Security.SecurityException exc)
                {
                    var restrictedNode = new TreeNode(subkey, 3, 3);
                    restrictedNode.ToolTipText = exc.Message;
                    node.Nodes.Add(restrictedNode);
                }
            }
            return node;
        }
        public Form1()
        {
            InitializeComponent();
            root = new TreeNode("Computer", 2, 2);
            //root.Nodes.Add(KeyToNode(Registry.ClassesRoot));
            root.Nodes.Add(KeyToNode(Registry.CurrentUser));
            //root.Nodes.Add(KeyToNode(Registry.LocalMachine));
            root.Nodes.Add(KeyToNode(Registry.Users));
            root.Nodes.Add(KeyToNode(Registry.CurrentConfig));
            treeView1.Nodes.Add(root);
            root.Expand();
            dataGridView1.Columns.Add("columnName", "Name");
            dataGridView1.Columns.Add("columnType", "Type");
            dataGridView1.Columns.Add("columnData", "Data");
        }
        private void registryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            RegistryKey key = null;
            if (string.IsNullOrEmpty(node.FullPath))
                return;
            string[] pathElements = node.FullPath.Split(new[] { '\\' });
            string subPath = node.FullPath.IndexOf('\\') == -1 ? node.FullPath : node.FullPath.Substring(node.FullPath.IndexOf('\\'));
            if (pathElements[1] == Registry.ClassesRoot.Name)
                key = Registry.ClassesRoot;
            else if (pathElements[1] == Registry.CurrentUser.Name)
                key = Registry.CurrentUser;
            else if (pathElements[1] == Registry.LocalMachine.Name)
                key = Registry.LocalMachine;
            else if (pathElements[1] == Registry.Users.Name)
                key = Registry.Users;
            subPath = subPath.Replace("\\" + key.Name + "\\", "");
            using (RegistryKey rk = key.OpenSubKey(subPath, true))
            {
                int k = rk.SubKeyCount;
                rk.CreateSubKey("New registry #" + (k + 1).ToString());
                treeView1.SelectedNode.Nodes.Add("New registry #" + (k + 1).ToString());
            }
        }
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
                this.contextMenuStrip1.Show(treeView1, e.Location);
            }
        } 
        private string chooseType(RegistryValueKind value)
        {
            switch (value.ToString())
            {
                case "String":
                    return "REG_SZ";
                case "ExpandString":
                    return "REG_EXPAND_SZ";
                case "Binary":
                    return "REG_BINARY";
                case "DWord":
                    return "REG_DWORD";
                case "MultiString":
                    return "REG_MULTI_SZ";
                case "QWord":
                    return "REG_QWORD";
                case "Unknown":
                    return "0";
                default:
                    return "unchecked((int)0xFFFFFFFF)";
            }
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            RegistryKey key = null;
            if (string.IsNullOrEmpty(node.FullPath))
                return;
            string[] pathElements = node.FullPath.Split(new[] { '\\' });
            string name = pathElements[pathElements.Length - 1];
            string subPath = node.FullPath.IndexOf('\\') == -1 ? node.FullPath : node.FullPath.Substring(node.FullPath.IndexOf('\\'));
            if (pathElements[1] == Registry.ClassesRoot.Name)
                key = Registry.ClassesRoot;
            else if (pathElements[1] == Registry.CurrentUser.Name)
                key = Registry.CurrentUser;
            else if (pathElements[1] == Registry.LocalMachine.Name)
                key = Registry.LocalMachine;
            else if (pathElements[1] == Registry.Users.Name)
                key = Registry.Users;
            subPath = subPath.Replace("\\" + key.Name + "\\", "");
            using (RegistryKey rk = key.OpenSubKey(subPath, true))
            {
                if (rk.SubKeyCount == 0)
                {
                    key.DeleteSubKeyTree(name, false);
                    node.Remove();
                }
                else
                {
                    var keys = rk.GetSubKeyNames();
                    foreach (var n in keys)
                        rk.DeleteSubKeyTree(n, false);
                    key.DeleteSubKeyTree(name, false);
                    node.Remove();
                }
            }
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.dataGridView1.Rows.Clear();
            RegistryKey key = null;
            if (string.IsNullOrEmpty(e.Node.FullPath))
                return;
            string[] pathElements = e.Node.FullPath.Split(new[] { '\\' });
            string rootKeyName = pathElements[0];
            string subPath = e.Node.FullPath.IndexOf('\\') == -1 ? e.Node.FullPath : e.Node.FullPath.Substring(e.Node.FullPath.IndexOf('\\'));
            if (pathElements.Length >= 2)
            {
                if (pathElements[1] == Registry.ClassesRoot.Name)
                    key = Registry.ClassesRoot;
                else if (pathElements[1] == Registry.CurrentUser.Name)
                    key = Registry.CurrentUser;
                else if (pathElements[1] == Registry.LocalMachine.Name)
                    key = Registry.LocalMachine;
                else if (pathElements[1] == Registry.Users.Name)
                    key = Registry.Users;
                if (pathElements.Length > 2)
                {
                    try
                    {
                        subPath = subPath.Replace("\\" + key.Name + "\\", "");
                        key = key.OpenSubKey(subPath);
                        foreach (string valueName in key.GetValueNames())
                            this.dataGridView1.Rows.Add(new object[] { valueName, chooseType(key.GetValueKind(valueName)), key.GetValue(valueName).ToString() });
                        key.Close();
                    }
                    catch (System.Security.SecurityException exc)
                    {
                        MessageBox.Show(exc.Message);
                    }
                }
            }
        }
    }
}
