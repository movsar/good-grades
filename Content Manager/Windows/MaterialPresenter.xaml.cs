﻿using Data.Interfaces;
using Data.Models;
using Plugin.SimpleAudioPlayer;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Content_Manager.Windows
{
    public partial class MaterialPresenter : Window
    {

        #region Initialization
        private void SharedInitialization()
        {
            PurgeCache();
            InitializeComponent();
        }
        public MaterialPresenter(string title, string text, byte[] image)
        {
            // Reading material presenter mode

            Title = title;

            SharedInitialization();
            LoadDocument(text, image);
        }

        public MaterialPresenter(string title, string text, byte[] image, byte[] audio)
        {
            // Listening material presenter mode

            SharedInitialization();
            LoadDocument(text, image);

            Title = title;

            CrossSimpleAudioPlayer.Current.Load(new MemoryStream(audio));
            spAudioControls.Visibility = Visibility.Visible;
        }
        #endregion

        #region AudioControls
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            CrossSimpleAudioPlayer.Current.Play();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            CrossSimpleAudioPlayer.Current.Stop();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            CrossSimpleAudioPlayer.Current.Pause();
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //do my stuff before closing
            CrossSimpleAudioPlayer.Current.Stop();
            CrossSimpleAudioPlayer.Current.Dispose();
            PurgeCache();
            base.OnClosing(e);
        }
        #endregion

        private void LoadDocument(string rtf, byte[] image)
        {
            if (image != null)
            {
                BitmapImage logo = new BitmapImage();
                logo.BeginInit();
                logo.StreamSource = new MemoryStream(image);
                logo.EndInit();

                imgMain.Source = logo;
            }
            else
            {
                flowDocument.Blocks.Remove(flowImageParagraph);
            }

            MemoryStream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(rtf));
            var txtRange = new TextRange(flowContentParagraph.ContentStart, flowContentParagraph.ContentEnd);
            txtRange.Load(stream, DataFormats.Rtf);
        }
        private void PurgeCache()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
            var potentialFileNames = Enumerable.Range(0, 20).Select(number => $"{number}.wav");

            foreach (var file in Directory.GetFiles(path, "*.wav"))
            {
                if (potentialFileNames.Contains(Path.GetFileName(file)))
                {
                    File.Delete(file);
                }
            }
        }
    }
}