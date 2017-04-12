using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace OSBIDE.Library.Commands
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };
        private Func<object, bool> _canExecute;
        private Action<object> _executeAction;
        private bool _canExecuteCache;

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            _executeAction = executeAction;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            bool temp = _canExecute(parameter);
            if (_canExecuteCache != temp)
            {
                _canExecuteCache = temp;
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
            }
            return _canExecuteCache;
        }

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }
    }
}
