using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using ImageShrinker.Common;
using ImageShrinker.DialogService;
using ImageShrinker.Factories;
using ImageShrinker.Model;
using ImageShrinker.ViewModel;


public class MainViewModel : ViewModelBase
{

    private string _path;
    private ObservableCollection<IShrinkableImage> _images = new ObservableCollection<IShrinkableImage>();
    private readonly string[] _acceptableImageTypes = { "jpeg", "jpg", "png", "bmp" };
    private readonly ShrinkerService _shrinkerService;
    private readonly IDialogService _dialogService;
    private readonly IShrinkableImageFactory _shrinkableImageFactory;
    private string _shrinkDestinationPath;
    private string _remainingPerFinished;
    private bool _isBrowseButtonEnabled = true;
    private bool _isShrinkButtonEnabled = true;
    private bool _isCancelButtonEnabled;
    private int _progressPercent;
    private int _quality;
    private string _statusLabel = "Ready to shrink.";
    private bool _shrinkingInProgress;
    private bool _cancelled;


    public RelayCommand<string> ImageSetCommand { get; set; }
    public RelayCommand BrowseCommand { get; set; }
    public RelayCommand ExitCommand { get; set; }
    public RelayCommand ShrinkCommand { get; set; }
    public RelayCommand BrowseResizePathCommand { get; set; }
    public RelayCommand CancelCommand { get; set; }


    public int Quality
    {
        get
        {
            return _quality;
        }
        set
        {
            if (_quality == value) return;
            _quality = value;
            RaisePropertyChanged();
        }
    }

    public string StatusLabel
    {
        get
        {

            return _statusLabel;
        }
        set
        {

            if (_statusLabel == value) return;
            _statusLabel = value;
            RaisePropertyChanged();
        }
    }

    public string ShrinkDestinationPath
    {
        get
        {

            if (string.IsNullOrWhiteSpace(_shrinkDestinationPath))
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Resized\\";
            return _shrinkDestinationPath;
        }
        set
        {

            if (_shrinkDestinationPath == value) return;
            _shrinkDestinationPath = value;
            RaisePropertyChanged();
        }
    }

    public int ProgressPercent
    {
        get
        {

            return _progressPercent;

        }
        set
        {

            if (_progressPercent == value) return;
            _progressPercent = value;
            RaisePropertyChanged();
        }
    }

    public string RemainingPerFinished
    {
        get
        {

            return _remainingPerFinished;
        }
        set
        {

            if (_remainingPerFinished == value) return;
            _remainingPerFinished = value;
            RaisePropertyChanged();
        }
    }

    private bool Cancelled
    {
        get
        {

            return _cancelled;
        }
        set
        {

            _cancelled = value;

        }
    }

    public string PathImages
    {
        get
        {

            return _path;
        }
        set
        {

            if (_path == value) return;
            _path = value;
            RaisePropertyChanged();
            Images.Clear();
            ImageSetCommand.Execute(value);
        }
    }

    public ObservableCollection<IShrinkableImage> Images
    {
        get
        {

            return _images;
        }
        set
        {

            if (_images == value) return;
            _images = value;
            RaisePropertyChanged();
        }
    }


    public bool IsBrowseButtonEnabled
    {
        get
        {
            return _isBrowseButtonEnabled;
        }
        set
        {
            if (_isBrowseButtonEnabled == value) return;
            _isBrowseButtonEnabled = value;
            RaisePropertyChanged();

        }
    }

    public bool IsShrinkButtonEnabled
    {
        get
        {
            return _isShrinkButtonEnabled;
        }
        set
        {
            if (_isShrinkButtonEnabled == value) return;
            _isShrinkButtonEnabled = value;
            RaisePropertyChanged();
        }
    }

    public bool IsCancelButtonEnabled
    {
        get
        {
            return _isCancelButtonEnabled;
        }
        set
        {
            if (_isCancelButtonEnabled == value) return;
            _isCancelButtonEnabled = value;
            RaisePropertyChanged();
        }
    }


    public MainViewModel(IDialogService dialogservice, ShrinkerService shrinkerService, IShrinkableImageFactory shrinkableImageFactory)
    {
        _shrinkableImageFactory = shrinkableImageFactory;
        _shrinkerService = shrinkerService;
        _dialogService = dialogservice;

        BrowseCommand = new RelayCommand(SetPathImages);
        BrowseResizePathCommand = new RelayCommand(SetShrinkDestinationPath);
        ImageSetCommand = new RelayCommand<string>(FillImageCollection);
        ShrinkCommand = new RelayCommand(Shrink);
        CancelCommand = new RelayCommand(CancelAppropriately);
        ExitCommand = new RelayCommand((() => CloseWindowWithConfirmation(null, null)));

		Quality = 90;
        _shrinkerService.CleanupTempFolder();
        _shrinkerService.ReportProgressEvent.Subscribe(ReportShrinkingProgress);
        _shrinkerService.ReportCurrentImageEvent.Subscribe(currentImage
            => UpdateStatusLabel(string.Format("Processing {0}...", currentImage)));
        _shrinkerService.CancelledEvent.Subscribe(ResetViewState);
    }



