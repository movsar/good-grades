using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Shared.Controls
{
    public partial class BuildingItemViewControl : UserControl
    {
        private List<string> _shuffledWords = new List<string>();

        public BuildingItemViewControl(string phrase)
        {
            InitializeComponent();

            // Shuffle words and add them as buttons to the canvas
            _shuffledWords = phrase.Split(" ").OrderBy(w => Guid.NewGuid()).ToList();
            foreach (string word in _shuffledWords)
            {
                var btnWord = new Button
                {
                    Content = word,
                    Style = (Style)FindResource("BuilderItemButtonStyle"),
                    AllowDrop = true
                };
                btnWord.PreviewMouseLeftButtonDown += BtnWord_MouseLeftButtonDown;

                canvasWords.Children.Add(btnWord);
            }
        }

        private void BtnWord_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button btnWord = sender as Button;
            if (btnWord == null) return;

            DragDrop.DoDragDrop(btnWord, btnWord.Content, DragDropEffects.Move);
        }

        private void spDropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string word = (string)e.Data.GetData(DataFormats.StringFormat);

                var btnWord = new Button
                {
                    Content = word,
                    Style = (Style)FindResource("BuilderItemButtonStyle"),
                    AllowDrop = true
                };

                spDropZone.Children.Add(btnWord);
                // Remove the word button from the canvas
                var buttonsToRemove = canvasWords.Children.OfType<Button>().Where(b => b.Content.Equals(word)).ToList();
                foreach (var button in buttonsToRemove)
                {
                    canvasWords.Children.Remove(button);
                }
            }
        }
    }
}
