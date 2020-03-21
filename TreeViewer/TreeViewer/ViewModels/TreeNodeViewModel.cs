using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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

        private void Self_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Value) || e.PropertyName == nameof(Value2))
                RaisePropertyChanged(nameof(Average));
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems.Cast<TreeNodeViewModel>())
                    Subscribe(item);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems.Cast<TreeNodeViewModel>())
                    Unsubscribe(item);
            }
            Calculate();
        }

        private void Subscribe(TreeNodeViewModel item)
        {
            item.PropertyChanged += Children_PropertyChanged;
        }

        private void Unsubscribe(TreeNodeViewModel item)
        {
            item.PropertyChanged -= Children_PropertyChanged;
        }

        private void Children_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Value) || e.PropertyName == nameof(Value2))
                Calculate();
        }

        private void Calculate()
        {
            Value = Children.Any(x => x.Value.HasValue) ? Children.Sum(x => x.Value) : null;
            Value2 = Children.Any(x => x.Value2.HasValue) ? Children.Sum(x => x.Value2) : null;
        }

        public ObservableCollection<TreeNodeViewModel> Children { get; }
    }
}