    private void FillImageCollection(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");
        try
        {
            Cancelled = false;
            DisableButtons();
            var pathList = Directory.EnumerateFiles(path).Where(IsImage).ToList();
            for (int i = 0; !Cancelled && i < pathList.Count; i++)
            {
                var i2 = i;
                Dispatcher.CurrentDispatcher.Invoke(
                    () => UpdateStatusLabel(string.Format("Adding {0}...", new FileInfo(pathList[i2]).Name)),
                    DispatcherPriority.Background);

                Dispatcher.CurrentDispatcher.Invoke(
                    () => Images.Add(_shrinkableImageFactory.CreateShrinkableImage(pathList[i2], _shrinkerService)),
                    DispatcherPriority.Background);

            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowErrorBox(
                @"Error while filling up the app with shrinkable images! Error description:\n\r" + ex.Message, @"Error");
        }
        finally
        {
            Dispatcher.CurrentDispatcher.Invoke(() => UpdateStatusLabel("Ready to shrink."),
                    DispatcherPriority.Background);
            EnableButtons();

        }
    }

    private bool IsImage(string filePath)
    {
        return ContainsAny(new FileInfo(filePath).Extension.ToLower(), _acceptableImageTypes);
    }

    private static bool ContainsAny(string text, params string[] valuesToSearchFrom)
    {
        return valuesToSearchFrom.Any(text.Contains);
    }

    private async void Shrink()
    {
        if (Images.Count == 0)
        {
            _dialogService.ShowErrorBox(@"No folder specified! Please select a folder using Browse function.", @"Error");
            return;
        }
        try
        {
            Cancelled = false;
            DisableButtons();
            _shrinkingInProgress = true;
            await _shrinkerService.ShrinkAllImagesAsync(Images, ShrinkDestinationPath, Quality);
            _shrinkingInProgress = false;
            EnableButtons();

            if (Cancelled) _dialogService.ShowInformationBox(@"Image shrinking has been cancelled!", @"Aborted");
            else _dialogService.ShowInformationBox(@"Successfully shrinked all pictures!.", @"Shrinking  finished");

            UpdateStatusLabel("Ready to shrink");
        }
        catch (Exception ex)
        {
            _dialogService.ShowErrorBox(@"An error occoured while shrinking. Error details: " + ex.Message, @"Error");
        }
        finally
        {
            UpdateStatusLabel("Ready to shrink.");
        }
    }

    private void EnableButtons()
    {
        IsBrowseButtonEnabled = true;
        IsShrinkButtonEnabled = true;
        IsCancelButtonEnabled = false;
    }

    private void DisableButtons()
    {
        IsBrowseButtonEnabled = false;
        IsShrinkButtonEnabled = false;
        IsCancelButtonEnabled = true;
    }

    private void ReportShrinkingProgress(int remainingImages)
    {
        ProgressPercent = (int)((remainingImages / (double)Images.Count) * 100);
        RemainingPerFinished = remainingImages + "/" + Images.Count;
    }

    private void SetShrinkDestinationPath()
    {
        string selectedPath = _dialogService.OpenFolderBrowser();
        if (string.IsNullOrWhiteSpace(selectedPath)) return;
        ShrinkDestinationPath = selectedPath;
    }

    private void SetPathImages()
    {
        string selectedPath = _dialogService.OpenFolderBrowser();
        if (string.IsNullOrWhiteSpace(selectedPath)) return;
        PathImages = selectedPath;
    }

    private void UpdateStatusLabel(string text)
    {
        StatusLabel = text;
    }

    private void CancelAppropriately()
    {
        if (_shrinkingInProgress) CancelShrinking();
        else CancelAddingImages();
    }

    private void CancelAddingImages()
    {
        Cancelled = true;
    }

    private void CancelShrinking()
    {
        Cancelled = true;
        _shrinkerService.StopShrinking();
    }

    private void ResetViewState()
    {
        ProgressPercent = 0;
        RemainingPerFinished = "0/" + Images.Count;
    }

    internal void CloseWindowWithConfirmation(object sender, CancelEventArgs e)
    {
        if (_isCancelButtonEnabled)
        {
            if (!_dialogService.ShowYesNoQuestionBox(@"Are you sure you want to close the application?", @"Confirmation"))
            {
                e.Cancel = true;
                return;
            }
        }
        Environment.Exit(0);
    }
}