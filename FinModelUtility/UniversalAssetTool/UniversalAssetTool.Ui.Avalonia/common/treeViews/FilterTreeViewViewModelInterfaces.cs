﻿using System;
using System.Collections.ObjectModel;

using Avalonia.Media;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.treeViews {
  // Top-level view model types

  public interface IFilterTreeViewViewModel : IViewModelBase {
    void ChangeSelection(INode node);
  }

  public interface IFilterTreeViewViewModel<T> : IFilterTreeViewViewModel {
    event EventHandler<INode<T>>? NodeSelected;
  }

  // Node types
  public interface INode : IViewModelBase {
    IImage? Icon { get; }
    string Label { get; }
  }

  public interface INode<T> : INode {
    ObservableCollection<INode<T>>? SubNodes { get; }
  }
}