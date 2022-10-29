﻿using Data.Interfaces;
using Data.Models;
using Data.Services;
using MongoDB.Bson;
using Realms;
using static System.Net.Mime.MediaTypeNames;

namespace Data.Entities
{
    public class SegmentEntity : RealmObject, ISegment, IEntityBase
    {
        public SegmentEntity() { }
        public SegmentEntity(SegmentEntity segment)
        {
            Id = segment.Id;
            Title = segment.Title;
            Description = segment.Description;

            CelebrityWordsQuiz = new CelebrityWordsQuizEntity(segment.CelebrityWordsQuiz);
            ProverbBuilderQuiz = new ProverbBuilderQuizEntity(segment.ProverbBuilderQuiz);
            ProverbSelectionQuiz = new ProverbSelectionQuizEntity(segment.ProverbSelectionQuiz);
            GapFillerQuiz = new GapFillerQuizEntity(segment.GapFillerQuiz);
            TestingQuiz = new TestingQuizEntity(segment.TestingQuiz);

            ReadingMaterials = new List<ReadingMaterialEntity>(segment
                        .ReadingMaterials
                        .Select(rm => new ReadingMaterialEntity(rm))
                    );

            ListeningMaterials = new List<ListeningMaterialEntity>(segment
                        .ListeningMaterials
                        .Select(lm => new ListeningMaterialEntity(lm))
                    );
        }

        [PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }

        public CelebrityWordsQuizEntity CelebrityWordsQuiz { get; set; }
        public ProverbSelectionQuizEntity ProverbSelectionQuiz { get; set; }
        public ProverbBuilderQuizEntity ProverbBuilderQuiz { get; set; }
        public GapFillerQuizEntity GapFillerQuiz { get; set; }
        public TestingQuizEntity TestingQuiz { get; set; }

        public IList<ReadingMaterialEntity> ReadingMaterials { get; }
        public IList<ListeningMaterialEntity> ListeningMaterials { get; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.Now;

        public IModelBase ToModel()
        {
            return new Segment(this);
        }
        public void SetFromModel(IModelBase model)
        {
            // Set segment info, stuff that happens prior committing data from the Segment model
            var segment = model as Segment;
            Title = segment!.Title;
            Description = segment.Description;
            ModifiedAt = DateTime.Now;

            // These two method calls include all the changes made in reading and listening materials
            // while saving a Segment. Instead, it would be better to save them thorugh their own
            // repositories.
            Utils.SyncLists(ListeningMaterials, segment.ListeningMaterials);
            Utils.SyncLists(ReadingMaterials, segment.ReadingMaterials);

            if (segment.Id == null)
            {
                // Initialize a new segment
                CelebrityWordsQuiz = new();
                ProverbSelectionQuiz = new();
                ProverbBuilderQuiz = new();
                GapFillerQuiz = new();
                TestingQuiz = new();
            }
        }
    }
}
