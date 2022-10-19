﻿using Data.Interfaces;
using Data.Models;
using MongoDB.Bson;
using Realms;
using static System.Net.Mime.MediaTypeNames;

namespace Data.Entities {
    public class SegmentEntity : RealmObject, ISegment, IEntityBase {
        [PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public IList<ReadingMaterialEntity> ReadingMaterials { get; }
        public IList<ListeningMaterialEntity> ListeningMaterials { get; }
        public CelebrityWordsQuizEntity CelebrityWordsQuiz { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.Now;
        public IModelBase AsModel() {
            return new Segment(this);
        }
        public void SetFromModel(IModelBase model) {
            // Set segment info, stuff that happens prior committing data from the Segment model
            var segment = model as Segment;
            Title = segment!.Title;
            Description = segment.Description;
            ModifiedAt = DateTime.Now;

            // These two methods include all the changes made in reading and listening materials
            // while saving a Segment. Instead, it would be better to save them thorugh their own
            // repositories.
            SetReadingMaterialsFromModel(segment);
            SetListeningMaterialsFromModel(segment);
            CelebrityWordsQuiz = new CelebrityWordsQuizEntity() { SegmentId = Id };
        }

        private void SetListeningMaterialsFromModel(Segment segment) {
            if (segment.ListeningMaterials == null) {
                return;
            }

            // Commit removed materials
            var currentListeningMaterialsIds = segment.ListeningMaterials.Select(x => x.Id).ToList();
            var listeningMaterialsToRemove = ListeningMaterials.Where(rm => !currentListeningMaterialsIds.Contains(rm.Id));
            foreach (var listeningMaterial in listeningMaterialsToRemove) {
                ListeningMaterials.Remove(listeningMaterial);
            }

            // Add or update materials
            foreach (var material in segment.ListeningMaterials) {
                var existingListeningMaterial = ListeningMaterials?.FirstOrDefault((rm => rm.Id == material.Id));
                if (existingListeningMaterial != null) {
                    existingListeningMaterial.SetFromModel(material);
                } else {
                    var newListeningMaterial = new ListeningMaterialEntity();
                    newListeningMaterial.SetFromModel(material);
                    ListeningMaterials?.Add(newListeningMaterial);
                }
            }
        }

        private void SetReadingMaterialsFromModel(Segment segment) {
            if (segment.ReadingMaterials == null) {
                return;
            }

            // Commit removed reading materials
            var currentReadingMaterialsIds = segment.ReadingMaterials.Select(x => x.Id).ToList();
            var readingMaterialsToRemove = ReadingMaterials.Where(rm => !currentReadingMaterialsIds.Contains(rm.Id));
            foreach (var readingMaterial in readingMaterialsToRemove) {
                ReadingMaterials.Remove(readingMaterial);
            }

            // Add or update reading materials
            foreach (var material in segment.ReadingMaterials) {
                var existingReadingMaterial = ReadingMaterials?.FirstOrDefault((rm => rm.Id == material.Id));
                if (existingReadingMaterial != null) {
                    existingReadingMaterial.SetFromModel(material);
                } else {
                    var newReadingMaterial = new ReadingMaterialEntity();
                    newReadingMaterial.SetFromModel(material);
                    ReadingMaterials?.Add(newReadingMaterial);
                }
            }
        }
    }
}
