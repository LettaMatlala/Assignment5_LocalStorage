
using Assignment5.Models;
using Assignment5.Services;
using Microsoft.Maui.Storage;
using Supabase.Gotrue.Mfa;

namespace Assignment5.View
{
    public partial class MainPage : ContentPage
    {
        private readonly SupabaseService _supabase;
        private Guid _userId;
        private string _currentImageUrl = string.Empty;
        private string _localImagePath = string.Empty;

        public MainPage()
        {
            InitializeComponent();

            // Default image
            ProfileImage.Source = "profileicon.png";

            _supabase = new SupabaseService();

            // Persistent user ID
            var stored = Preferences.Get("UserId", string.Empty);
            if (!string.IsNullOrEmpty(stored))
                _userId = Guid.Parse(stored);
            else
            {
                _userId = Guid.NewGuid();
                Preferences.Set("UserId", _userId.ToString());
            }

            // Restore local avatar if available
            string savedLocal = Preferences.Get("LocalAvatarPath", string.Empty);
            if (!string.IsNullOrEmpty(savedLocal) && File.Exists(savedLocal))
            {
                _localImagePath = savedLocal;
                ProfileImage.Source = ImageSource.FromFile(savedLocal);
            }
        }

        // load profile when page appears
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (string.IsNullOrEmpty(_localImagePath))
                ProfileImage.Source = "profileicon.png";

            await LoadProfileAsync();
        }

        // load profile from Supabase
        private async Task LoadProfileAsync()
        {
            try
            {
                var profile = await _supabase.GetProfileByIdAsync(_userId);
                RestoreImage();

                if (profile != null)
                {
                    NameEntry.Text = profile.Name;
                    SurnameEntry.Text = profile.Surname;
                    EmailEntry.Text = profile.EmailAddress;
                    BioEditor.Text = profile.Bio;

                    if (!string.IsNullOrEmpty(profile.ProfileIconPath))
                    {
                        _currentImageUrl = profile.ProfileIconPath;
                        if (!string.IsNullOrEmpty(_localImagePath) && File.Exists(_localImagePath))
                            ProfileImage.Source = ImageSource.FromFile(_localImagePath);
                        else
                            ApplyRemoteImage(_currentImageUrl);
                    }
                }
            }
            catch
            {
                RestoreImage();
            }
        }

        // save button click handler
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                var profile = new UserProfile
                {
                    Id = _userId,
                    Name = NameEntry.Text?.Trim() ?? string.Empty,
                    Surname = SurnameEntry.Text?.Trim() ?? string.Empty,
                    EmailAddress = EmailEntry.Text?.Trim() ?? string.Empty,
                    Bio = BioEditor.Text?.Trim() ?? string.Empty,
                    ProfileIconPath = _currentImageUrl
                };

                await _supabase.SaveProfileAsync(profile);
                RestoreImage();
                await DisplayAlert("Success", "Profile saved!", "OK");
            }
            catch (Exception ex)
            {
                RestoreImage();
                await DisplayAlert("Save Error", ex.Message, "OK");
            }
        }

        // choose image button click handler
        private async void OnChooseImageClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a profile picture",
                    FileTypes = FilePickerFileType.Images
                });

                if (result == null) { RestoreImage(); return; }

                string fileName = $"avatar_{_userId}{Path.GetExtension(result.FullPath)}";
                string localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                File.Copy(result.FullPath, localPath, true);

                _localImagePath = localPath;
                Preferences.Set("LocalAvatarPath", localPath);

                ProfileImage.Source = ImageSource.FromFile(localPath);

                string? publicUrl = await _supabase.UploadProfilePictureAsync(_userId, localPath);
                RestoreImage();

                if (!string.IsNullOrEmpty(publicUrl))
                {
                    _currentImageUrl = publicUrl;
                    var profile = new UserProfile
                    {
                        Id = _userId,
                        Name = NameEntry.Text?.Trim() ?? string.Empty,
                        Surname = SurnameEntry.Text?.Trim() ?? string.Empty,
                        EmailAddress = EmailEntry.Text?.Trim() ?? string.Empty,
                        Bio = BioEditor.Text?.Trim() ?? string.Empty,
                        ProfileIconPath = _currentImageUrl
                    };
                    await _supabase.SaveProfileAsync(profile);
                }

                await DisplayAlert("Success", "Profile picture updated!", "OK");
            }
            catch (Exception ex)
            {
                RestoreImage();
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // restore image logic
        private void RestoreImage()
        {
            if (!string.IsNullOrEmpty(_localImagePath) && File.Exists(_localImagePath))
            {
                ProfileImage.Source = ImageSource.FromFile(_localImagePath);
                return;
            }

            string saved = Preferences.Get("LocalAvatarPath", string.Empty);
            if (!string.IsNullOrEmpty(saved) && File.Exists(saved))
            {
                _localImagePath = saved;
                ProfileImage.Source = ImageSource.FromFile(saved);
                return;
            }

            if (!string.IsNullOrEmpty(_currentImageUrl))
            {
                ApplyRemoteImage(_currentImageUrl);
                return;
            }

            ProfileImage.Source = "profileicon.png";
        }

        // apply remote image with cache busting
        private void ApplyRemoteImage(string url)
        {
            ProfileImage.Source = new UriImageSource
            {
                Uri = new Uri($"{url}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}"),
                CachingEnabled = false
            };
        }
    }
}


