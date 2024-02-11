﻿using Content_Player.Models;
using Data.Entities;
using Data.Interfaces;
using Shared.Services;
using Shared.Viewers;
using System.Windows;
using System.Windows.Input;

namespace Content_Player.Windows
{
    public partial class SegmentWindow : Window
    {
        private readonly Segment _segment;

        public string SegmentTitle { get; }
        public List<IMaterial> Materials { get; } = new List<IMaterial>();
        public SegmentWindow(Segment segment)
        {
            InitializeComponent();
            DataContext = this;

            RtfService.LoadRtfFromText(rtbDescription, segment.Description);
            Title = segment.Title;

            Materials.AddRange(segment.ListeningMaterials.Cast<IMaterial>());
            Materials.AddRange(segment.ReadingMaterials.Cast<IMaterial>());
            Materials.Add(new FakeSegmentMaterial()
            {
                Id = "tasks",
                Title = "Хаарш зер"
            });

            _segment = segment;
        }

        private void OnListViewItemSelected()
        {
            if (lvMaterials.SelectedItem == null)
            {
                return;
            }

            var segmentItem = (IMaterial)lvMaterials.SelectedItem;

            switch (segmentItem)
            {
                case ReadingMaterial:
                    var readingMaterial = segmentItem as ReadingMaterial;
                    var readingPresenter = new ReadingViewer(readingMaterial.Title, readingMaterial.Text, readingMaterial.Image);
                    readingPresenter.ShowDialog();
                    break;
                case ListeningMaterial:
                    var listeningMaterial = segmentItem as ListeningMaterial;
                    var listeningPresenter = new ListeningViewer(listeningMaterial.Title, listeningMaterial.Text, listeningMaterial.Image, listeningMaterial.Audio);
                    listeningPresenter.ShowDialog();

                    break;
                case FakeSegmentMaterial:
                    var assignments = GetAllAssignments();
                    var assignmentsPresenter = new AssignmentSelector(assignments);
                    assignmentsPresenter.ShowDialog();
                    break;
            }
        }

        private List<IAssignment> GetAllAssignments()
        {
            List<IAssignment> allAssignments = _segment!.MatchingTasks.Cast<IAssignment>().ToList();
            allAssignments.AddRange(_segment!.FillingTasks);
            allAssignments.AddRange(_segment!.BuildingTasks);
            allAssignments.AddRange(_segment!.TestingTasks);
            allAssignments.AddRange(_segment!.SelectingTasks);

            return allAssignments;
        }
        private void lvMaterialsItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnListViewItemSelected();
        }

        private void lvMaterials_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                OnListViewItemSelected();
            }
        }
    }
}
