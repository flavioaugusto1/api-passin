﻿using PassIn.Communication.Requests;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using System.Net.Mail;

namespace PassIn.Application.UseCases.Events.RegisterAttendee;

public class RegisterAttendeeOnEventUseCase
{
    private readonly PassInDbContext _dbContext;
    public RegisterAttendeeOnEventUseCase()
    {
        _dbContext = new PassInDbContext();

    }

    public ResponseRegisteredJson Execute(Guid eventId, RequestRegisterEventJson request)
    {
        Validate(eventId, request);

        var entity = new Infrastructure.Entities.Attendee
        {
            Name = request.Name,
            Email = request.Email,
            Event_id = eventId,
            Created_At = DateTime.UtcNow,
        };

        _dbContext.Attendees.Add(entity);
        _dbContext.SaveChanges();
        
        return new ResponseRegisteredJson
        {
            Id = entity.Id,
        };
    }

    private void Validate(Guid eventId, RequestRegisterEventJson request)
    {
        var eventExist = _dbContext.Events.Any(e => e.Id == eventId);

        if (!eventExist)
        {
            throw new NotFoundException("An event with this ID does not exist.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ErrorOnValidationException("The name is invalid.");
        }

        if (!EmailIsValid(request.Email))
        {
            throw new ErrorOnValidationException("The e-mail is invalid.");
        }

        var attendeeAlreadyRegistered = _dbContext
            .Attendees
            .Any(attendee => attendee.Email.Equals(request.Email) && attendee.Event_id == eventId);

        if (attendeeAlreadyRegistered)
        {
            throw new ErrorOnValidationException("You can not register twice on the same event.");
        }


    }

    private bool EmailIsValid(string email)
    {
        try
        {
            new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }

    }
}