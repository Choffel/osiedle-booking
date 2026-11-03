namespace Users.Domain.ValueObjects;

public class Role : IEnumerable<Role>
{
    public static readonly Role User = new(Guid.NewGuid(), nameof(User));
    public static readonly Role Owner = new(Guid.NewGuid(), nameof(Owner));
    public static readonly Role Admin = new(Guid.NewGuid(), nameof(Admin));

    public Guid Id { get; }
    public string Name { get; }

    private Role(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Role FromName(string name)
    {
        var role = List().FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
        if (role == null)
            throw new ArgumentException($"Invalid role name: {name}", nameof(name));

        return role;
    }

    public static Role FromId(int id)
    {
        var role = List().FirstOrDefault(r => r.Id == id);
        if (role == null)
            throw new ArgumentException($"Invalid role id: {id}", nameof(id));

        return role;
    }

    public static IEnumerable<Role> List() => new[] { User, Owner, Admin };
    
    public bool Equals(Role? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Role);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Role? left, Role? right) => Equals(left, right);

    public static bool operator !=(Role? left, Role? right) => !Equals(left, right);

    public override string ToString() => Name;
}