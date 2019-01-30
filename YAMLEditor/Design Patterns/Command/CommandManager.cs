using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YAMLEditor
{
    public class CommandManager 
    {
        protected List<Command> commandList = new List<Command>();
        private int _current = -1;

        public void addCommand(TreeNode tnode ,string type)
        {
            switch (type)
            {
                case "remove":
                    _current++;
                    if (commandList.Count > 0)
                    {
                        commandList.RemoveRange(_current, (commandList.Count - _current));
                    }
                    commandList.Add(new RemoveNode(tnode));
                    break;
                case "add":
                    _current++;
                    if (commandList.Count > 0)
                    {
                        commandList.RemoveRange(_current, (commandList.Count - _current));
                    }
                    commandList.Add(new AddNode(tnode));
                    break;
                case "edit":
                    _current++;
                    if (commandList.Count > 0)
                    {
                        commandList.RemoveRange(_current, (commandList.Count - _current));
                    }
                    commandList.Add(new EditNode(tnode));
                    break;
            }
        }

        public void Undo()
        {
            if (_current == -1) return;
            commandList[_current].UnExecute();
            _current--;
        }

        public void Redo()
        {
            if (commandList.Count == _current + 1) return;
            _current++;
            commandList[_current].Execute();
        }
    }
}