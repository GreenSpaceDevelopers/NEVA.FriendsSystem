using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Profile;
using Domain.Models.Users;
using GS.CommonLibrary.Dtos;
using GS.CommonLibrary.Services;
using Quartz;

namespace Application.Jobs;

public class ProcessCreatedUsersJob(ISender mediator, IQueue<AspNetUserDto> queue) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var tasks = new List<Task>();

        while (context.CancellationToken.IsCancellationRequested!)
        {
            var dto = await queue.ReadAsync(nameof(AspNetUserDto), nameof(AspNetUserDto), context.CancellationToken);

            if (!dto.isSuccess)
            {
                return;
            }

            // TODO seed roles
            var aspNetUser = new AspNetUser()
            {
                Id = Guid.NewGuid(),
                UserName = dto.data!.UserName,
                Email = dto.data.UserName,
                RoleId = Guid.NewGuid()
            };
            var createProfileCommand = new CreateProfileCommand(aspNetUser);

            tasks.Add(mediator.SendAsync(createProfileCommand, context.CancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}