using CommonDomain.Persistence;
using Discitur.Domain.Common;
using Discitur.Domain.Messages.Commands;
using Discitur.Domain.Model;
using Discitur.Infrastructure;
using FluentValidation;
using System;
using System.Linq;

namespace Discitur.CommandStack.Logic.Validators
{
    public class SaveNewDraftLessonCommandValidator : AbstractValidator<SaveNewDraftLessonCommand>
    {
        private readonly IRepository repo;

        public SaveNewDraftLessonCommandValidator(IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(repository != null, "repository");
            repo = repository;

            RuleFor(command => command.CreationDate).NotEmpty();
            RuleFor(command => command.AuthorId).NotEmpty();
            RuleFor(command => command.AuthorId).Must(BeValidUserId).WithMessage("Author does NOT exists. Contact support please.");
        }

        private bool BeValidUserId(Guid authorId)
        {
            User user = repo.GetById<User>(authorId);
            return (user != null && user.Id.Equals(authorId));
        }
    }

    public class AddNewCommentCommandValidator : AbstractValidator<AddNewCommentCommand>
    {
        public AddNewCommentCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.CommentId).NotEmpty();
            RuleFor(command => command.AuthorId).NotEmpty();
            RuleFor(command => command.Content).NotEmpty();
            RuleFor(command => command.Date).NotEmpty().LessThanOrEqualTo(DateTime.Now);
        }
    }

    public class EditCommentCommandValidator : AbstractValidator<EditCommentCommand>
    {
        public EditCommentCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.CommentId).NotEmpty();
            RuleFor(command => command.Content).NotEmpty();
            RuleFor(command => command.Date).NotEmpty().LessThanOrEqualTo(DateTime.Now);
        }
    }

    public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
    {
        private readonly IRepository repo;

        public DeleteCommentCommandValidator(IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(repository != null, "repository");
            repo = repository;

            RuleFor(command => command.CommentId).NotEmpty();
            RuleFor(command => command.Date).NotEmpty().LessThanOrEqualTo(DateTime.Now);
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.Id).Must(BeNotRepliedComment).WithMessage("Comment linked by other user's comments");
        }

        private bool BeNotRepliedComment(DeleteCommentCommand command, Guid lessonId)
        {
            Lesson lesson = repo.GetById<Lesson>(lessonId);
            LessonComment comment = lesson.Comments.FirstOrDefault(c => c.ParentId.Equals(command.CommentId));

            return (comment == null);
        }
    }

    public class AddNewRatingCommandValidator : AbstractValidator<AddNewRatingCommand>
    {
        private readonly IRepository repo;

        public AddNewRatingCommandValidator(IRepository repository)
        {
            Contract.Requires<ArgumentNullException>(repository != null, "repository");
            repo = repository;

            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.RatingId).NotEmpty();
            RuleFor(command => command.UserId).NotEmpty();
            RuleFor(command => command.UserId).Must(BeNotSubmittedBySameUser).WithMessage("User has already has submitted a rating for this lesson");
            RuleFor(command => command.Rating).GreaterThan(0);
            RuleFor(command => command.Date).NotEmpty().LessThanOrEqualTo(DateTime.Now);
        }

        private bool BeNotSubmittedBySameUser(AddNewRatingCommand command, Guid userId)
        {
            Lesson lesson = repo.GetById<Lesson>(command.Id);
            return !lesson.Ratings.Any(r => r.UserId.Equals(userId) && r.RecordState.Equals(Constants.RECORD_STATE_ACTIVE));

        }
    }

    public class EditRatingCommandValidator : AbstractValidator<EditRatingCommand>
    {
        public EditRatingCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.RatingId).NotEmpty();
            RuleFor(command => command.Rating).GreaterThan(0);
            RuleFor(command => command.Date).NotEmpty().LessThanOrEqualTo(DateTime.Now);
        }
    }

    public class DeleteRatingCommandValidator : AbstractValidator<DeleteRatingCommand>
    {
        public DeleteRatingCommandValidator()
        {
            RuleFor(command => command.Id).NotEmpty();
            RuleFor(command => command.RatingId).NotEmpty();
            RuleFor(command => command.Date).NotEmpty().LessThanOrEqualTo(DateTime.Now);
        }
    }

}
