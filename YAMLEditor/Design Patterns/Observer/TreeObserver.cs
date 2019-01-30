using System;
using System.Drawing.Text;
using System.Dynamic;
using System.IO;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class TreeObserver : AbstractObserver
    {
        private Singleton nodeSingleton = Singleton.Instance();
        private TreeView _tview = new TreeView();

        public TreeObserver(TreeView tview)
        {
            this._tview = tview;
        }

        public override void Update(AbstractSubject subject)
        {
            autoSave();
        }

        private void autoSave()
        {
            var fileText = nodeSingleton.convertTreeViewtoCode(_tview);
            var path = Program.path + @"\recover.yaml";
            File.Delete(path);
            using (Stream s = File.Open(path, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(s))
                {
                    File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
                    sw.Write(fileText);
                }
            }
        }
    }
}