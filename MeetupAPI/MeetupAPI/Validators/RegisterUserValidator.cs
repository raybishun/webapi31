using FluentValidation;
using MeetupAPI.Entities;
using MeetupAPI.Models;
using System.Linq;

namespace MeetupAPI.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserValidator(MeetupContext meetupContext)
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Password).MinimumLength(6);
            RuleFor(x => x.Password).Equal(x => x.ConfirmPassword);
            RuleFor(x => x.Email).Custom((value, context) =>
            {
                var userAlreadyExists = meetupContext.Users.Any(user => user.Email == value);
                if (userAlreadyExists)
                {
                    context.AddFailure("Email", "That email address is taken");
                }
            });
        }
    }
}
