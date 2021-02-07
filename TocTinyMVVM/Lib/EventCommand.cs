using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TocTiny.Lib
{
    class EventCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public event EventHandler ExecuteCallback;
        public EventCommand(EventHandler Callback)
        {
            ExecuteCallback += Callback;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ExecuteCallback.Invoke(this,new EventArgs());
        }
    }
}
