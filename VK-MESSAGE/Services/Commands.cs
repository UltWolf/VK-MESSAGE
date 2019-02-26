using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VK_MESSAGE.Services
{
   public class Command : ICommand
    {
        public Command(Action<object> command)
        {
            this.ActionCommand = command;
        }
        public Command(Action<object> command,Func<object,bool> canExecute)
        {
            this.ActionCommand = command;
            this.CanActionCommand = canExecute;
        }
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
        public Action<object> ActionCommand;
        public Func<object, bool> CanActionCommand;

        public bool CanExecute(object parameter)
        {
            if (CanActionCommand != null)
            {
                return CanActionCommand.Invoke(parameter);
            }
            return true;
        }

        public void Execute(object parameter)
        {
            if(ActionCommand !=null)
              ActionCommand.Invoke(parameter);
        }
    }
}
