using InDuckTor.Vestnik.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InDuckTor.Vestnik.Infrastructure.Database.EntityConfigurations;

public class ClientAppRegistrationConfiguration : IEntityTypeConfiguration<ClientAppRegistration>
{
    public void Configure(EntityTypeBuilder<ClientAppRegistration> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ApplicationId);
    }
}