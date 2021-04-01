using System.Windows;
using static Interface.MainWindow;

namespace Interface
{
    class TeacherRepository : IAction
    {
        void IAction.Add()
        {
            MessageBox.Show("已新增老師");
        }
    }
}
