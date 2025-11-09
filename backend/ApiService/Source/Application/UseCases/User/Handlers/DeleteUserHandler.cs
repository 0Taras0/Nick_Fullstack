using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Queries;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentValidation.Results;
using MediatR;
using RoomAggregate = Epam.ItMarathon.ApiService.Domain.Aggregate.Room.Room;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers
{
    /// <summary>
    /// Handler for Users request.
    /// </summary>
    /// <param name="roomRepository">Implementation of <see cref="IRoomRepository"/> for operating with database.</param>
    public class DeleteUserHandler(IRoomRepository roomRepository)
        : IRequestHandler<DeleteUserRequest, Result<RoomAggregate, ValidationResult>>
    {
        ///<inheritdoc/>
        public async Task<Result<RoomAggregate, ValidationResult>> Handle(DeleteUserRequest request,
            CancellationToken cancellationToken)
        {

            //1. Get Room by UserCode
            var roomResult = await roomRepository.GetByUserCodeAsync(request.UserCode, cancellationToken);
            if (roomResult.IsFailure)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(nameof(request.UserCode), "User with such code not found")
                    ]));
            }

            //2. Get Room Aggregate
            var room = roomResult.Value;

            //3. Check if Room is closed
            if (room.ClosedOn is not null)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(nameof(RoomAggregate.ClosedOn), "Cannot delete user from closed room.")
                    ]));
            }

            var users = room.Users;

            //6. Check if User exists in Room
            if (!users.Any(x => x.Id == request.UserId))
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new NotFoundError([
                    new ValidationFailure("UserId", "User with such Id not found in the room.")
                    ]));
            }


            //4. Check if UserId and UserCode correspond to the same User
            var userToDelete = users.FirstOrDefault(x => x.Id == request.UserId);
            if (userToDelete.AuthCode == request.UserCode)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure("UserId", "User cannot delete himself from the room.")
                    ]));
            }

            //5. Check if User is Admin
            var admin = users.FirstOrDefault(x => x.IsAdmin);
            if (admin.Id == request.UserId)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure("UserId", "Room admin cannot be deleted.")
                    ]));
            }
            bool isAdmin = admin.AuthCode == request.UserCode;
            if (isAdmin)
            {
                var deleteResult = room.DeleteUser(request.UserId);
                if (deleteResult.IsFailure)
                {
                    return deleteResult;
                }
            }
            else
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new NotAuthorizedError([
                    new ValidationFailure("UserCode", "Only room admin can delete users.")
                    ]));
            }

            
            


            //7. Update Room in Repository
            var updateResult = await roomRepository.UpdateAsync(room, cancellationToken);
            if (updateResult.IsFailure)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(nameof(RoomAggregate), updateResult.Error)
                    ]));
            }

            //8. Update Room in DB
            var updatedRoomResult = await roomRepository.GetByUserCodeAsync(request.UserCode, cancellationToken);
            return updatedRoomResult;
        }
    }
}