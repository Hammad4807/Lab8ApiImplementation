using FluentValidation;
using Lab8ApiImplementation.Dtos;

namespace Lab8ApiImplementation.Validators
{
    public class ChatRequestValidator : AbstractValidator<ChatRequest>
    {
        public ChatRequestValidator()
        {
            // The list itself
            RuleFor(x => x.Messages)
                .NotEmpty().WithMessage("At least one message is required.")
                .Must(messages => messages.Count <= 40).WithMessage("A chat can have at most 40 messages. Start a new chat.");

            // Every single message inside the list
            RuleForEach(x => x.Messages).ChildRules(message =>
            {
                // Clients may only send "user" and "assistant".
                // "system" is added by our server, so users cannot change the AI's instructions.
                message.RuleFor(m => m.Role)
                    .Must(role => role == "user" || role == "assistant")
                    .WithMessage("Role must be 'user' or 'assistant'.");

                message.RuleFor(m => m.Content)
                    .NotEmpty().WithMessage("Message content is required.")
                    .MaximumLength(4000).WithMessage("A message can be at most 4000 characters.");
            });

            // The last message must be the user's new question
            RuleFor(x => x.Messages)
                .Must(messages => messages[messages.Count - 1].Role == "user")
                .When(x => x.Messages.Count > 0)
                .WithMessage("The last message must be from the user.");
        }
    }
}
