using System.Diagnostics.CodeAnalysis;
using BCC.Core.Model.CheckRunSubmission;
using Bogus;

namespace BCC.Web.Tests.Util
{
    public static class FakerHelpers
    {
        [SuppressMessage("ReSharper", "ArgumentsStyleOther")]
        [SuppressMessage("ReSharper", "ArgumentsStyleNamedExpression")]
        public static readonly Faker<CreateCheckRun> FakeCreateCheckRun = new Faker<CreateCheckRun>()
            .CustomInstantiator(f => new CreateCheckRun(
                name: f.Random.Word(),
                title: f.Random.Word(),
                summary: f.Random.Word(),
                conclusion: f.Random.Enum<CheckConclusion>(),
                startedAt: f.Date.PastOffset(2),
                completedAt: f.Date.PastOffset())
            {
                Annotations = f.Random.Bool() ? null : FakeAnnotation.Generate(f.Random.Int(2, 10)).ToArray(),
                Images = f.Random.Bool() ? null : FakeCheckRunImage.Generate(f.Random.Int(2, 10)).ToArray()
            });

        [SuppressMessage("ReSharper", "ArgumentsStyleOther")]
        [SuppressMessage("ReSharper", "ArgumentsStyleNamedExpression")]
        public static readonly Faker<Annotation> FakeAnnotation = new Faker<Annotation>()
            .CustomInstantiator(f =>
            {
                var lineNumber = f.Random.Int(1);
                return new Annotation(
                    filename: f.System.FileName(),
                    startLine: lineNumber,
                    endLine: lineNumber,
                    annotationLevel: f.PickRandom<AnnotationLevel>(),
                    message: f.Lorem.Word())
                {
                    Title = f.Random.Words(3)
                };
            });

        [SuppressMessage("ReSharper", "ArgumentsStyleOther")]
        [SuppressMessage("ReSharper", "ArgumentsStyleNamedExpression")]
        public static readonly Faker<CheckRunImage> FakeCheckRunImage = new Faker<CheckRunImage>()
            .CustomInstantiator(f => new CheckRunImage(alt: f.Random.Words(3), imageUrl: f.Internet.Url())
            {
                Caption = f.Random.Words(3)
            });
    }
}