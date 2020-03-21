using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TreeViewer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {
            LoadedCommand = new DelegateCommand(InitTreeAsync);

            Tree = new List<TreeNodeViewModel>
            {
                new TreeNodeViewModel
                {
                    Name = "root"
                }
            };
        }

        private async void InitTreeAsync()
        {
            const int deep = 3;
            const int childrenCount = 5;

            Func<TreeNodeViewModel, int, Func<string>, Task> strategyDelegate = FeelNodeAsync; // 18 sec
            //Func<TreeNodeViewModel, int, Func<string>, Task> strategyDelegate = FeelNode2Async; // 18 sec

            await strategyDelegate(Tree[0], childrenCount, () => "item");
            var currentNodes = Tree.SelectMany(x => x.Children);

            foreach (var child in Tree)
            {
                await strategyDelegate(child, childrenCount, null);
            }

            for (var i = 0; i < deep; i++)
            {
                foreach(var child in currentNodes)
                {
                    await strategyDelegate(child, childrenCount, null);
                }
                currentNodes = currentNodes.SelectMany(x => x.Children);
            }
        }

        private async Task FeelNodeAsync(TreeNodeViewModel currentNode, int childrenCount, Func<string> templateSelector = null)
        {
            var nodes = await CreateNodesAsync(childrenCount, templateSelector?.Invoke() ?? currentNode.Name);
            foreach (var node in nodes)
            {
                currentNode.Children.Add(node);
            }
        }

        private async Task FeelNode2Async(TreeNodeViewModel currentNode, int childrenCount, Func<string> templateSelector = null)
        {
            await foreach (var node in CreateNodes2Async(childrenCount, templateSelector?.Invoke() ?? currentNode.Name))
            {
                currentNode.Children.Add(node);
            }
        }

        private Task<IEnumerable<TreeNodeViewModel>> CreateNodesAsync(int count, string nameTemplate)
        {
            return Task.Run(() =>
            {
                var result = new List<TreeNodeViewModel>();
                for (var i = 0; i < count; i++)
                {
                    var rnd = new Random((int)DateTime.Now.Ticks);
                    result.Add(new TreeNodeViewModel
                    {
                        Name = $"{nameTemplate}_{i}",
                        Value = rnd.Next(1, 10),
                        Value2 = rnd.Next(1,10)
                    });
                }
                return result.AsEnumerable();
            });
        }

        private async IAsyncEnumerable<TreeNodeViewModel> CreateNodes2Async(int count, string nameTemplate)
        {
            for (var i = 0; i < count; i++)
            {
                var node = await Task.Run(() =>
                {
                    var rnd = new Random((int)DateTime.Now.Ticks);
                    return new TreeNodeViewModel
                    {
                        Name = $"{nameTemplate}_{i}",
                        Value = rnd.Next(1, 10),
                        Value2 = rnd.Next(1, 10)
                    };
                });

                yield return node;
            }
        }

        public DelegateCommand LoadedCommand { get; }

        public List<TreeNodeViewModel> Tree { get; }
    }
}
