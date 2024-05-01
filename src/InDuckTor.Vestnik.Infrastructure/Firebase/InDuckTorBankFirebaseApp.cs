using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Microsoft.Extensions.Options;

namespace InDuckTor.Vestnik.Infrastructure.Firebase;

public class InDuckTorBankFirebaseApp : IDisposable
{
    public const string Name = "InDuckTorBank";

    private readonly IOptionsMonitor<FirebaseConfiguration> _optionsMonitor;
    private readonly IDisposable? _onChangeTracker;

    public FirebaseApp App { get; }

    public InDuckTorBankFirebaseApp(IOptionsMonitor<FirebaseConfiguration> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
        var firebaseConfiguration = _optionsMonitor.Get(Name);

        var appOptions = new AppOptions
        {
            Credential = GoogleCredential.FromFile(firebaseConfiguration.CredentialsFilePath),
            ProjectId = firebaseConfiguration.ProjectId
        };

        App = FirebaseApp.Create(appOptions, Name);

        _onChangeTracker = _optionsMonitor.OnChange(OnConfigurationChanged);
    }

    private void OnConfigurationChanged(FirebaseConfiguration configuration, string? name)
    {
        if (name != App.Name) return;

        App.Options.ProjectId = configuration.ProjectId;
        App.Options.Credential = GoogleCredential.FromFile(configuration.CredentialsFilePath);
    }

    public void Dispose() => _onChangeTracker?.Dispose();
}