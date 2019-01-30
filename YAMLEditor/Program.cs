using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YAMLEditor
{
    static class Program
    {
        public static string path;
        public static bool hasRecover;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            path = Environment.CurrentDirectory +(@"..\..\..\..\Recover\");
            hasRecover = File.Exists(path + "recover.yaml");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new YAMLEditorForm() );
        }
    }
}
