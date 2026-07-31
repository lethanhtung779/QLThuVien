namespace QLTV.ViewModels
{
    public class MenuItem
    {
        public string DisplayName { get; }
        public BaseViewModel ViewModel { get; }

        public MenuItem(string displayName, BaseViewModel viewModel)
        {
            DisplayName = displayName;
            ViewModel = viewModel;
        }
    }
}
