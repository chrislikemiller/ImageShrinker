
namespace ImageShrinker.DialogService
{
    public interface IDialogService
    {
        void ShowErrorBox(string message, string caption);
        void ShowInformationBox(string message, string caption);
        bool ShowYesNoQuestionBox(string message, string caption);
        string OpenFolderBrowser();
    }
}
