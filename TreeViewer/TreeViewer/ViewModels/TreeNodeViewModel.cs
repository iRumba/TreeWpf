using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace TreeViewer.ViewModels
{
    public class TreeNodeViewModel : BindableBase
    {
        private string _name;
        private int? _value;
        private int? _value2;

        public string Name { 
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public int? Value2
        {
            get => _value2;
            set => SetProperty(ref _value2, value);
        }

        public int? Average => _value is null && _value2 is null
            ? default
            : ((_value ?? 0) + (_value2 ?? 0)) / 2;

        public bool HasChildren => Children.Count > 0;

        public TreeNodeViewModel()
        {
            Children = new ObservableCollection<TreeNodeViewModel>();
            Children.CollectionChanged += Children_CollectionChanged;

            PropertyChanged += Self_PropertyChanged;
        }

        private void Self_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Value) || e.PropertyName == nameof(Value2))
                RaisePropertyChanged(nameof(Average));
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (var item in e.NewItems.Cast<TreeNodeViewModel>())
                            Subscribe(item);
                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (var item in e.OldItems.Cast<TreeNodeViewModel>())
                            Unsubscribe(item);
                        break;
                    }
            }
            CalculateValue();
            CalculateValue2();
        }

        private void Subscribe(TreeNodeViewModel item)
        {
            item.PropertyChanged += Children_PropertyChanged;
        }

        private void Unsubscribe(TreeNodeViewModel item)
        {
            item.PropertyChanged -= Children_PropertyChanged;
        }

        private void Children_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Value):
                    CalculateValue();
                    break;
                case nameof(Value2):
                    CalculateValue2();
                    break;
            }
        }

        private void CalculateValue()
        {
            if (Children.Count == 0)
                Value = null;
            else
                Value = Children.Any(x => x.Value.HasValue) ? Children.Sum(x => x.Value) : null;
        }

        private void CalculateValue2()
        {
            if (Children.Count == 0)
                Value2 = null;
            else
                Value2 = Children.Any(x => x.Value2.HasValue) ? Children.Sum(x => x.Value2) : null;
        }

        public ObservableCollection<TreeNodeViewModel> Children { get; }
    }
}
