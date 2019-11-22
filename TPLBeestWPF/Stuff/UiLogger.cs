using System.Collections.ObjectModel;

namespace TPLBeestWPF.Stuff
{
    public class UiLogger : ILogger
    {
        public ObservableCollection<ListBoxThing> Items { get; }

        public UiLogger(ObservableCollection<ListBoxThing> items)
        {
            Items = items;
        }

        public void ThingChanged(ListBoxThing thing)
        {
            if (Items.Contains(thing))
            {
                var pos = Items.IndexOf(thing);
                Items.Remove(thing);
                Items.Insert(pos, thing);
            }
            else
            {
                Items.Add(thing);
            }
        }
    }
}
