using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using TPLBeestWPF.Stuff;

namespace TPLBeestWPF.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<ListBoxThing> State { get; } = new ObservableCollection<ListBoxThing>();
        public ListBoxThing SelectedItem { get; set; }

        public MainViewModel()
        {
            GoCommand = new RelayCommand(async () => await GoCommandImp(), () => true);
        }


        public ICommand GoCommand { get; private set; }
        private async Task GoCommandImp()
        {
            var logger = new UiLogger(State);

            var tpl = new TplLoggingToUiIssue(logger);

            await tpl.Go();
        }


    }
}