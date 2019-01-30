using System;
using System.IO;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class TreeObserver : AbstractObserver
    {


        public TreeObserver()
        {
            // call new save class
        }

        public override void Update(AbstractSubject subject)
        {
            //autoSave();
            MessageBox.Show("Observer");
        }

        //autosave       PASSAR PARA O OBSERVER.UPDATE 
        private void autoSave()
        {
            var fileText = convertTreeViewtoCode();
            var path = Program.path + @"\recover.yaml";

            using (Stream s = File.Open(path, FileMode.OpenOrCreate))
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