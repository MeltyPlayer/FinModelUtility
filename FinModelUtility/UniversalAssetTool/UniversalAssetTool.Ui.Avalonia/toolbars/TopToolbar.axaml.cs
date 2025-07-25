using Avalonia.Controls;

using fin.io.bundles;
using ReactiveUI;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.toolbars;

public class TopToolbarModelForDesigner : TopToolbarModel {
  public IFileBundle? FileBundle => null;
}

public class TopToolbarModel : ViewModelBase {
  public IFileBundle? FileBundle {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class TopToolbar : UserControl {
  public TopToolbar() => this.InitializeComponent();
}