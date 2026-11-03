namespace Business.Domain.Enums;

public enum BusinessStatus
{
    Pending = 1,     
    Active = 2,      
    Suspended = 3, //without subscription
    Archived = 4 //soft delete
}