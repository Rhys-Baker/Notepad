﻿using Notepad.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Notepad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Gets or sets the content data of the current document.
        /// </summary>
        public string FileData { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the current document file.
        /// </summary>
        public string FileName { get; set; } = "Untitled";

        /// <summary>
        /// Gets or sets the full path to the current document file.
        /// </summary>
        public string FilePath { get; set; } = "";

        /// <summary>
        /// Gets or sets a flag indicating whether there are unsaved changes in the current document.
        /// </summary>
        public bool ShouldSave { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the execution of the "New" command, which creates a new empty document.
        /// If there are unsaved changes in the current document, it prompts the user to save or discard them.
        /// </summary>
        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Check if there are unsaved changes in the current document.
            if (ShouldSave)
            {
                // Prompt the user to save changes and get their choice (true for save, false for discard, null for cancel).
                bool? result = AskToSaveFile();

                if (result == true && File.Exists(FilePath))
                {
                    // Save the current document before creating a new one.
                    SaveOldDocument();
                }
                //else if (result == null || (result == true ))//&& SaveAsNewDocument() == false
                else if (result == null || (result == true && SaveAsNewDocument() == false))
                {
                    // User canceled or encountered an error while saving, so do not create a new document.
                    return;
                }
            }

            // Create a new empty document.
            CreateNewDocument();
        }

        /// <summary>
        /// Opens a custom dialog to ask the user if they want to save changes to the current document.
        /// </summary>
        /// <returns>
        ///   <para><c>true</c> if the user chooses to save changes,</para>
        ///   <para><c>false</c> if the user chooses to discard changes,</para>
        ///   <para><c>null</c> if the user cancels the operation.</para>
        /// </returns>
        private bool? AskToSaveFile()
        {
            // Create a new instance of the SaveMessageBox, a custom dialog for asking to save changes.
            SaveMessageBox askToSave = new SaveMessageBox
            {
                // Set the message text based on whether a file path (FilePath) exists or not.
                Message = File.Exists(FilePath)
                    ? "Do you want to save changes to " + FilePath + "?" // File path exists, use it in the message.
                    : "Do you want to save changes to " + FileName + "?", // File path doesn't exist, use the file name.
                Owner = this // Set the owner of the dialog to this current window.
            };

            // Show the dialog to the user and Return the result of the user's choice (true for save, false for discard, null for cancel).
            return askToSave.ShowDialog();
        }

        /// <summary>
        /// Creates a new, empty document by clearing the text area and resetting document properties.
        /// </summary>
        private void CreateNewDocument()
        {
            // Clear the text area, removing any existing content.
            TextArea.Clear();

            // Set the document properties for a new, untitled document.
            FileName = "Untitled";  // Set the default file name.
            FilePath = FileData = "";  // Clear file-related data as it's a new document.

            // Update the application title to reflect the new document state.
            ShouldSave = false;  // No unsaved changes in the new document.
            this.Title = FileName + " - Notepad";  // Update the window title.
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// Executes the Save command, allowing the user to save the current document.
        /// </summary>
        /// <param name="sender">The sender of the command.</param>
        /// <param name="e">The event arguments.</param>
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Check if the current document already exists on the file system and save it.
            if (File.Exists(FilePath))
                SaveOldDocument();
            // If the document is new or has not been saved before, use SaveAsNewDocument to specify a location.
            else
                SaveAsNewDocument();
        }

        /// <summary>
        /// Saves the contents of the text area to the previously opened file.
        /// Updates the FileData, ShouldSave, and window title accordingly.
        /// </summary>
        private void SaveOldDocument()
        {
            // Write the text in the text area to the file at FilePath.
            File.WriteAllText(FilePath, TextArea.Text);

            // Update the FileData to match the saved content.
            FileData = TextArea.Text;

            // The document is now saved, so ShouldSave is set to false.
            ShouldSave = false;

            // Update the window title to reflect the saved state.
            this.Title = FileName + " - Notepad";
        }

        /// <summary>
        /// Opens a Save File dialog for the user to save the current document as a new file.
        /// </summary>
        /// <param name="defaultExtension">The default file extension to use for the new file.</param>
        /// <returns>
        /// A <see cref="bool"/> representing the dialog result. 
        /// <see langword="true"/> if the document was saved successfully, 
        /// <see langword="false"/> if the user canceled the operation, 
        /// and <see langword="null"/> if an error occurred.
        /// </returns>
        private bool? SaveAsNewDocument(string defaultExtension = ".txt")
        {
            // Create a new SaveFileDialog instance for saving the document.
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save As", // Set the dialog title.
                FileName = FileName + defaultExtension, // Set the default file name using the specified default extension.
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), // Set the initial directory to the user's documents folder.
                RestoreDirectory = true, // Allow the dialog to restore the last used directory.
                DefaultExt = defaultExtension, // Set the default file extension.
                Filter = "Text Document (*.txt)|*.txt|All Files (*.*)|*.*" // Define the filter for the file types in the dialog.
            };

            // Show the SaveFileDialog and store the result (true for success, false for failure, null for cancel).
            bool? result = saveFileDialog.ShowDialog();

            // Check if the user selected a file and clicked the "Save" button.
            if (result == true)
            {
                File.WriteAllText(saveFileDialog.FileName, TextArea.Text); // Write the content of the TextArea to the selected file.
                FilePath = saveFileDialog.FileName; // Update the FilePath to the selected file's path.
                FileName = System.IO.Path.GetFileNameWithoutExtension(FilePath); // Extract the file name without its extension.
                FileData = TextArea.Text; // Update the stored document data.
                this.Title = FileName + " - Notepad"; // Update the window title to reflect the new file name.
                ShouldSave = false; // Reset the "ShouldSave" flag as the document is now saved.
            }
            // Return the result (true for success, false for failure, null for cancel).
            return result;
        }

        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void NewWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void GoTo_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void TimeDate_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void TextArea_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if the current text matches the loaded file data
            if (TextArea.Text == FileData)
            {
                // If the text matches, set the window title to the file name without an asterisk
                this.Title = FileName + " - Notepad";

                // Mark that there's no need to save changes
                ShouldSave = false;
            }
            else
            {
                // If the text has changed, set the window title with an asterisk to indicate unsaved changes
                this.Title = "*" + FileName + " - Notepad";

                // Mark that there are unsaved changes
                ShouldSave = true;
            }
        }
    }
}
