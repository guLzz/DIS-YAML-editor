using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class Singleton
    {
        private static Singleton _instance;

        // Constructor is 'protected'
        protected Singleton()
        {

        }

        public static Singleton Instance()
        {
            // Uses lazy initialization.
            // Note: this is not thread safe.
            if (_instance == null)
            {
                _instance = new Singleton();
            }

            return _instance;
        }

        public string convertTreeViewtoCode(TreeView mainTreeView)
        {
            var code = "";

            var root = mainTreeView.TopNode;

            var rootNodeCount = root.GetNodeCount(false);

            if (rootNodeCount > 0)
            {
                code = nodeToCodeRoot(root, rootNodeCount);
            }

            return code;
        }

        private string nodeToCodeRoot(TreeNode node, int subNodes)
        {
            var code = "";
            int tabsRequired = 0;               //tabs required for code building

            for (int i = 0; i < subNodes; i++)
            {
                code += nodeToCode(node.Nodes[i], tabsRequired, false);
            }

            return code;
        }

        private string nodeToCode(TreeNode node, int tabsRequired, bool hasHyphen)
        {
            var code = "";

            var tab = "  ";

            var subNodeCount = node.GetNodeCount(false);

            int spaceIndex = node.Text.IndexOf(" ");

            for (int t = 0; t < tabsRequired; t++)
            {
                code += tab;
            }

            if (spaceIndex == -1)
            {
                if (hasHyphen && node.SelectedImageIndex == 0)
                {
                    code += "- " + node.Text + "\n";
                }
                else if (hasHyphen && node.SelectedImageIndex != 0)
                {
                    code += "- " + node.Text + ":\n";
                }
                else
                {
                    code += node.Text + ":\n";
                }
            }
            else
            {
                if (hasHyphen)
                {
                    code += "- ";

                    var nodeKey = node.Text.Substring(0, spaceIndex - 1);
                    var nodeValue = node.Text.Substring(spaceIndex + 1);

                    bool containSymbol = false;
                    bool containsSpace = false;
                    bool containsSlash = false;
                    bool isIf = false;

                    var regexItem = new Regex("^[a-zA-Z0-9 ].*$");

                    if (regexItem.IsMatch(nodeValue))
                    {
                        containSymbol = false;
                    }
                    else
                    {
                        containSymbol = true;
                    }


                    int aux = nodeValue.IndexOf(" ");

                    if (aux != -1)
                        containsSpace = true;

                    aux = nodeValue.IndexOf("/");

                    if (aux != -1)
                        containsSlash = true;

                    if (nodeValue.IndexOf(" if ") != -1 || nodeValue.IndexOf(" elif ") != -1 || nodeValue.IndexOf(" else ") != -1
                        || nodeValue.IndexOf(" endif ") != -1)
                    {
                        isIf = true;
                    }

                    if (isIf)
                    {
                        code += nodeKey + ": >-\n";
                        code += nodeToCodeIf(nodeValue, tabsRequired, false);

                    }
                    else if (nodeKey == "alias" || nodeKey == "at" || nodeKey == "api_key" || nodeKey == "target" || nodeKey == "message"
                            || nodeValue == "on" || nodeValue == "off" || nodeKey == "to" || nodeKey == "offset" || containSymbol
                            || containsSpace || containsSlash)
                    {
                        code += nodeKey + ": '" + nodeValue + "'\n";
                    }
                    else if (nodeKey == "password")
                    {
                        code += nodeKey + ": !secret " + nodeValue + "\n";
                    }
                    else
                    {
                        code += node.Text + "\n";
                    }


                }
                else
                {
                    var nodeKey = node.Text.Substring(0, spaceIndex - 1);
                    var nodeValue = node.Text.Substring(spaceIndex + 1);

                    bool containSymbol = false;
                    bool containsSpace = false;
                    bool containsSlash = false;
                    bool isIf = false;

                    var regexItem = new Regex("^[a-zA-Z0-9 ].*$");

                    if (regexItem.IsMatch(nodeValue))
                    {
                        containSymbol = false;
                    }
                    else
                    {
                        containSymbol = true;
                    }

                    int aux = nodeValue.IndexOf(" ");

                    if (aux != -1)
                        containsSpace = true;

                    aux = nodeValue.IndexOf("/");

                    if (aux != -1)
                        containsSlash = true;

                    if (nodeValue.IndexOf(" if ") != -1 || nodeValue.IndexOf(" elif ") != -1 || nodeValue.IndexOf(" else ") != -1
                        || nodeValue.IndexOf(" endif ") != -1)
                    {
                        isIf = true;
                    }

                    if (isIf)
                    {
                        code += nodeKey + ": >-\n";
                        code += nodeToCodeIf(nodeValue, tabsRequired, false);

                    }
                    else if (nodeKey == "alias" || nodeKey == "at" || nodeKey == "api_key" || nodeKey == "target" || nodeKey == "message"
                            || nodeValue == "on" || nodeValue == "off" || nodeKey == "to" || nodeKey == "offset" || containSymbol
                            || containsSpace || containsSlash)
                    {
                        code += nodeKey + ": '" + nodeValue + "'\n";
                    }
                    else if (nodeKey == "password")
                    {
                        code += nodeKey + ": !secret " + nodeValue + "\n";
                    }
                    else
                    {
                        code += node.Text + "\n";
                    }
                }
            }

            if (node.SelectedImageIndex == 3)
            {
                for (int i = 0; i < subNodeCount; i++)
                {
                    if (node.Nodes[i].SelectedImageIndex == 0)
                    {
                        code += nodeToCode(node.Nodes[i], tabsRequired+1, true);
                    }
                    else
                    {
                        var subSubNodeCount = node.Nodes[i].GetNodeCount(false);

                        for (int j = 0; j < subSubNodeCount; j++)
                        {
                            if (j == 0)
                            {
                                code += nodeToCode(node.Nodes[i].Nodes[j], tabsRequired, true);
                            }
                            else
                            {
                                code += nodeToCode(node.Nodes[i].Nodes[j], tabsRequired + 1, false);
                            }
                        }
                    }
                    if (tabsRequired == 0)
                        code += "\n";

                }
            }
            else
            {
                for (int i = 0; i < subNodeCount; i++)
                {
                    code += nodeToCode(node.Nodes[i], tabsRequired + 1, false);
                }
            }

            return code;
        }

        private string nodeToCodeIf(string nodeValue, int tabsRequired, bool foundEnd)
        {
            var code = "";
            var tab = "  ";
            bool isStatement = false;

            int beginOfLine = 0;
            int endOfLine = 0;
            var newvalue = "";
            var line = "";

            if (nodeValue.IndexOf("{%") <= nodeValue.IndexOf("{{") || nodeValue.IndexOf("{{") == -1)
            {
                isStatement = true;
            }

            if (isStatement)
            {
                beginOfLine = nodeValue.IndexOf("{%");
                endOfLine = nodeValue.IndexOf("%}");

                if (nodeValue.Length < endOfLine - beginOfLine + 2) return code;

                line = nodeValue.Substring(beginOfLine, (endOfLine - beginOfLine) + 2);

                if (nodeValue.Length != endOfLine + 2)       //check for more lines
                {
                    newvalue = nodeValue.Substring(endOfLine + 2);
                }

                

                for (int t = 0; t < tabsRequired + 2; t++)
                {
                    code += tab;
                }

                code += line + "\n";
                

                if (newvalue.Length > 0)
                {
                    if(newvalue.IndexOf("end") < newvalue.IndexOf("{{") && newvalue.IndexOf("{{") != -1)
                    {
                        code += nodeToCodeIf(newvalue, tabsRequired - 1, true);
                    }
                    else if(newvalue.IndexOf("{%") < newvalue.IndexOf("{{") && newvalue.IndexOf("el") > newvalue.IndexOf("%}"))
                    {
                        code += nodeToCodeIf(newvalue, tabsRequired + 1, false);
                    }
                    else
                    {

                        code += nodeToCodeIf(newvalue, tabsRequired, false);
                    }
                }
            }
            else
            {
                beginOfLine = nodeValue.IndexOf("{{");
                endOfLine = nodeValue.IndexOf("}}");

                line = nodeValue.Substring(beginOfLine, (endOfLine - beginOfLine) + 2);

                if (nodeValue.Length != (endOfLine - beginOfLine) + 2)       //check for more lines
                {
                    newvalue = nodeValue.Substring(endOfLine + 2);
                }

                for (int t = 0; t < tabsRequired + 3; t++)
                {
                    code += tab;
                }

                code += line + "\n";

                if (newvalue.Length > 0)
                {
                    code += nodeToCodeIf(newvalue, tabsRequired, false);
                }

            }

            return code;
        }

    }
}
