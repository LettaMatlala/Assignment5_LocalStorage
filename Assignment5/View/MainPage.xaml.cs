using Assignment5.ViewModel; // Make sure Profile.cs is in ViewModel namespace
using System.Text.Json; // For JSON serialization

namespace Assignment5
{
    public partial class MainPage : ContentPage
    {
        // Define the file path where profile.json will be stored
        private readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "profile.json");

        public MainPage()
        {
            InitializeComponent();
        }

        // Called when the page appears (loads saved profile data if available)
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadProfile();
        }

        // Loads profile data from JSON file and populates UI fields
        private void LoadProfile()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var profile = JsonSerializer.Deserialize<Profile>(json);

                // Populate UI with saved values
                NameEntry.Text = profile.Name;
                SurnameEntry.Text = profile.Surname;
                EmailEntry.Text = profile.Email;
                BioEditor.Text = profile.Bio;

                // Load profile picture if path exists
                if (!string.IsNullOrEmpty(profile.ProfileImagePath))
                    ProfileImage.Source = ImageSource.FromFile(profile.ProfileImagePath);
            }
        }

        // Saves profile data to JSON file when Save button is clicked
        private void OnSaveClicked(object sender, EventArgs e)
        {
            var profile = new Profile
            {
                Name = NameEntry.Text,
                Surname = SurnameEntry.Text,
                Email = EmailEntry.Text,
                Bio = BioEditor.Text,
                ProfileImagePath = (ProfileImage.Source as FileImageSource)?.File // Save image path
            };

            // Serialize profile object to JSON and write to file
            string json = JsonSerializer.Serialize(profile);
            File.WriteAllText(filePath, json);

            DisplayAlert("Saved", "Profile saved successfully!", "OK");
        }

        // Allows user to pick an image from device and set it as profile picture
        private async void OnChooseImageClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select Profile Picture",
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    // Copy selected image to app's local storage
                    string destPath = Path.Combine(FileSystem.AppDataDirectory, result.FileName);
                    using var stream = await result.OpenReadAsync();
                    using var fileStream = File.Create(destPath);
                    await stream.CopyToAsync(fileStream);

                    // Display chosen image
                    ProfileImage.Source = ImageSource.FromFile(destPath);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Image selection failed: {ex.Message}", "OK");
            }
        }
    }
}

