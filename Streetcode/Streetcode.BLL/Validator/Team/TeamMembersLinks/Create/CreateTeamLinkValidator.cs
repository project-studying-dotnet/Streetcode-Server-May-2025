using FluentValidation;
using Streetcode.BLL.MediatR.Team.TeamMembersLinks.Create;
using Streetcode.BLL.Validator;

namespace StreetcodeBLL.Validator.Team.TeamMembersLinks.Create;

public sealed class CreateTeamLinkValidator : AbstractValidator<CreateTeamLinkCommand>
{
    public CreateTeamLinkValidator()
    {
        RuleFor(c => c.teamMember.TargetUrl).ValidUrl();
        RuleFor(c => c.teamMember.TeamMemberId).ValidId();
    }
}